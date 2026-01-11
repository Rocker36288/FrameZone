using FrameZone_WebApi.Shopping.DTOs;
using FrameZone_WebApi.Shopping.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FrameZone_WebApi.Shopping.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SellersController : ControllerBase
    {
        private readonly ProductService _productService;

        public SellersController(ProductService productService)
        {
            _productService = productService;
        }

        // GET: api/sellers/{userId}/profile
        [HttpGet("{userId}/profile")]
        public IActionResult GetSellerProfile(long userId)
        {
            var profile = _productService.GetSellerProfile(userId);
            if (profile == null)
            {
                return NotFound(new { message = "找不到該賣家的資料" });
            }
            return Ok(profile);
        }

        // GET: api/sellers/{userId}/products
        [HttpGet("{userId}/products")]
        public IActionResult GetSellerProducts(
            long userId, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 20,
            [FromQuery] int? categoryId = null,
            [FromQuery] string keyword = null)
        {
            long? observerUserId = GetUserIdFromToken();
            var result = _productService.GetProductsByUserIdPaged(userId, page, pageSize, categoryId, keyword, observerUserId);
            return Ok(result);
        }

        private long? GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(userIdClaim, out long userId))
            {
                return userId;
            }
            return null;
        }

        // GET: api/sellers/{userId}/categories
        [HttpGet("{userId}/categories")]
        public IActionResult GetSellerCategories(long userId)
        {
            var categories = _productService.GetSellerCategories(userId);
            return Ok(categories);
        }

        // GET: api/sellers/{userId}/reviews
        [HttpGet("{userId}/reviews")]
        public IActionResult GetSellerReviews(long userId)
        {
            var reviews = _productService.GetSellerReviews(userId);
            return Ok(reviews);
        }
    }
}
