using FrameZone_WebApi.Controllers;
using FrameZone_WebApi.Shopping.DTOs;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace FrameZone_WebApi.Shopping.Services
{
    public class ECPayService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ECPayService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // 設定各參數的最大長度（只列特別需要檢查的）
        private Dictionary<string, int> ECPayOrderParamsMaxLength = new Dictionary<string, int> {
            //{"MerchantID", 10},
            {"MerchantTradeNo", 20},
            //{"MerchantTradeDate", 20},
            //{"PaymentType", 20},
            {"TradeDesc", 200},
            {"ItemName", 400},
            //{"ReturnURL", 200},
            //{"ChoosePayment", 20},
            //{"ClientBackURL", 200},
            //{"OrderResultURL", 200},
        };

        public ECPayService(IConfiguration configuration, ILogger<ECPayService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetBaseUrl()
        {
            var ctx = _httpContextAccessor.HttpContext;
            if (ctx == null) return string.Empty;
            return $"{ctx.Request.Scheme}://{ctx.Request.Host}";
        }

        // 準備綠界要的參數
        // 將 merchantTradeNo 改為外部傳入，是為了讓 Controller 能夠精確控制交易編號（例如包含 OrderId），
        // 這樣在綠界非同步回傳付款結果時，才能準確地抓到是對應哪一筆本地訂單。
        public ECPayOrderParamsDto CreateECPayOrder(OrderDto order, string merchantTradeNo)
        {
            try
            {
                ECPayOrderParamsDto orderParams = new ECPayOrderParamsDto();

                orderParams.Params["MerchantID"] = _configuration["ECPaySettings:MerchantID"];
                orderParams.Params["MerchantTradeNo"] = merchantTradeNo;
                orderParams.Params["MerchantTradeDate"] = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                orderParams.Params["TotalAmount"] = order.TotalAmount;
                orderParams.Params["TradeDesc"] = "FrameZone Order " + merchantTradeNo;
                orderParams.Params["ChoosePayment"] = order.PaymentMethod;

                // 商品明細
                if (order.OrderItems != null)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    bool isFirst = true;

                    foreach (var item in order.OrderItems)
                    {
                        if (isFirst)
                        {
                            stringBuilder.Append(item.Name?.ToString() + " * " + item.Quantity.ToString());
                            isFirst = false;
                        }
                        else
                        {
                            stringBuilder.Append("#" + item.Name?.ToString() + " * " + item.Quantity.ToString());
                        }
                    }
                    orderParams.Params["ItemName"] = stringBuilder.ToString();
                }

                // 其他可選參數帶入前端給的資訊
                if (order.OptionParams != null)
                {
                    foreach (KeyValuePair<string, object> option in order.OptionParams)
                    {
                        // 使用索引器而不是 Add()，這樣不會因重複 key 報錯
                        orderParams.Params[option.Key] = option.Value;
                    }
                }

                // ===== URL 設定（必須在 optionParams 之後） =====
                var baseUrl = GetBaseUrl().TrimEnd('/');

                // 1) 綠界 Server-to-Server 付款通知：只回 1|OK
                var payResultPath = _configuration["ECPaySettings:PayResultPath"] ?? "/api/order/pay-result";
                var serverReturnUrl = $"{baseUrl}{payResultPath}";

                // 2) 使用者付款完成回跳：導到中轉頁（它會再導回前端 order-success）
                var successRedirectPath = _configuration["ECPaySettings:SuccessRedirectPath"] ?? "/api/order/success-redirect";

                // 把 MerchantTradeNo 掛在 querystring，避免 GET 回跳拿不到資料
                var successRedirectUrlWithTradeNo =
                    $"{baseUrl}{successRedirectPath}?MerchantTradeNo={HttpUtility.UrlEncode(merchantTradeNo)}";

                // ReturnURL（Server-to-Server）
                orderParams.Params["ReturnURL"] = serverReturnUrl;

                // OrderResultURL（回跳）
                orderParams.Params["OrderResultURL"] = successRedirectUrlWithTradeNo;

                // ClientBackURL（返回商店）
                orderParams.Params["ClientBackURL"] = successRedirectUrlWithTradeNo;

                _logger.LogInformation($"最終 ReturnURL: {orderParams.Params["ReturnURL"]}");
                _logger.LogInformation($"最終 OrderResultURL: {orderParams.Params["OrderResultURL"]}");
                _logger.LogInformation($"最終 ClientBackURL: {orderParams.Params["ClientBackURL"]}");

                return CheckECPayOrderParams(orderParams);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CreateECPayOrder 錯誤: {ex.Message}");
                _logger.LogError($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }


        public void HandleECPayOrderResult(FormCollection result)
        {
            //_logger.LogInformation(result.ToString()); //付款完成結果紀錄在log，還未寫進資料庫
        }


        public class QueryTradeInfoResult
        {
            public bool IsSuccess { get; set; }
            public string ErrorMessage { get; set; } = "";
            public string MerchantTradeNo { get; set; } = "";
            public string TradeStatus { get; set; } = "";   // 0=未付款, 1=已付款
            public string PaymentDate { get; set; } = "";
            public string TradeNo { get; set; } = "";
            public Dictionary<string, string> Raw { get; set; } = new();
        }

        // 綠界：訂單查詢（QueryTradeInfo/V5）
        // 用途：非即時付款（ATM）在本機開發收不到 ReturnURL 時，可由後端主動查詢是否已入帳。
        public async Task<QueryTradeInfoResult> QueryTradeInfoAsync(string merchantTradeNo)
        {
            var result = new QueryTradeInfoResult { MerchantTradeNo = merchantTradeNo };

            try
            {
                var merchantId = _configuration["ECPaySettings:MerchantID"] ?? "";
                if (string.IsNullOrWhiteSpace(merchantId))
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = "Missing ECPaySettings:MerchantID";
                    return result;
                }

                // 依綠界規範：TimeStamp 必須在 3 分鐘內
                var timeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

                var reqParams = new SortedDictionary<string, string>
                {
                    ["MerchantID"] = merchantId,
                    ["MerchantTradeNo"] = merchantTradeNo,
                    ["TimeStamp"] = timeStamp
                };

                var paramString = string.Join("&", reqParams.Select(kv => $"{kv.Key}={kv.Value}"));
                var checkMac = BuildCheckMacValue(paramString);

                var postParams = new Dictionary<string, string>(reqParams)
                {
                    ["CheckMacValue"] = checkMac
                };

                var url = GetQueryTradeInfoUrl();

                using var http = new HttpClient();
                http.Timeout = TimeSpan.FromSeconds(15);

                using var content = new FormUrlEncodedContent(postParams);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                var resp = await http.PostAsync(url, content);
                var body = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = $"ECPay QueryTradeInfo HTTP {(int)resp.StatusCode}: {body}";
                    return result;
                }

                // 回傳格式通常為 querystring：Key=Value&Key=Value...
                var nvc = HttpUtility.ParseQueryString(body ?? "");
                foreach (var keyObj in nvc.AllKeys)
                {
                    if (string.IsNullOrWhiteSpace(keyObj)) continue;
                    result.Raw[keyObj] = nvc[keyObj] ?? "";
                }

                // 基本欄位
                result.TradeStatus = result.Raw.TryGetValue("TradeStatus", out var ts) ? ts : "";
                result.PaymentDate = result.Raw.TryGetValue("PaymentDate", out var pd) ? pd : "";
                result.TradeNo = result.Raw.TryGetValue("TradeNo", out var tn) ? tn : "";

                // 驗簽（回傳的 CheckMacValue）
                if (result.Raw.TryGetValue("CheckMacValue", out var respMac) && !string.IsNullOrWhiteSpace(respMac))
                {
                    var verifyDict = new SortedDictionary<string, string>(
                        result.Raw
                            .Where(kv => !string.Equals(kv.Key, "CheckMacValue", StringComparison.OrdinalIgnoreCase))
                            .ToDictionary(kv => kv.Key, kv => kv.Value)
                    );

                    var verifyString = string.Join("&", verifyDict.Select(kv => $"{kv.Key}={kv.Value}"));
                    var verifyMac = BuildCheckMacValue(verifyString);

                    if (!string.Equals(verifyMac, respMac, StringComparison.OrdinalIgnoreCase))
                    {
                        result.IsSuccess = false;
                        result.ErrorMessage = "QueryTradeInfo CheckMacValue mismatch";
                        return result;
                    }
                }

                result.IsSuccess = true;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "QueryTradeInfoAsync failed. MerchantTradeNo={MerchantTradeNo}", merchantTradeNo);
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
        }

        private string GetQueryTradeInfoUrl()
        {
            // 可用設定覆蓋；否則依 ServiceURL 或 UseStage 判斷
            var overrideUrl = _configuration["ECPaySettings:QueryTradeInfoUrl"];
            if (!string.IsNullOrWhiteSpace(overrideUrl))
                return overrideUrl;

            bool useStage = false;

            var useStageStr = _configuration["ECPaySettings:UseStage"];
            if (bool.TryParse(useStageStr, out var parsed))
                useStage = parsed;

            if (!useStage)
            {
                var serviceUrl = _configuration["ECPaySettings:ServiceURL"] ?? _configuration["ECPaySettings:ServiceUrl"] ?? "";
                if (serviceUrl.Contains("stage", StringComparison.OrdinalIgnoreCase))
                    useStage = true;
            }

            return useStage
                ? "https://payment-stage.ecpay.com.tw/Cashier/QueryTradeInfo/V5"
                : "https://payment.ecpay.com.tw/Cashier/QueryTradeInfo/V5";
        }

        private ECPayOrderParamsDto CheckECPayOrderParams(ECPayOrderParamsDto orderParams)
        {
            // 檢查各項參數是否超過最大長度
            foreach (KeyValuePair<string, int> kvp in ECPayOrderParamsMaxLength)
            {
                object? checkObject;
                if (orderParams.Params.TryGetValue(kvp.Key, out checkObject)) //檢查是否有參數名稱
                {
                    string checkString = (string)checkObject;
                    if (checkString.Length >= kvp.Value) //檢查參數長度是否超過設定值
                    {
                        orderParams.Params[kvp.Key] = checkString.Substring(0, kvp.Value); //如果長度超過只取到最大長度
                    }
                }
            }

            // 計算檢查碼
            string orderParamsString = string.Join("&", orderParams.Params.Keys.OrderBy(key => key).Select(key => key + "=" + orderParams.Params[key].ToString()).ToList());
            orderParams.Params["CheckMacValue"] = BuildCheckMacValue(orderParamsString);
            //綠界範例:orderParams.Params["CheckMacValue"] = BuildCheckMacValue("ChoosePayment=ALL&EncryptType=1&ItemName=Apple iphone 15&MerchantID=3002607&MerchantTradeDate=2023/03/12 15:30:23&MerchantTradeNo=ecpay20230312153023&PaymentType=aio&ReturnURL=https://www.ecpay.com.tw/receive.php&TotalAmount=30000&TradeDesc=促銷方案");

            //Debug 檢查用
            //string debug = string.Format("HashKey={0}&{1}&HashIV={2}", _configuration["ECPaySettings:HashKey"], orderParamsString, _configuration["ECPaySettings:HashIV"]);
            //orderParams.OrderParamsString = debug;


            return orderParams;
        }

        //參考綠界文件說明計算檢查碼
        //https://developers.ecpay.com.tw/?p=2902
        private string BuildCheckMacValue(string orderParamsString)
        {
            string result = "";
            SHA256 encoder = SHA256.Create();

            result = string.Format("HashKey={0}&{1}&HashIV={2}", _configuration["ECPaySettings:HashKey"], orderParamsString, _configuration["ECPaySettings:HashIV"]);
            result = HttpUtility.UrlEncode(result).ToLower();

            byte[] data = encoder.ComputeHash(Encoding.UTF8.GetBytes(result));
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                stringBuilder.Append(data[i].ToString("X2"));
            }

            result = stringBuilder.ToString();

            return result;
        }
    }
}
