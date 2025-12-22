namespace FrameZone_WebApi.Helpers
{
    /// <summary>
    /// 照片系統統一常數定義
    /// 集中管理所有魔術字串，避免硬編碼
    /// </summary>
    public static class PhotoConstants
    {
        #region 分類來源 (PhotoClassificationSource)

        /// <summary>
        /// 分類來源：EXIF 自動分類
        /// </summary>
        public const string SOURCE_EXIF = "EXIF";

        /// <summary>
        /// 分類來源：使用者手動分類
        /// </summary>
        public const string SOURCE_MANUAL = "MANUAL";

        /// <summary>
        /// 分類來源：AI 圖像識別分類
        /// </summary>
        public const string SOURCE_AI = "AI";

        /// <summary>
        /// 分類來源：GPS 地理編碼
        /// </summary>
        public const string SOURCE_GEOCODING = "GEOCODING";

        #endregion

        #region 分類來源 ID (PhotoClassificationSource.SourceId)

        /// <summary>
        /// 分類來源 ID：EXIF 自動分類
        /// </summary>
        public const int SOURCE_ID_EXIF = 1;

        /// <summary>
        /// 分類來源 ID：使用者手動分類
        /// </summary>
        public const int SOURCE_ID_MANUAL = 2;

        /// <summary>
        /// 分類來源 ID：AI 圖像識別分類
        /// </summary>
        public const int SOURCE_ID_AI = 3;

        /// <summary>
        /// 分類來源 ID：GPS 地理編碼
        /// </summary>
        public const int SOURCE_ID_GEOCODING = 4;

        #endregion

        #region 分類類型 (PhotoCategory)

        /// <summary>
        /// 分類：時間
        /// </summary>
        public const string CATEGORY_TIME = "TIME";

        /// <summary>
        /// 分類：地點
        /// </summary>
        public const string CATEGORY_LOCATION = "LOCATION";

        /// <summary>
        /// 分類：相機
        /// </summary>
        public const string CATEGORY_CAMERA = "CAMERA";

        /// <summary>
        /// 分類：AI 識別（場景、內容）
        /// </summary>
        public const string CATEGORY_AI_DETECTION = "AI_DETECTION";

        /// <summary>
        /// 分類：一般（預設）
        /// </summary>
        public const string CATEGORY_GENERAL = "GENERAL";

        /// <summary>
        /// 分類：拍攝場景（系統管理員提供）
        /// </summary>
        public const string CATEGORY_SCENE = "SCENE";

        /// <summary>
        /// 分類：使用者自訂標籤
        /// </summary>
        public const string TAG_TYPE_CUSTOM = "CUSTOM";

        #endregion

        #region 標籤類型 (PhotoTag.TagType)

        /// <summary>
        /// 標籤類型：系統自動生成
        /// </summary>
        public const string TAG_TYPE_SYSTEM = "SYSTEM";

        /// <summary>
        /// 標籤類型：使用者自定義
        /// </summary>
        public const string TAG_TYPE_USER = "USER";

        /// <summary>
        /// 標籤類型：AI 識別
        /// </summary>
        public const string TAG_TYPE_AI = "AI";

        #endregion

        #region 檔案限制

        /// <summary>
        /// 允許的圖片副檔名
        /// </summary>
        public static readonly string[] ALLOWED_IMAGE_EXTENSIONS = new[]
        {
            ".jpg", ".jpeg", ".png", ".heic", ".gif", ".bmp", ".webp"
        };

        /// <summary>
        /// 單一檔案大小限制 (50MB)
        /// </summary>
        public const long MAX_FILE_SIZE_BYTES = 50 * 1024 * 1024;

        /// <summary>
        /// 檔案大小限制 (MB)
        /// </summary>
        public const int MAX_FILE_SIZE_MB = 50;

        #endregion

        #region 日期格式

        /// <summary>
        /// 年份標籤的正規表示式 (例如: 2025)
        /// </summary>
        public const string REGEX_YEAR = @"^\d{4}$";

        #endregion

        #region 相機品牌判斷

        /// <summary>
        /// 知名相機品牌列表（用於自動分類）
        /// 未來應移至資料庫管理，這裡僅作為備用方案
        /// </summary>
        public static readonly string[] CAMERA_BRANDS = new[]
        {
            "Canon", "Nikon", "Sony", "Fujifilm", "Panasonic",
            "Olympus", "Leica", "Pentax", "Hasselblad", "Phase One",
            "DJI", "GoPro", "Apple", "Samsung", "Huawei"
        };

        #endregion

        #region 查詢相關
        
        public const int MAX_PAGE_SIZE = 100;

        #endregion
    }

    public static class FileUploadConstants
    {
        public const long MAX_SINGLE_FILE_SIZE = 104_857_600;   // 100MB
        public const long MAX_BATCH_FILE_SIZE = 209_715_200;    // 200MB
        public const int THUMBNAIL_CACHE_SECONDS = 86400;       // 24小時
    }
}