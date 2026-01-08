using FrameZone_WebApi.Shopping.DTOs;
using FrameZone_WebApi.Shopping.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FrameZone_WebApi.Shopping.Controllers
{
    [ApiController]
    [Route("api/shopping/member")]
    [Authorize]
    public class ShoppingMemberController : ControllerBase
    {
        private readonly IShoppingMemberService _shoppingMemberService;

        public ShoppingMemberController(IShoppingMemberService shoppingMemberService)
        {
            _shoppingMemberService = shoppingMemberService;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized(new { success = false, message = "未授權" });
            }

            var result = await _shoppingMemberService.GetProfileAsync(userId.Value);
            return Ok(result);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] ShoppingMemberProfileDto dto)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized(new { success = false, message = "未授權" });
            }

            var result = await _shoppingMemberService.UpdateProfileAsync(userId.Value, dto);
            if (!result.Success) return BadRequest(result);

            return Ok(result);
        }

        private long? GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long userId))
            {
                return null;
            }
            return userId;
        }
    }
}
