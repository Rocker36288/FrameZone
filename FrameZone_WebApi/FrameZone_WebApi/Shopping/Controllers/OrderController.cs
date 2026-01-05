using FrameZone_WebApi.Shopping.DTOs;
using FrameZone_WebApi.Shopping.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;

namespace FrameZone_WebApi.Shopping.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ECPayService _ecpayService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OrderController> _logger;

        public OrderController(ECPayService ecpayService, ILogger<OrderController> logger, IConfiguration configuration)
        {
            _ecpayService = ecpayService;
            _configuration = configuration;
            _logger = logger;
        }

        //測試用可以連到後端OrderController
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok("Test!");
        }

        //前端準備結帳資訊丟給後端
        [HttpPost("create")]
        public async Task<ActionResult<ECPayOrderParamsDto>> CreateOrder([FromBody] OrderDto order)
        {
            ECPayOrderParamsDto result = _ecpayService.CreateECPayOrder(order); //準備綠界要的資訊

            return Ok(result);
        }

        //最後付款完成通知
        [HttpPost("pay-result")]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> OrderPayResult(IFormCollection result)
        {
            //_ecpayService.HandleECPayOrderResult(result);

            return Content("1|OK");  //告訴綠界有收通知
        }

        //判斷付款成功或失敗要重導到哪個頁面
        [HttpPost("success-redirect")]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> OrderSuccessRedirect(IFormCollection result)
        {
            string frontendUrl = _configuration["AppSettings:FrontendUrl"] ?? "https://localhost:4200";

            if (result["RtnCode"] == "1")
            {
                _logger.LogInformation("ECPay Order Success!");
                return Redirect(frontendUrl + "/order-success");
            }
            else
            {
                _logger.LogWarning("ECPay Order Fail!");
                return Redirect(frontendUrl + "/shopping/home");
            }
        }
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

