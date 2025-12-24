using System;
using System.Collections.Generic;

namespace FrameZone_WebApi.DTOs
{
    #region 照片查詢相關 DTO

    /// <summary>
    /// 照片查詢請求 DTO
    /// 用於前端 Sidebar 篩選條件的傳遞
    /// </summary>
    public class PhotoQueryRequestDTO
    {
        #region 分頁參數

        /// <summary>
        /// 頁碼（從 1 開始）
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// 每頁筆數
        /// </summary>
        public int PageSize { get; set; } = 20;

        #endregion

        #region 時間篩選

        /// <summary>
        /// 開始日期（拍攝時間）
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 結束日期（拍攝時間）
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// 年份列表（例如：2025, 2024）
        /// 多選支援
        /// </summary>
        public List<int>? Years { get; set; }

        /// <summary>
        /// 月份列表（例如：1, 2, 12）
        /// 多選支援，需搭配 Years 使用
        /// </summary>
        public List<int>? Months { get; set; }

        #endregion

        #region 分類與標籤篩選

        /// <summary>
        /// 分類 ID 列表
        /// 多選支援（例如：[1, 2, 3] 代表選擇多個分類）
        /// </summary>
        public List<int>? CategoryIds { get; set; }

        /// <summary>
        /// 標籤 ID 列表
        /// 多選支援（例如：[10, 20, 30]）
        /// </summary>
        public List<int>? TagIds { get; set; }

        /// <summary>
        /// 標籤名稱列表（供關鍵字搜尋使用）
        /// 例如：["Canon", "2025", "台北市"]
        /// </summary>
        public List<string>? TagNames { get; set; }

        #endregion

        #region 地點篩選

        /// <summary>
        /// 國家（例如：台灣）
        /// </summary>
        public string? Country { get; set; }

        /// <summary>
        /// 城市（例如：台北市）
        /// </summary>
        public string? City { get; set; }

        /// <summary>
        /// 行政區（例如：大安區）
        /// </summary>
        public string? District { get; set; }

        /// <summary>
        /// 地點名稱（例如：台北101）
        /// </summary>
        public string? PlaceName { get; set; }

        /// <summary>
        /// 是否只查詢有 GPS 資訊的照片
        /// </summary>
        public bool? HasLocation { get; set; }

        #endregion

        #region 相機篩選

        /// <summary>
        /// 相機品牌（例如：Canon）
        /// </summary>
        public string? CameraMake { get; set; }

        /// <summary>
        /// 相機型號（例如：EOS R5）
        /// </summary>
        public string? CameraModel { get; set; }

        /// <summary>
        /// 鏡頭型號（例如：RF 24-70mm F2.8）
        /// </summary>
        public string? LensModel { get; set; }

        #endregion

        #region 拍攝參數篩選

        /// <summary>
        /// 最小 ISO 值
        /// </summary>
        public int? MinISO { get; set; }

        /// <summary>
        /// 最大 ISO 值
        /// </summary>
        public int? MaxISO { get; set; }

        /// <summary>
        /// 最小光圈值（例如：1.4）
        /// </summary>
        public decimal? MinAperture { get; set; }

        /// <summary>
        /// 最大光圈值（例如：22）
        /// </summary>
        public decimal? MaxAperture { get; set; }

        /// <summary>
        /// 最小焦距（mm）
        /// </summary>
        public decimal? MinFocalLength { get; set; }

        /// <summary>
        /// 最大焦距（mm）
        /// </summary>
        public decimal? MaxFocalLength { get; set; }

        #endregion

        #region 檔案屬性篩選

        /// <summary>
        /// 檔案名稱關鍵字（模糊搜尋）
        /// </summary>
        public string? FileNameKeyword { get; set; }

        /// <summary>
        /// 最小檔案大小（bytes）
        /// </summary>
        public long? MinFileSize { get; set; }

        /// <summary>
        /// 最大檔案大小（bytes）
        /// </summary>
        public long? MaxFileSize { get; set; }

        /// <summary>
        /// 檔案副檔名（例如：jpg, png）
        /// </summary>
        public List<string>? FileExtensions { get; set; }

        #endregion

        #region 排序參數

        /// <summary>
        /// 排序欄位
        /// 可選值：DateTaken, UploadedAt, FileName, FileSize
        /// </summary>
        public string SortBy { get; set; } = "DateTaken";

        /// <summary>
        /// 排序方向
        /// 可選值：asc（升冪）, desc（降冪）
        /// </summary>
        public string SortOrder { get; set; } = "desc";

        #endregion

        #region 進階篩選

        /// <summary>
        /// 是否只查詢有 EXIF 資訊的照片
        /// </summary>
        public bool? HasExif { get; set; }

