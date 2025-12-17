namespace FrameZone_WebApi.DTOs
{
    /// <summary>
    /// Email 內容 DTO
    /// </summary>
    public class EmailDto
    {
        // 收件者 Email 地址
        public string ToEmail { get; set; } = string.Empty;

        // 收件者姓名
        public string? ToName { get; set; }

        // Email 主旨
        public string Subject { get; set; } = string.Empty;

        // Email 內容
        public string Body { get; set; } = string.Empty;

        // 是否為 HTML 格式
        public bool IsHtml { get; set; } = true;
    }

    /// <summary>
    /// 驗證碼 Email DTO
    /// </summary>
    public class VerificationEmailDto
    {
        // 收件者 Email
        public string ToEmail { get; set; } = string.Empty;

        // 收件者姓名
        public string? ToName { get; set; }

        // 驗證碼
        public string VerificationCode { get; set; } = string.Empty;

        // 驗證碼過期時間
        public int ExpirationMinutes { get; set; }

        // 驗證目的
        public string Purpose { get; set; } = string.Empty;
    }

    /// <summary>
    /// 重設密碼 Email DTO
    /// </summary>
    public class ResetPasswordEmailDto
    {
        // 收件者 Email
        public string ToEmail { get; set; } = string.Empty;

        // 收件者姓名
        public string? ToName { get; set; }

        // 重設密碼的 URL
        public string ResetUrl { get; set; } = string.Empty;

        // Token 過期時間
        public int ExpirationHours { get; set; }
    }
}
