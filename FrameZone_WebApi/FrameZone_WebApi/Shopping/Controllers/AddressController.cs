using FrameZone_WebApi.Shopping.DTOs;
using FrameZone_WebApi.Shopping.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FrameZone_WebApi.Shopping.Controllers
{
    /// <summary>
    /// 收件地址管理 API Controller
    /// </summary>
    [ApiController]
    [Route("api/shopping/addresses")]
    [Authorize] // 所有端點都需要登入
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;
        private readonly ILogger<AddressController> _logger;

        public AddressController(
            IAddressService addressService,
            ILogger<AddressController> logger)
        {
            _addressService = addressService;
            _logger = logger;
        }

        /// <summary>
        /// 取得當前使用者的所有收件地址
        /// </summary>
        /// <returns>收件地址列表</returns>
        /// <response code="200">成功取得收件地址列表</response>
        /// <response code="401">未登入</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<ReceivingAddressDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserAddresses()
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(new { message = "無效的登入資訊" });
                }

                var addresses = await _addressService.GetUserAddressesAsync(userId.Value);

                return Ok(new
                {
                    success = true,
                    data = addresses,
                    message = "成功取得收件地址列表"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得收件地址列表時發生錯誤");
                return StatusCode(500, new
                {
                    success = false,
                    message = "伺服器錯誤，請稍後再試"
                });
            }
        }

        /// <summary>
        /// 建立新的收件地址
        /// </summary>
        /// <param name="dto">建立地址 DTO</param>
        /// <returns>建立結果</returns>
        /// <response code="200">建立成功</response>
        /// <response code="400">資料驗證失敗</response>
        /// <response code="401">未登入</response>
        [HttpPost]
        [ProducesResponseType(typeof(ReceivingAddressDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateAddress([FromBody] CreateAddressDto dto)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(new { message = "無效的登入資訊" });
                }

                // ModelState 驗證
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new
                    {
                        success = false,
                        message = "資料驗證失敗",
                        errors = errors
                    });
                }

                // 呼叫 Service 建立地址
                var result = await _addressService.CreateAddressAsync(userId.Value, dto);

                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = result.Message
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = result.Data,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立收件地址時發生錯誤");
                return StatusCode(500, new
                {
                    success = false,
                    message = "伺服器錯誤，請稍後再試"
                });
            }
        }

        [HttpPut("{addressId}")]
        public async Task<IActionResult> UpdateAddress(int addressId, [FromBody] CreateAddressDto dto)
        {
            var userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            var result = await _addressService.UpdateAddressAsync(userId.Value, addressId, dto);
            if (!result.Success) return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        [HttpDelete("{addressId}")]
        public async Task<IActionResult> DeleteAddress(int addressId)
        {
            var userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            var result = await _addressService.DeleteAddressAsync(userId.Value, addressId);
            if (!result.Success) return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        [HttpPut("{addressId}/default")]
        public async Task<IActionResult> SetDefault(int addressId)
        {
            var userId = GetUserIdFromToken();
            if (userId == null) return Unauthorized();

            var result = await _addressService.SetDefaultAddressAsync(userId.Value, addressId);
            if (!result.Success) return BadRequest(new { success = false, message = result.Message });

            return Ok(new { success = true, message = result.Message });
        }

        /// <summary>
        /// 從 JWT Token 取得使用者 ID
        /// </summary>
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
