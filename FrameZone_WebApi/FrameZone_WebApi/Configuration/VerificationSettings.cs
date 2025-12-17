namespace FrameZone_WebApi.Configuration
{
    /// <summary>
    /// 驗證設定類別
    /// </summary>
    public class VerificationSettings
    {
        // 驗證碼長度（預設 6 位數）
        public int CodeLength { get; set; } = 6;

        // 驗證碼過期時間（分鐘）
        public int CodeExpirationMinutes { get; set; } = 15;

        // 重設密碼 Token 過期時間（小時）
        public int ResetTokenExpirationHours { get; set; } = 1;

        // 最大失敗嘗試次數
        public int MaxFailedAttempts { get; set; } = 5;
    }
}
