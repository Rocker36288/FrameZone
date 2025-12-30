using FrameZone_WebApi.DTOs.Member;

namespace FrameZone_WebApi.Services.Member
{
    /// <summary>
    /// 會員通知偏好設定服務介面
    /// 處理通知偏好設定相關的業務邏輯
    /// </summary>
    public interface IMemberNotificationService
    {
        // ============================================================================
        // 通知偏好設定
        // ============================================================================

        /// <summary>
        /// 取得通知偏好設定
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>通知偏好設定</returns>
        Task<GetNotificationPreferenceResponseDto> GetNotificationPreferenceAsync(long userId);

        /// <summary>
        /// 更新通知偏好設定
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="dto">更新通知偏好設定 DTO</param>
        /// <returns>更新結果</returns>
        Task<UpdateNotificationPreferenceResponseDto> UpdateNotificationPreferenceAsync(long userId, UpdateNotificationPreferenceDto dto);
    }
}