using System.ComponentModel.DataAnnotations;

namespace FrameZone_WebApi.DTOs.Member
{
    // ============================================================================
    // 通知偏好設定
    // ============================================================================

    /// <summary>
    /// 通知偏好設定 DTO
    /// </summary>
    public class NotificationPreferenceDto
    {
        /// <summary>
        /// 偏好設定 ID
        /// </summary>
        public long PreferenceId { get; set; }

        /// <summary>
        /// 使用者 ID
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 電子郵件通知
        /// </summary>
        public bool EmailNotification { get; set; }

        /// <summary>
        /// 簡訊通知
        /// </summary>
        public bool SmsNotification { get; set; }

        /// <summary>
        /// 推播通知
        /// </summary>
        public bool PushNotification { get; set; }

        /// <summary>
        /// 行銷郵件
        /// </summary>
        public bool MarketingEmail { get; set; }

        /// <summary>
        /// 訂單更新
        /// </summary>
        public bool OrderUpdate { get; set; }

        /// <summary>
        /// 促銷通知
        /// </summary>
        public bool PromotionAlert { get; set; }

        /// <summary>
        /// 系統公告
        /// </summary>
        public bool SystemAnnouncement { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// 更新通知偏好設定 DTO
    /// </summary>
    public class UpdateNotificationPreferenceDto
    {
        /// <summary>
        /// 電子郵件通知
        /// </summary>
        [Required(ErrorMessage = "請指定是否接收電子郵件通知")]
        public bool EmailNotification { get; set; }

        /// <summary>
        /// 簡訊通知
        /// </summary>
        [Required(ErrorMessage = "請指定是否接收簡訊通知")]
        public bool SmsNotification { get; set; }

        /// <summary>
        /// 推播通知
        /// </summary>
        [Required(ErrorMessage = "請指定是否接收推播通知")]
        public bool PushNotification { get; set; }

        /// <summary>
        /// 行銷郵件
        /// </summary>
        [Required(ErrorMessage = "請指定是否接收行銷郵件")]
        public bool MarketingEmail { get; set; }

        /// <summary>
        /// 訂單更新
        /// </summary>
        [Required(ErrorMessage = "請指定是否接收訂單更新")]
        public bool OrderUpdate { get; set; }

        /// <summary>
        /// 促銷通知
        /// </summary>
        [Required(ErrorMessage = "請指定是否接收促銷通知")]
        public bool PromotionAlert { get; set; }

        /// <summary>
        /// 系統公告
        /// </summary>
        [Required(ErrorMessage = "請指定是否接收系統公告")]
        public bool SystemAnnouncement { get; set; }
    }

    /// <summary>
    /// 取得通知偏好設定回應 DTO
    /// </summary>
    public class GetNotificationPreferenceResponseDto
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 訊息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 通知偏好設定資料
        /// </summary>
        public NotificationPreferenceDto? Data { get; set; }
    }

    /// <summary>
    /// 更新通知偏好設定回應 DTO
    /// </summary>
    public class UpdateNotificationPreferenceResponseDto
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 訊息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 更新後的通知偏好設定資料
        /// </summary>
        public NotificationPreferenceDto? Data { get; set; }
    }
}