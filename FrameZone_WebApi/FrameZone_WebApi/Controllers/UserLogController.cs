using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FrameZone_WebApi.DTOs.Member;
using FrameZone_WebApi.Services.Member;
using System.Security.Claims;

namespace FrameZone_WebApi.Controllers
{
    /// <summary>
    /// 使用者活動記錄 API Controller
    /// 提供查詢、統計、匯出等功能
    /// </summary>
    [ApiController]
    [Route("api/member/logs")]
    [Authorize] // 所有端點都需要登入
    public class UserLogController : ControllerBase
    {
        private readonly IUserLogService _userLogService;
        private readonly ILogger<UserLogController> _logger;

        public UserLogController(
            IUserLogService userLogService,
            ILogger<UserLogController> logger)
        {
            _userLogService = userLogService;
            _logger = logger;
        }

        #region 查詢活動記錄

        /// <summary>
        /// 分頁查詢使用者活動記錄
        /// </summary>
        /// <param name="queryDto">查詢參數</param>
        /// <returns>分頁活動記錄</returns>
        /// <response code="200">查詢成功</response>
        /// <response code="400">查詢參數錯誤</response>
        /// <response code="401">未登入</response>
        [HttpGet]
        [ProducesResponseType(typeof(UserLogPagedResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserLogs([FromQuery] UserLogQueryDto queryDto)
        {
            try
            {
                // 從 JWT Token 取得使用者 ID
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
                        message = "查詢參數錯誤",
                        errors
                    });
                }

                // 呼叫 Service 查詢
                var result = await _userLogService.GetUserLogsAsync(userId.Value, queryDto);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查詢使用者活動記錄時發生錯誤");
                return StatusCode(500, new
                {
                    success = false,
                    message = "伺服器錯誤，請稍後再試"
                });
            }
        }

        /// <summary>
        /// 取得使用者活動統計資料
        /// </summary>
        /// <returns>活動統計資料</returns>
        /// <response code="200">取得成功</response>
        /// <response code="401">未登入</response>
        [HttpGet("stats")]
        [ProducesResponseType(typeof(UserLogStatsResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserLogStats()
        {
            try
            {
                // 從 JWT Token 取得使用者 ID
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(new { message = "無效的登入資訊" });
                }

                // 呼叫 Service 取得統計資料
                var result = await _userLogService.GetUserLogStatsAsync(userId.Value);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得使用者活動統計時發生錯誤");
                return StatusCode(500, new
                {
                    success = false,
                    message = "伺服器錯誤，請稍後再試"
                });
            }
        }

        /// <summary>
        /// 取得單筆活動記錄詳細資料
        /// </summary>
        /// <param name="logId">日誌 ID</param>
        /// <returns>活動記錄詳細資料</returns>
        /// <response code="200">取得成功</response>
        /// <response code="401">未登入</response>
        /// <response code="404">找不到記錄</response>
        [HttpGet("{logId}")]
        [ProducesResponseType(typeof(UserLogPagedResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserLogById(long logId)
        {
            try
            {
                // 從 JWT Token 取得使用者 ID
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(new { message = "無效的登入資訊" });
                }

                // 呼叫 Service 取得詳細資料
                var result = await _userLogService.GetUserLogByIdAsync(userId.Value, logId);

                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得活動記錄詳細資料時發生錯誤：LogId={LogId}", logId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "伺服器錯誤，請稍後再試"
                });
            }
        }

        /// <summary>
        /// 取得最近登入記錄
        /// </summary>
        /// <param name="count">取得數量（預設 5 筆，最多 20 筆）</param>
        /// <returns>最近登入記錄</returns>
        /// <response code="200">取得成功</response>
        /// <response code="401">未登入</response>
        [HttpGet("recent-logins")]
        [ProducesResponseType(typeof(UserLogPagedResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetRecentLoginLogs([FromQuery] int count = 5)
        {
            try
            {
                // 從 JWT Token 取得使用者 ID
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(new { message = "無效的登入資訊" });
                }

                // 呼叫 Service 取得最近登入記錄
                var result = await _userLogService.GetRecentLoginLogsAsync(userId.Value, count);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得最近登入記錄時發生錯誤");
                return StatusCode(500, new
                {
                    success = false,
                    message = "伺服器錯誤，請稍後再試"
                });
            }
        }

        #endregion

        #region 匯出功能

        /// <summary>
        /// 匯出使用者活動記錄為 CSV 檔案
        /// </summary>
        /// <param name="queryDto">查詢參數（用於篩選要匯出的資料）</param>
        /// <returns>CSV 檔案</returns>
        /// <response code="200">匯出成功</response>
        /// <response code="401">未登入</response>
        [HttpGet("export")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ExportUserLogs([FromQuery] UserLogQueryDto queryDto)
        {
            try
            {
                // 從 JWT Token 取得使用者 ID
                var userId = GetUserIdFromToken();
                if (userId == null)
                {
                    return Unauthorized(new { message = "無效的登入資訊" });
                }

                _logger.LogInformation("匯出使用者活動記錄：UserId={UserId}", userId.Value);

                // 呼叫 Service 匯出 CSV
                var csvData = await _userLogService.ExportUserLogsToCsvAsync(userId.Value, queryDto);

                // 產生檔案名稱（包含日期時間）
                var fileName = $"user_activity_logs_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                // 回傳檔案
                return File(
                    csvData,
                    "text/csv",
                    fileName
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "匯出使用者活動記錄時發生錯誤");
                return StatusCode(500, new
                {
                    success = false,
                    message = "匯出失敗，請稍後再試"
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