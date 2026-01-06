using System.Text.Json.Serialization;

namespace FrameZone_WebApi.DTOs.AI
{
    /// <summary>
    /// Google Places API (Legacy) Nearby Search / Text Search 回應
    /// 注意：使用 Legacy Web Service，非 Places API (New)
    /// </summary>
    public class GooglePlacesResponseDto
    {
        /// <summary>
        /// 景點列表
        /// </summary>
        [JsonPropertyName("results")]
        public List<PlaceResult> Results { get; set; } = new();

        /// <summary>
        /// API 狀態碼（OK, ZERO_RESULTS, OVER_QUERY_LIMIT, REQUEST_DENIED, INVALID_REQUEST, UNKNOWN_ERROR）
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// 錯誤訊息（如果有）
        /// </summary>
        [JsonPropertyName("error_message")]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 下一頁 Token（用於分頁）
        /// </summary>
        [JsonPropertyName("next_page_token")]
        public string? NextPageToken { get; set; }

        /// <summary>
        /// HTML 歸屬文字（必須顯示）
        /// </summary>
        [JsonPropertyName("html_attributions")]
        public List<string> HtmlAttributions { get; set; } = new();
    }

    /// <summary>
    /// Google Places API 常見狀態碼
    /// </summary>
    public static class PlacesApiStatus
    {
        public const string OK = "OK";
        public const string ZERO_RESULTS = "ZERO_RESULTS";
        public const string OVER_QUERY_LIMIT = "OVER_QUERY_LIMIT";
        public const string REQUEST_DENIED = "REQUEST_DENIED";
        public const string INVALID_REQUEST = "INVALID_REQUEST";
        public const string UNKNOWN_ERROR = "UNKNOWN_ERROR";
    }

    /// <summary>
    /// 景點結果
    /// </summary>
    public class PlaceResult
    {
        /// <summary>
        /// Place ID（Google 唯一識別碼）
        /// </summary>
        [JsonPropertyName("place_id")]
        public string PlaceId { get; set; } = string.Empty;

        /// <summary>
        /// 景點名稱
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 景點類型列表（例如：tourist_attraction, museum, park）
        /// </summary>
        [JsonPropertyName("types")]
        public List<string> Types { get; set; } = new();

        /// <summary>
        /// 格式化地址
        /// </summary>
        [JsonPropertyName("formatted_address")]
        public string? FormattedAddress { get; set; }

        /// <summary>
        /// 附近地址（簡短版）
        /// </summary>
        [JsonPropertyName("vicinity")]
        public string? Vicinity { get; set; }

        /// <summary>
        /// 地理位置
        /// </summary>
        [JsonPropertyName("geometry")]
        public PlaceGeometry? Geometry { get; set; }

        /// <summary>
        /// 評分 (0.0 - 5.0)
        /// </summary>
        [JsonPropertyName("rating")]
        public double? Rating { get; set; }

        /// <summary>
        /// 評論數量
        /// </summary>
        [JsonPropertyName("user_ratings_total")]
        public int? UserRatingsTotal { get; set; }

        /// <summary>
        /// 價格等級 (0-4, 0=免費, 4=非常昂貴)
        /// </summary>
        [JsonPropertyName("price_level")]
        public int? PriceLevel { get; set; }

        /// <summary>
        /// 照片列表
        /// </summary>
        [JsonPropertyName("photos")]
        public List<PlacePhoto> Photos { get; set; } = new();

        /// <summary>
        /// 營業時間（包含 open_now 狀態）
        /// </summary>
        [JsonPropertyName("opening_hours")]
        public PlaceOpeningHours? OpeningHours { get; set; }

        /// <summary>
        /// 圖示 URL（注意：長期可用性較弱）
        /// </summary>
        [JsonPropertyName("icon")]
        public string? Icon { get; set; }

        /// <summary>
        /// 商業狀態（OPERATIONAL, CLOSED_TEMPORARILY, CLOSED_PERMANENTLY）
        /// 建議使用此欄位取代 permanently_closed
        /// </summary>
        [JsonPropertyName("business_status")]
        public string? BusinessStatus { get; set; }

        /// <summary>
        /// 是否為永久關閉（已 deprecated，請使用 business_status）
        /// </summary>
        [JsonPropertyName("permanently_closed")]
        [Obsolete("已 deprecated，請使用 BusinessStatus 欄位")]
        public bool? PermanentlyClosed { get; set; }
    }

