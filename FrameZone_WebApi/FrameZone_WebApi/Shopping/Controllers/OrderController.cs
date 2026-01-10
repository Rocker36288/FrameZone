using FrameZone_WebApi.Shopping.DTOs;
using FrameZone_WebApi.Shopping.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FrameZone_WebApi.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using System.Text;
using System.Globalization;


namespace FrameZone_WebApi.Shopping.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ECPayService _ecpayService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OrderController> _logger;
        private readonly AAContext _context;

        public OrderController(ECPayService ecpayService, ILogger<OrderController> logger, IConfiguration configuration, AAContext context)
        {
            _ecpayService = ecpayService;
            _configuration = configuration;
            _logger = logger;
            _context = context;
        }

        //測試用可以連到後端OrderController
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok("Test!");
        }

        //前端準備結帳資訊丟給後端
        [Authorize]
        [HttpPost("create")]
        public async Task<ActionResult<ECPayOrderParamsDto>> CreateOrder([FromBody] OrderDto order)
        {
            long? userId = GetUserIdFromToken();
            _logger.LogInformation("Creating order for UserId: {UserId}", userId);
            if (userId == null)
            {
                _logger.LogWarning("CreateOrder failed: Unauthorized. User not found in token.");
                return Unauthorized();
            }

            // 1. 建立訂單主表
            var newOrder = new Order
            {
                UserId = userId.Value,
                OrderStatus = order.PaymentMethod == "COD" ? "Pending Shipment" : "Pending Payment", // COD 直接待出貨，其餘待付款
                TotalAmount = order.TotalAmount,
                RecipientName = order.RecipientName ?? "",
                PhoneNumber = order.PhoneNumber ?? "",
                ShippingAddress = order.ShippingAddress ?? "",
                ShippingMethod = order.ShippingMethod ?? "",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Orders.Add(newOrder);
            await _context.SaveChangesAsync(); // 先存檔以產生 OrderId
            _logger.LogInformation("Order {OrderId} created successfully for UserId {UserId}.", newOrder.OrderId, userId);

            // 2. 建立訂單明細
            if (order.OrderItems != null)
            {
                foreach (var item in order.OrderItems)
                {
                    var detail = new OrderDetail
                    {
                        OrderId = newOrder.OrderId,
                        SpecificationId = item.Id, // 這裡對應前端傳來的 ID (應為規格 ID)
                        Quantity = item.Quantity,
                        TransactionPrice = item.Price, // 對應 TransactionPrice 欄位
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _context.OrderDetails.Add(detail);
                }
                await _context.SaveChangesAsync();
            }

            // 3. 建立初步付款紀錄
            string merchantTradeNo = "FZ" + newOrder.OrderId.ToString("D8") + DateTime.Now.ToString("HHmmss");

            // 4. 如果是貨到付款，不需要跳轉綠界，直接結束並回傳空參數 (前端會根據此判斷直接導向成功頁)
            if (order.PaymentMethod == "COD")
            {
                // 貨到付款直接記錄為成功 (待出貨)
                await RecordPaymentTransactionAsync(newOrder, order.PaymentMethod, merchantTradeNo, "Success", "待出貨");
                
                newOrder.OrderStatus = "Pending Shipment";
                newOrder.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                
                // 更新付款紀錄為成功 (COD 下單即視為交易紀錄成立)
                // await RecordPaymentTransactionAsync(newOrder, order.PaymentMethod, "FZ" + newOrder.OrderId.ToString("D8"), "Success", "COD Order Confirmed");
                
                return Ok(new ECPayOrderParamsDto());
            }
            else
            {
                // 信用卡/ATM 記錄為處理中 (待付款)
                await RecordPaymentTransactionAsync(newOrder, order.PaymentMethod, merchantTradeNo, "Pending", "待付款");
            }

            // 5. 準備綠界要的資訊
            // string merchantTradeNo = "FZ" + newOrder.OrderId.ToString("D8"); 

            ECPayOrderParamsDto ecpay = _ecpayService.CreateECPayOrder(order, merchantTradeNo);

            // 將 ECPay 參數包成 HTML form
            string htmlForm = BuildEcpayForm(ecpay);

            return Ok(new
            {
                orderId = newOrder.OrderId,
                ecPayForm = htmlForm
            });
        }

        private static string BuildEcpayForm(ECPayOrderParamsDto ecpay)
        {
            // 假設 DTO 內有：ServiceURL、HttpMethod、Params(Dictionary<string,string>)
            var sb = new StringBuilder();

            var method = string.IsNullOrWhiteSpace(ecpay.HttpMethod) ? "POST" : ecpay.HttpMethod;

            sb.Append($"<form id=\"ecpayForm\" method=\"{WebUtility.HtmlEncode(method)}\" action=\"{WebUtility.HtmlEncode(ecpay.ServiceURL)}\">");

            if (ecpay.Params != null)
            {
                foreach (var kv in ecpay.Params)
                {
                    var valueStr = Convert.ToString(kv.Value, CultureInfo.InvariantCulture) ?? string.Empty;

                    sb.Append(
                        $"<input type=\"hidden\" name=\"{WebUtility.HtmlEncode(kv.Key)}\" value=\"{WebUtility.HtmlEncode(valueStr)}\" />"
                    );
                }

            }

            sb.Append("</form>");
            return sb.ToString();
        }


        // 最後付款完成通知 (綠界 Server to Server)
        // 此方法不受網址重定向影響，是綠界伺服器直接發送給本系統的確認訊息，最為可靠。
        [HttpPost("pay-result")]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> OrderPayResult(IFormCollection result)
        {
            //_ecpayService.HandleECPayOrderResult(result);
            _logger.LogInformation("ECPay PayResult Received: {Result}", string.Join("&", result.Select(x => $"{x.Key}={x.Value}")));

            string merchantTradeNo = result["MerchantTradeNo"];
            string rtnCode = result["RtnCode"]; // 1 為成功

            if (rtnCode == "1")
            {
                // 解析 OrderId (從第 3 位開始取 8 位)
                // 註記：透過解析交易編號，我們可以確保更新的是正確的本地訂單。
                if (merchantTradeNo.StartsWith("FZ") && merchantTradeNo.Length >= 10)
                {
                    string orderIdStr = merchantTradeNo.Substring(2, 8);
                    if (int.TryParse(orderIdStr, out int orderId))
                    {
                        var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId); // 先查詢訂單
                        if (order != null)
                        {
                            order.OrderStatus = "Pending Shipment"; // 已付款，改為 Pending Shipment
                            order.UpdatedAt = DateTime.Now;

                            // 先查詢必要的 TypeId，避免 FK 錯誤
                            // var transactionType = await _context.TransactionTypes.FirstOrDefaultAsync(t => t.TypeCode == "Payment");
                            // if (transactionType == null)
                            // {
                            //     transactionType = new TransactionType
                            //     {
                            //         TypeCode = "Payment",
                            //         TypeName = "Payment",
                            //         Description = "Payment Transaction",
                            //         IsActive = true,
                            //         CreatedAt = DateTime.Now,
                            //         UpdatedAt = DateTime.Now
                            //     };
                            //     _context.TransactionTypes.Add(transactionType);
                            //     await _context.SaveChangesAsync(); // 保存以獲取 ID
                            // }

                            // var statusType = await _context.TransactionStatusTypes.FirstOrDefaultAsync(s => s.StatusCode == "Success");
                            // if (statusType == null)
                            // {
                            //     statusType = new TransactionStatusType
                            //     {
                            //         StatusCode = "Success",
                            //         StatusName = "Payment Success",
                            //         Description = "Payment Successful",
                            //         IsFinal = false,
                            //         IsSuccess = true,
                            //         CreatedAt = DateTime.Now,
                            //         UpdatedAt = DateTime.Now
                            //     };
                            //     _context.TransactionStatusTypes.Add(statusType);
                            //     await _context.SaveChangesAsync(); // 保存以獲取 ID
                            // }

                            // //  Auto-create PaymentMethodType and PaymentMethod if missing(自動建立)
                            // var paymentMethodType = await _context.PaymentMethodTypes.FirstOrDefaultAsync(t => t.TypeCode == "CreditCard");
                            // if (paymentMethodType == null)
                            // {
                            //     paymentMethodType = new PaymentMethodType
                            //     {
                            //         TypeCode = "CreditCard",
                            //         TypeName = "信用卡",
                            //         Description = "Credit Card Payment",
                            //         IsActive = true,
                            //         CreatedAt = DateTime.Now,
                            //         UpdatedAt = DateTime.Now
                            //     };
                            //     _context.PaymentMethodTypes.Add(paymentMethodType);
                            //     await _context.SaveChangesAsync();
                            // }

                            // var paymentMethod = await _context.PaymentMethods.FirstOrDefaultAsync(m => m.User.UserId == order.UserId && m.PaymentMethodType.TypeCode == "CreditCard" && m.CardLast4 == "ECPay");
                            // // 這裡的 PaymentMethod 通常是指使用者儲存的付款方式，或是系統全域的付款渠道
                            // // 若此表是用來記錄「本次交易使用的付款方式」，則 logic 如下；若是指「系統支援的付款方式」，則不應綁定 UserId
                            // // 根據 Model 定義有 UserId，推測是「使用者儲存的卡片/方式」。
                            // // 為避免錯誤，我們這裡建立一個綁定該 User 的「ECPay 預設」紀錄，或查找既有的。

                            // // 搜尋該用戶是否有綠界金流紀錄，若無則新增
                            // if (paymentMethod == null)
                            // {
                            //     paymentMethod = new PaymentMethod
                            //     {
                            //         UserId = order.UserId,
                            //         PaymentMethodTypeId = paymentMethodType.PaymentMethodTypeId,
                            //         IsDefault = false,
                            //         IsActive = true,
                            //         CardLast4 = "1234",
                            //         CardholderName = order.RecipientName ?? "ECPay User",
                            //         CreatedAt = DateTime.Now,
                            //         UpdatedAt = DateTime.Now
                            //     };
                            //     _context.PaymentMethods.Add(paymentMethod);
                            //     await _context.SaveChangesAsync();
                            // }

                            // // 1. 寫入 PaymentTransaction
                            // var transaction = new PaymentTransaction
                            // {
                            //     UserId = order.UserId,
                            //     PaymentMethodId = paymentMethod.PaymentMethodId,
                            //     TransactionTypeId = transactionType.TransactionTypeId,
                            //     TransactionNo = merchantTradeNo, // 使用商店交易編號
                            //     Amount = order.TotalAmount,
                            //     Currency = "TWD",
                            //     Description = "ECPay Payment",
                            //     MerchantTradeNo = merchantTradeNo,
                            //     GatewayTransactionId = result["TradeNo"], // 綠界的交易編號
                            //     GatewayName = "ECPay",
                            //     CreatedAt = DateTime.Now
                            // };

                            // // 2. 寫入 TransactionStatusLog
                            // var statusLog = new TransactionStatusLog
                            // {
                            //     Transaction = transaction,
                            //     StatusTypeId = statusType.StatusTypeId,
                            //     StatusMessage = "Payment Success",
                            //     GatewayResponse = string.Join("&", result.Select(x => $"{x.Key}={x.Value}")),
                            //     CreatedAt = DateTime.Now
                            // };

                            // _context.PaymentTransactions.Add(transaction);
                            // _context.TransactionStatusLogs.Add(statusLog);

                            await _context.SaveChangesAsync();

                            // 串接所有回傳參數作為 GatewayResponse
                            string fullResponse = string.Join("&", result.Select(x => $"{x.Key}={x.Value}"));

                            // 使用集中管理的方法紀錄付款
                            await RecordPaymentTransactionAsync(order, "Credit", merchantTradeNo, "Success", "付款成功 (綠界非同步通知)", result["TradeNo"], fullResponse);
                            
                            _logger.LogInformation("Order {OrderId} status updated via callback.", orderId);
                        }
                    }
                }
            }

            return Content("1|OK");  // 告訴綠界有收到通知，若沒回傳 1|OK 綠界會持續重發
        }

        // 判斷付款成功或失敗要重導到哪個頁面 (綠界 Client to Server)
        // 此方法原本只做畫面轉導；本機開發時若 pay-result 打不到，會在此做一次狀態更新的 fallback。正式環境仍以 pay-result 為準。
        //[HttpPost("success-redirect")]
        //[Consumes("application/x-www-form-urlencoded")]
        //public async Task<IActionResult> OrderSuccessRedirect(IFormCollection result)
        //{
        //    // Update default URL to Devtunnels to support cross-domain auth persistence
        //    string frontendUrl = _configuration["AppSettings:FrontendUrl"] ?? "https://0k2p097z-4200.asse.devtunnels.ms";

        //    if (result["RtnCode"] == "1")
        //    {
        //        _logger.LogInformation("ECPay Order Success Redirect! Redirecting to: {RedirectUrl}", frontendUrl + "/order-success");
        //        return Redirect(frontendUrl + "/order-success");
        //    }
        //    else
        //    {
        //        _logger.LogWarning("ECPay Order Fail Redirect!");
        //        return Redirect(frontendUrl + "/shopping/home");
        //    }
        //}


        //中轉頁面，用這個方式不需要上方前端連接埠的寫法
        // 判斷付款成功或失敗要重導到哪個頁面 (綠界 Client to Server)
        [HttpGet("success-redirect")]
        [HttpPost("success-redirect")]
        public async Task<IActionResult> OrderSuccessRedirect()
        {
            string merchantTradeNo = "";
            if (Request.HasFormContentType)
                merchantTradeNo = Request.Form["MerchantTradeNo"].ToString();
            if (string.IsNullOrWhiteSpace(merchantTradeNo))
                merchantTradeNo = Request.Query["MerchantTradeNo"].ToString();

            string rtnCode = "";
            if (Request.HasFormContentType)
                rtnCode = Request.Form["RtnCode"].ToString();
            if (string.IsNullOrWhiteSpace(rtnCode))
                rtnCode = Request.Query["RtnCode"].ToString();

            // B 方案：只要有 MerchantTradeNo 就當作 ATM 取號成功
            if (string.IsNullOrWhiteSpace(rtnCode) && !string.IsNullOrWhiteSpace(merchantTradeNo))
                rtnCode = "2";

            if (rtnCode == "1" || rtnCode == "2")
            {
                _logger.LogInformation("ECPay success-redirect received. RtnCode={RtnCode}, MerchantTradeNo={MerchantTradeNo}",
                    rtnCode, merchantTradeNo);

                // 只有 RtnCode == "1" (已付款) 才執行 fallback 更新狀態
                // RtnCode == "2" (ATM 取號完成) 不需要更新狀態，保持 Pending Payment
                if (rtnCode == "1")
                {
                    // 嘗試從 Form 或 Query 取得 TradeNo
                    string tradeNo = "";
                    if (Request.HasFormContentType) tradeNo = Request.Form["TradeNo"].ToString();
                    if (string.IsNullOrWhiteSpace(tradeNo)) tradeNo = Request.Query["TradeNo"].ToString();

                    await TryMarkOrderPaidFallbackAsync(merchantTradeNo, rtnCode, tradeNo);
                }

                var redirectUrl = $"http://localhost:4200/order-success?tradeNo={merchantTradeNo}";
                return Content($"<script>window.location.href='{redirectUrl}';</script>", "text/html");
            }

            _logger.LogWarning("ECPay redirect fail/cancel. RtnCode={RtnCode}", rtnCode);
            return Content("<script>window.location.href='http://localhost:4200/shopping/home';</script>", "text/html");
        }

        // 取得該會員儲存的取貨門市資料
        [Authorize]
        [HttpGet("pickup-stores")]
        public async Task<ActionResult<IEnumerable<PickupStoreDto>>> GetPickupStores()
        {
            long? userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            var stores = await _context.PickupConvenienceStores
                .Where(s => s.UserId == userId.Value)
                .OrderByDescending(s => s.IsDefault)
                .Select(s => new PickupStoreDto
                {
                    ConvenienceStoreId = s.ConvenienceStoreId,
                    ConvenienceStoreName = s.ConvenienceStoreName,
                    ConvenienceStoreCode = s.ConvenienceStoreCode,
                    RecipientName = s.RecipientName,
                    PhoneNumber = s.PhoneNumber,
                    IsDefault = s.IsDefault,
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();

            return Ok(stores);
        }

        // 取得當前登入會員的完整訂單歷史紀錄
        [Authorize]
        [HttpGet("my-orders")]
        public async Task<ActionResult> GetMyOrders()
        {
            long? userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            var orders = await _context.Orders
                .Where(o => o.UserId == userId.Value)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Specification)
                        .ThenInclude(s => s.Product)
                            .ThenInclude(p => p.ProductImages)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Specification)
                        .ThenInclude(s => s.Product)
                            .ThenInclude(p => p.User)
                                .ThenInclude(u => u.UserProfile)
                                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Reviews)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var result = orders.Select(o => {
            var firstProduct = o.OrderDetails.FirstOrDefault();
            var sellerName = firstProduct?.Specification?.Product?.User?.UserProfile?.DisplayName ?? "FrameZone 精選賣場";

            return new
            {
                id = "ORD" + o.OrderId.ToString("D6"),
                orderId = o.OrderId,
                shopName = sellerName,
                status = o.OrderStatus == "Pending Payment" ? "pay" :
                         o.OrderStatus == "Pending Shipment" ? "ship" : "done",
                statusText = o.OrderStatus == "Pending Payment" ? "待付款" :
                             o.OrderStatus == "Pending Shipment" ? "待出貨" : "已完成",
                totalAmount = o.TotalAmount,
                createdAt = o.CreatedAt,
                products = o.OrderDetails.Select(d => {
                    var review = d.Reviews.FirstOrDefault();
                    var img = d.Specification?.Product?.ProductImages?.FirstOrDefault(i => i.IsMainImage)?.ImageUrl
                              ?? d.Specification?.Product?.ProductImages?.FirstOrDefault()?.ImageUrl;
                    
                    return new
                    {
                        name = d.Specification?.Product?.ProductName ?? "未知商品",
                        spec = "預設規格", // 暫時固定，因資料庫結構較深
                        price = d.TransactionPrice,
                        quantity = d.Quantity,
                        productId = d.Specification?.ProductId ?? 0,
                        imageUrl = !string.IsNullOrEmpty(img) ? baseUrl + img : "https://placehold.co/80x80?text=No+Image",
                        isReviewed = review != null,
                        rating = review?.Rating ?? 0
                    };
                })
            };
        });
            return Ok(result);
        }

        // 非即時付款（ATM）在「取號成功」時仍是待付款；入帳後若收不到 ReturnURL（例如本機開發），
        // 可由前端主動呼叫此端點，後端使用綠界「訂單查詢(QueryTradeInfo)」確認是否已付款並更新狀態。
        [Authorize]
        [HttpPost("refresh-payment/{orderId:int}")]
        public async Task<IActionResult> RefreshPaymentStatus(int orderId)
        {
            long? userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId.Value);
            if (order == null) return NotFound(new { message = "Order not found" });

            // 只有待付款才需要查
            if (order.OrderStatus != "Pending Payment")
            {
                return Ok(new { updated = false, status = order.OrderStatus });
            }

            // MerchantTradeNo 規則：FZ + 8位訂單ID（需與 CreateOrder 一致）
            string merchantTradeNo = "FZ" + orderId.ToString("D8");

            var tradeInfo = await _ecpayService.QueryTradeInfoAsync(merchantTradeNo);

            if (!tradeInfo.IsSuccess)
            {
                _logger.LogWarning("QueryTradeInfo failed. MerchantTradeNo={MerchantTradeNo}, Message={Message}", merchantTradeNo, tradeInfo.ErrorMessage);
                return StatusCode(502, new { updated = false, status = order.OrderStatus, message = tradeInfo.ErrorMessage });
            }

            // TradeStatus: 0=未付款, 1=已付款
            if (tradeInfo.TradeStatus == "1")
            {
                order.OrderStatus = "Pending Shipment";
                order.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Order {OrderId} updated to Pending Shipment via QueryTradeInfo.", orderId);

                return Ok(new
                {
                    updated = true,
                    status = order.OrderStatus,
                    tradeStatus = tradeInfo.TradeStatus,
                    paymentDate = tradeInfo.PaymentDate,
                    tradeNo = tradeInfo.TradeNo
                });
            }

            return Ok(new
            {
                updated = false,
                status = order.OrderStatus,
                tradeStatus = tradeInfo.TradeStatus,
                paymentDate = tradeInfo.PaymentDate,
                tradeNo = tradeInfo.TradeNo
            });
        }


        #region Helpers

        private async Task<bool> TryMarkOrderPaidFallbackAsync(IFormCollection form)
        {
            try
            {
                var merchantTradeNo = form["MerchantTradeNo"].ToString();
                var rtnCode = form["RtnCode"].ToString();

                // 信用卡：1 = 付款成功
                // ATM：2 = 取號成功（僅導回，不更新狀態）
                if (rtnCode != "1") return false;

                if (!(merchantTradeNo.StartsWith("FZ") && merchantTradeNo.Length >= 10))
                    return false;

                var orderIdStr = merchantTradeNo.Substring(2, 8);
                if (!int.TryParse(orderIdStr, out int orderId))
                    return false;

                var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
                if (order == null) return false;

                if (order.OrderStatus != "Pending Shipment")
                {
                    order.OrderStatus = "Pending Shipment";
                    order.UpdatedAt = DateTime.Now;
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Fallback: Order {OrderId} marked as Pending Shipment. RtnCode={RtnCode}, MerchantTradeNo={MerchantTradeNo}",
                        orderId, rtnCode, merchantTradeNo);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallback: Failed to mark order via success-redirect.");
                return false;
            }
        }

        private async Task<bool> TryMarkOrderPaidFallbackAsync(string merchantTradeNo, string rtnCode, string tradeNo = null)
        {
            if (rtnCode != "1") return false;
            if (!(merchantTradeNo.StartsWith("FZ") && merchantTradeNo.Length >= 10)) return false;

            var orderIdStr = merchantTradeNo.Substring(2, 8);
            if (!int.TryParse(orderIdStr, out int orderId)) return false;

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null) return false;

            if (order.OrderStatus != "Pending Shipment")
            {
                order.OrderStatus = "Pending Shipment";
                order.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                // 取得目前 Request 中的參數作為備份
                string backupResp = Request.HasFormContentType 
                    ? string.Join("&", Request.Form.Select(x => $"{x.Key}={x.Value}"))
                    : Request.QueryString.ToString();

                // 補寫入付款紀錄
                await RecordPaymentTransactionAsync(order, "Credit", merchantTradeNo, "Success", "付款成功 (導回確認)", tradeNo, backupResp);
            }

            return true;
        }

        private async Task RecordPaymentTransactionAsync(Order order, string paymentMethodCode, string merchantTradeNo, string statusCode, string message, string gatewayId = null, string gatewayResponse = null)
        {
            try
            {
                // 1. 確保基礎類型資料存在
                var transactionType = await _context.TransactionTypes.FirstOrDefaultAsync(t => t.TypeCode == "Payment")
                    ?? new TransactionType { TypeCode = "Payment", TypeName = "付款", Description = "Order Payment", IsActive = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now };
                if (transactionType.TransactionTypeId == 0) { _context.TransactionTypes.Add(transactionType); await _context.SaveChangesAsync(); }

                var statusType = await _context.TransactionStatusTypes.FirstOrDefaultAsync(s => s.StatusCode == statusCode);
                if (statusType == null)
                {
                    statusType = new TransactionStatusType 
                    { 
                        StatusCode = statusCode, 
                        StatusName = statusCode == "Success" ? "成功" : (statusCode == "Pending" ? "處理中" : statusCode), 
                        Description = statusCode, 
                        IsFinal = (statusCode == "Success"), 
                        IsSuccess = (statusCode == "Success"), 
                        CreatedAt = DateTime.Now, 
                        UpdatedAt = DateTime.Now 
                    };
                    _context.TransactionStatusTypes.Add(statusType);
                    await _context.SaveChangesAsync();
                }

                // 2. 處理付款方式類型
                string dbMethodTypeCode = paymentMethodCode switch
                {
                    "Credit" => "CreditCard",
                    "ATM" => "ATM",
                    "COD" => "COD",
                    _ => paymentMethodCode
                };

                var paymentMethodType = await _context.PaymentMethodTypes.FirstOrDefaultAsync(t => t.TypeCode == dbMethodTypeCode);
                if (paymentMethodType == null)
                {
                    paymentMethodType = new PaymentMethodType 
                    { 
                        TypeCode = dbMethodTypeCode, 
                        TypeName = dbMethodTypeCode switch {
                            "CreditCard" => "信用卡",
                            "ATM" => "ATM轉帳",
                            "COD" => "貨到付款",
                            _ => dbMethodTypeCode
                        },
                        Description = dbMethodTypeCode + " Payment", 
                        IsActive = true, 
                        CreatedAt = DateTime.Now, 
                        UpdatedAt = DateTime.Now 
                    };
                    _context.PaymentMethodTypes.Add(paymentMethodType);
                    await _context.SaveChangesAsync();
                }

                // 3. 處理 PaymentMethod (改為 Find-or-Create 避免違反資料庫唯一約束)
                // 註記：搜尋條件必須與下方建立時填入的欄位一致，否則會找不到舊紀錄。
                var paymentMethod = await _context.PaymentMethods.FirstOrDefaultAsync(m => m.UserId == order.UserId && m.PaymentMethodTypeId == paymentMethodType.PaymentMethodTypeId && m.CardLast4 == "0000");
                if (paymentMethod == null)
                {
                    paymentMethod = new PaymentMethod
                    {
                        UserId = order.UserId,
                        PaymentMethodTypeId = paymentMethodType.PaymentMethodTypeId,
                        IsDefault = false,
                        IsActive = true,
                        CardLast4 = "0000",
                        CardholderName = order.RecipientName ?? "User",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _context.PaymentMethods.Add(paymentMethod);
                    await _context.SaveChangesAsync();
                }

                // 4. 建立或尋找交易紀錄
                var transaction = await _context.PaymentTransactions.FirstOrDefaultAsync(t => t.MerchantTradeNo == merchantTradeNo);
                if (transaction == null)
                {
                    transaction = new PaymentTransaction
                    {
                        UserId = order.UserId,
                        PaymentMethodId = paymentMethod.PaymentMethodId,
                        TransactionTypeId = transactionType.TransactionTypeId,
                        TransactionNo = merchantTradeNo,
                        Amount = order.TotalAmount,
                        Currency = "TWD",
                        Description = $"{dbMethodTypeCode} Order Payment", // 依支付方式分開描述
                        MerchantTradeNo = merchantTradeNo,
                        GatewayTransactionId = gatewayId,
                        GatewayName = "ECPay",
                        CreatedAt = DateTime.Now
                    };
                    _context.PaymentTransactions.Add(transaction);
                }
                else
                {
                    transaction.GatewayTransactionId = gatewayId ?? transaction.GatewayTransactionId;
                    // 如果交易已存在但 PaymentMethod 沒對上，更新它（雖然通常交易是唯一的）
                    transaction.PaymentMethodId = paymentMethod.PaymentMethodId;
                }

                // 5. 寫入狀態 Log
                var statusLog = new TransactionStatusLog
                {
                    Transaction = transaction,
                    StatusTypeId = statusType.StatusTypeId,
                    StatusMessage = message, // 這裡可以維持中文供識別
                    GatewayResponse = gatewayResponse ?? (gatewayId != null ? $"TradeNo:{gatewayId}" : ""),
                    CreatedAt = DateTime.Now
                };
                _context.TransactionStatusLogs.Add(statusLog);

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to record payment transaction for Order {OrderId}", order.OrderId);
            }
        }


        //識別當前是哪位會員在結帳
        //當會員登入後，瀏覽器發送請求時會帶著一個加密的憑證（Token）。GetUserIdFromToken()方法的作用就是從這個憑證中解析出該會員在網站資料庫裡的 唯一識別碼（UserId）
        private long? GetUserIdFromToken()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return null;
                }

                if (long.TryParse(userIdClaim, out var userId))
                {
                    return userId;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}



//綠界流程
//購物車頁面→結帳頁面→點擊確認結帳後在前端準備的這些結帳資訊(購買商品/數量/價格/付款方式/訂單成立後要回來的Url等等)會丟給後端
//接著後端用這些資訊產生要發給綠界的資訊(綠界有自己的訂單格式)，再回給前端
//前端收到這些資訊後記錄在隱藏表單再發送給綠界
//接著導向綠界付款畫面(信用卡或ATM)
//如果是信用卡，填完資料之後完成付款會發送兩個通知到後端再導回到前端訂單完成頁面
//(一個是付款完成通知，
//  一個是訂單成立要回訂單完成頁面通知，因為前端無法處理，所以發通知到後端讓後端直接導回前端訂單完成頁面)
//如果是ATM，選擇付款帳戶收到虛擬帳號資料後點擊返回首頁就會直接導回到前端訂單完成頁面
//因為ATM是收到虛擬帳號資料還不算付款完成，等到虛擬帳號確實收到款項完成付款就會發通知到後端
