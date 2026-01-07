using FrameZone_WebApi.Shopping.DTOs;
using FrameZone_WebApi.Shopping.Services;
using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;

namespace FrameZone_WebApi.Shopping.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _service;

        public ProductsController(ProductService service)
        {
            _service = service;
        }

        // GET: api/products
        [HttpGet]
        public IActionResult Get()
        {
            var userId = GetUserIdFromToken();
            var result = _service.GetAvailableProducts(userId);
            return Ok(result);
        }

        // GET: api/products/5
        [HttpGet("{id}")]
        public IActionResult GetById(long id)
        {
            var userId = GetUserIdFromToken();
            var result = _service.GetProductDetail(id, userId);

            if (result == null)
            {
                return NotFound(new { message = "找不到此商品或商品未上架" });
            }

            return Ok(result);
        }

        // ProductsController.cs
        [HttpGet("{id}/similar")]
        public IActionResult GetSimilarProducts(long id)
        {
            var userId = GetUserIdFromToken();
            var similar = _service.GetSimilarProducts(id, userId);

            return Ok(similar);
        }

        // GET: api/products/recommended
        [HttpGet("recommended")]
        public IActionResult GetRecommendedProducts()
        {
            var userId = GetUserIdFromToken();
            var result = _service.GetRecommendedProducts(userId);
            return Ok(result);
        }

        [HttpGet("popular")]
        public IActionResult GetPopularProducts()
        {
            var userId = GetUserIdFromToken();
            var result = _service.GetPopularProducts(userId);
            return Ok(result);
        }

        [HttpPost("batch")]
        public IActionResult GetProductsByIds([FromBody] List<long> productIds)
        {
            var userId = GetUserIdFromToken();
            var result = _service.GetProductsByIds(productIds, userId);
            return Ok(result);
        }

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
    }
}
