using System;
using System.Collections.Generic;

namespace FrameZone_WebApi.DTOs
{
    /// <summary>
    /// 照片元數據 DTO
    /// </summary>
    public class PhotoMetadataDTO
    {
        // 基本資訊
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string FileExtension { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        // 拍攝時間
        public DateTime? DateTaken { get; set; }

        // GPS 資訊
        public decimal? GPSLatitude { get; set; }
        public decimal? GPSLongitude { get; set; }

        // 相機資訊
        public string CameraMake { get; set; }
        public string CameraModel { get; set; }

        // 拍攝參數
        public decimal? FocalLength { get; set; }
        public decimal? Aperture { get; set; }
        public string ShutterSpeed { get; set; }
        public int? ISO { get; set; }
        public string ExposureMode { get; set; }
        public string WhiteBalance { get; set; }
        public string LensModel { get; set; }

        // 方向
        public int? Orientation { get; set; }

        // 自動分類標籤
        public List<string> AutoTags { get; set; } = new List<string>();

        // Hash (用於去重)
        public string Hash { get; set; }

        // Blob Storage URL
        public string BlobUrl { get; set; }

        // 縮圖 URL
        public string ThumbnailUrl { get; set; }
    }

    /// <summary>
    /// 照片上傳回應 DTO
    /// </summary>
    public class PhotoUploadResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public PhotoUploadDataDTO Data { get; set; }
    }

    public class PhotoUploadDataDTO
    {
        public long PhotoId { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public PhotoMetadataDTO Metadata { get; set; }
        public List<string> AutoTags { get; set; }
        public string BlobUrl { get; set; }
        public string ThumbnailUrl { get; set; }
    }

    public class BatchUploadResponseDTO
    {
        public bool Success { get; set; }
        public int TotalFiles { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<BatchUploadResultDTO> Results { get; set; }
    }

    public class BatchUploadResultDTO
    {
        public string FileName { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
        public long? PhotoId { get; set; }
    }

    /// <summary>
    /// EXIF 測試回應 DTO
    /// </summary>
    public class ExifTestResponseDTO
    {
        public bool Success { get; set; }
        public PhotoMetadataDTO Metadata { get; set; }
    }

    /// <summary>
    /// 照片詳細資訊 DTO
    /// </summary>
    public class PhotoDetailDTO
    {
        public long PhotoId { get; set; }
        public long UserId { get; set; }
        public string FileName { get; set; }
        public string FileExtension { get; set; }
        public long FileSize { get; set; }
        public string BlobUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public DateTime UploadedAt { get; set; }

        // EXIF 元數據
        public PhotoMetadataDTO Metadata { get; set; }

        // 標籤
        public List<string> ExifTags { get; set; } = new List<string>();
        public List<string> ManualTags { get; set; } = new List<string>();
        public List<string> AiTags { get; set; } = new List<string>();
    }
}