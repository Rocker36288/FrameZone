using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FrameZone_WebApi.DTOs.Member;
using FrameZone_WebApi.Services.Member;
using System.Security.Claims;

namespace FrameZone_WebApi.Controllers
{
    /// <summary>
    /// 會員安全 API Controller
    /// 提供密碼變更、登入裝置管理、帳號鎖定狀態等安全相關功能
    /// </summary>
    [ApiController]
    [Route("api/member/security")]
    [Authorize] // 所有端點都需要登入
    public class MemberSecurityController : ControllerBase
    {
        private readonly IMemberSecurityService _securityService;
        private readonly ILogger<MemberSecurityController> _logger;

        public MemberSecurityController(
            IMemberSecurityService securityService,
            ILogger<MemberSecurityController> logger)
        {
            _securityService = securityService;
            _logger = logger;
        }

        // ============================================================================
        // 變更密碼
        // ============================================================================

        /// <summary>
        /// 變更密碼
        /// </summary>
        /// <param name="dto">變更密碼 DTO</param>
        /// <returns>變更結果</returns>
        /// <response code="200">密碼變更成功</response>
        /// <response code="400">驗證失敗或密碼錯誤</response>
        /// <response code="401">未登入</response>
        [HttpPost("change-password")]
        [ProducesResponseType(typeof(ChangePasswordResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            try
            {
                // 從 JWT Token 取得使用者 ID
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(new ChangePasswordResponseDto
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

                    return BadRequest(new ChangePasswordResponseDto
                    {
                        Success = false,
                        Message = string.Join(", ", errors)
                    });
                }

                // 呼叫 Service 變更密碼
                var result = await _securityService.ChangePasswordAsync(userId.Value, dto);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "變更密碼時發生錯誤");
                return StatusCode(500, new ChangePasswordResponseDto
                {
                    Success = false,
                    Message = "伺服器錯誤，請稍後再試"
                });
            }
        }

        // ============================================================================
        // 登入裝置管理
        // ============================================================================

        /// <summary>
        /// 取得所有登入裝置
        /// </summary>
        /// <returns>裝置列表</returns>
        /// <response code="200">取得成功</response>
        /// <response code="401">未登入</response>
        [HttpGet("sessions")]
        [ProducesResponseType(typeof(GetUserSessionsResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSessions()
        {
            try
            {
                // 從 JWT Token 取得使用者 ID
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(new GetUserSessionsResponseDto
                    {
                        Success = false,
                        Message = "無效的登入資訊"
                    });
                }

                // TODO: 取得目前 Session ID（可從 Cookie 或其他方式取得）
                long? currentSessionId = null;

                var result = await _securityService.GetUserSessionsAsync(userId.Value, currentSessionId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得登入裝置時發生錯誤");
                return StatusCode(500, new GetUserSessionsResponseDto
                {
                    Success = false,
                    Message = "伺服器錯誤，請稍後再試"
                });
            }
        }

        /// <summary>
        /// 登出特定裝置
        /// </summary>
        /// <param name="sessionId">要登出的 Session ID</param>
        /// <returns>登出結果</returns>
        /// <response code="200">登出成功</response>
        /// <response code="400">找不到裝置</response>
        /// <response code="401">未登入</response>
        [HttpDelete("sessions/{sessionId}")]
        [ProducesResponseType(typeof(LogoutSessionResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LogoutSession(long sessionId)
        {
            try
            {
                // 從 JWT Token 取得使用者 ID
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(new LogoutSessionResponseDto
                    {
                        Success = false,
                        Message = "無效的登入資訊"
                    });
                }

                var result = await _securityService.LogoutSessionAsync(userId.Value, sessionId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "登出裝置時發生錯誤");
                return StatusCode(500, new LogoutSessionResponseDto
                {
                    Success = false,
                    Message = "伺服器錯誤，請稍後再試"
                });
            }
        }

        /// <summary>
        /// 登出所有其他裝置
        /// </summary>
        /// <returns>登出結果</returns>
        /// <response code="200">登出成功</response>
        /// <response code="401">未登入</response>
        [HttpDelete("sessions/others")]
        [ProducesResponseType(typeof(LogoutSessionResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LogoutOtherSessions()
        {
            try
            {
                // 從 JWT Token 取得使用者 ID
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(new LogoutSessionResponseDto
                    {
                        Success = false,
                        Message = "無效的登入資訊"
                    });
                }

                // TODO: 取得目前 Session ID（可從 Cookie 或其他方式取得）
                // 暫時使用 0，需要實作 Session 管理機制
                long currentSessionId = 0;

                var result = await _securityService.LogoutOtherSessionsAsync(userId.Value, currentSessionId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "登出其他裝置時發生錯誤");
                return StatusCode(500, new LogoutSessionResponseDto
                {
                    Success = false,
                    Message = "伺服器錯誤，請稍後再試"
                });
            }
        }

        // ============================================================================
        // 帳號鎖定狀態
        // ============================================================================

        /// <summary>
        /// 取得帳號鎖定狀態
        /// </summary>
        /// <returns>鎖定狀態</returns>
        /// <response code="200">取得成功</response>
        /// <response code="401">未登入</response>
        [HttpGet("lock-status")]
        [ProducesResponseType(typeof(GetAccountLockStatusResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetLockStatus()
        {
            try
            {
                // 從 JWT Token 取得使用者 ID
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(new GetAccountLockStatusResponseDto
                    {
                        Success = false,
                        Message = "無效的登入資訊"
                    });
                }

                var result = await _securityService.GetAccountLockStatusAsync(userId.Value);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得帳號鎖定狀態時發生錯誤");
                return StatusCode(500, new GetAccountLockStatusResponseDto
                {
                    Success = false,
                    Message = "伺服器錯誤，請稍後再試"
                });
            }
        }

        // ============================================================================
        // 安全性概覽
        // ============================================================================

        /// <summary>
        /// 取得安全性概覽
        /// </summary>
        /// <returns>安全性概覽</returns>
        /// <response code="200">取得成功</response>
        /// <response code="401">未登入</response>
        [HttpGet("overview")]
        [ProducesResponseType(typeof(GetSecurityOverviewResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSecurityOverview()
        {
            try
            {
                // 從 JWT Token 取得使用者 ID
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(new GetSecurityOverviewResponseDto
                    {
                        Success = false,
                        Message = "無效的登入資訊"
                    });
                }

                var result = await _securityService.GetSecurityOverviewAsync(userId.Value);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得安全性概覽時發生錯誤");
                return StatusCode(500, new GetSecurityOverviewResponseDto
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