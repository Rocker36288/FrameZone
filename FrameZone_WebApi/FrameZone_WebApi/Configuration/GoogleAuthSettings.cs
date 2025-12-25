namespace FrameZone_WebApi.Configuration
{
    /// <summary>
    /// Google 第三方登入設定類別
    /// </summary>
    public class GoogleAuthSettings
    {
        /// <summary>
        /// Google OAuth 2.0 Client ID
        /// 從 Google Cloud Console 取得
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// Google OAuth 2.0 Client Secret
        /// 從 Google Cloud Console 取得
        /// </summary>
        public string ClientSecret { get; set; } = string.Empty;

        /// <summary>
        /// 授權後的重導向 URI
        /// 必須在 Google Cloud Console 中設定
        /// </summary>
        public string RedirectUri { get; set; } = string.Empty;

        /// <summary>
        /// 是否啟用 Google 登入功能
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Google Token 驗證 API URL
        /// </summary>
        public string TokenValidationUrl { get; set; } = "https://oauth2.googleapis.com/tokeninfo";

        /// <summary>
        /// Google UserInfo API URL
        /// </summary>
        public string UserInfoUrl { get; set; } = "https://www.googleapis.com/oauth2/v3/userinfo";
    }
}