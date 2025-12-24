namespace FrameZone_WebApi.DTOs
{
    /// <summary>
    /// 縮圖資料 DTO
    /// </summary>
    public class ThumbnailDataDTO
    {
        /// <summary>
        /// 照片 ID
        /// </summary>
        public long PhotoId { get; set; }

        /// <summary>
        /// 使用者 ID
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 縮圖資料
        /// </summary>
        public byte[] ThumbnailData { get; set; }

        /// <summary>
        /// 檔案副檔名
        /// </summary>
        public string FileExtension { get; set; }

        /// <summary>
        /// 檔案名稱
        /// </summary>
        public string FileName { get; set; }
    }

    /// <summary>
    /// 原圖資料 DTO
    /// </summary>
    public class PhotoDataDTO
    {
        /// <summary>
        /// 照片 ID
        /// </summary>
        public long PhotoId { get; set; }

        /// <summary>
        /// 使用者 ID
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 原圖資料
        /// </summary>
        public byte[] PhotoData { get; set; }

        /// <summary>
        /// 檔案副檔名
        /// </summary>
        public string FileExtension { get; set; }
    }
}
