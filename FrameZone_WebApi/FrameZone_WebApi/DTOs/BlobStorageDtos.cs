namespace FrameZone_WebApi.DTOs
{
    #region Blob Storage 上傳相關 DTO

    /// <summary>
    /// Blob 上傳請求 DTO
    /// 用於批次上傳時傳遞檔案資訊
    /// </summary>
    public class BlobUploadRequestDto
    {
        /// <summary>
        /// 檔案串流
        /// </summary>
        public Stream Stream { get; set; }

        /// <summary>
        /// Blob 相對路徑
        /// 格式：{userId}/{year}/{month}/{photoId}_original.jpg
        /// </summary>
        public string BlobPath { get; set; }

        /// <summary>
        /// 容器名稱
        /// 例如：framezone-photos, framezone-thumbnails
        /// </summary>
        public string ContainerName { get; set; }

        /// <summary>
        /// 內容類型（MIME Type）
        /// 例如：image/jpeg, image/png, image/heic
        /// </summary>
        public string ContentType { get; set; } = "image/jpeg";
    }

    /// <summary>
    /// Blob 上傳結果 DTO
    /// </summary>
    public class BlobUploadResultDto
    {
        /// <summary>
        /// 是否上傳成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Blob 相對路徑
        /// </summary>
        public string BlobPath { get; set; }

        /// <summary>
        /// Blob 完整 URL
        /// 可能是 Blob Storage URL 或 CDN URL
        /// </summary>
        public string BlobUrl { get; set; }

        /// <summary>
        /// 錯誤訊息（如果上傳失敗）
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 上傳時間
        /// </summary>
        public DateTime? UploadedAt { get; set; }

        /// <summary>
        /// 檔案大小（bytes）
        /// </summary>
        public long? FileSizeBytes { get; set; }
    }

    #endregion

    #region Blob Storage 刪除相關 DTO

    /// <summary>
    /// 批次刪除結果 DTO
    /// </summary>
    public class BatchDeleteResultDto
    {
        /// <summary>
        /// 成功刪除的數量
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// 刪除失敗的數量
        /// </summary>
        public int FailedCount { get; set; }

        /// <summary>
        /// 總共處理的數量
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 刪除失敗的路徑列表
        /// </summary>
        public List<string> FailedPaths { get; set; } = new List<string>();

        /// <summary>
        /// 刪除失敗的詳細資訊
        /// Key: Blob 路徑, Value: 錯誤訊息
        /// </summary>
        public Dictionary<string, string> FailedDetails { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// 是否全部成功
        /// </summary>
        public bool AllSuccess => FailedCount == 0;

        /// <summary>
        /// 是否全部失敗
        /// </summary>
        public bool AllFailed => SuccessCount == 0;
    }

    #endregion

    #region Blob Storage 統計相關 DTO

    /// <summary>
    /// 容器統計資訊 DTO
    /// 用於顯示容器的使用狀況
    /// </summary>
    public class BlobContainerStatsDto
    {
        /// <summary>
        /// 容器名稱
        /// </summary>
        public string ContainerName { get; set; }

        /// <summary>
        /// Blob 總數
        /// </summary>
        public long TotalBlobs { get; set; }

        /// <summary>
        /// 總大小（bytes）
        /// </summary>
        public long TotalSizeBytes { get; set; }

        /// <summary>
        /// 總大小（MB）
        /// </summary>
        public double TotalSizeMB => Math.Round(TotalSizeBytes / 1024.0 / 1024.0, 2);

        /// <summary>
        /// 總大小（GB）
        /// </summary>
        public double TotalSizeGB => Math.Round(TotalSizeBytes / 1024.0 / 1024.0 / 1024.0, 2);

        /// <summary>
        /// 平均檔案大小（bytes）
        /// </summary>
        public long AverageSizeBytes => TotalBlobs > 0 ? TotalSizeBytes / TotalBlobs : 0;

        /// <summary>
        /// 平均檔案大小（MB）
        /// </summary>
        public double AverageSizeMB => Math.Round(AverageSizeBytes / 1024.0 / 1024.0, 2);

        /// <summary>
        /// 統計時間
        /// </summary>
        public DateTime StatsDate { get; set; } = DateTime.UtcNow;
    }

    #endregion

    #region Blob Storage 查詢相關 DTO

    /// <summary>
    /// Blob 資訊 DTO
    /// 用於列出 Blob 詳細資訊
    /// </summary>
    public class BlobInfoDto
    {
        /// <summary>
        /// Blob 名稱（相對路徑）
        /// </summary>
        public string BlobName { get; set; }

        /// <summary>
        /// Blob 完整 URL
        /// </summary>
        public string BlobUrl { get; set; }

        /// <summary>
        /// 檔案大小（bytes）
        /// </summary>
        public long SizeBytes { get; set; }

        /// <summary>
        /// 檔案大小（MB）
        /// </summary>
        public double SizeMB => Math.Round(SizeBytes / 1024.0 / 1024.0, 2);

        /// <summary>
        /// 內容類型
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// 建立時間（UTC）
        /// </summary>
        public DateTime? CreatedOn { get; set; }

        /// <summary>
        /// 最後修改時間（UTC）
        /// </summary>
        public DateTime? LastModified { get; set; }

        /// <summary>
        /// ETag（用於快取控制）
        /// </summary>
        public string ETag { get; set; }

        /// <summary>
        /// 存取層級
        /// Hot, Cool, Cold, Archive
        /// </summary>
        public string AccessTier { get; set; }
    }

    #endregion
}