using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FrameZone_WebApi.DTOs.Member;
using FrameZone_WebApi.Services.Member;
using System.Security.Claims;

namespace FrameZone_WebApi.Controllers
{
    /// <summary>
    /// 會員個人資料 API Controller
    /// 提供個人資料的查詢和更新功能
    /// </summary>
    [ApiController]
    [Route("api/member")]
    [Authorize] // 所有端點都需要登入
    public class MemberProfileController : ControllerBase
    {
        private readonly IMemberProfileService _memberProfileService;
        private readonly ILogger<MemberProfileController> _logger;

        public MemberProfileController(
            IMemberProfileService memberProfileService,
            ILogger<MemberProfileController> logger)
        {
            _memberProfileService = memberProfileService;
            _logger = logger;
        }

        #region 取得個人資料

        /// <summary>
        /// 取得目前登入使用者的個人資料
        /// </summary>
        /// <returns>個人資料 DTO</returns>
        /// <response code="200">成功取得個人資料</response>
        /// <response code="401">未登入</response>
        /// <response code="404">找不到使用者</response>
        [HttpGet("profile")]
        [ProducesResponseType(typeof(GetProfileResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                // 從 JWT Token 取得使用者 ID
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(new { message = "無效的登入資訊" });
                }

                // 呼叫 Service 取得個人資料
                var result = await _memberProfileService.GetProfileAsync(userId.Value);

                if (!result.Success)
                {
                    return NotFound(new { message = result.Message });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得個人資料時發生錯誤");
                return StatusCode(500, new { message = "伺服器錯誤，請稍後再試" });
            }
        }

        #endregion

        #region 更新個人資料

        /// <summary>
        /// 更新目前登入使用者的個人資料
        /// </summary>
        /// <param name="dto">更新資料 DTO（支援 multipart/form-data 上傳圖片）</param>
        /// <returns>更新結果</returns>
        /// <response code="200">更新成功</response>
        /// <response code="400">資料驗證失敗</response>
        /// <response code="401">未登入</response>
        [HttpPut("profile")]
        [Consumes("multipart/form-data")] // 支援檔案上傳
        [ProducesResponseType(typeof(UpdateProfileResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateUserProfileDto dto)
        {
            try
            {
                // 從 JWT Token 取得使用者 ID
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(new { message = "無效的登入資訊" });
                }

                // ModelState 驗證（Data Annotations）
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new UpdateProfileResponseDto
                    {
                        Success = false,
                        Message = "資料驗證失敗",
                        Errors = errors
                    });
                }

                // 呼叫 Service 更新個人資料
                var result = await _memberProfileService.UpdateProfileAsync(userId.Value, dto);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新個人資料時發生錯誤");
                return StatusCode(500, new
                {
                    success = false,
                    message = "伺服器錯誤，請稍後再試"
                });
            }
        }

        #endregion

        #region 輔助方法

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

        #endregion
    }
}