namespace FrameZone_WebApi.Configuration
{
    /// <summary>
    /// Email 設定類別
    /// </summary>
    public class EmailSettings
    {
        // SMTP 伺服器位址
        public string SmtpServer { get; set; } = string.Empty;

        // SMTP 伺服器埠口
        public int SmtpPort { get; set; }

        // 發件者 Email 地址
        public string SenderEmail { get; set; } = string.Empty;

        // 發件者顯示名稱
        public string SenderName { get; set; } = string.Empty;

        // SMTP 登入帳號
        public string Username { get; set; } = string.Empty;

        // SMTP 登入密碼
        public string Password { get; set; } = string.Empty;

        // 是否啟用 SSL/TLS 加密
        public bool EnableSsl { get; set; }
    }
}
