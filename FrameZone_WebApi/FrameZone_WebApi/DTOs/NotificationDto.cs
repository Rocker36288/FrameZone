namespace FrameZone_WebApi.DTOs
{
    /// <summary>
    /// 通知詳細資訊 DTO
    /// </summary>
    public class NotificationDto
    {
        /// <summary>
        /// 通知接收者 ID（主鍵）
        /// </summary>
        public long RecipientId { get; set; }

        /// <summary>
        /// 通知 ID
        /// </summary>
        public long NotificationId { get; set; }

        /// <summary>
        /// 使用者 ID
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 系統模組代碼
        /// </summary>
        public string SystemCode { get; set; } = string.Empty;

        /// <summary>
        /// 系統模組名稱
        /// </summary>
        public string SystemName { get; set; } = string.Empty;

        /// <summary>
        /// 通知類別代碼
        /// </summary>
        public string CategoryCode { get; set; } = string.Empty;

        /// <summary>
        /// 通知類別名稱
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;

        /// <summary>
        /// 通知類型圖示
        /// </summary>
        public string CategoryIcon { get; set; } = string.Empty;

        /// <summary>
        /// 優先級代碼
        /// </summary>
        public string PriorityCode { get; set; } = string.Empty;

        /// <summary>
        /// 優先級等級（數字越小越高）
        /// </summary>
        public int PriorityLevel { get; set; }

        /// <summary>
        /// 通知標題
        /// </summary>
        public string NotificationTitle { get; set; } = string.Empty;

        /// <summary>
        /// 通知內容
        /// </summary>
        public string NotificationContent { get; set; } = string.Empty;

        /// <summary>
        /// 相關物件類型
        /// </summary>
        public string? RelatedObjectType { get; set; }

        /// <summary>
        /// 相關物件 ID
        /// </summary>
        public long? RelatedObjectId { get; set; }

        /// <summary>
        /// 是否已讀
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// 讀取時間
        /// </summary>
        public DateTime? ReadAt { get; set; }

        /// <summary>
        /// 通知建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 通知過期時間
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
    }

    /// <summary>
    /// 未讀數量 DTO
    /// </summary>
    public class UnreadCountDto
    {
        /// <summary>
        /// 總未讀數量
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 各系統未讀數量
        /// </summary>
        public Dictionary<string, int> SystemCounts { get; set; } = new();
    }

    /// <summary>
    /// 通知清單查詢參數 DTO
    /// </summary>
    public class NotificationQueryDto
    {
        /// <summary>
        /// 系統模組代碼（篩選用，null = 全部）
        /// </summary>
        public string? SystemCode { get; set; }

        /// <summary>
        /// 只顯示未讀（null = 全部，true = 只顯示未讀，false = 只顯示已讀）
        /// </summary>
        public bool? IsUnreadOnly { get; set; }

        /// <summary>
        /// 頁碼（從 1 開始）
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// 每頁筆數
        /// </summary>
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 通知分頁結果 DTO
    /// </summary>
    public class NotificationPagedResultDto
    {
        /// <summary>
        /// 通知清單
        /// </summary>
        public List<NotificationDto> Items { get; set; } = new();

        /// <summary>
        /// 總筆數
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 當前頁碼
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// 每頁筆數
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 總頁數
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// 是否有上一頁
        /// </summary>
        public bool HasPrevious { get; set; }

        /// <summary>
        /// 是否有下一頁
        /// </summary>
        public bool HasNext { get; set; }
    }

    /// <summary>
    /// 標記已讀 DTO
    /// </summary>
    public class MarkAsReadDto
    {
        /// <summary>
        /// 通知接收者 ID 列表
        /// </summary>
        public List<long> RecipientIds { get; set; } = new();
    }

    /// <summary>
    /// 刪除通知 DTO
    /// </summary>
    public class DeleteNotificationDto
    {
        /// <summary>
        /// 通知接收者 ID 列表
        /// </summary>
        public List<long> RecipientIds { get; set; } = new();
    }

    /// <summary>
    /// 系統模組選項 DTO
    /// </summary>
    public class SystemModuleDto
    {
        /// <summary>
        /// 系統代碼
        /// </summary>
        public string SystemCode { get; set; } = string.Empty;

        /// <summary>
        /// 系統名稱
        /// </summary>
        public string SystemName { get; set; } = string.Empty;

        /// <summary>
        /// 未讀數量
        /// </summary>
        public int UnreadCount { get; set; }
    }

    /// <summary>
    /// 通知偏好設定 DTO（用於檢查是否應發送通知）
    /// </summary>
    public class NotificationPreferenceCheckDto
    {
        /// <summary>
        /// 使用者 ID
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 通知類別代碼
        /// </summary>
        public string CategoryCode { get; set; } = string.Empty;

        /// <summary>
        /// 是否應發送站內通知
        /// </summary>
        public bool ShouldSendBellNotification { get; set; }

        /// <summary>
        /// 是否應發送 Email
        /// </summary>
        public bool ShouldSendEmail { get; set; }

        /// <summary>
        /// 是否應發送 SMS
        /// </summary>
        public bool ShouldSendSms { get; set; }

        /// <summary>
        /// 是否應發送推播
        /// </summary>
        public bool ShouldSendPush { get; set; }
    }
}