        /// <summary>
        /// 是否只查詢已分類的照片
        /// </summary>
        public bool? IsCategorized { get; set; }

        /// <summary>
        /// 是否只查詢有標籤的照片
        /// </summary>
        public bool? HasTags { get; set; }

        /// <summary>
        /// 上傳時間範圍 - 開始
        /// </summary>
        public DateTime? UploadedAfter { get; set; }

        /// <summary>
        /// 上傳時間範圍 - 結束
        /// </summary>
        public DateTime? UploadedBefore { get; set; }

        #endregion
    }

    /// <summary>
    /// 照片查詢回應 DTO
    /// </summary>
    public class PhotoQueryResponseDTO
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// 錯誤訊息（如果有）
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 照片列表
        /// </summary>
        public List<PhotoItemDTO> Photos { get; set; } = new List<PhotoItemDTO>();

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
        public bool HasPreviousPage { get; set; }

        /// <summary>
        /// 是否有下一頁
        /// </summary>
        public bool HasNextPage { get; set; }

        /// <summary>
        /// 查詢執行時間（毫秒）
        /// </summary>
        public long ExecutionTimeMs { get; set; }
    }

    /// <summary>
    /// 照片項目 DTO（用於列表顯示）
    /// </summary>
    public class PhotoItemDTO
    {
        #region 基本資訊

        /// <summary>
        /// 照片 ID
        /// </summary>
        public long PhotoId { get; set; }

        /// <summary>
        /// 檔案名稱
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 檔案副檔名
        /// </summary>
        public string FileExtension { get; set; }

        /// <summary>
        /// 檔案大小（bytes）
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// 縮圖 URL
        /// TODO: 之後實作縮圖功能
        /// </summary>
        public string ThumbnailUrl { get; set; }

        /// <summary>
        /// 預覽 URL
        /// TODO: 之後實作預覽功能
        /// </summary>
        public string PreviewUrl { get; set; }

        /// <summary>
        /// 原圖 URL（帶 SAS Token，從 Blob Storage 取得）
        /// 前端可直接使用此 URL 顯示原圖
        /// </summary>
        public string PhotoUrl { get; set; }

        #endregion

        #region 時間資訊

        /// <summary>
        /// 拍攝時間
        /// </summary>
        public DateTime? DateTaken { get; set; }

        /// <summary>
        /// 上傳時間
        /// </summary>
        public DateTime UploadedAt { get; set; }

        #endregion

        #region 地點資訊

        /// <summary>
        /// 地點描述（例如：台灣 - 台北市 - 大安區）
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// 國家
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// 行政區
        /// </summary>
        public string District { get; set; }

        /// <summary>
        /// 地點名稱
        /// </summary>
        public string PlaceName { get; set; }

        /// <summary>
        /// GPS 經度
        /// </summary>
        public decimal? Latitude { get; set; }

        /// <summary>
        /// GPS 緯度
        /// </summary>
        public decimal? Longitude { get; set; }

        #endregion

        #region 相機資訊

        /// <summary>
        /// 相機描述（例如：Canon - EOS R5）
        /// </summary>
        public string Camera { get; set; }

        /// <summary>
        /// 相機品牌
        /// </summary>
        public string CameraMake { get; set; }

        /// <summary>
        /// 相機型號
        /// </summary>
        public string CameraModel { get; set; }

        /// <summary>
        /// 鏡頭型號
        /// </summary>
        public string LensModel { get; set; }

        #endregion

        #region 拍攝參數

        /// <summary>
        /// ISO 值
        /// </summary>
        public int? ISO { get; set; }

        /// <summary>
        /// 光圈值
        /// </summary>
        public decimal? Aperture { get; set; }

        /// <summary>
        /// 快門速度
        /// </summary>
        public string ShutterSpeed { get; set; }

        /// <summary>
        /// 焦距
        /// </summary>
        public decimal? FocalLength { get; set; }

        #endregion

        #region 標籤與分類

        /// <summary>
        /// 所有標籤名稱列表
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// EXIF 標籤列表（系統自動生成）
        /// </summary>
        public List<string> ExifTags { get; set; } = new List<string>();

        /// <summary>
        /// 手動標籤列表
        /// </summary>
        public List<string> ManualTags { get; set; } = new List<string>();

        /// <summary>
        /// AI 標籤列表
        /// </summary>
        public List<string> AiTags { get; set; } = new List<string>();

        /// <summary>
        /// 所屬分類列表
        /// </summary>
        public List<string> Categories { get; set; } = new List<string>();

        #endregion

        #region 圖片尺寸

        /// <summary>
        /// 圖片寬度（像素）
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// 圖片高度（像素）
        /// </summary>
        public int? Height { get; set; }

        #endregion
    }

    #endregion
}