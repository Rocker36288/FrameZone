using FrameZone_WebApi.Models;

namespace FrameZone_WebApi.Repositories.Member
{
    /// <summary>
    /// 會員通知偏好設定 Repository 介面
    /// 負責通知偏好設定相關的資料庫存取操作
    /// </summary>
    public interface IMemberNotificationRepository
    {
        // ============================================================================
        // UserNotificationPreference (通知偏好設定)
        // ============================================================================

        /// <summary>
        /// 根據 UserId 取得通知偏好設定
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>通知偏好設定實體</returns>
        Task<UserNotificationPreference?> GetNotificationPreferenceAsync(long userId);

        /// <summary>
        /// 建立通知偏好設定
        /// </summary>
        /// <param name="preference">通知偏好設定實體</param>
        Task CreateNotificationPreferenceAsync(UserNotificationPreference preference);

        /// <summary>
        /// 更新通知偏好設定
        /// </summary>
        /// <param name="preference">通知偏好設定實體</param>
        Task UpdateNotificationPreferenceAsync(UserNotificationPreference preference);

        // ============================================================================
        // UserLog (操作日誌)
        // ============================================================================

        /// <summary>
        /// 建立操作日誌
        /// </summary>
        /// <param name="log">日誌實體</param>
        Task CreateLogAsync(UserLog log);

        // ============================================================================
        // 儲存變更
        // ============================================================================

        /// <summary>
        /// 儲存變更
        /// </summary>
        Task<int> SaveChangesAsync();
    }
}