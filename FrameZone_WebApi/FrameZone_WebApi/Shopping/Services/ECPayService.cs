using FrameZone_WebApi.Controllers;
using FrameZone_WebApi.Shopping.DTOs;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace FrameZone_WebApi.Shopping.Services
{
    public class ECPayService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ECPayService> _logger;

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

        public ECPayService(IConfiguration configuration, ILogger<ECPayService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        //準備綠界要的參數
        public ECPayOrderParamsDto CreateECPayOrder(OrderDto order)
        {
            ECPayOrderParamsDto orderParams = new ECPayOrderParamsDto();
            string serialNum = DateTime.Now.Ticks.ToString().Substring(0, 10);

            orderParams.Params["MerchantID"] = _configuration["ECPaySettings:MerchantID"];
            orderParams.Params["MerchantTradeNo"] = "TestOrder" + serialNum;
            orderParams.Params["MerchantTradeDate"] = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            orderParams.Params["TotalAmount"] = order.TotalAmount;
            orderParams.Params["TradeDesc"] = "TestDesc" + serialNum;
            orderParams.Params["ChoosePayment"] = order.PaymentMethod;
            orderParams.Params["ReturnURL"] = order.ReturnURL;

            //寫在綠界付款畫面的商品明細
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
                        //第二樣以上商品用#換行
                        stringBuilder.Append("#" + item.Name?.ToString() + " * " + item.Quantity.ToString());
                    }
                }

                orderParams.Params["ItemName"] = stringBuilder.ToString();
            }

            //其他可選參數帶入前端給的資訊
            if (order.OptionParams != null)
            {
                foreach (KeyValuePair<string, object> option in order.OptionParams)
                {
                    orderParams.Params.Add(option.Key, option.Value);
                }
            }

            return CheckECPayOrderParams(orderParams);
        }

        public void HandleECPayOrderResult(FormCollection result)
        {
            //_logger.LogInformation(result.ToString()); //付款完成結果紀錄在log，還未寫進資料庫
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
