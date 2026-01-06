using FrameZone_WebApi.DTOs;
using FrameZone_WebApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FrameZone_WebApi.Controllers
{
    /// <summary>
    /// 通知控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(
            INotificationService notificationService,
            ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// 取得當前使用者 ID
        /// </summary>
        private long GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("無法取得使用者 ID");
            }
            return userId;
        }

        /// <summary>
        /// 取得未讀通知數量
        /// </summary>
        /// <returns>未讀數量統計</returns>
        [HttpGet("unread-count")]
        [ProducesResponseType(typeof(ServiceResult<UnreadCountDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _notificationService.GetUnreadCountAsync(userId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "未授權的請求");
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得未讀數量時發生錯誤");
                return StatusCode(500, new { success = false, message = "伺服器錯誤" });
            }
        }

        /// <summary>
        /// 取得通知清單（分頁）
        /// </summary>
        /// <param name="systemCode">系統代碼（選填）</param>
        /// <param name="isUnreadOnly">只顯示未讀（選填）</param>
        /// <param name="page">頁碼（預設 1）</param>
        /// <param name="pageSize">每頁筆數（預設 20）</param>
        /// <returns>分頁通知清單</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ServiceResult<NotificationPagedResultDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetNotifications(
            [FromQuery] string? systemCode = null,
            [FromQuery] bool? isUnreadOnly = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = GetCurrentUserId();
                var query = new NotificationQueryDto
                {
                    SystemCode = systemCode,
                    IsUnreadOnly = isUnreadOnly,
                    Page = page,
                    PageSize = pageSize
                };

                var result = await _notificationService.GetNotificationsAsync(userId, query);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "未授權的請求");
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得通知清單時發生錯誤");
                return StatusCode(500, new { success = false, message = "伺服器錯誤" });
            }
        }

        /// <summary>
        /// 取得單筆通知詳細資訊
        /// </summary>
        /// <param name="recipientId">通知接收者 ID</param>
        /// <returns>通知詳細資訊</returns>
        [HttpGet("{recipientId}")]
        [ProducesResponseType(typeof(ServiceResult<NotificationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetNotificationById(long recipientId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _notificationService.GetNotificationByIdAsync(userId, recipientId);

                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "未授權的請求");
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得通知詳細資訊時發生錯誤 - RecipientId: {RecipientId}", recipientId);
                return StatusCode(500, new { success = false, message = "伺服器錯誤" });
            }
        }

        /// <summary>
        /// 標記單筆通知為已讀
        /// </summary>
        /// <param name="recipientId">通知接收者 ID</param>
        /// <returns>操作結果</returns>
        [HttpPut("{recipientId}/read")]
        [ProducesResponseType(typeof(ServiceResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MarkAsRead(long recipientId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _notificationService.MarkAsReadAsync(userId, recipientId);

                if (!result.Success && result.ErrorCode == "UNAUTHORIZED")
                {
                    return Unauthorized(result);
                }

                if (!result.Success && result.ErrorCode == "NOT_FOUND")
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "未授權的請求");
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "標記已讀時發生錯誤 - RecipientId: {RecipientId}", recipientId);
                return StatusCode(500, new { success = false, message = "伺服器錯誤" });
            }
        }

        /// <summary>
        /// 批次標記通知為已讀
        /// </summary>
        /// <param name="dto">標記已讀 DTO</param>
        /// <returns>操作結果</returns>
        [HttpPut("read-batch")]
        [ProducesResponseType(typeof(ServiceResult<int>), StatusCodes.Status200OK)]
        public async Task<IActionResult> MarkBatchAsRead([FromBody] MarkAsReadDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _notificationService.MarkBatchAsReadAsync(userId, dto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "未授權的請求");
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批次標記已讀時發生錯誤");
                return StatusCode(500, new { success = false, message = "伺服器錯誤" });
            }
        }

        /// <summary>
        /// 標記所有通知為已讀
        /// </summary>
        /// <param name="systemCode">系統代碼（選填）</param>
        /// <returns>操作結果</returns>
        [HttpPut("read-all")]
        [ProducesResponseType(typeof(ServiceResult<int>), StatusCodes.Status200OK)]
        public async Task<IActionResult> MarkAllAsRead([FromQuery] string? systemCode = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _notificationService.MarkAllAsReadAsync(userId, systemCode);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "未授權的請求");
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "標記所有通知為已讀時發生錯誤");
                return StatusCode(500, new { success = false, message = "伺服器錯誤" });
            }
        }

        /// <summary>
        /// 刪除單筆通知
        /// </summary>
        /// <param name="recipientId">通知接收者 ID</param>
        /// <returns>操作結果</returns>
        [HttpDelete("{recipientId}")]
        [ProducesResponseType(typeof(ServiceResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteNotification(long recipientId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _notificationService.DeleteNotificationAsync(userId, recipientId);

                if (!result.Success && result.ErrorCode == "UNAUTHORIZED")
                {
                    return Unauthorized(result);
                }

                if (!result.Success && result.ErrorCode == "NOT_FOUND")
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "未授權的請求");
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除通知時發生錯誤 - RecipientId: {RecipientId}", recipientId);
                return StatusCode(500, new { success = false, message = "伺服器錯誤" });
            }
        }

        /// <summary>
        /// 批次刪除通知
        /// </summary>
        /// <param name="dto">刪除通知 DTO</param>
        /// <returns>操作結果</returns>
        [HttpDelete("delete-batch")]
        [ProducesResponseType(typeof(ServiceResult<int>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteBatchNotifications([FromBody] DeleteNotificationDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _notificationService.DeleteBatchNotificationsAsync(userId, dto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "未授權的請求");
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批次刪除通知時發生錯誤");
                return StatusCode(500, new { success = false, message = "伺服器錯誤" });
            }
        }

        /// <summary>
        /// 清空所有通知
        /// </summary>
        /// <param name="systemCode">系統代碼（選填）</param>
        /// <returns>操作結果</returns>
        [HttpDelete("clear-all")]
        [ProducesResponseType(typeof(ServiceResult<int>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ClearAllNotifications([FromQuery] string? systemCode = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _notificationService.ClearAllNotificationsAsync(userId, systemCode);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "未授權的請求");
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清空所有通知時發生錯誤");
                return StatusCode(500, new { success = false, message = "伺服器錯誤" });
            }
        }

        /// <summary>
        /// 取得系統模組清單（含未讀數）
        /// </summary>
        /// <returns>系統模組清單</returns>
        [HttpGet("systems")]
        [ProducesResponseType(typeof(ServiceResult<List<SystemModuleDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSystemModules()
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _notificationService.GetSystemModulesAsync(userId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "未授權的請求");
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得系統模組清單時發生錯誤");
                return StatusCode(500, new { success = false, message = "伺服器錯誤" });
            }
        }
    }
}