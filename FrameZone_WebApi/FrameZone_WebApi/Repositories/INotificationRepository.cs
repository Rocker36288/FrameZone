using FrameZone_WebApi.DTOs;

namespace FrameZone_WebApi.Repositories.Interfaces
{
    /// <summary>
    /// 通知 Repository 介面
    /// </summary>
    public interface INotificationRepository
    {
        /// <summary>
        /// 取得使用者未讀通知數量
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>未讀數量統計</returns>
        Task<UnreadCountDto> GetUnreadCountAsync(long userId);

        /// <summary>
        /// 取得使用者通知清單（分頁）
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="query">查詢參數</param>
        /// <returns>分頁通知清單</returns>
        Task<NotificationPagedResultDto> GetNotificationsAsync(long userId, NotificationQueryDto query);

        /// <summary>
        /// 取得單筆通知詳細資訊
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="recipientId">通知接收者 ID</param>
        /// <returns>通知詳細資訊</returns>
        Task<NotificationDto?> GetNotificationByIdAsync(long userId, long recipientId);

        /// <summary>
        /// 標記通知為已讀
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="recipientIds">通知接收者 ID 列表</param>
        /// <returns>成功標記的數量</returns>
        Task<int> MarkAsReadAsync(long userId, List<long> recipientIds);

        /// <summary>
        /// 標記所有通知為已讀
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="systemCode">系統代碼（null = 全部系統）</param>
        /// <returns>成功標記的數量</returns>
        Task<int> MarkAllAsReadAsync(long userId, string? systemCode = null);

        /// <summary>
        /// 刪除通知（軟刪除）
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="recipientIds">通知接收者 ID 列表</param>
        /// <returns>成功刪除的數量</returns>
        Task<int> DeleteNotificationsAsync(long userId, List<long> recipientIds);

        /// <summary>
        /// 清空所有通知（軟刪除）
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="systemCode">系統代碼（null = 全部系統）</param>
        /// <returns>成功刪除的數量</returns>
        Task<int> ClearAllNotificationsAsync(long userId, string? systemCode = null);

        /// <summary>
        /// 取得系統模組清單（含未讀數）
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>系統模組清單</returns>
        Task<List<SystemModuleDto>> GetSystemModulesAsync(long userId);

        /// <summary>
        /// 檢查通知是否屬於該使用者
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="recipientId">通知接收者 ID</param>
        /// <returns>是否屬於該使用者</returns>
        Task<bool> IsNotificationOwnedByUserAsync(long userId, long recipientId);

        /// <summary>
        /// 取得使用者通知偏好設定
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="categoryCode">通知類別代碼</param>
        /// <returns>通知偏好設定</returns>
        Task<NotificationPreferenceCheckDto?> GetNotificationPreferenceAsync(long userId, string categoryCode);

        /// <summary>
        /// 建立新通知（給特定使用者）
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="systemCode">系統代碼</param>
        /// <param name="categoryCode">類別代碼</param>
        /// <param name="priorityCode">優先級代碼</param>
        /// <param name="title">標題</param>
        /// <param name="content">內容</param>
        /// <param name="relatedObjectType">相關物件類型</param>
        /// <param name="relatedObjectId">相關物件 ID</param>
        /// <returns>通知 ID</returns>
        Task<long> CreateNotificationAsync(
            long userId,
            string systemCode,
            string categoryCode,
            string priorityCode,
            string title,
            string content,
            string? relatedObjectType = null,
            long? relatedObjectId = null);

        /// <summary>
        /// 批次建立通知（給多個使用者）
        /// </summary>
        /// <param name="userIds">使用者 ID 列表</param>
        /// <param name="systemCode">系統代碼</param>
        /// <param name="categoryCode">類別代碼</param>
        /// <param name="priorityCode">優先級代碼</param>
        /// <param name="title">標題</param>
        /// <param name="content">內容</param>
        /// <param name="relatedObjectType">相關物件類型</param>
        /// <param name="relatedObjectId">相關物件 ID</param>
        /// <returns>成功建立的數量</returns>
        Task<int> CreateBatchNotificationsAsync(
            List<long> userIds,
            string systemCode,
            string categoryCode,
            string priorityCode,
            string title,
            string content,
            string? relatedObjectType = null,
            long? relatedObjectId = null);

        /// <summary>
        /// 刪除過期通知
        /// </summary>
        /// <returns>刪除的數量</returns>
        Task<int> DeleteExpiredNotificationsAsync();
    }
}
