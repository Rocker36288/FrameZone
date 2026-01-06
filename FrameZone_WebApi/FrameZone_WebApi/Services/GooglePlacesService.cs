using System.Text.Json;
using FrameZone_WebApi.DTOs.AI;

namespace FrameZone_WebApi.Services
{
    /// <summary>
    /// Google Places API 服務實作
    /// 提供地點查詢、景點識別、反向地理編碼等功能
    /// </summary>
    public class GooglePlacesService : IGooglePlacesService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GooglePlacesService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;

        // Google Places API 端點（使用 Legacy Web Service API）
        private const string PLACES_NEARBY_SEARCH_URL = "https://maps.googleapis.com/maps/api/place/nearbysearch/json";
        private const string PLACES_DETAILS_URL = "https://maps.googleapis.com/maps/api/place/details/json";
        private const string GEOCODING_URL = "https://maps.googleapis.com/maps/api/geocode/json";
        private const string PLACES_PHOTO_URL = "https://maps.googleapis.com/maps/api/place/photo";

        public GooglePlacesService(
            HttpClient httpClient,
            ILogger<GooglePlacesService> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;

            // 從設定檔讀取 API Key
            // 支援：GooglePlaces:ApiKey（root）/ AIServices:GooglePlaces:ApiKey（集中管理）/ GoogleMaps:ApiKey（若共用同一把 key）
            _apiKey = _configuration["GooglePlaces:ApiKey"]
                ?? _configuration["AIServices:GooglePlaces:ApiKey"]
                ?? _configuration["GoogleMaps:ApiKey"]
                ?? throw new InvalidOperationException("❌ Google Places API Key 未設定（GooglePlaces:ApiKey / AIServices:GooglePlaces:ApiKey / GoogleMaps:ApiKey）");

            _logger.LogInformation("✅ Google Places Service 初始化完成");
        }

        #region 基礎 API 呼叫方法

