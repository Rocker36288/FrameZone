using System.ComponentModel.DataAnnotations;

namespace FrameZone_WebApi.DTOs.Member
{
    #region 查詢參數

    /// <summary>
    /// 使用者活動記錄查詢參數 DTO
    /// </summary>
    public class UserLogQueryDto
    {
        /// <summary>
        /// 頁碼（從 1 開始）
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "頁碼必須大於 0")]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// 每頁筆數
        /// </summary>
        [Range(1, 100, ErrorMessage = "每頁筆數必須在 1-100 之間")]
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// 操作類型篩選
        /// 例如: Login, Logout, ProfileUpdate, PasswordChange
        /// </summary>
        public string? ActionType { get; set; }

        /// <summary>
        /// 操作類別篩選
        /// 例如: Security, Profile, Settings, System
        /// </summary>
        public string? ActionCategory { get; set; }

        /// <summary>
        /// 開始日期（篩選）
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 結束日期（篩選）
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// 狀態篩選
        /// Success, Failure
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// 嚴重性篩選
        /// Info, Warning, Error
        /// </summary>
        public string? Severity { get; set; }
    }

    #endregion

    #region 單筆記錄

    /// <summary>
    /// 使用者活動記錄 DTO（回傳用）
    /// </summary>
    public class UserLogDto
    {
        /// <summary>
        /// 記錄 ID
        /// </summary>
        public long LogId { get; set; }

        /// <summary>
        /// 使用者 ID
        /// </summary>
        public long? UserId { get; set; }

        /// <summary>
        /// 狀態（Success, Failure）
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// 操作類型
        /// </summary>
        public string ActionType { get; set; } = string.Empty;

        /// <summary>
        /// 操作類別
        /// </summary>
        public string ActionCategory { get; set; } = string.Empty;

        /// <summary>
        /// 操作描述
        /// </summary>
        public string ActionDescription { get; set; } = string.Empty;

        /// <summary>
        /// 目標類型
        /// </summary>
        public string? TargetType { get; set; }

        /// <summary>
        /// 目標 ID
        /// </summary>
        public long? TargetId { get; set; }

        /// <summary>
        /// 舊值（變更前）
        /// </summary>
        public string? OldValue { get; set; }

        /// <summary>
        /// 新值（變更後）
        /// </summary>
        public string? NewValue { get; set; }

        /// <summary>
        /// IP 位址
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// User Agent
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// 裝置類型
        /// </summary>
        public string? DeviceType { get; set; }

        /// <summary>
        /// 系統名稱
        /// </summary>
        public string SystemName { get; set; } = string.Empty;

        /// <summary>
        /// 嚴重性（Info, Warning, Error）
        /// </summary>
        public string Severity { get; set; } = string.Empty;

        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 執行時間（毫秒）
        /// </summary>
        public int? ExecutionTime { get; set; }

        /// <summary>
        /// 執行者
        /// </summary>
        public string? PerformedBy { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }

    #endregion

    #region 分頁回應

    /// <summary>
    /// 活動記錄分頁回應 DTO
    /// </summary>
    public class UserLogPagedResponseDto
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
        /// 分頁資料
        /// </summary>
        public PagedData<UserLogDto>? Data { get; set; }
    }

    /// <summary>
    /// 分頁資料包裝類別
    /// </summary>
    public class PagedData<T>
    {
        /// <summary>
        /// 資料列表
        /// </summary>
        public List<T> Items { get; set; } = new List<T>();

        /// <summary>
        /// 總筆數
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 當前頁碼
        /// </summary>
        public int PageNumber { get; set; }

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
        public bool HasPreviousPage => PageNumber > 1;

        /// <summary>
        /// 是否有下一頁
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;
    }

    #endregion

    #region 統計資料

    /// <summary>
    /// 活動記錄統計 DTO
    /// </summary>
    public class UserLogStatsDto
    {
        /// <summary>
        /// 總記錄數
        /// </summary>
        public int TotalLogs { get; set; }

        /// <summary>
        /// 成功次數
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// 失敗次數
        /// </summary>
        public int FailureCount { get; set; }

        /// <summary>
        /// 最後登入時間
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// 最後活動時間
        /// </summary>
        public DateTime? LastActivityAt { get; set; }

        /// <summary>
        /// 依操作類型分組統計
        /// </summary>
        public Dictionary<string, int> ActionTypeStats { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// 依操作類別分組統計
        /// </summary>
        public Dictionary<string, int> ActionCategoryStats { get; set; } = new Dictionary<string, int>();
    }

    /// <summary>
    /// 活動記錄統計回應 DTO
    /// </summary>
    public class UserLogStatsResponseDto
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
        /// 統計資料
        /// </summary>
        public UserLogStatsDto? Data { get; set; }
    }

    #endregion
}