    /// <summary>
    /// 商業狀態常數
    /// </summary>
    public static class BusinessStatus
    {
        public const string OPERATIONAL = "OPERATIONAL";
        public const string CLOSED_TEMPORARILY = "CLOSED_TEMPORARILY";
        public const string CLOSED_PERMANENTLY = "CLOSED_PERMANENTLY";
    }

    /// <summary>
    /// 景點地理位置
    /// </summary>
    public class PlaceGeometry
    {
        /// <summary>
        /// 位置座標
        /// </summary>
        [JsonPropertyName("location")]
        public PlaceLocation Location { get; set; } = new();

        /// <summary>
        /// 視窗範圍
        /// </summary>
        [JsonPropertyName("viewport")]
        public PlaceViewport? Viewport { get; set; }
    }

    /// <summary>
    /// 位置座標（WGS84 座標系）
    /// </summary>
    public class PlaceLocation
    {
        /// <summary>
        /// 緯度 (-90.0 ~ 90.0)
        /// </summary>
        [JsonPropertyName("lat")]
        public double Lat { get; set; }

        /// <summary>
        /// 經度 (-180.0 ~ 180.0)
        /// </summary>
        [JsonPropertyName("lng")]
        public double Lng { get; set; }
    }

    /// <summary>
    /// 視窗範圍
    /// </summary>
    public class PlaceViewport
    {
        /// <summary>
        /// 東北角
        /// </summary>
        [JsonPropertyName("northeast")]
        public PlaceLocation Northeast { get; set; } = new();

        /// <summary>
        /// 西南角
        /// </summary>
        [JsonPropertyName("southwest")]
        public PlaceLocation Southwest { get; set; } = new();
    }

    /// <summary>
    /// 景點照片
    /// </summary>
    public class PlacePhoto
    {
        /// <summary>
        /// 照片參考碼（用於取得實際照片）
        /// </summary>
        [JsonPropertyName("photo_reference")]
        public string PhotoReference { get; set; } = string.Empty;

        /// <summary>
        /// 照片高度（像素）
        /// </summary>
        [JsonPropertyName("height")]
        public int Height { get; set; }

        /// <summary>
        /// 照片寬度（像素）
        /// </summary>
        [JsonPropertyName("width")]
        public int Width { get; set; }

        /// <summary>
        /// HTML 歸屬文字
        /// </summary>
        [JsonPropertyName("html_attributions")]
        public List<string> HtmlAttributions { get; set; } = new();
    }

    /// <summary>
    /// 營業時間
    /// </summary>
    public class PlaceOpeningHours
    {
        /// <summary>
        /// 是否目前營業（來源：opening_hours.open_now）
        /// </summary>
        [JsonPropertyName("open_now")]
        public bool OpenNow { get; set; }

        /// <summary>
        /// 每週營業時間文字描述
        /// </summary>
        [JsonPropertyName("weekday_text")]
        public List<string> WeekdayText { get; set; } = new();

        /// <summary>
        /// 營業時段詳細資訊
        /// </summary>
        [JsonPropertyName("periods")]
        public List<PlacePeriod> Periods { get; set; } = new();
    }

    /// <summary>
    /// 營業時段
    /// </summary>
    public class PlacePeriod
    {
        /// <summary>
        /// 開始時間
        /// </summary>
        [JsonPropertyName("open")]
        public PlaceTime Open { get; set; } = new();

        /// <summary>
        /// 結束時間（24 小時營業時可能為 null）
        /// </summary>
        [JsonPropertyName("close")]
        public PlaceTime? Close { get; set; }
    }

    /// <summary>
    /// 時間點
    /// </summary>
    public class PlaceTime
    {
        /// <summary>
        /// 星期幾 (0=週日, 1=週一, ..., 6=週六)
        /// </summary>
        [JsonPropertyName("day")]
        public int Day { get; set; }

        /// <summary>
        /// 時間 (HHMM 格式，例如 "0900" 表示 9:00 AM)
        /// </summary>
        [JsonPropertyName("time")]
        public string Time { get; set; } = string.Empty;
    }

    /// <summary>
    /// Google Places 查詢請求（Nearby Search Legacy）
    /// </summary>
    public class GooglePlacesRequestDto
    {
        /// <summary>
        /// 緯度
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// 經度
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// 搜尋半徑（公尺）
        /// 注意：當 RankBy=distance 時不能使用此參數
        /// 最大值依據是否提供 keyword/name 而有不同限制
        /// </summary>
        public int? Radius { get; set; } = 500;

