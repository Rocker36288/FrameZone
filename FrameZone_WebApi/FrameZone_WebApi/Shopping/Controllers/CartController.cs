using FrameZone_WebApi.Shopping.DTOs;
using FrameZone_WebApi.Shopping.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FrameZone_WebApi.Shopping.Controllers
{
    [Route("api/shopping/[controller]")]
    [ApiController]
    [Authorize] // 必須登入才能操作購物車資料庫
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        private long GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = GetCurrentUserId();
            var cart = await _cartService.GetUserCartAsync(userId);
            return Ok(cart);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] CartDto cartDto)
        {
            var userId = GetCurrentUserId();
            await _cartService.AddToCartAsync(userId, cartDto);
            return Ok(new { success = true, message = "已加入購物車" });
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateCartItem([FromBody] CartDto cartDto)
        {
            var userId = GetCurrentUserId();
            await _cartService.UpdateCartItemAsync(userId, cartDto);
            return Ok(new { success = true });
        }

        [HttpDelete("remove/{specificationId}")]
        public async Task<IActionResult> RemoveFromCart(int specificationId)
        {
            var userId = GetCurrentUserId();
            await _cartService.RemoveFromCartAsync(userId, specificationId);
            return Ok(new { success = true });
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetCurrentUserId();
            await _cartService.ClearCartAsync(userId);
            return Ok(new { success = true });
        }

        [HttpPost("sync")]
        public async Task<IActionResult> SyncCart([FromBody] List<CartDto> cartItems)
        {
            var userId = GetCurrentUserId();
            await _cartService.SyncCartAsync(userId, cartItems);
            return Ok(new { success = true });
        }
    }
}
