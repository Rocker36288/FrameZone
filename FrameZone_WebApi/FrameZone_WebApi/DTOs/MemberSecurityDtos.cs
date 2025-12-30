using System.ComponentModel.DataAnnotations;
using FrameZone_WebApi.Helpers;

namespace FrameZone_WebApi.DTOs.Member
{
    // ============================================================================
    // 變更密碼
    // ============================================================================

    /// <summary>
    /// 變更密碼請求 DTO
    /// </summary>
    public class ChangePasswordDto
    {
        /// <summary>
        /// 目前密碼
        /// </summary>
        [Required(ErrorMessage = "請輸入目前密碼")]
        public string CurrentPassword { get; set; } = string.Empty;

        /// <summary>
        /// 新密碼
        /// </summary>
        [Required(ErrorMessage = "請輸入新密碼")]
        [MinLength(SecurityConstants.PASSWORD_MIN_LENGTH, ErrorMessage = "密碼長度至少需要 {1} 個字元")]
        [MaxLength(SecurityConstants.PASSWORD_MAX_LENGTH, ErrorMessage = "密碼長度不能超過 {1} 個字元")]
        public string NewPassword { get; set; } = string.Empty;

        /// <summary>
        /// 確認新密碼
        /// </summary>
        [Required(ErrorMessage = "請輸入確認密碼")]
        [Compare(nameof(NewPassword), ErrorMessage = "新密碼與確認密碼不相符")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// 變更密碼回應 DTO
    /// </summary>
    public class ChangePasswordResponseDto
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 訊息
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }

    // ============================================================================
    // 登入裝置管理
    // ============================================================================

    /// <summary>
    /// 使用者登入裝置 (Session) DTO
    /// </summary>
    public class UserSessionDto
    {
        /// <summary>
        /// Session ID
        /// </summary>
        public long SessionId { get; set; }

        /// <summary>
        /// 使用者 ID
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// User Agent（瀏覽器資訊）
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// 是否為活躍狀態
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// 最後活動時間
        /// </summary>
        public DateTime LastActivityAt { get; set; }

        /// <summary>
        /// 過期時間
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 是否為目前裝置
        /// </summary>
        public bool IsCurrentSession { get; set; }

        /// <summary>
        /// 裝置類型（Desktop, Mobile, Tablet）
        /// </summary>
        public string DeviceType { get; set; } = "Desktop";

        /// <summary>
        /// 瀏覽器名稱
        /// </summary>
        public string BrowserName { get; set; } = "Unknown";

        /// <summary>
        /// 作業系統
        /// </summary>
        public string OperatingSystem { get; set; } = "Unknown";

        /// <summary>
        /// IP 位址（從 UserLog 或其他來源取得）
        /// </summary>
        public string? IpAddress { get; set; }
    }

    /// <summary>
    /// 取得所有登入裝置回應 DTO
    /// </summary>
    public class GetUserSessionsResponseDto
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
        /// 裝置列表
        /// </summary>
        public List<UserSessionDto>? Data { get; set; }
    }

    /// <summary>
    /// 登出特定裝置回應 DTO
    /// </summary>
    public class LogoutSessionResponseDto
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 訊息
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }

    // ============================================================================
    // 帳號鎖定狀態
    // ============================================================================

    /// <summary>
    /// 帳號鎖定狀態 DTO
    /// </summary>
    public class AccountLockStatusDto
    {
        /// <summary>
        /// 使用者 ID
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 是否被鎖定
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// 失敗登入次數
        /// </summary>
        public int FailedLoginAttempts { get; set; }

        /// <summary>
        /// 最後失敗登入時間
        /// </summary>
        public DateTime? LastFailedLoginAt { get; set; }

        /// <summary>
        /// 鎖定時間
        /// </summary>
        public DateTime? LockedAt { get; set; }

        /// <summary>
        /// 鎖定到期時間
        /// </summary>
        public DateTime? LockedUntil { get; set; }

        /// <summary>
        /// 鎖定原因
        /// </summary>
        public string? LockedReason { get; set; }

        /// <summary>
        /// 鎖定者
        /// </summary>
        public string? LockedBy { get; set; }

        /// <summary>
        /// 解鎖時間
        /// </summary>
        public DateTime? UnlockedAt { get; set; }

        /// <summary>
        /// 剩餘鎖定時間（分鐘）
        /// </summary>
        public int? RemainingLockMinutes { get; set; }
    }

    /// <summary>
    /// 取得帳號鎖定狀態回應 DTO
    /// </summary>
    public class GetAccountLockStatusResponseDto
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
        /// 鎖定狀態資料
        /// </summary>
        public AccountLockStatusDto? Data { get; set; }
    }

    // ============================================================================
    // 安全性概覽（選用）
    // ============================================================================

    /// <summary>
    /// 安全性概覽 DTO
    /// </summary>
    public class SecurityOverviewDto
    {
        /// <summary>
        /// 帳號鎖定狀態
        /// </summary>
        public AccountLockStatusDto? LockStatus { get; set; }

        /// <summary>
        /// 目前活躍裝置數量
        /// </summary>
        public int ActiveSessionCount { get; set; }

        /// <summary>
        /// 最後一次變更密碼時間
        /// </summary>
        public DateTime? LastPasswordChangeAt { get; set; }

        /// <summary>
        /// 最後一次登入時間
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// 最後一次登入 IP
        /// </summary>
        public string? LastLoginIp { get; set; }

        /// <summary>
        /// 是否建議變更密碼（超過 90 天）
        /// </summary>
        public bool ShouldChangePassword { get; set; }
    }

    /// <summary>
    /// 取得安全性概覽回應 DTO
    /// </summary>
    public class GetSecurityOverviewResponseDto
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
        /// 安全性概覽資料
        /// </summary>
        public SecurityOverviewDto? Data { get; set; }
    }
}