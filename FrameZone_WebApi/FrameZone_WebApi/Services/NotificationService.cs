using FrameZone_WebApi.DTOs;
using FrameZone_WebApi.Repositories.Interfaces;
using FrameZone_WebApi.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace FrameZone_WebApi.Services
{
    /// <summary>
    /// 通知服務實作
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            INotificationRepository notificationRepository,
            ILogger<NotificationService> logger)
        {
            _notificationRepository = notificationRepository;
            _logger = logger;
        }

        /// <summary>
        /// 取得使用者未讀通知數量
        /// </summary>
        public async Task<ServiceResult<UnreadCountDto>> GetUnreadCountAsync(long userId)
        {
            try
            {
                var count = await _notificationRepository.GetUnreadCountAsync(userId);
                return ServiceResult<UnreadCountDto>.SuccessResult(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得未讀通知數量失敗 - UserId: {UserId}", userId);
                return ServiceResult<UnreadCountDto>.FailureResult("取得未讀通知數量失敗");
            }
        }

        /// <summary>
        /// 取得使用者通知清單（分頁）
        /// </summary>
        public async Task<ServiceResult<NotificationPagedResultDto>> GetNotificationsAsync(long userId, NotificationQueryDto query)
        {
            try
            {
                // 驗證分頁參數
                if (query.Page < 1)
                    query.Page = 1;

                if (query.PageSize < 1 || query.PageSize > 100)
                    query.PageSize = 20;

                var result = await _notificationRepository.GetNotificationsAsync(userId, query);
                return ServiceResult<NotificationPagedResultDto>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得通知清單失敗 - UserId: {UserId}", userId);
                return ServiceResult<NotificationPagedResultDto>.FailureResult("取得通知清單失敗");
            }
        }

        /// <summary>
        /// 取得單筆通知詳細資訊
        /// </summary>
        public async Task<ServiceResult<NotificationDto>> GetNotificationByIdAsync(long userId, long recipientId)
        {
            try
            {
                // 檢查通知是否屬於該使用者
                var isOwned = await _notificationRepository.IsNotificationOwnedByUserAsync(userId, recipientId);
                if (!isOwned)
                {
                    return ServiceResult<NotificationDto>.FailureResult("無權限存取此通知", "UNAUTHORIZED");
                }

                var notification = await _notificationRepository.GetNotificationByIdAsync(userId, recipientId);
                if (notification == null)
                {
                    return ServiceResult<NotificationDto>.FailureResult("通知不存在", "NOT_FOUND");
                }

                return ServiceResult<NotificationDto>.SuccessResult(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得通知詳細資訊失敗 - UserId: {UserId}, RecipientId: {RecipientId}", userId, recipientId);
                return ServiceResult<NotificationDto>.FailureResult("取得通知詳細資訊失敗");
            }
        }

        /// <summary>
        /// 標記單筆通知為已讀
        /// </summary>
        public async Task<ServiceResult<bool>> MarkAsReadAsync(long userId, long recipientId)
        {
            try
            {
                // 檢查通知是否屬於該使用者
                var isOwned = await _notificationRepository.IsNotificationOwnedByUserAsync(userId, recipientId);
                if (!isOwned)
                {
                    return ServiceResult<bool>.FailureResult("無權限存取此通知", "UNAUTHORIZED");
                }

                var count = await _notificationRepository.MarkAsReadAsync(userId, new List<long> { recipientId });
                return ServiceResult<bool>.SuccessResult(count > 0, count > 0 ? "標記已讀成功" : "通知已是已讀狀態");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "標記已讀失敗 - UserId: {UserId}, RecipientId: {RecipientId}", userId, recipientId);
                return ServiceResult<bool>.FailureResult("標記已讀失敗");
            }
        }

        /// <summary>
        /// 批次標記通知為已讀
        /// </summary>
        public async Task<ServiceResult<int>> MarkBatchAsReadAsync(long userId, MarkAsReadDto dto)
        {
            try
            {
                if (dto.RecipientIds == null || dto.RecipientIds.Count == 0)
                {
                    return ServiceResult<int>.FailureResult("請提供要標記的通知 ID", "INVALID_INPUT");
                }

                var count = await _notificationRepository.MarkAsReadAsync(userId, dto.RecipientIds);
                return ServiceResult<int>.SuccessResult(count, $"成功標記 {count} 則通知為已讀");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批次標記已讀失敗 - UserId: {UserId}", userId);
                return ServiceResult<int>.FailureResult("批次標記已讀失敗");
            }
        }

        /// <summary>
        /// 標記所有通知為已讀
        /// </summary>
        public async Task<ServiceResult<int>> MarkAllAsReadAsync(long userId, string? systemCode = null)
        {
            try
            {
                var count = await _notificationRepository.MarkAllAsReadAsync(userId, systemCode);
                var message = string.IsNullOrEmpty(systemCode)
                    ? $"成功標記 {count} 則通知為已讀"
                    : $"成功標記 {systemCode} 系統的 {count} 則通知為已讀";

                return ServiceResult<int>.SuccessResult(count, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "標記所有通知為已讀失敗 - UserId: {UserId}, SystemCode: {SystemCode}", userId, systemCode);
                return ServiceResult<int>.FailureResult("標記所有通知為已讀失敗");
            }
        }

        /// <summary>
        /// 刪除單筆通知
        /// </summary>
        public async Task<ServiceResult<bool>> DeleteNotificationAsync(long userId, long recipientId)
        {
            try
            {
                // 檢查通知是否屬於該使用者
                var isOwned = await _notificationRepository.IsNotificationOwnedByUserAsync(userId, recipientId);
                if (!isOwned)
                {
                    return ServiceResult<bool>.FailureResult("無權限存取此通知", "UNAUTHORIZED");
                }

                var count = await _notificationRepository.DeleteNotificationsAsync(userId, new List<long> { recipientId });
                return ServiceResult<bool>.SuccessResult(count > 0, count > 0 ? "刪除通知成功" : "通知已被刪除");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除通知失敗 - UserId: {UserId}, RecipientId: {RecipientId}", userId, recipientId);
                return ServiceResult<bool>.FailureResult("刪除通知失敗");
            }
        }

        /// <summary>
        /// 批次刪除通知
        /// </summary>
        public async Task<ServiceResult<int>> DeleteBatchNotificationsAsync(long userId, DeleteNotificationDto dto)
        {
            try
            {
                if (dto.RecipientIds == null || dto.RecipientIds.Count == 0)
                {
                    return ServiceResult<int>.FailureResult("請提供要刪除的通知 ID", "INVALID_INPUT");
                }

                var count = await _notificationRepository.DeleteNotificationsAsync(userId, dto.RecipientIds);
                return ServiceResult<int>.SuccessResult(count, $"成功刪除 {count} 則通知");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批次刪除通知失敗 - UserId: {UserId}", userId);
                return ServiceResult<int>.FailureResult("批次刪除通知失敗");
            }
        }

        /// <summary>
        /// 清空所有通知
        /// </summary>
        public async Task<ServiceResult<int>> ClearAllNotificationsAsync(long userId, string? systemCode = null)
        {
            try
            {
                var count = await _notificationRepository.ClearAllNotificationsAsync(userId, systemCode);
                var message = string.IsNullOrEmpty(systemCode)
                    ? $"成功清空 {count} 則通知"
                    : $"成功清空 {systemCode} 系統的 {count} 則通知";

                return ServiceResult<int>.SuccessResult(count, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清空所有通知失敗 - UserId: {UserId}, SystemCode: {SystemCode}", userId, systemCode);
                return ServiceResult<int>.FailureResult("清空所有通知失敗");
            }
        }

        /// <summary>
        /// 取得系統模組清單（含未讀數）
        /// </summary>
        public async Task<ServiceResult<List<SystemModuleDto>>> GetSystemModulesAsync(long userId)
        {
            try
            {
                var systems = await _notificationRepository.GetSystemModulesAsync(userId);
                return ServiceResult<List<SystemModuleDto>>.SuccessResult(systems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得系統模組清單失敗 - UserId: {UserId}", userId);
                return ServiceResult<List<SystemModuleDto>>.FailureResult("取得系統模組清單失敗");
            }
        }

        /// <summary>
        /// 發送通知給單一使用者
        /// </summary>
        public async Task<ServiceResult<long>> SendNotificationAsync(
            long userId,
            string systemCode,
            string categoryCode,
            string title,
            string content,
            string priorityCode = "MEDIUM",
            string? relatedObjectType = null,
            long? relatedObjectId = null)
        {
            try
            {
                // 驗證必填參數
                if (string.IsNullOrWhiteSpace(systemCode))
                    return ServiceResult<long>.FailureResult("系統代碼不可為空", "INVALID_INPUT");

                if (string.IsNullOrWhiteSpace(categoryCode))
                    return ServiceResult<long>.FailureResult("類別代碼不可為空", "INVALID_INPUT");

                if (string.IsNullOrWhiteSpace(title))
                    return ServiceResult<long>.FailureResult("標題不可為空", "INVALID_INPUT");

                if (string.IsNullOrWhiteSpace(content))
                    return ServiceResult<long>.FailureResult("內容不可為空", "INVALID_INPUT");

                var notificationId = await _notificationRepository.CreateNotificationAsync(
                    userId,
                    systemCode,
                    categoryCode,
                    priorityCode,
                    title,
                    content,
                    relatedObjectType,
                    relatedObjectId);

                if (notificationId == 0)
                {
                    return ServiceResult<long>.FailureResult("使用者不希望接收此類通知", "USER_PREFERENCE_DISABLED");
                }

                _logger.LogInformation("發送通知成功 - UserId: {UserId}, NotificationId: {NotificationId}", userId, notificationId);
                return ServiceResult<long>.SuccessResult(notificationId, "發送通知成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發送通知失敗 - UserId: {UserId}, SystemCode: {SystemCode}, CategoryCode: {CategoryCode}",
                    userId, systemCode, categoryCode);
                return ServiceResult<long>.FailureResult("發送通知失敗");
            }
        }

        /// <summary>
        /// 批次發送通知給多個使用者
        /// </summary>
        public async Task<ServiceResult<int>> SendBatchNotificationsAsync(
            List<long> userIds,
            string systemCode,
            string categoryCode,
            string title,
            string content,
            string priorityCode = "MEDIUM",
            string? relatedObjectType = null,
            long? relatedObjectId = null)
        {
            try
            {
                // 驗證必填參數
                if (userIds == null || userIds.Count == 0)
                    return ServiceResult<int>.FailureResult("使用者 ID 列表不可為空", "INVALID_INPUT");

                if (string.IsNullOrWhiteSpace(systemCode))
                    return ServiceResult<int>.FailureResult("系統代碼不可為空", "INVALID_INPUT");

                if (string.IsNullOrWhiteSpace(categoryCode))
                    return ServiceResult<int>.FailureResult("類別代碼不可為空", "INVALID_INPUT");

                if (string.IsNullOrWhiteSpace(title))
                    return ServiceResult<int>.FailureResult("標題不可為空", "INVALID_INPUT");

                if (string.IsNullOrWhiteSpace(content))
                    return ServiceResult<int>.FailureResult("內容不可為空", "INVALID_INPUT");

                var count = await _notificationRepository.CreateBatchNotificationsAsync(
                    userIds,
                    systemCode,
                    categoryCode,
                    priorityCode,
                    title,
                    content,
                    relatedObjectType,
                    relatedObjectId);

                _logger.LogInformation("批次發送通知成功 - 總使用者數: {TotalUsers}, 成功發送: {SuccessCount}",
                    userIds.Count, count);

                return ServiceResult<int>.SuccessResult(count, $"成功發送通知給 {count} 位使用者");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批次發送通知失敗 - SystemCode: {SystemCode}, CategoryCode: {CategoryCode}",
                    systemCode, categoryCode);
                return ServiceResult<int>.FailureResult("批次發送通知失敗");
            }
        }
    }
}