using FrameZone_WebApi.Shopping.DTOs;
using FrameZone_WebApi.Shopping.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FrameZone_WebApi.Shopping.Controllers
{
    [ApiController]
    [Route("api/shopping/stores")]
    [Authorize]
    public class ShoppingStoreController : ControllerBase
    {
        private readonly IShoppingStoreService _storeService;
        private readonly ILogger<ShoppingStoreController> _logger;

        public ShoppingStoreController(IShoppingStoreService storeService, ILogger<ShoppingStoreController> logger)
        {
            _storeService = storeService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserStores()
        {
            var userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            var stores = await _storeService.GetUserStoresAsync(userId.Value);
            return Ok(new { success = true, data = stores });
        }

        [HttpPost]
        public async Task<IActionResult> CreateStore([FromBody] CreatePickupStoreDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            var result = await _storeService.CreateStoreAsync(userId.Value, dto);
            if (!result.Success) return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, data = result.Data, message = result.Message });
        }

        [HttpPut("{storeId}")]
        public async Task<IActionResult> UpdateStore(int storeId, [FromBody] CreatePickupStoreDto dto)
        {
            var userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            var result = await _storeService.UpdateStoreAsync(userId.Value, storeId, dto);
            if (!result.Success) return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        [HttpDelete("{storeId}")]
        public async Task<IActionResult> DeleteStore(int storeId)
        {
            var userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            var result = await _storeService.DeleteStoreAsync(userId.Value, storeId);
            if (!result.Success) return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        [HttpPut("{storeId}/default")]
        public async Task<IActionResult> SetDefault(int storeId)
        {
            var userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            var result = await _storeService.SetDefaultStoreAsync(userId.Value, storeId);
            if (!result.Success) return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        private long? GetUserIdFromToken()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null && long.TryParse(claim.Value, out long userId)) return userId;
            return null;
        }
    }
}
