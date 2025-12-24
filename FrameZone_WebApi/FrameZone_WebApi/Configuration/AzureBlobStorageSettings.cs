namespace FrameZone_WebApi.Configuration
{
    /// <summary>
    /// Azure Blob Storage 設定類別
    /// 用於從 appsettings.json 讀取 Azure Storage 相關設定
    /// </summary>
    public class AzureBlobStorageSettings
    {
        /// <summary>
        /// 設定區段名稱（用於 IOptions 模式）
        /// </summary>
        public const string SectionName = "AzureBlobStorage";

        /// <summary>
        /// Azure Storage 連線字串
        /// 格式：DefaultEndpointsProtocol=https;AccountName=youraccountname;AccountKey=youraccountkey;EndpointSuffix=core.windows.net
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// 原圖容器名稱
        /// 建議命名：framezone-photos
        /// 注意：容器名稱必須小寫，只能包含字母、數字和連字號
        /// </summary>
        public string ContainerName { get; set; } = "framezone-photos";

        /// <summary>
        /// 縮圖容器名稱
        /// 建議命名：framezone-thumbnails
        /// 可以與原圖使用同一個容器，但分開管理更清晰
        /// </summary>
        public string ThumbnailContainerName { get; set; } = "framezone-thumbnails";

        /// <summary>
        /// CDN 端點 URL（選用）
        /// 格式：https://yourcdn.azureedge.net
        /// 使用 CDN 可以加速全球訪問速度
        /// </summary>
        public string CdnEndpoint { get; set; }

        /// <summary>
        /// 是否使用 CDN
        /// true：返回 CDN URL 給前端
        /// false：返回原始 Blob Storage URL
        /// </summary>
        public bool UseCdn { get; set; } = false;

        /// <summary>
        /// 預設存取層級
        /// 可選值：Hot, Cool, Cold, Archive
        /// 初期建議：Hot（最快存取速度）
        /// </summary>
        public string DefaultAccessTier { get; set; } = "Hot";

        /// <summary>
        /// SAS Token 有效期限（分鐘）
        /// 用於生成暫時性的安全存取 URL
        /// 預設：60 分鐘
        /// </summary>
        public int SasTokenExpiryMinutes { get; set; } = 60;

        /// <summary>
        /// 最大並行上傳數
        /// 用於批次上傳時的併發控制
        /// 預設：10 個檔案同時上傳
        /// </summary>
        public int MaxConcurrentUploads { get; set; } = 10;

        /// <summary>
        /// 是否啟用 Blob Storage（功能開關）
        /// false：使用資料庫儲存（舊版方式）
        /// true：使用 Azure Blob Storage（新版方式）
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// 區域設定（選用）
        /// 例如：eastasia, southeastasia, japaneast
        /// 用於記錄或顯示資訊
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// 驗證設定是否完整
        /// </summary>
        /// <returns>驗證結果和錯誤訊息</returns>
        public (bool IsValid, string ErrorMessage) Validate()
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                return (false, "Azure Storage ConnectionString 未設定");
            }

            if (string.IsNullOrWhiteSpace(ContainerName))
            {
                return (false, "容器名稱 (ContainerName) 未設定");
            }

            if (string.IsNullOrWhiteSpace(ThumbnailContainerName))
            {
                return (false, "縮圖容器名稱 (ThumbnailContainerName) 未設定");
            }

            // 驗證容器名稱格式
            if (!IsValidContainerName(ContainerName))
            {
                return (false, $"容器名稱 '{ContainerName}' 格式不正確（必須小寫、3-63字元、只能包含字母數字和連字號）");
            }

            if (!IsValidContainerName(ThumbnailContainerName))
            {
                return (false, $"縮圖容器名稱 '{ThumbnailContainerName}' 格式不正確");
            }

            // 驗證存取層級
            var validTiers = new[] { "Hot", "Cool", "Cold", "Archive" };
            if (!validTiers.Contains(DefaultAccessTier, StringComparer.OrdinalIgnoreCase))
            {
                return (false, $"預設存取層級 '{DefaultAccessTier}' 無效（可選值：Hot, Cool, Cold, Archive）");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// 驗證容器名稱是否符合 Azure 規範
        /// 規則：
        /// - 3-63 個字元
        /// - 必須小寫
        /// - 只能包含字母、數字和連字號
        /// - 必須以字母或數字開頭和結尾
        /// - 不能有連續的連字號
        /// </summary>
        private bool IsValidContainerName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            if (name.Length < 3 || name.Length > 63)
                return false;

            if (name != name.ToLowerInvariant())
                return false;

            if (!char.IsLetterOrDigit(name[0]) || !char.IsLetterOrDigit(name[^1]))
                return false;

            if (name.Contains("--"))
                return false;

            return name.All(c => char.IsLetterOrDigit(c) || c == '-');
        }
    }
}
