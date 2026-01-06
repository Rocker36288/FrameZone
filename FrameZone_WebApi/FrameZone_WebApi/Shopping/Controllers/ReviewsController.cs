using FrameZone_WebApi.Shopping.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FrameZone_WebApi.Shopping.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        // 取得賣家收到的評價
        [HttpGet("seller/{userId}")]
        public IActionResult GetSellerReviews(long userId, [FromQuery] int take = 20, [FromQuery] int skip = 0)
        {
            var reviews = _reviewService.GetSellerReviews(userId, take, skip);
            return Ok(reviews);
        }

        // 取得使用者發出的評價
        [HttpGet("user/{userId}")]
        public IActionResult GetUserSentReviews(long userId, [FromQuery] int take = 20, [FromQuery] int skip = 0)
        {
            var reviews = _reviewService.GetUserSentReviews(userId, take, skip);
            return Ok(reviews);
        }

        // 取得當前登入使用者的評價 (收到的)
        [HttpGet("my/received")]
        public IActionResult GetMyReceivedReviews([FromQuery] int take = 20, [FromQuery] int skip = 0)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(userIdStr, out long userId))
            {
                var reviews = _reviewService.GetSellerReviews(userId, take, skip);
                return Ok(reviews);
            }
            return Unauthorized();
        }

        // 取得當前登入使用者的評價 (發出的)
        [HttpGet("my/sent")]
        public IActionResult GetMySentReviews([FromQuery] int take = 20, [FromQuery] int skip = 0)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(userIdStr, out long userId))
            {
                var reviews = _reviewService.GetUserSentReviews(userId, take, skip);
                return Ok(reviews);
            }
            return Unauthorized();
        }

        // 建立評價 (支援批次與圖片上傳)
        [HttpPost("batch")]
        public async Task<IActionResult> CreateReviews([FromForm] List<DTOs.CreateReviewDto> reviews)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdStr, out long userId))
            {
                return Unauthorized();
            }

            try
            {
                await _reviewService.CreateReviewsAsync(userId, reviews);
                return Ok(new { success = true, message = "評價已成功建立" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
