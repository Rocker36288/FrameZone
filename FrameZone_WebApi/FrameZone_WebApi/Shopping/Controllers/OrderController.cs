using FrameZone_WebApi.Shopping.DTOs;
using FrameZone_WebApi.Shopping.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FrameZone_WebApi.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

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
                OrderStatus = "Pending Payment", // 初始狀態設為 Pending Payment
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

            // 3. 如果是貨到付款，不需要跳轉綠界，直接結束並回傳空參數 (前端會根據此判斷直接導向成功頁)
            if (order.PaymentMethod == "COD")
            {
                return Ok(new ECPayOrderParamsDto());
            }

            // 4. 準備綠界要的資訊
            // 交易編號規則: FZ + 8位訂單ID + 6位時間(分秒)
            // 註記：這裡使用固定格式 (FZ+ID) 是為了讓 'pay-result' 回傳時能精確反推資料庫中的 OrderId
            string merchantTradeNo = "FZ" + newOrder.OrderId.ToString("D8") + DateTime.Now.ToString("HHmmss");
            if (merchantTradeNo.Length > 20) merchantTradeNo = merchantTradeNo.Substring(0, 20);

            ECPayOrderParamsDto result = _ecpayService.CreateECPayOrder(order, merchantTradeNo);

            return Ok(result);
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
                            var transactionType = await _context.TransactionTypes.FirstOrDefaultAsync(t => t.TypeCode == "Payment");
                            if (transactionType == null)
                            {
                                transactionType = new TransactionType
                                {
                                    TypeCode = "Payment",
                                    TypeName = "Payment",
                                    Description = "Payment Transaction",
                                    IsActive = true,
                                    CreatedAt = DateTime.Now,
                                    UpdatedAt = DateTime.Now
                                };
                                _context.TransactionTypes.Add(transactionType);
                                await _context.SaveChangesAsync(); // 保存以獲取 ID
                            }

                            var statusType = await _context.TransactionStatusTypes.FirstOrDefaultAsync(s => s.StatusCode == "Success");
                            if (statusType == null)
                            {
                                statusType = new TransactionStatusType
                                {
                                    StatusCode = "Success",
                                    StatusName = "Payment Success",
                                    Description = "Payment Successful",
                                    IsFinal = false,
                                    IsSuccess = true,
                                    CreatedAt = DateTime.Now,
                                    UpdatedAt = DateTime.Now
                                };
                                _context.TransactionStatusTypes.Add(statusType);
                                await _context.SaveChangesAsync(); // 保存以獲取 ID
                            }
                            
                            //  Auto-create PaymentMethodType and PaymentMethod if missing(自動建立)
                            var paymentMethodType = await _context.PaymentMethodTypes.FirstOrDefaultAsync(t => t.TypeCode == "CreditCard");
                            if (paymentMethodType == null)
                            {
                                paymentMethodType = new PaymentMethodType
                                {
                                    TypeCode = "CreditCard",
                                    TypeName = "信用卡",
                                    Description = "Credit Card Payment",
                                    IsActive = true,
                                    CreatedAt = DateTime.Now,
                                    UpdatedAt = DateTime.Now
                                };
                                _context.PaymentMethodTypes.Add(paymentMethodType);
                                await _context.SaveChangesAsync();
                            }

                            var paymentMethod = await _context.PaymentMethods.FirstOrDefaultAsync(m => m.User.UserId == order.UserId && m.PaymentMethodType.TypeCode == "CreditCard" && m.CardLast4 == "ECPay");
                            // 這裡的 PaymentMethod 通常是指使用者儲存的付款方式，或是系統全域的付款渠道
                            // 若此表是用來記錄「本次交易使用的付款方式」，則 logic 如下；若是指「系統支援的付款方式」，則不應綁定 UserId
                            // 根據 Model 定義有 UserId，推測是「使用者儲存的卡片/方式」。
                            // 為避免錯誤，我們這裡建立一個綁定該 User 的「ECPay 預設」紀錄，或查找既有的。
                            
                            // 搜尋該用戶是否有綠界金流紀錄，若無則新增
                            if (paymentMethod == null)
                            {
                                paymentMethod = new PaymentMethod
                                {
                                    UserId = order.UserId,
                                    PaymentMethodTypeId = paymentMethodType.PaymentMethodTypeId,
                                    IsDefault = false,
                                    IsActive = true,
                                    CardLast4 = "1234",
                                    CardholderName = order.RecipientName ?? "ECPay User",
                                    CreatedAt = DateTime.Now,
                                    UpdatedAt = DateTime.Now
                                };
                                _context.PaymentMethods.Add(paymentMethod);
                                await _context.SaveChangesAsync();
                            }

                            // 1. 寫入 PaymentTransaction
                            var transaction = new PaymentTransaction
                            {
                                UserId = order.UserId,
                                PaymentMethodId = paymentMethod.PaymentMethodId, 
                                TransactionTypeId = transactionType.TransactionTypeId,
                                TransactionNo = merchantTradeNo, // 使用商店交易編號
                                Amount = order.TotalAmount,
                                Currency = "TWD",
                                Description = "ECPay Payment",
                                MerchantTradeNo = merchantTradeNo,
                                GatewayTransactionId = result["TradeNo"], // 綠界的交易編號
                                GatewayName = "ECPay",
                                CreatedAt = DateTime.Now
                            };
                            
                            // 2. 寫入 TransactionStatusLog
                            var statusLog = new TransactionStatusLog
                            {
                                Transaction = transaction,
                                StatusTypeId = statusType.StatusTypeId,
                                StatusMessage = "Payment Success",
                                GatewayResponse = string.Join("&", result.Select(x => $"{x.Key}={x.Value}")),
                                CreatedAt = DateTime.Now
                            };

                            _context.PaymentTransactions.Add(transaction);
                            _context.TransactionStatusLogs.Add(statusLog);

                            await _context.SaveChangesAsync();
                            _logger.LogInformation("Order {OrderId} status updated and Transaction recorded.", orderId);
                        }
                    }
                }
            }

            return Content("1|OK");  // 告訴綠界有收到通知，若沒回傳 1|OK 綠界會持續重發
        }

        // 判斷付款成功或失敗要重導到哪個頁面 (綠界 Client to Server)
        // 此方法僅負責「畫面轉導」，不應用於更新資料庫狀態，因為使用者可能會關閉瀏覽器導致此請求失敗。
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
        [HttpPost("success-redirect")]
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult OrderSuccessRedirect() // 移除 async，因為這裡只做導向
        {
            // 綠界 POST 回來的所有參數都在 Request.Form 裡面
            var merchantTradeNo = Request.Form["MerchantTradeNo"];
            var rtnCode = Request.Form["RtnCode"];

            string redirectUrl = "";

            // 判斷是否成功 (RtnCode == "1" 代表成功)
            if (rtnCode == "1")
            {
                _logger.LogInformation("綠界付款成功，準備中轉回 localhost");
                // 導向前端的成功頁面，並帶上交易編號供前端查詢
                redirectUrl = $"http://localhost:4200/order-success?tradeNo={merchantTradeNo}";
            }
            else
            {
                _logger.LogWarning("綠界付款失敗或使用者取消");
                // 導向前端的失敗頁面或購物首頁
                redirectUrl = "http://localhost:4200/shopping/home";
            }

            // 重點：利用瀏覽器認得 localhost 的特性進行中轉
            return Content($"<script>window.location.href='{redirectUrl}';</script>", "text/html");
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
                    Id = s.ConvenienceStoreId,
                    StoreName = s.ConvenienceStoreName,
                    StoreCode = s.ConvenienceStoreCode,
                    //Address = "", // 這裡根據資料庫欄位調整，若資料庫沒存地址則留空
                    RecipientName = s.RecipientName,
                    PhoneNumber = s.PhoneNumber
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
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var result = orders.Select(o => {
                var firstProduct = o.OrderDetails.FirstOrDefault();
                var sellerName = firstProduct?.Specification?.Product?.User?.UserProfile?.DisplayName ?? "FrameZone 精選賣場";
                
                return new {
                    id = "ORD" + o.OrderId.ToString("D6"),
                    orderId = o.OrderId,
                    shopName = sellerName,
                    status = o.OrderStatus == "Pending Payment" ? "pay" : 
                             o.OrderStatus == "Pending Shipment" ? "ship" : "done",
                    statusText = o.OrderStatus == "Pending Payment" ? "待付款" : 
                                 o.OrderStatus == "Pending Shipment" ? "待出貨" : "已完成",
                    totalAmount = o.TotalAmount,
                    createdAt = o.CreatedAt,
                    products = o.OrderDetails.Select(d => new {
                        name = d.Specification?.Product?.ProductName ?? "未知商品",
                        spec = "預設規格", // 暫時固定，因資料庫結構較深
                        price = d.TransactionPrice,
                        quantity = d.Quantity,
                        productId = d.Specification?.ProductId ?? 0,
                        imageUrl = d.Specification?.Product?.ProductImages?.FirstOrDefault(i => i.IsMainImage)?.ImageUrl 
                                   ?? d.Specification?.Product?.ProductImages?.FirstOrDefault()?.ImageUrl
                                   ?? "https://placehold.co/80x80?text=No+Image"
                    })
                };
            });

            return Ok(result);
        }

        #region Helpers
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