        /// <summary>
        /// 附近搜尋（Nearby Search）
        /// </summary>
        public async Task<GooglePlacesResponseDto> NearbySearchAsync(GooglePlacesRequestDto request)
        {
            try
            {
                _logger.LogInformation($"🔍 開始 Google Places 附近搜尋");
                _logger.LogInformation($"📍 座標: ({request.Latitude}, {request.Longitude})");
                _logger.LogInformation($"📏 搜尋半徑: {request.Radius ?? 0} 公尺");

                // 驗證請求參數
                if (!request.IsValid(out string? validationError))
                {
                    _logger.LogWarning($"⚠️ 請求參數驗證失敗: {validationError}");
                    return new GooglePlacesResponseDto
                    {
                        Status = PlacesApiStatus.INVALID_REQUEST,
                        ErrorMessage = validationError
                    };
                }

                // 建立 API 請求 URL
                var url = BuildNearbySearchUrl(request);
                _logger.LogDebug($"🌐 API URL: {url.Replace(_apiKey, "***API_KEY***")}");

                // 發送 HTTP 請求
                var response = await _httpClient.GetAsync(url);
                var jsonContent = await response.Content.ReadAsStringAsync();

                // 解析回應
                var result = JsonSerializer.Deserialize<GooglePlacesResponseDto>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new GooglePlacesResponseDto();

                // 記錄結果
                _logger.LogInformation($"📊 API 狀態: {result.Status}");
                _logger.LogInformation($"📊 找到景點: {result.Results.Count} 個");

                // 檢查 API 錯誤
                if (result.Status != PlacesApiStatus.OK && result.Status != PlacesApiStatus.ZERO_RESULTS)
                {
                    _logger.LogWarning($"⚠️ Google Places API 回應錯誤: {result.Status}");
                    if (!string.IsNullOrEmpty(result.ErrorMessage))
                    {
                        _logger.LogWarning($"⚠️ 錯誤訊息: {result.ErrorMessage}");
                    }
                }

                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "❌ HTTP 請求失敗");
                return new GooglePlacesResponseDto
                {
                    Status = PlacesApiStatus.UNKNOWN_ERROR,
                    ErrorMessage = $"HTTP 請求失敗: {ex.Message}"
                };
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "❌ JSON 解析失敗");
                return new GooglePlacesResponseDto
                {
                    Status = PlacesApiStatus.UNKNOWN_ERROR,
                    ErrorMessage = $"回應解析失敗: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Google Places 附近搜尋發生未預期錯誤");
                return new GooglePlacesResponseDto
                {
                    Status = PlacesApiStatus.UNKNOWN_ERROR,
                    ErrorMessage = $"未預期錯誤: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// 取得景點詳細資訊（Place Details）
        /// </summary>
        public async Task<PlaceResult?> GetPlaceDetailsAsync(PlaceDetailsRequestDto request)
        {
            try
            {
                _logger.LogInformation($"🔍 開始查詢景點詳細資訊");
                _logger.LogInformation($"🆔 Place ID: {request.PlaceId}");

                if (string.IsNullOrWhiteSpace(request.PlaceId))
                {
                    _logger.LogWarning("⚠️ Place ID 不能為空");
                    return null;
                }

                // 建立 API 請求 URL
                var url = BuildPlaceDetailsUrl(request);
                _logger.LogDebug($"🌐 API URL: {url.Replace(_apiKey, "***API_KEY***")}");

                // 發送 HTTP 請求
                var response = await _httpClient.GetAsync(url);
                var jsonContent = await response.Content.ReadAsStringAsync();

                // 解析回應（Place Details 的回應格式略有不同）
                var jsonDoc = JsonDocument.Parse(jsonContent);
                var root = jsonDoc.RootElement;

                var status = root.GetProperty("status").GetString() ?? PlacesApiStatus.UNKNOWN_ERROR;
                _logger.LogInformation($"📊 API 狀態: {status}");

                if (status != PlacesApiStatus.OK)
                {
                    var errorMessage = root.TryGetProperty("error_message", out var errMsg)
                        ? errMsg.GetString()
                        : null;
                    _logger.LogWarning($"⚠️ Google Places API 回應錯誤: {status}");
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        _logger.LogWarning($"⚠️ 錯誤訊息: {errorMessage}");
                    }
                    return null;
                }

                // 提取 result 物件
                if (!root.TryGetProperty("result", out var resultElement))
                {
                    _logger.LogWarning("⚠️ 回應中沒有 result 欄位");
                    return null;
                }

                var placeResult = JsonSerializer.Deserialize<PlaceResult>(resultElement.GetRawText(), new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation($"✅ 成功取得景點詳細資訊: {placeResult?.Name ?? "未知"}");
                return placeResult;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "❌ HTTP 請求失敗");
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "❌ JSON 解析失敗");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 查詢景點詳細資訊發生未預期錯誤");
                return null;
            }
        }

        /// <summary>
        /// 反向地理編碼（Reverse Geocoding）
        /// </summary>
        public async Task<GeocodeResult?> ReverseGeocodeAsync(
            double latitude,
            double longitude,
            string language = "zh-TW")
        {
            try
            {
                _logger.LogInformation($"🔍 開始反向地理編碼");
                _logger.LogInformation($"📍 座標: ({latitude}, {longitude})");

                // 建立 API 請求 URL
                var url = $"{GEOCODING_URL}?latlng={latitude},{longitude}&language={language}&key={_apiKey}";
                _logger.LogDebug($"🌐 API URL: {url.Replace(_apiKey, "***API_KEY***")}");

                // 發送 HTTP 請求
                var response = await _httpClient.GetAsync(url);
                var jsonContent = await response.Content.ReadAsStringAsync();

                // 解析回應
                var jsonDoc = JsonDocument.Parse(jsonContent);
                var root = jsonDoc.RootElement;

                var status = root.GetProperty("status").GetString() ?? "UNKNOWN_ERROR";
                _logger.LogInformation($"📊 API 狀態: {status}");

                if (status != "OK")
                {
                    var errorMessage = root.TryGetProperty("error_message", out var errMsg)
                        ? errMsg.GetString()
                        : null;
                    _logger.LogWarning($"⚠️ Geocoding API 回應錯誤: {status}");
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        _logger.LogWarning($"⚠️ 錯誤訊息: {errorMessage}");
                    }
                    return null;
                }

                // 提取第一個結果
                if (!root.TryGetProperty("results", out var resultsElement) || resultsElement.GetArrayLength() == 0)
                {
                    _logger.LogWarning("⚠️ 沒有找到地理編碼結果");
                    return null;
                }

                var firstResult = resultsElement[0];
                var geocodeResult = ParseGeocodeResult(firstResult);

                _logger.LogInformation($"✅ 反向地理編碼成功");
                _logger.LogInformation($"📍 地址: {geocodeResult.FormattedAddress}");
                _logger.LogInformation($"🌍 國家: {geocodeResult.Country ?? "未知"}");
                _logger.LogInformation($"🏙️ 城市: {geocodeResult.City ?? "未知"}");

                return geocodeResult;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "❌ HTTP 請求失敗");
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "❌ JSON 解析失敗");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 反向地理編碼發生未預期錯誤");
                return null;
            }
        }

        #endregion

        #region 高階智能分析方法

        /// <summary>
        /// 智能景點識別
        /// 這是一個綜合分析方法，會使用多個 Google API 來判斷某個地點是否為旅遊景點
        /// </summary>
        public async Task<TouristSpotIdentificationDto> IdentifyTouristSpotAsync(
            double latitude,
            double longitude,
            int searchRadius = 500)
        {
            _logger.LogInformation($"🎯 開始智能景點識別");
            _logger.LogInformation($"📍 座標: ({latitude}, {longitude})");
            _logger.LogInformation($"📏 搜尋半徑: {searchRadius} 公尺");

            var result = new TouristSpotIdentificationDto
            {
                IsTouristSpot = false,
                Confidence = 0.0,
                AnalyzedAt = DateTimeOffset.UtcNow
            };

            try
            {
                // 步驟 1: 搜尋附近的旅遊景點
                _logger.LogInformation("📍 步驟 1: 搜尋附近的旅遊景點");
                var nearbyRequest = new GooglePlacesRequestDto
                {
                    Latitude = latitude,
                    Longitude = longitude,
                    Radius = searchRadius,
                    Type = "tourist_attraction",
                    Language = "zh-TW"
                };

                var nearbyResponse = await NearbySearchAsync(nearbyRequest);

                if (nearbyResponse.Status != PlacesApiStatus.OK || nearbyResponse.Results.Count == 0)
                {
                    _logger.LogInformation("ℹ️ 附近沒有找到標記為旅遊景點的地點");
                    result.MatchedBy = "none";
                    result.Evidence = "附近 500 公尺內沒有找到旅遊景點標記";
                    return result;
                }

                // 步驟 2: 找出最接近且評分最高的景點
                _logger.LogInformation($"📍 步驟 2: 分析 {nearbyResponse.Results.Count} 個候選景點");
                var bestSpot = FindBestTouristSpot(nearbyResponse.Results, latitude, longitude);

                if (bestSpot.Spot == null)
                {
                    _logger.LogInformation("ℹ️ 沒有找到符合條件的景點（評分太低或距離太遠）");
                    result.MatchedBy = "filtered_out";
                    result.Evidence = "找到候選景點但品質不符合標準（評分 < 3.5 或評論 < 10）";
                    return result;
                }

                // 步驟 3: 計算信心分數
                _logger.LogInformation($"📍 步驟 3: 計算信心分數");
                var confidence = CalculateConfidence(bestSpot.Spot, bestSpot.Distance);
                _logger.LogInformation($"🎯 信心分數: {confidence:F2}");

                // 步驟 4: 建立識別結果
                result.IsTouristSpot = confidence >= 0.9; // 信心分數 >= 60% 才判定為景點
                result.SpotName = bestSpot.Spot.Name;
                result.SpotTypes = bestSpot.Spot.Types;
                result.Confidence = confidence;
                result.PlaceId = bestSpot.Spot.PlaceId;
                result.Rating = bestSpot.Spot.Rating;
                result.ReviewCount = bestSpot.Spot.UserRatingsTotal;
                result.DistanceMeters = bestSpot.Distance;
                result.MatchedBy = "nearby_search";
                result.RawPlaceData = bestSpot.Spot;

                // 步驟 5: 生成建議標籤
                result.SuggestedTags = GenerateSuggestedTags(bestSpot.Spot);

                // 步驟 6: 生成識別證據
                result.Evidence = $"在 {bestSpot.Distance:F0} 公尺內找到景點「{bestSpot.Spot.Name}」，" +
                    $"評分 {bestSpot.Spot.Rating:F1} ({bestSpot.Spot.UserRatingsTotal} 則評論)";

                _logger.LogInformation($"✅ 景點識別完成");
                _logger.LogInformation($"🎯 結果: {(result.IsTouristSpot ? "是旅遊景點" : "不是旅遊景點")}");
                _logger.LogInformation($"📌 景點名稱: {result.SpotName}");
                _logger.LogInformation($"🎯 信心分數: {result.Confidence:P0}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 智能景點識別發生未預期錯誤");
                result.Evidence = $"分析過程發生錯誤: {ex.Message}";
                return result;
            }
        }

        #endregion

        #region 輔助方法

        /// <summary>
        /// 建立 Google Place Photo URL
        /// </summary>
        public string BuildPhotoUrl(string photoReference, int maxWidth = 400, int? maxHeight = null)
        {
            var url = $"{PLACES_PHOTO_URL}?photoreference={photoReference}&maxwidth={maxWidth}&key={_apiKey}";

            if (maxHeight.HasValue)
            {
                url += $"&maxheight={maxHeight.Value}";
            }

            return url;
        }

        /// <summary>
        /// 測試 Google Places API 連線
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                _logger.LogInformation("🔍 測試 Google Places API 連線");

                // 使用一個簡單的 Geocoding 請求來測試（台北 101 的座標）
                var testLat = 25.033964;
                var testLng = 121.564468;

                var url = $"{GEOCODING_URL}?latlng={testLat},{testLng}&key={_apiKey}";
                var response = await _httpClient.GetAsync(url);
                var jsonContent = await response.Content.ReadAsStringAsync();

                var jsonDoc = JsonDocument.Parse(jsonContent);
                var status = jsonDoc.RootElement.GetProperty("status").GetString();

                if (status == "OK" || status == "ZERO_RESULTS")
                {
                    _logger.LogInformation("✅ Google Places API 連線測試成功");
                    return true;
                }
                else if (status == "REQUEST_DENIED")
                {
                    _logger.LogError("❌ API Key 無效或權限不足");
                    return false;
                }
                else if (status == "OVER_QUERY_LIMIT")
                {
                    _logger.LogError("❌ API 配額已用盡");
                    return false;
                }
                else
                {
                    _logger.LogWarning($"⚠️ API 回應異常狀態: {status}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Google Places API 連線測試失敗");
                return false;
            }
        }

        #endregion

        #region 私有輔助方法

        /// <summary>
        /// 建立 Nearby Search API URL
        /// </summary>
        private string BuildNearbySearchUrl(GooglePlacesRequestDto request)
        {
            var url = $"{PLACES_NEARBY_SEARCH_URL}?location={request.Latitude},{request.Longitude}&key={_apiKey}";

            if (request.Radius.HasValue)
                url += $"&radius={request.Radius.Value}";

            if (!string.IsNullOrWhiteSpace(request.Type))
                url += $"&type={request.Type}";

            if (!string.IsNullOrWhiteSpace(request.Keyword))
                url += $"&keyword={Uri.EscapeDataString(request.Keyword)}";

            if (!string.IsNullOrWhiteSpace(request.Name))
                url += $"&name={Uri.EscapeDataString(request.Name)}";

            if (!string.IsNullOrWhiteSpace(request.Language))
                url += $"&language={request.Language}";

            if (request.OpenNow.HasValue && request.OpenNow.Value)
                url += "&opennow=true";

            if (!string.IsNullOrWhiteSpace(request.RankBy))
                url += $"&rankby={request.RankBy}";

            return url;
        }

        /// <summary>
        /// 建立 Place Details API URL
        /// </summary>
        private string BuildPlaceDetailsUrl(PlaceDetailsRequestDto request)
        {
            var url = $"{PLACES_DETAILS_URL}?place_id={request.PlaceId}&language={request.Language}&key={_apiKey}";

            if (request.Fields != null && request.Fields.Count > 0)
            {
                url += $"&fields={string.Join(",", request.Fields)}";
            }

            return url;
        }

        /// <summary>
        /// 解析 Geocoding 回應結果
        /// </summary>
        private GeocodeResult ParseGeocodeResult(JsonElement resultElement)
        {
            var result = new GeocodeResult();

            // 格式化地址
            if (resultElement.TryGetProperty("formatted_address", out var formattedAddr))
            {
                result.FormattedAddress = formattedAddr.GetString() ?? string.Empty;
            }

            // Place ID
            if (resultElement.TryGetProperty("place_id", out var placeId))
            {
                result.PlaceId = placeId.GetString();
            }

            // 解析地址組成元素
            if (resultElement.TryGetProperty("address_components", out var components))
            {
                foreach (var component in components.EnumerateArray())
                {
                    var addressComponent = new AddressComponent
                    {
                        LongName = component.GetProperty("long_name").GetString() ?? string.Empty,
                        ShortName = component.GetProperty("short_name").GetString() ?? string.Empty,
                        Types = component.GetProperty("types").EnumerateArray()
                            .Select(t => t.GetString() ?? string.Empty)
                            .ToList()
                    };

                    result.AddressComponents.Add(addressComponent);

                    // 提取常用欄位
                    if (addressComponent.Types.Contains("country"))
                        result.Country = addressComponent.LongName;

                    if (addressComponent.Types.Contains("locality") ||
                        addressComponent.Types.Contains("administrative_area_level_2"))
                        result.City = addressComponent.LongName;

                    if (addressComponent.Types.Contains("administrative_area_level_3") ||
                        addressComponent.Types.Contains("sublocality"))
                        result.District = addressComponent.LongName;

                    if (addressComponent.Types.Contains("route"))
                        result.Street = addressComponent.LongName;

                    if (addressComponent.Types.Contains("postal_code"))
                        result.PostalCode = addressComponent.LongName;
                }
            }

            return result;
        }

        /// <summary>
        /// 找出最佳的旅遊景點候選
        /// 綜合考慮距離、評分、評論數等因素
        /// </summary>
        private (PlaceResult? Spot, double Distance) FindBestTouristSpot(
            List<PlaceResult> candidates,
            double targetLat,
            double targetLng)
        {
            PlaceResult? bestSpot = null;
            double bestScore = 0;
            double bestDistance = double.MaxValue;

            foreach (var spot in candidates)
            {
                // 必須有座標資訊
                if (spot.Geometry?.Location == null)
                    continue;

                // 計算距離（公尺）
                var distance = CalculateDistance(
                    targetLat, targetLng,
                    spot.Geometry.Location.Lat, spot.Geometry.Location.Lng);

                // 基本過濾條件：
                // 1. 評分至少 3.5 星
                // 2. 至少有 10 則評論（避免假景點）
                // 3. 距離不超過搜尋半徑
                if (spot.Rating < 3.5 || spot.UserRatingsTotal < 10)
                    continue;

                // 計算綜合分數：
                // - 距離越近越好（權重 40%）
                // - 評分越高越好（權重 30%）
                // - 評論數越多越好（權重 30%）
                var distanceScore = Math.Max(0, 1 - (distance / 500.0)); // 500 公尺內滿分
                var ratingScore = (spot.Rating ?? 0) / 5.0;
                var reviewScore = Math.Min(1.0, (spot.UserRatingsTotal ?? 0) / 100.0); // 100 則評論以上滿分

                var totalScore = (distanceScore * 0.4) + (ratingScore * 0.3) + (reviewScore * 0.3);

                _logger.LogDebug($"📊 候選景點: {spot.Name}");
                _logger.LogDebug($"   距離: {distance:F0}m (分數: {distanceScore:F2})");
                _logger.LogDebug($"   評分: {spot.Rating:F1} (分數: {ratingScore:F2})");
                _logger.LogDebug($"   評論: {spot.UserRatingsTotal} 則 (分數: {reviewScore:F2})");
                _logger.LogDebug($"   總分: {totalScore:F2}");

                if (totalScore > bestScore)
                {
                    bestScore = totalScore;
                    bestSpot = spot;
                    bestDistance = distance;
                }
            }

            if (bestSpot != null)
            {
                _logger.LogInformation($"🏆 最佳候選景點: {bestSpot.Name} (距離 {bestDistance:F0}m, 總分 {bestScore:F2})");
            }

            return (bestSpot, bestDistance);
        }

        /// <summary>
        /// 計算兩個座標之間的距離（公尺）
        /// 使用 Haversine 公式
        /// </summary>
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371000; // 地球半徑（公尺）

            var dLat = DegreesToRadians(lat2 - lat1);
            var dLon = DegreesToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        /// <summary>
        /// 角度轉弧度
        /// </summary>
        private double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        /// <summary>
        /// 計算景點識別的信心分數 (0.0 - 1.0)
        /// </summary>
        private double CalculateConfidence(PlaceResult spot, double distance)
        {
            // 基礎分數從 0.5 開始
            double confidence = 0.5;

            // 距離因素（最多 +0.3）
            if (distance < 50) confidence += 0.3;
            else if (distance < 100) confidence += 0.2;
            else if (distance < 200) confidence += 0.1;

            // 評分因素（最多 +0.1）
            if (spot.Rating >= 4.5) confidence += 0.1;
            else if (spot.Rating >= 4.0) confidence += 0.05;

            // 評論數因素（最多 +0.1）
            if (spot.UserRatingsTotal >= 500) confidence += 0.1;
            else if (spot.UserRatingsTotal >= 100) confidence += 0.05;

            // 確保在 0.0 - 1.0 範圍內
            return Math.Max(0.0, Math.Min(1.0, confidence));
        }

        /// <summary>
        /// 根據景點類型生成建議標籤
        /// </summary>
        private List<string> GenerateSuggestedTags(PlaceResult spot)
        {
            var tags = new List<string>();

            // 根據 Google Places 的類型對應到我們的標籤系統
            var typeMapping = new Dictionary<string, string>
            {
                { "tourist_attraction", "旅遊景點" },
                { "museum", "博物館" },
                { "art_gallery", "藝術館" },
                { "park", "公園" },
                { "amusement_park", "遊樂園" },
                { "aquarium", "水族館" },
                { "zoo", "動物園" },
                { "church", "教堂" },
                { "mosque", "清真寺" },
                { "hindu_temple", "印度教寺廟" },
                { "synagogue", "猶太教會堂" },
                { "stadium", "體育場" },
                { "shopping_mall", "購物中心" },
                { "landmark", "地標" },
                { "point_of_interest", "興趣點" },
                { "natural_feature", "自然景觀" },
                { "beach", "海灘" },
                { "mountain", "山" }
            };

            foreach (var type in spot.Types)
            {
                if (typeMapping.TryGetValue(type, out var tag))
                {
                    tags.Add(tag);
                }
            }

            // 如果沒有對應到任何標籤，至少加入景點名稱
            if (tags.Count == 0)
            {
                tags.Add(spot.Name);
            }

            return tags.Distinct().ToList();
        }

        #endregion
    }
}