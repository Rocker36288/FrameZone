using FrameZone_WebApi.Shopping.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FrameZone_WebApi.Shopping.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FavoritesController : ControllerBase
    {
        private readonly FavoriteService _service;

        public FavoritesController(FavoriteService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserFavorites()
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized(new { message = "無效的登入資訊" });
            }

            var favorites = await _service.GetUserFavoritesAsync(userId.Value);
            return Ok(favorites);
        }

        [HttpPost("{productId}")]
        public async Task<IActionResult> ToggleFavorite(long productId)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
            {
                return Unauthorized(new { message = "無效的登入資訊" });
            }

            await _service.ToggleFavoriteAsync(userId.Value, productId);
            return Ok(new { message = "操作成功" });
        }

        private long? GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            return null;
        }
    }
}
