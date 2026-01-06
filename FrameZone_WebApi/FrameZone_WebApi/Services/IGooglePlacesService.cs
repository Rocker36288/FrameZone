using FrameZone_WebApi.DTOs.AI;

namespace FrameZone_WebApi.Services
{
    /// <summary>
    /// Google Places API 服務介面
    /// 提供地點查詢、景點識別、反向地理編碼等功能
    /// </summary>
    public interface IGooglePlacesService
    {
        #region 基礎 API 呼叫方法

        /// <summary>
        /// 附近搜尋（Nearby Search）
        /// 根據經緯度座標搜尋附近的景點或地點
        /// </summary>
        /// <param name="request">搜尋請求參數</param>
        /// <returns>Google Places API 搜尋結果</returns>
        /// <remarks>
        /// 這是最常用的方法，用於找出照片拍攝地點附近有哪些景點。
        /// 例如：在台北 101 附近拍照，可以找到「台北 101」、「象山」等景點。
        /// </remarks>
        Task<GooglePlacesResponseDto> NearbySearchAsync(GooglePlacesRequestDto request);

        /// <summary>
        /// 取得景點詳細資訊（Place Details）
        /// 根據 Place ID 查詢某個景點的完整資訊
        /// </summary>
        /// <param name="request">詳細資訊請求參數</param>
        /// <returns>景點詳細資訊</returns>
        /// <remarks>
        /// 當你已經有景點的 Place ID（從 Nearby Search 取得），
        /// 可以用這個方法取得更詳細的資訊，像是營業時間、電話號碼、網站等。
        /// </remarks>
        Task<PlaceResult?> GetPlaceDetailsAsync(PlaceDetailsRequestDto request);

        /// <summary>
        /// 反向地理編碼（Reverse Geocoding）
        /// 將經緯度座標轉換成可讀的地址資訊
        /// </summary>
        /// <param name="latitude">緯度</param>
        /// <param name="longitude">經度</param>
        /// <param name="language">語言代碼（預設：zh-TW）</param>
        /// <returns>地址資訊（國家、城市、地區等）</returns>
        /// <remarks>
        /// 這個方法會告訴你「這個座標在哪裡」，回傳結果像是：
        /// "台灣台北市信義區信義路五段7號"
        /// 這對於建立照片的地點標籤非常有用。
        /// </remarks>
        Task<GeocodeResult?> ReverseGeocodeAsync(double latitude, double longitude, string language = "zh-TW");

        #endregion

        #region 高階智能分析方法

        /// <summary>
        /// 智能景點識別
        /// 綜合使用多種 Google API 來判斷某個地點是否為旅遊景點
        /// </summary>
        /// <param name="latitude">緯度</param>
        /// <param name="longitude">經度</param>
        /// <param name="searchRadius">搜尋半徑（公尺，預設 500）</param>
        /// <returns>景點識別結果，包含是否為景點、景點名稱、信心分數等</returns>
        /// <remarks>
        /// 這是一個智能方法，它會：
        /// 1. 先搜尋附近是否有標記為 tourist_attraction 的景點
        /// 2. 檢查評分和評論數量來判斷重要性
        /// 3. 計算距離來判斷照片是否在景點範圍內
        /// 4. 給出一個信心分數和建議的標籤
        /// 
        /// 例如：在故宮博物院拍照，會識別出這是「國立故宮博物院」，
        /// 信心分數很高，並建議加上「博物館」、「文化」等標籤。
        /// </remarks>
        Task<TouristSpotIdentificationDto> IdentifyTouristSpotAsync(
            double latitude,
            double longitude,
            int searchRadius = 500);

        #endregion

        #region 輔助方法

        /// <summary>
        /// 建立 Google Place Photo URL
        /// 根據 photo_reference 建立可以直接存取的照片 URL
        /// </summary>
        /// <param name="photoReference">照片參考碼（從 PlacePhoto.PhotoReference 取得）</param>
        /// <param name="maxWidth">照片最大寬度（像素，最大 1600）</param>
        /// <param name="maxHeight">照片最大高度（像素，最大 1600）</param>
        /// <returns>完整的照片 URL</returns>
        /// <remarks>
        /// Google Places API 回傳的照片資訊只有 photo_reference，
        /// 需要透過這個方法轉換成實際可以下載的 URL。
        /// </remarks>
        string BuildPhotoUrl(string photoReference, int maxWidth = 400, int? maxHeight = null);

        /// <summary>
        /// 測試 Google Places API 連線
        /// </summary>
        /// <returns>是否連線成功</returns>
        /// <remarks>
        /// 這個方法會發送一個簡單的 Geocoding 請求來測試：
        /// 1. API Key 是否有效
        /// 2. 網路連線是否正常
        /// 3. 配額是否已用盡
        /// </remarks>
        Task<bool> TestConnectionAsync();

        #endregion
    }

    /// <summary>
    /// Geocoding API 回應結果
    /// 用於反向地理編碼功能
    /// </summary>
    public class GeocodeResult
    {
        /// <summary>
        /// 格式化地址（完整地址字串）
        /// </summary>
        public string FormattedAddress { get; set; } = string.Empty;

        /// <summary>
        /// 國家
        /// </summary>
        public string? Country { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        public string? City { get; set; }

        /// <summary>
        /// 行政區
        /// </summary>
        public string? District { get; set; }

        /// <summary>
        /// 街道地址
        /// </summary>
        public string? Street { get; set; }

        /// <summary>
        /// 郵遞區號
        /// </summary>
        public string? PostalCode { get; set; }

        /// <summary>
        /// Place ID
        /// </summary>
        public string? PlaceId { get; set; }

        /// <summary>
        /// 地址組成元素（原始資料）
        /// </summary>
        public List<AddressComponent> AddressComponents { get; set; } = new();
    }

    /// <summary>
    /// 地址組成元素
    /// </summary>
    public class AddressComponent
    {
        /// <summary>
        /// 完整名稱
        /// </summary>
        public string LongName { get; set; } = string.Empty;

        /// <summary>
        /// 簡稱
        /// </summary>
        public string ShortName { get; set; } = string.Empty;

        /// <summary>
        /// 類型列表
        /// 例如：["country", "political"] 表示這是國家級的行政區域
        /// </summary>
        public List<string> Types { get; set; } = new();
    }
}