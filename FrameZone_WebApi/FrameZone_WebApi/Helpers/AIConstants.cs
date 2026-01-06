namespace FrameZone_WebApi.Constants
{
    /// <summary>
    /// AI 服務常數
    /// </summary>
    public static class AIConstants
    {
        #region === Azure Computer Vision ===

        public static class Azure
        {
            /// <summary>
            /// 設定檔 Key（appsettings.json）
            /// </summary>
            public const string ConfigSection = "AIServices:Azure";
            public const string EndpointKey = "AIServices:Azure:Endpoint";
            public const string ApiKeyKey = "AIServices:Azure:ApiKey";
            public const string RegionKey = "AIServices:Azure:Region";

            /// <summary>
            /// API 端點路徑
            /// </summary>
            public const string AnalyzeEndpoint = "vision/v4.0/analyze";

            /// <summary>
            /// API 版本
            /// </summary>
            public const string ApiVersion = "2024-02-01";

            /// <summary>
            /// 分析功能（Features）
            /// </summary>
            public static class Features
            {
                public const string Objects = "objects";
                public const string Tags = "tags";
                public const string Description = "description";
                public const string Categories = "categories";
                public const string Adult = "adult";
                public const string Color = "color";
                public const string ImageType = "imageType";
                public const string Faces = "faces";
                public const string Brands = "brands";
            }

            /// <summary>
            /// 模型版本
            /// </summary>
            public const string ModelVersion = "2024-02-01";

            /// <summary>
            /// Provider 識別
            /// </summary>
            public const string ProviderName = "Azure Computer Vision";

            /// <summary>
            /// 限制
            /// </summary>
            public static class Limits
            {
                public const int MaxImageSizeMB = 4;
                public const int MaxImageDimensionPx = 10000;
                public const int RequestsPerSecond = 10;
                public const int RequestsPerMinute = 600;
            }

            /// <summary>
            /// 重試設定
            /// </summary>
            public static class Retry
            {
                public const int MaxRetries = 3;
                public const int DelayMilliseconds = 1000;
                public const int MaxDelayMilliseconds = 5000;
            }
        }

        #endregion

        #region === Google Places ===

        public static class Google
        {
            /// <summary>
            /// 設定檔 Key
            /// </summary>
            public const string ConfigSection = "AIServices:Google";
            public const string ApiKeyKey = "AIServices:Google:ApiKey";

            /// <summary>
            /// API 端點
            /// </summary>
            public const string BaseUrl = "https://maps.googleapis.com/maps/api/place";
            public const string NearbySearchEndpoint = "nearbysearch/json";
            public const string PlaceDetailsEndpoint = "details/json";

            /// <summary>
            /// Provider 識別
            /// </summary>
            public const string ProviderName = "Google Places";

            /// <summary>
            /// 搜尋參數
            /// </summary>
            public static class Search
            {
                /// <summary>
                /// 預設搜尋半徑（公尺）
                /// </summary>
                public const int DefaultRadius = 500;

                /// <summary>
                /// 最大搜尋半徑（公尺）
                /// </summary>
                public const int MaxRadius = 50000;

                /// <summary>
                /// 最大結果數（rankby=distance 時不能設定 radius）
                /// </summary>
                public const int MaxResults = 20;

                /// <summary>
                /// 排序方式
                /// </summary>
                public const string RankByProminence = "prominence";
                public const string RankByDistance = "distance";
            }

            /// <summary>
            /// 景點類型（用於判斷是否為旅遊景點）
            /// </summary>
            public static class TouristSpotTypes
            {
                public static readonly HashSet<string> Types = new()
                {
                    "tourist_attraction",
                    "museum",
                    "art_gallery",
                    "amusement_park",
                    "aquarium",
                    "zoo",
                    "park",
                    "natural_feature",
                    "point_of_interest",
                    "place_of_worship",
                    "castle",
                    "church",
                    "hindu_temple",
                    "mosque",
                    "synagogue"
                };
            }

            /// <summary>
            /// 限制
            /// </summary>
            public static class Limits
            {
                public const int RequestsPerSecond = 10;
                public const int RequestsPerDay = 100000;
            }

            /// <summary>
            /// 重試設定
            /// </summary>
            public static class Retry
            {
                public const int MaxRetries = 3;
                public const int DelayMilliseconds = 500;
            }
        }

        #endregion

        #region === Claude (Anthropic) ===

        public static class Claude
        {
            /// <summary>
            /// 設定檔 Key
            /// </summary>
            public const string ConfigSection = "AIServices:Claude";
            public const string ApiKeyKey = "AIServices:Claude:ApiKey";
            public const string BaseUrlKey = "AIServices:Claude:BaseUrl";

            /// <summary>
            /// API 端點
            /// </summary>
            public const string BaseUrl = "https://api.anthropic.com/v1/";
            public const string MessagesEndpoint = "messages";

            /// <summary>
            /// HTTP Headers
            /// </summary>
            public const string HeaderApiKey = "x-api-key";
            public const string HeaderAnthropicVersion = "anthropic-version";
            public const string HeaderContentType = "content-type";

            /// <summary>
            /// API 版本
            /// </summary>
            public const string ApiVersion = "2023-06-01";

            /// <summary>
            /// Provider 識別
            /// </summary>
            public const string ProviderName = "Claude";

            /// <summary>
            /// 模型
            /// </summary>
            public static class Models
            {
                public const string Sonnet4 = "claude-sonnet-4-20250514";
                public const string Opus4 = "claude-opus-4-20250514";
                public const string Haiku4 = "claude-haiku-4-20250514";

                /// <summary>
                /// 預設使用的模型
                /// </summary>
                public const string Default = Sonnet4;
            }

            /// <summary>
            /// 訊息角色（用於對話訊息）
            /// </summary>
            public static class Role
            {
                /// <summary>
                /// 使用者角色（發送給 Claude 的訊息）
                /// </summary>
                public const string User = "user";

                /// <summary>
                /// 助手角色（Claude 的回應）
                /// </summary>
                public const string Assistant = "assistant";
            }

            /// <summary>
            /// 內容區塊類型（Content Block Types）
            /// </summary>
            public static class ContentType
            {
                /// <summary>
                /// 文字內容
                /// </summary>
                public const string Text = "text";

                /// <summary>
                /// 圖片內容
                /// </summary>
                public const string Image = "image";

                /// <summary>
                /// 思考過程（Extended Thinking）
                /// </summary>
                public const string Thinking = "thinking";

                /// <summary>
                /// 工具使用（Function Calling）
                /// </summary>
                public const string ToolUse = "tool_use";

                /// <summary>
                /// 工具結果
                /// </summary>
                public const string ToolResult = "tool_result";
            }

            /// <summary>
            /// 圖片來源類型（Image Source Types）
            /// </summary>
            public static class ImageSourceType
            {
                /// <summary>
                /// Base64 編碼的圖片
                /// </summary>
                public const string Base64 = "base64";

                /// <summary>
                /// URL 圖片（未來可能支援）
                /// </summary>
                public const string Url = "url";
            }

            /// <summary>
            /// 圖片媒體類型（Image Media Types）
            /// </summary>
            public static class ImageMediaType
            {
                /// <summary>
                /// JPEG 圖片
                /// </summary>
                public const string Jpeg = "image/jpeg";

                /// <summary>
                /// PNG 圖片
                /// </summary>
                public const string Png = "image/png";

                /// <summary>
                /// GIF 圖片
                /// </summary>
                public const string Gif = "image/gif";

                /// <summary>
                /// WebP 圖片
                /// </summary>
                public const string WebP = "image/webp";
            }

            /// <summary>
            /// Token 限制
            /// </summary>
            public static class TokenLimits
            {
                public const int Sonnet4MaxOutput = 8192;
                public const int Opus4MaxOutput = 8192;
                public const int Haiku4MaxOutput = 8192;

                /// <summary>
                /// 預設最大輸出 Token
                /// </summary>
                public const int DefaultMaxTokens = 2048;
            }

            /// <summary>
            /// 預設參數
            /// </summary>
            public static class Defaults
            {
                public const double Temperature = 0.7;
                public const double TopP = 0.9;
                public const int TopK = 40;
            }

            /// <summary>
            /// 限制
            /// </summary>
            public static class Limits
            {
                public const int RequestsPerMinute = 50;
                public const int TokensPerMinute = 40000;
            }

            /// <summary>
            /// 重試設定
            /// </summary>
            public static class Retry
            {
                public const int MaxRetries = 3;
                public const int DelayMilliseconds = 2000;
                public const int MaxDelayMilliseconds = 10000;
            }

            /// <summary>
            /// Prompt 模板識別
            /// </summary>
            public static class PromptTemplates
            {
                public const string TouristSpotAnalysis = "tourist_spot_analysis_v1";
                public const string ImageSemanticAnalysis = "image_semantic_v1";
            }
        }

        #endregion

        #region === AI 分析通用常數 ===

        public static class Analysis
        {
            /// <summary>
            /// 分析狀態
            /// </summary>
            public static class Status
            {
                public const string Success = "Success";
                public const string Failed = "Failed";
                public const string Pending = "Pending";
                public const string Processing = "Processing";
            }

            /// <summary>
            /// 分類來源（SourceId，對應 PhotoClassificationSource）
            /// </summary>
            public static class SourceId
            {
                public const int EXIF = 1;
                public const int Manual = 2;
                public const int AI = 3;
            }

            /// <summary>
            /// AI 提供者
            /// </summary>
            public static class Provider
            {
                public const string Azure = "Azure";
                public const string Google = "Google";
                public const string Claude = "Claude";
                public const string Combined = "Combined";
            }

            /// <summary>
            /// 信心分數閾值
            /// </summary>
            public static class ConfidenceThreshold
            {
                /// <summary>
                /// 最低信心分數（預設）
                /// </summary>
                public const double Minimum = 0.9;

                /// <summary>
                /// 高信心分數
                /// </summary>
                public const double High = 0.95;

                /// <summary>
                /// 中等信心分數
                /// </summary>
                public const double Medium = 0.93;

                /// <summary>
                /// 低信心分數
                /// </summary>
                public const double Low = 0.9;
            }

            /// <summary>
            /// 批次處理
            /// </summary>
            public static class Batch
            {
                /// <summary>
                /// 最大批次大小
                /// </summary>
                public const int MaxSize = 50;

                /// <summary>
                /// 非同步處理閾值（超過此數量自動非同步）
                /// </summary>
                public const int AsyncThreshold = 10;

                /// <summary>
                /// 並行處理數量
                /// </summary>
                public const int ConcurrentTasks = 3;
            }
        }

        #endregion

        #region === 標籤系統 ===

        public static class Tags
        {
            /// <summary>
            /// 套用狀態
            /// </summary>
            public static class ApplyStatus
            {
                public const string Applied = "Applied";
                public const string Skipped = "Skipped";
                public const string Failed = "Failed";
            }

            /// <summary>
            /// 分類類型（對應 PhotoCategoryType）
            /// </summary>
            public static class CategoryType
            {
                public const string EXIF = "EXIF";
                public const string Time = "Time";
                public const string Tag = "Tag";
                public const string Location = "Location";
                public const string Custom = "Custom";
            }

            /// <summary>
            /// AI 生成的分類名稱（預設）
            /// </summary>
            public static class AIGeneratedCategories
            {
                public const string Objects = "物件";
                public const string Scenes = "場景";
                public const string TouristSpots = "景點";
                public const string Activities = "活動";
                public const string Nature = "自然";
            }

            /// <summary>
            /// 標籤分類 ID 對照（對應 PhotoCategory 表）
            /// </summary>
            /// <remarks>
            /// 這是暫時的寫死方案，用於將 AI 產生的標籤分配到正確的 Category。
            /// 
            /// <para><b>分類邏輯</b></para>
            /// 
            /// - CategoryId 1 (TIME)：時間相關標籤（由 EXIF 處理，AI 不產生）
            /// - CategoryId 2 (CAMERA)：相機相關標籤（由 EXIF 處理，AI 不產生）
            /// - CategoryId 3 (GENERAL)：一般標籤（保留給系統使用）
            /// - CategoryId 4 (LOCATION)：地點、景點名稱
            /// - CategoryId 5 (SCENE)：場景、環境描述
            /// - CategoryId 6 (CUSTOM)：使用者自訂標籤（AI 不產生）
            /// - CategoryId 10 (AI)：其他 AI 識別標籤
            /// 
            /// <para><b>未來升級方向</b></para>
            /// 
            /// 1. 建立標籤與分類的多對多關聯表
            /// 2. 在 Tag 表中新增 SuggestedCategoryId 欄位
            /// 3. 使用 NLP 技術動態判斷標籤應屬於哪個分類
            /// </remarks>
            public static class CategoryId
            {
                /// <summary>
                /// 時間（由 EXIF 處理）
                /// </summary>
                public const int Time = 1;

                /// <summary>
                /// 相機（由 EXIF 處理）
                /// </summary>
                public const int Camera = 2;

                /// <summary>
                /// 一般（系統保留）
                /// </summary>
                public const int General = 3;

                /// <summary>
                /// 地點、景點
                /// </summary>
                public const int Location = 4;

                /// <summary>
                /// 拍攝場景
                /// </summary>
                public const int Scene = 5;

                /// <summary>
                /// 自訂標籤（使用者手動建立）
                /// </summary>
                public const int Custom = 6;

                /// <summary>
                /// AI 標籤（其他無法明確分類的 AI 識別標籤）
                /// </summary>
                public const int AI = 10;

                /// <summary>
                /// 場景相關關鍵字（用於判斷標籤是否應歸類為 Scene）
                /// </summary>
                public static readonly HashSet<string> SceneKeywords = new()
                {
                    // 建築類
                    "building", "architecture", "構造", "建築", "房屋",
                    
                    // 環境類
                    "outdoor", "indoor", "landscape", "sky", "cloud",
                    "戶外", "室內", "風景", "天空", "雲",
                    
                    // 自然類
                    "mountain", "sea", "ocean", "river", "lake", "forest", "tree", "flower",
                    "山", "海", "海洋", "河", "湖", "森林", "樹", "花",
                    
                    // 氣候類
                    "sunset", "sunrise", "night", "day", "雨", "晴", "陰",
                    "日落", "日出", "夜晚", "白天", "rain", "sunny", "cloudy",
                    
                    // 場景類
                    "street", "road", "path", "garden", "park",
                    "街道", "道路", "小徑", "花園", "公園"
                };
            }
        }

        #endregion

        #region === 錯誤碼 ===

        public static class ErrorCodes
        {
            /// <summary>
            /// Azure Vision 錯誤
            /// </summary>
            public static class Azure
            {
                public const string InvalidImage = "AZURE_INVALID_IMAGE";
                public const string ImageTooLarge = "AZURE_IMAGE_TOO_LARGE";
                public const string Unauthorized = "AZURE_UNAUTHORIZED";
                public const string RateLimitExceeded = "AZURE_RATE_LIMIT";
                public const string ServiceUnavailable = "AZURE_SERVICE_UNAVAILABLE";
            }

            /// <summary>
            /// Google Places 錯誤
            /// </summary>
            public static class Google
            {
                public const string InvalidRequest = "GOOGLE_INVALID_REQUEST";
                public const string ZeroResults = "GOOGLE_ZERO_RESULTS";
                public const string OverQueryLimit = "GOOGLE_OVER_QUERY_LIMIT";
                public const string RequestDenied = "GOOGLE_REQUEST_DENIED";
                public const string UnknownError = "GOOGLE_UNKNOWN_ERROR";
            }

            /// <summary>
            /// Claude 錯誤
            /// </summary>
            public static class Claude
            {
                public const string InvalidRequest = "CLAUDE_INVALID_REQUEST";
                public const string Authentication = "CLAUDE_AUTHENTICATION";
                public const string RateLimit = "CLAUDE_RATE_LIMIT";
                public const string Overloaded = "CLAUDE_OVERLOADED";
                public const string ApiError = "CLAUDE_API_ERROR";
            }

            /// <summary>
            /// 通用錯誤
            /// </summary>
            public static class General
            {
                public const string NetworkError = "AI_NETWORK_ERROR";
                public const string Timeout = "AI_TIMEOUT";
                public const string InvalidResponse = "AI_INVALID_RESPONSE";
                public const string QuotaExceeded = "AI_QUOTA_EXCEEDED";
                public const string UnknownError = "AI_UNKNOWN_ERROR";
            }
        }

        #endregion

        #region === 超時設定 ===

        public static class Timeout
        {
            /// <summary>
            /// Azure Vision 超時（秒）
            /// </summary>
            public const int AzureSeconds = 30;

            /// <summary>
            /// Google Places 超時（秒）
            /// </summary>
            public const int GoogleSeconds = 20;

            /// <summary>
            /// Claude 超時（秒）
            /// </summary>
            public const int ClaudeSeconds = 60;

            /// <summary>
            /// 整體分析超時（秒）
            /// </summary>
            public const int TotalAnalysisSeconds = 120;
        }

        #endregion

        #region === 快取設定 ===

        public static class Cache
        {
            /// <summary>
            /// 快取 Key 前綴
            /// </summary>
            public const string KeyPrefix = "ai_analysis:";

            /// <summary>
            /// 分析結果快取時間（分鐘）
            /// </summary>
            public const int AnalysisResultMinutes = 60;

            /// <summary>
            /// Place Details 快取時間（小時）
            /// </summary>
            public const int PlaceDetailsHours = 24;

            /// <summary>
            /// 是否啟用快取（預設）
            /// </summary>
            public const bool EnabledByDefault = true;
        }

        #endregion

        #region === 日誌與除錯 ===

        public static class Logging
        {
            /// <summary>
            /// 日誌類別
            /// </summary>
            public static class Category
            {
                public const string AzureVision = "FrameZone.AI.Azure";
                public const string GooglePlaces = "FrameZone.AI.Google";
                public const string Claude = "FrameZone.AI.Claude";
                public const string AIService = "FrameZone.AI.Service";
            }

            /// <summary>
            /// 是否記錄原始回應（除錯用）
            /// </summary>
            public const bool LogRawResponse = false;

            /// <summary>
            /// 是否記錄 Prompt（除錯用）
            /// </summary>
            public const bool LogPrompt = false;
        }

        #endregion
    }
}