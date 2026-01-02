namespace FrameZone_WebApi.Helpers
{
    /// <summary>
    /// 照片系統統一常數定義
    /// 集中管理所有魔術字串和配置參數，避免硬編碼
    /// 前後端應使用相同的配置值
    /// </summary>
    public static class PhotoConstants
    {
        #region 檔案上傳限制

        /// <summary>
        /// 單一檔案大小上限 (50 MB)
        /// 前端也應使用此值進行驗證
        /// </summary>
        public const long MAX_FILE_SIZE_BYTES = 50L * 1024 * 1024;

        /// <summary>
        /// 單一檔案大小上限 (MB) - 方便顯示用
        /// </summary>
        public const int MAX_FILE_SIZE_MB = 50;

        /// <summary>
        /// 單次批次上傳的檔案數量上限
        /// </summary>
        public const int MAX_BATCH_UPLOAD_COUNT = 50;

        /// <summary>
        /// 批次上傳總大小上限 (500 MB)
        /// </summary>
        public const long MAX_BATCH_TOTAL_SIZE_BYTES = 500L * 1024 * 1024;

        /// <summary>
        /// 允許的圖片副檔名
        /// </summary>
        public static readonly string[] ALLOWED_IMAGE_EXTENSIONS = new[]
        {
            ".jpg", ".jpeg", ".png", ".heic", ".gif", ".bmp", ".webp", ".tiff", ".raw"
        };

        /// <summary>
        /// 允許的視頻副檔名
        /// </summary>
        public static readonly string[] ALLOWED_VIDEO_EXTENSIONS = new[]
        {
            ".mp4", ".mov", ".avi", ".mkv", ".wmv", ".flv"
        };

        #endregion

        #region 縮圖生成配置

        /// <summary>
        /// 縮圖寬度 (像素)
        /// 前端卡片顯示建議使用相同尺寸
        /// </summary>
        public const int THUMBNAIL_WIDTH = 600;

        /// <summary>
        /// 縮圖高度 (像素)
        /// </summary>
        public const int THUMBNAIL_HEIGHT = 450;

        /// <summary>
        /// 縮圖 JPEG 品質 (0-100)
        /// 建議值: 85-95
        /// </summary>
        public const int THUMBNAIL_JPEG_QUALITY = 92;

        /// <summary>
        /// 縮圖銳化強度 (0.0-3.0)
        /// 建議值: 0.5-1.5
        /// </summary>
        public const float THUMBNAIL_SHARPEN_STRENGTH = 1.0f;

        /// <summary>
        /// 預覽圖寬度 (中等尺寸，用於燈箱預覽)
        /// </summary>
        public const int PREVIEW_WIDTH = 1200;

        /// <summary>
        /// 預覽圖高度
        /// </summary>
        public const int PREVIEW_HEIGHT = 900;

        /// <summary>
        /// 預覽圖 JPEG 品質
        /// </summary>
        public const int PREVIEW_JPEG_QUALITY = 90;

        #endregion

        #region 重試與超時配置

        /// <summary>
        /// 檔案處理最大重試次數
        /// </summary>
        public const int MAX_RETRY_COUNT = 3;

        /// <summary>
        /// 重試間隔基礎時間 (毫秒)
        /// 實際延遲 = BASE_RETRY_DELAY_MS * 重試次數
        /// </summary>
        public const int BASE_RETRY_DELAY_MS = 500;

        /// <summary>
        /// HTTP 請求超時時間 (秒)
        /// </summary>
        public const int HTTP_REQUEST_TIMEOUT_SECONDS = 30;

        /// <summary>
        /// 檔案上傳超時時間 (分鐘)
        /// </summary>
        public const int FILE_UPLOAD_TIMEOUT_MINUTES = 10;

        #endregion

        #region 快取配置

        /// <summary>
        /// 縮圖快取時間 (小時)
        /// </summary>
        public const int THUMBNAIL_CACHE_HOURS = 24;

        /// <summary>
        /// EXIF 元數據快取時間 (小時)
        /// </summary>
        public const int METADATA_CACHE_HOURS = 48;

        /// <summary>
        /// 地理編碼結果快取時間 (天)
        /// </summary>
        public const int GEOCODING_CACHE_DAYS = 30;

        /// <summary>
        /// 照片查詢結果快取時間 (分鐘)
        /// </summary>
        public const int QUERY_CACHE_MINUTES = 5;

        #endregion

        #region 分頁配置

        /// <summary>
        /// 預設每頁顯示數量
        /// 前端應使用相同值
        /// </summary>
        public const int DEFAULT_PAGE_SIZE = 20;

        /// <summary>
        /// 最大每頁顯示數量
        /// </summary>
        public const int MAX_PAGE_SIZE = 100;

        /// <summary>
        /// 最小每頁顯示數量
        /// </summary>
        public const int MIN_PAGE_SIZE = 1;

        #endregion

        #region SAS Token 配置

        /// <summary>
        /// SAS Token 預設有效時間 (分鐘)
        /// </summary>
        public const int SAS_TOKEN_DEFAULT_EXPIRY_MINUTES = 60;

        /// <summary>
        /// SAS Token 最大有效時間 (小時)
        /// </summary>
        public const int SAS_TOKEN_MAX_EXPIRY_HOURS = 24;

        /// <summary>
        /// SAS Token 提前開始時間 (分鐘)
        /// 避免時間差導致 Token 無效
        /// </summary>
        public const int SAS_TOKEN_START_OFFSET_MINUTES = 5;

        #endregion

        #region 並行處理配置

        /// <summary>
        /// Blob Storage 並行上傳數量上限
        /// </summary>
        public const int MAX_CONCURRENT_BLOB_UPLOADS = 5;

        /// <summary>
        /// 地理編碼並行處理數量上限
        /// </summary>
        public const int MAX_CONCURRENT_GEOCODING = 3;

        /// <summary>
        /// AI 分類並行處理數量上限
        /// </summary>
        public const int MAX_CONCURRENT_AI_PROCESSING = 2;

        #endregion

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
        /// 分類：用戶自定義（使用者建立的自訂標籤預設分類）
        /// </summary>
        public const string CATEGORY_USER_CUSTOM = "USER_CUSTOM";

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

        /// <summary>
        /// 標籤類型：使用者自訂標籤
        /// </summary>
        public const string TAG_TYPE_CUSTOM = "CUSTOM";

        #endregion

        #region 日期格式

        /// <summary>
        /// 年份標籤的正規表示式 (例如: 2025)
        /// </summary>
        public const string REGEX_YEAR = @"^\d{4}$";

        /// <summary>
        /// 標準日期格式
        /// </summary>
        public const string DATE_FORMAT = "yyyy-MM-dd";

        /// <summary>
        /// 標準日期時間格式
        /// </summary>
        public const string DATETIME_FORMAT = "yyyy-MM-dd HH:mm:ss";

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
            "DJI", "GoPro", "Apple", "Samsung", "Huawei", "Xiaomi"
        };

        #endregion

        #region 圖片品質設定

        /// <summary>
        /// 原圖 JPEG 品質（如果需要轉換格式）
        /// </summary>
        public const int ORIGINAL_JPEG_QUALITY = 95;

        /// <summary>
        /// WebP 格式品質
        /// </summary>
        public const int WEBP_QUALITY = 92;

        #endregion

        #region 錯誤訊息

        /// <summary>
        /// 檔案大小超過限制錯誤訊息
        /// </summary>
        public static string GetFileSizeExceededMessage()
            => $"檔案大小不能超過 {MAX_FILE_SIZE_MB} MB";

        /// <summary>
        /// 不支援的檔案格式錯誤訊息
        /// </summary>
        public static string GetUnsupportedFileFormatMessage()
            => $"不支援的檔案格式，僅支援: {string.Join(", ", ALLOWED_IMAGE_EXTENSIONS)}";

        /// <summary>
        /// 批次上傳數量超過限制錯誤訊息
        /// </summary>
        public static string GetBatchCountExceededMessage()
            => $"單次最多只能上傳 {MAX_BATCH_UPLOAD_COUNT} 個檔案";

        #endregion
    }

    /// <summary>
    /// Blob Storage 相關常數
    /// </summary>
    public static class BlobStorageConstants
    {
        /// <summary>
        /// Blob 存取層級：Hot（經常存取）
        /// </summary>
        public const string ACCESS_TIER_HOT = "Hot";

        /// <summary>
        /// Blob 存取層級：Cool（不常存取）
        /// </summary>
        public const string ACCESS_TIER_COOL = "Cool";

        /// <summary>
        /// Blob 存取層級：Archive（封存）
        /// </summary>
        public const string ACCESS_TIER_ARCHIVE = "Archive";

        /// <summary>
        /// 預設存取層級
        /// </summary>
        public const string DEFAULT_ACCESS_TIER = ACCESS_TIER_HOT;

        /// <summary>
        /// Cache-Control header 值（1年）
        /// </summary>
        public const string CACHE_CONTROL_ONE_YEAR = "public, max-age=31536000";

        /// <summary>
        /// 照片容器名稱
        /// </summary>
        public const string PHOTO_CONTAINER = "photos";

        /// <summary>
        /// 縮圖容器名稱
        /// </summary>
        public const string THUMBNAIL_CONTAINER = "thumbnails";
    }

    /// <summary>
    /// 地理編碼相關常數
    /// </summary>
    public static class GeocodingConstants
    {
        /// <summary>
        /// GPS 座標精度（小數位數）
        /// 4位小數 ≈ 11公尺精度
        /// </summary>
        public const int GPS_COORDINATE_PRECISION = 4;

        /// <summary>
        /// 預設語言
        /// </summary>
        public const string DEFAULT_LANGUAGE = "zh-TW";

        /// <summary>
        /// 地理編碼結果絕對過期時間（天）
        /// </summary>
        public const int CACHE_ABSOLUTE_EXPIRATION_DAYS = 90;

        /// <summary>
        /// 地理編碼結果滑動過期時間（天）
        /// </summary>
        public const int CACHE_SLIDING_EXPIRATION_DAYS = 30;
    }

    /// <summary>
    /// 使用者認證相關常數
    /// </summary>
    public static class AuthConstants
    {
        /// <summary>
        /// JWT Token 預設有效期（天）- 不記住我
        /// </summary>
        public const int JWT_EXPIRY_DAYS_DEFAULT = 1;

        /// <summary>
        /// JWT Token 延長有效期（天）- 記住我
        /// </summary>
        public const int JWT_EXPIRY_DAYS_REMEMBER = 7;

        /// <summary>
        /// 登入失敗鎖定次數
        /// </summary>
        public const int LOGIN_FAILED_LOCK_COUNT = 5;

        /// <summary>
        /// 帳號鎖定時間（分鐘）
        /// </summary>
        public const int ACCOUNT_LOCK_DURATION_MINUTES = 30;

        /// <summary>
        /// 密碼最小長度
        /// </summary>
        public const int PASSWORD_MIN_LENGTH = 8;

        /// <summary>
        /// 密碼最大長度
        /// </summary>
        public const int PASSWORD_MAX_LENGTH = 100;
    }

    /// <summary>
    /// Email 相關常數
    /// </summary>
    public static class EmailConstants
    {
        /// <summary>
        /// 驗證碼有效時間（分鐘）
        /// </summary>
        public const int VERIFICATION_CODE_EXPIRY_MINUTES = 15;

        /// <summary>
        /// 密碼重設連結有效時間（小時）
        /// </summary>
        public const int RESET_PASSWORD_EXPIRY_HOURS = 24;

        /// <summary>
        /// 驗證碼長度
        /// </summary>
        public const int VERIFICATION_CODE_LENGTH = 6;
    }
}