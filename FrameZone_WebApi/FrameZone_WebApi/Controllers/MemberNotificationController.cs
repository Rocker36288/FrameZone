using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FrameZone_WebApi.DTOs.Member;
using FrameZone_WebApi.Services.Member;
using System.Security.Claims;

namespace FrameZone_WebApi.Controllers
{
    /// <summary>
    /// 會員通知偏好設定 API Controller
    /// 提供通知偏好設定的查詢與更新功能
    /// </summary>
    [ApiController]
    [Route("api/member/notifications")]
    [Authorize] // 所有端點都需要登入
    public class MemberNotificationController : ControllerBase
    {
        private readonly IMemberNotificationService _notificationService;
        private readonly ILogger<MemberNotificationController> _logger;

        public MemberNotificationController(
            IMemberNotificationService notificationService,
            ILogger<MemberNotificationController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        // ============================================================================
        // 通知偏好設定
        // ============================================================================

        /// <summary>
        /// 取得通知偏好設定
        /// </summary>
        /// <returns>通知偏好設定</returns>
        /// <response code="200">取得成功</response>
        /// <response code="401">未登入</response>
        [HttpGet("preferences")]
        [ProducesResponseType(typeof(GetNotificationPreferenceResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetNotificationPreferences()
        {
            try
            {
                // 從 JWT Token 取得使用者 ID
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(new GetNotificationPreferenceResponseDto
                    {
                        Success = false,
                        Message = "無效的登入資訊"
                    });
                }

                var result = await _notificationService.GetNotificationPreferenceAsync(userId.Value);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得通知偏好設定時發生錯誤");
                return StatusCode(500, new GetNotificationPreferenceResponseDto
                {
                    Success = false,
                    Message = "伺服器錯誤，請稍後再試"
                });
            }
        }

        /// <summary>
        /// 更新通知偏好設定
        /// </summary>
        /// <param name="dto">更新通知偏好設定 DTO</param>
        /// <returns>更新結果</returns>
        /// <response code="200">更新成功</response>
        /// <response code="400">驗證失敗</response>
        /// <response code="401">未登入</response>
        [HttpPut("preferences")]
        [ProducesResponseType(typeof(UpdateNotificationPreferenceResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateNotificationPreferences([FromBody] UpdateNotificationPreferenceDto dto)
        {
            try
            {
                // 從 JWT Token 取得使用者 ID
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(new UpdateNotificationPreferenceResponseDto
                    {
                        Success = false,
                        Message = "無效的登入資訊"
                    });
                }

                // ModelState 驗證
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new UpdateNotificationPreferenceResponseDto
                    {
                        Success = false,
                        Message = string.Join(", ", errors)
                    });
                }

                var result = await _notificationService.UpdateNotificationPreferenceAsync(userId.Value, dto);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新通知偏好設定時發生錯誤");
                return StatusCode(500, new UpdateNotificationPreferenceResponseDto
                {
                    Success = false,
                    Message = "伺服器錯誤，請稍後再試"
                });
            }
        }

        // ============================================================================
        // 輔助方法
        // ============================================================================

        /// <summary>
        /// 從 JWT Token 取得使用者 ID
        /// </summary>
        /// <returns>使用者 ID，如果失敗則回傳 null</returns>
        private long? GetUserIdFromToken()
        {
            try
            {
                // 從 Claims 取得使用者 ID
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