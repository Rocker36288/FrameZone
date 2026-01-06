namespace FrameZone_WebApi.Helpers
{
    /// <summary>
    /// 通知系統常數定義
    /// </summary>
    public static class NotificationConstant
    {
        /// <summary>
        /// 系統模組代碼
        /// </summary>
        public static class SystemCodes
        {
            public const string PHOTO = "PHOTO";
            public const string SHOPPING = "SHOPPING";
            public const string SOCIAL = "SOCIAL";
            public const string VIDEO = "VIDEO";
            public const string PHOTOGRAPHER = "PHOTOGRAPHER";
        }

        /// <summary>
        /// 通知類別代碼範例（依實際需求調整）
        /// </summary>
        public static class CategoryCodes
        {
            // Photo 系統
            public const string PHOTO_UPLOAD = "PHOTO_UPLOAD";
            public const string PHOTO_SHARED = "PHOTO_SHARED";
            public const string PHOTO_CLASSIFIED = "PHOTO_CLASSIFIED";

            // Shopping 系統
            public const string ORDER_CREATED = "ORDER_CREATED";
            public const string ORDER_SHIPPED = "ORDER_SHIPPED";
            public const string ORDER_DELIVERED = "ORDER_DELIVERED";
            public const string ORDER_CANCELLED = "ORDER_CANCELLED";

            // Social 系統
            public const string POST_LIKED = "POST_LIKED";
            public const string POST_COMMENTED = "POST_COMMENTED";
            public const string POST_SHARED = "POST_SHARED";
            public const string NEW_FOLLOWER = "NEW_FOLLOWER";

            // Video 系統
            public const string VIDEO_UPLOADED = "VIDEO_UPLOADED";
            public const string VIDEO_LIKED = "VIDEO_LIKED";
            public const string VIDEO_COMMENTED = "VIDEO_COMMENTED";

            // Photographer 系統
            public const string BOOKING_CONFIRMED = "BOOKING_CONFIRMED";
            public const string BOOKING_CANCELLED = "BOOKING_CANCELLED";
            public const string BOOKING_REMINDER = "BOOKING_REMINDER";

            // 系統通用
            public const string SYSTEM_ANNOUNCEMENT = "SYSTEM_ANNOUNCEMENT";
            public const string ACCOUNT_SECURITY = "ACCOUNT_SECURITY";
        }

        /// <summary>
        /// 優先級代碼
        /// </summary>
        public static class PriorityCodes
        {
            public const string HIGH = "HIGH";
            public const string MEDIUM = "MEDIUM";
            public const string LOW = "LOW";
        }

        /// <summary>
        /// 預設分頁設定
        /// </summary>
        public static class Pagination
        {
            public const int DEFAULT_PAGE_SIZE = 20;
            public const int MAX_PAGE_SIZE = 100;
            public const int BELL_DROPDOWN_SIZE = 20; // 小鈴噹下拉選單顯示數量
        }

        /// <summary>
        /// 通知過期時間（天）
        /// </summary>
        public static class Expiration
        {
            public const int DEFAULT_DAYS = 30;
            public const int SYSTEM_ANNOUNCEMENT_DAYS = 90;
        }

        /// <summary>
        /// 系統模組中文名稱對應
        /// </summary>
        public static readonly Dictionary<string, string> SystemNames = new()
        {
            { SystemCodes.PHOTO, "照片系統" },
            { SystemCodes.SHOPPING, "購物系統" },
            { SystemCodes.SOCIAL, "社群系統" },
            { SystemCodes.VIDEO, "影片系統" },
            { SystemCodes.PHOTOGRAPHER, "攝影師預約" }
        };

        /// <summary>
        /// 通知類型圖示對應（前端可用）
        /// </summary>
        public static readonly Dictionary<string, string> CategoryIcons = new()
        {
            // Photo
            { CategoryCodes.PHOTO_UPLOAD, "📷" },
            { CategoryCodes.PHOTO_SHARED, "🔗" },
            { CategoryCodes.PHOTO_CLASSIFIED, "🏷️" },
            
            // Shopping
            { CategoryCodes.ORDER_CREATED, "🛒" },
            { CategoryCodes.ORDER_SHIPPED, "📦" },
            { CategoryCodes.ORDER_DELIVERED, "✅" },
            { CategoryCodes.ORDER_CANCELLED, "❌" },
            
            // Social
            { CategoryCodes.POST_LIKED, "❤️" },
            { CategoryCodes.POST_COMMENTED, "💬" },
            { CategoryCodes.POST_SHARED, "🔄" },
            { CategoryCodes.NEW_FOLLOWER, "👥" },
            
            // Video
            { CategoryCodes.VIDEO_UPLOADED, "🎬" },
            { CategoryCodes.VIDEO_LIKED, "👍" },
            { CategoryCodes.VIDEO_COMMENTED, "💭" },
            
            // Photographer
            { CategoryCodes.BOOKING_CONFIRMED, "📅" },
            { CategoryCodes.BOOKING_CANCELLED, "🚫" },
            { CategoryCodes.BOOKING_REMINDER, "⏰" },
            
            // System
            { CategoryCodes.SYSTEM_ANNOUNCEMENT, "📢" },
            { CategoryCodes.ACCOUNT_SECURITY, "🛡️" }
        };
    }
}