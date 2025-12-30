namespace FrameZone_WebApi.Helpers
{
    /// <summary>
    /// 會員系統統一常數定義
    /// 集中管理所有會員相關的配置參數，避免硬編碼
    /// 前後端應使用相同的配置值
    /// </summary>
    public static class MemberConstants
    {
        #region 個人資料欄位長度限制

        /// <summary>
        /// 顯示名稱最大長度
        /// 前端對應: MemberConstants.DISPLAY_NAME_MAX_LENGTH
        /// </summary>
        public const int DISPLAY_NAME_MAX_LENGTH = 30;

        /// <summary>
        /// 個人簡介最大長度
        /// 前端對應: MemberConstants.BIO_MAX_LENGTH
        /// </summary>
        public const int BIO_MAX_LENGTH = 500;

        /// <summary>
        /// 個人網站 URL 最大長度
        /// 前端對應: MemberConstants.WEBSITE_MAX_LENGTH
        /// </summary>
        public const int WEBSITE_MAX_LENGTH = 255;

        /// <summary>
        /// 所在地最大長度
        /// 前端對應: MemberConstants.LOCATION_MAX_LENGTH
        /// </summary>
        public const int LOCATION_MAX_LENGTH = 100;

        /// <summary>
        /// 電話號碼最大長度
        /// 前端對應: MemberConstants.PHONE_MAX_LENGTH
        /// </summary>
        public const int PHONE_MAX_LENGTH = 50;

        /// <summary>
        /// 真實姓名最大長度
        /// 前端對應: MemberConstants.REAL_NAME_MAX_LENGTH
        /// </summary>
        public const int REAL_NAME_MAX_LENGTH = 100;

        /// <summary>
        /// 國家名稱最大長度
        /// 前端對應: MemberConstants.COUNTRY_MAX_LENGTH
        /// </summary>
        public const int COUNTRY_MAX_LENGTH = 50;

        /// <summary>
        /// 城市名稱最大長度
        /// 前端對應: MemberConstants.CITY_MAX_LENGTH
        /// </summary>
        public const int CITY_MAX_LENGTH = 100;

        /// <summary>
        /// 郵遞區號最大長度
        /// 前端對應: MemberConstants.POSTAL_CODE_MAX_LENGTH
        /// </summary>
        public const int POSTAL_CODE_MAX_LENGTH = 20;

        /// <summary>
        /// 完整地址最大長度
        /// 前端對應: MemberConstants.FULL_ADDRESS_MAX_LENGTH
        /// </summary>
        public const int FULL_ADDRESS_MAX_LENGTH = 200;

        #endregion

        #region 圖片上傳限制

        /// <summary>
        /// 大頭貼檔案大小上限 (5 MB)
        /// 前端對應: MemberConstants.AVATAR_MAX_SIZE_BYTES
        /// </summary>
        public const long AVATAR_MAX_SIZE_BYTES = 5L * 1024 * 1024;

        /// <summary>
        /// 大頭貼檔案大小上限 (MB) - 方便顯示用
        /// </summary>
        public const int AVATAR_MAX_SIZE_MB = 5;

        /// <summary>
        /// 封面圖片檔案大小上限 (10 MB)
        /// 前端對應: MemberConstants.COVER_IMAGE_MAX_SIZE_BYTES
        /// </summary>
        public const long COVER_IMAGE_MAX_SIZE_BYTES = 10L * 1024 * 1024;

        /// <summary>
        /// 封面圖片檔案大小上限 (MB) - 方便顯示用
        /// </summary>
        public const int COVER_IMAGE_MAX_SIZE_MB = 10;

        /// <summary>
        /// 允許的圖片副檔名
        /// 前端對應: MemberConstants.ALLOWED_IMAGE_EXTENSIONS
        /// </summary>
        public static readonly string[] ALLOWED_IMAGE_EXTENSIONS = new[]
        {
            ".jpg", ".jpeg", ".png", ".heic", ".gif", ".bmp", ".webp", ".tiff", ".raw"
        };

        #endregion

        #region 圖片尺寸配置

        /// <summary>
        /// 大頭貼縮圖寬度 (像素)
        /// </summary>
        public const int AVATAR_THUMBNAIL_WIDTH = 200;

        /// <summary>
        /// 大頭貼縮圖高度 (像素)
        /// </summary>
        public const int AVATAR_THUMBNAIL_HEIGHT = 200;

        /// <summary>
        /// 封面圖片縮圖寬度 (像素)
        /// </summary>
        public const int COVER_THUMBNAIL_WIDTH = 1200;

        /// <summary>
        /// 封面圖片縮圖高度 (像素)
        /// </summary>
        public const int COVER_THUMBNAIL_HEIGHT = 400;

        /// <summary>
        /// 圖片 JPEG 品質 (0-100)
        /// </summary>
        public const int IMAGE_JPEG_QUALITY = 90;

        #endregion

        #region 性別選項

        /// <summary>
        /// 性別：男性
        /// </summary>
        public const string GENDER_MALE = "Male";

        /// <summary>
        /// 性別：女性
        /// </summary>
        public const string GENDER_FEMALE = "Female";

        /// <summary>
        /// 性別：其他
        /// </summary>
        public const string GENDER_OTHER = "Other";

        /// <summary>
        /// 性別：不願透露
        /// </summary>
        public const string GENDER_PREFER_NOT_TO_SAY = "PreferNotToSay";

        /// <summary>
        /// 允許的性別值
        /// </summary>
        public static readonly string[] ALLOWED_GENDERS = new[]
        {
            GENDER_MALE,
            GENDER_FEMALE,
            GENDER_OTHER,
            GENDER_PREFER_NOT_TO_SAY
        };

        #endregion

        #region Blob Storage 路徑配置

        /// <summary>
        /// 大頭貼存儲容器名稱
        /// </summary>
        public const string AVATAR_CONTAINER = "avatars";

        /// <summary>
        /// 封面圖片存儲容器名稱
        /// </summary>
        public const string COVER_IMAGE_CONTAINER = "covers";

        /// <summary>
        /// 大頭貼檔名前綴
        /// </summary>
        public const string AVATAR_FILE_PREFIX = "avatar_";

        /// <summary>
        /// 封面圖片檔名前綴
        /// </summary>
        public const string COVER_IMAGE_FILE_PREFIX = "cover_";

        #endregion

        #region 快取配置

        /// <summary>
        /// 個人資料快取時間 (分鐘)
        /// </summary>
        public const int PROFILE_CACHE_MINUTES = 30;

        /// <summary>
        /// 個人資料快取鍵前綴
        /// </summary>
        public const string PROFILE_CACHE_KEY_PREFIX = "UserProfile_";

        #endregion

        #region 驗證規則

        /// <summary>
        /// URL 正規表示式（簡化版）
        /// </summary>
        public const string URL_REGEX = @"^https?://[\w\-]+(\.[\w\-]+)+[/#?]?.*$";

        /// <summary>
        /// 電話號碼正規表示式（允許國際格式）
        /// 允許格式：+886-2-1234-5678, 0912-345-678, (02)1234-5678 等
        /// </summary>
        public const string PHONE_REGEX = @"^[\d\s\-\+\(\)]+$";

        #endregion

        #region 錯誤訊息

        /// <summary>
        /// 大頭貼檔案大小超過限制錯誤訊息
        /// </summary>
        public static string GetAvatarSizeExceededMessage()
            => $"大頭貼大小不能超過 {AVATAR_MAX_SIZE_MB} MB";

        /// <summary>
        /// 封面圖片檔案大小超過限制錯誤訊息
        /// </summary>
        public static string GetCoverImageSizeExceededMessage()
            => $"封面圖片大小不能超過 {COVER_IMAGE_MAX_SIZE_MB} MB";

        /// <summary>
        /// 不支援的圖片格式錯誤訊息
        /// </summary>
        public static string GetUnsupportedImageFormatMessage()
            => $"不支援的圖片格式，僅支援: {string.Join(", ", ALLOWED_IMAGE_EXTENSIONS)}";

        /// <summary>
        /// 性別值無效錯誤訊息
        /// </summary>
        public static string GetInvalidGenderMessage()
            => $"性別值無效，僅支援: {string.Join(", ", ALLOWED_GENDERS)}";

        /// <summary>
        /// 欄位長度超過限制錯誤訊息
        /// </summary>
        public static string GetFieldLengthExceededMessage(string fieldName, int maxLength)
            => $"{fieldName} 長度不能超過 {maxLength} 個字元";

        /// <summary>
        /// URL 格式錯誤訊息
        /// </summary>
        public static string GetInvalidUrlMessage()
            => "網站 URL 格式不正確，必須以 http:// 或 https:// 開頭";

        /// <summary>
        /// 電話號碼格式錯誤訊息
        /// </summary>
        public static string GetInvalidPhoneMessage()
            => "電話號碼格式不正確";

        #endregion

        #region 輔助方法

        /// <summary>
        /// 驗證性別值是否有效
        /// </summary>
        public static bool IsValidGender(string? gender)
        {
            if (string.IsNullOrWhiteSpace(gender))
                return true; // 允許為空

            return ALLOWED_GENDERS.Contains(gender);
        }

        /// <summary>
        /// 驗證圖片副檔名是否有效
        /// </summary>
        public static bool IsValidImageExtension(string fileName)
        {
            var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
            return !string.IsNullOrEmpty(extension) && ALLOWED_IMAGE_EXTENSIONS.Contains(extension);
        }

        /// <summary>
        /// 生成大頭貼檔名
        /// </summary>
        public static string GenerateAvatarFileName(long userId, string extension)
        {
            return $"{AVATAR_FILE_PREFIX}{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
        }

        /// <summary>
        /// 生成封面圖片檔名
        /// </summary>
        public static string GenerateCoverImageFileName(long userId, string extension)
        {
            return $"{COVER_IMAGE_FILE_PREFIX}{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
        }

        /// <summary>
        /// 取得個人資料快取鍵
        /// </summary>
        public static string GetProfileCacheKey(long userId)
        {
            return $"{PROFILE_CACHE_KEY_PREFIX}{userId}";
        }

        #endregion
    }

    /// <summary>
    /// 帳號安全相關常數
    /// </summary>
    public static class SecurityConstants
    {
        #region 密碼規則

        /// <summary>
        /// 密碼最小長度
        /// 前端對應: SecurityConstants.PASSWORD_MIN_LENGTH
        /// </summary>
        public const int PASSWORD_MIN_LENGTH = 8;

        /// <summary>
        /// 密碼最大長度
        /// 前端對應: SecurityConstants.PASSWORD_MAX_LENGTH
        /// </summary>
        public const int PASSWORD_MAX_LENGTH = 100;

        /// <summary>
        /// 是否要求密碼包含大寫字母
        /// </summary>
        public const bool PASSWORD_REQUIRE_UPPERCASE = true;

        /// <summary>
        /// 是否要求密碼包含小寫字母
        /// </summary>
        public const bool PASSWORD_REQUIRE_LOWERCASE = true;

        /// <summary>
        /// 是否要求密碼包含數字
        /// </summary>
        public const bool PASSWORD_REQUIRE_DIGIT = true;

        /// <summary>
        /// 是否要求密碼包含特殊字元
        /// </summary>
        public const bool PASSWORD_REQUIRE_SPECIAL_CHAR = true;

        #endregion

        #region 登入安全

        /// <summary>
        /// 登入失敗鎖定次數
        /// </summary>
        public const int LOGIN_FAILED_LOCK_COUNT = 5;

        /// <summary>
        /// 帳號鎖定時間（分鐘）
        /// </summary>
        public const int ACCOUNT_LOCK_DURATION_MINUTES = 30;

        #endregion

        #region OTP/2FA

        /// <summary>
        /// OTP 密鑰長度
        /// </summary>
        public const int OTP_SECRET_LENGTH = 32;

        /// <summary>
        /// OTP 驗證碼有效時間（秒）
        /// </summary>
        public const int OTP_VALIDITY_SECONDS = 30;

        #endregion

        #region Session

        /// <summary>
        /// Session 超時時間（分鐘）
        /// </summary>
        public const int SESSION_TIMEOUT_MINUTES = 30;

        /// <summary>
        /// 同時登入裝置數量上限
        /// </summary>
        public const int MAX_CONCURRENT_SESSIONS = 5;

        #endregion
    }

    /// <summary>
    /// 通知相關常數
    /// </summary>
    public static class NotificationConstants
    {
        /// <summary>
        /// 通知類型：Email 通知
        /// </summary>
        public const string TYPE_EMAIL = "Email";

        /// <summary>
        /// 通知類型：簡訊通知
        /// </summary>
        public const string TYPE_SMS = "SMS";

        /// <summary>
        /// 通知類型：推播通知
        /// </summary>
        public const string TYPE_PUSH = "Push";

        /// <summary>
        /// 通知類型：站內通知
        /// </summary>
        public const string TYPE_IN_APP = "InApp";
    }
}