        /// <summary>
        /// 景點類型過濾（例如：tourist_attraction, museum, park）
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// 關鍵字搜尋
        /// 注意：當 RankBy=distance 時必須提供 keyword, name 或 type 其中之一
        /// </summary>
        public string? Keyword { get; set; }

        /// <summary>
        /// 名稱搜尋
        /// 注意：當 RankBy=distance 時必須提供 keyword, name 或 type 其中之一
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 語言代碼（zh-TW=繁體中文）
        /// </summary>
        public string Language { get; set; } = "zh-TW";

        /// <summary>
        /// 最低評分過濾
        /// </summary>
        public double? MinRating { get; set; }

        /// <summary>
        /// 是否只顯示營業中的景點
        /// </summary>
        public bool? OpenNow { get; set; }

        /// <summary>
        /// 排名依據（prominence=重要性, distance=距離）
        /// 注意：
        /// - distance 時不能使用 Radius
        /// - distance 時必須提供 keyword, name 或 type 其中之一
        /// </summary>
        public string RankBy { get; set; } = "prominence";

        /// <summary>
        /// 驗證請求參數是否合法
        /// </summary>
        public bool IsValid(out string? errorMessage)
        {
            if (RankBy == "distance")
            {
                // RankBy=distance 時不能帶 radius
                if (Radius.HasValue)
                {
                    errorMessage = "當 RankBy=distance 時不能使用 Radius 參數";
                    return false;
                }

                // RankBy=distance 時必須有 keyword, name 或 type
                if (string.IsNullOrWhiteSpace(Keyword) &&
                    string.IsNullOrWhiteSpace(Name) &&
                    string.IsNullOrWhiteSpace(Type))
                {
                    errorMessage = "當 RankBy=distance 時必須提供 Keyword, Name 或 Type 其中之一";
                    return false;
                }
            }

            errorMessage = null;
            return true;
        }
    }

    /// <summary>
    /// 景點詳細資訊請求（Place Details Legacy）
    /// </summary>
    public class PlaceDetailsRequestDto
    {
        /// <summary>
        /// Place ID
        /// </summary>
        public string PlaceId { get; set; } = string.Empty;

        /// <summary>
        /// 語言代碼（zh-TW=繁體中文）
        /// </summary>
        public string Language { get; set; } = "zh-TW";

        /// <summary>
        /// 要查詢的欄位列表
        /// 注意：這是 Legacy API 的 fields 參數，非 New API 的 FieldMask
        /// </summary>
        public List<string>? Fields { get; set; }
    }

    /// <summary>
    /// 景點識別結果（系統內部整合分析結果）
    /// </summary>
    public class TouristSpotIdentificationDto
    {
        /// <summary>
        /// 是否識別為旅遊景點
        /// </summary>
        public bool IsTouristSpot { get; set; }

        /// <summary>
        /// 景點名稱（如果是景點）
        /// </summary>
        public string? SpotName { get; set; }

        /// <summary>
        /// 景點類型列表（例如：["tourist_attraction", "museum"]）
        /// </summary>
        public List<string> SpotTypes { get; set; } = new();

        /// <summary>
        /// 信心分數 (0.0 - 1.0)
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// Place ID（Google）
        /// </summary>
        public string? PlaceId { get; set; }

        /// <summary>
        /// 景點評分 (0.0 - 5.0)
        /// </summary>
        public double? Rating { get; set; }

        /// <summary>
        /// 評論數量
        /// </summary>
        public int? ReviewCount { get; set; }

        /// <summary>
        /// 與拍攝地點的距離（公尺）
        /// </summary>
        public double? DistanceMeters { get; set; }

        /// <summary>
        /// 景點描述（由 AI 生成）
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 建議的標籤列表
        /// </summary>
        public List<string> SuggestedTags { get; set; } = new();

        /// <summary>
        /// 匹配來源（landmark, nearby_search, text_search, reverse_geocode）
        /// </summary>
        public string? MatchedBy { get; set; }

        /// <summary>
        /// 識別證據（用於追溯與除錯）
        /// </summary>
        public string? Evidence { get; set; }

        /// <summary>
        /// 分析時間
        /// </summary>
        public DateTimeOffset AnalyzedAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// 原始 Google Places 資料
        /// </summary>
        public PlaceResult? RawPlaceData { get; set; }
    }
}