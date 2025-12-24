using FrameZone_WebApi.DTOs;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace FrameZone_WebApi.Services
{
    /// <summary>
    /// 地理編碼服務
    /// </summary>
    public class GeocodingService : IGeocodingService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeocodingService> _logger;
        private readonly IMemoryCache _cache;
        private readonly string _googleApiKey;

        private const string GoogleGeocodingApiUrl = "https://maps.googleapis.com/maps/api/geocode/json";

        public GeocodingService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<GeocodingService> logger,
            IMemoryCache cache)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _cache = cache;

            // 從 appsettings.json 讀取 Google API Key
            _googleApiKey = _configuration["GoogleMaps:ApiKey"];

            if (string.IsNullOrEmpty(_googleApiKey))
            {
                _logger.LogWarning("⚠️ Google Maps API Key 未設定，地理編碼功能將無法使用");
            }
        }

        /// <summary>
        /// 反向地理編碼 - 將 GPS 座標轉換為地址資訊
        /// </summary>
        public async Task<ReverseGeocodeResponseDTO> ReverseGeocodeAsync(
            decimal latitude,
            decimal longitude,
            string language = "zh-TW")
        {
            return await ReverseGeocodeAsync(new ReverseGeocodeRequestDTO
            {
                Latitude = latitude,
                Longitude = longitude,
                Language = language
            });
        }

        /// <summary>
        /// 反向地理編碼 - 使用 DTO 請求
        /// </summary>
        public async Task<ReverseGeocodeResponseDTO> ReverseGeocodeAsync(ReverseGeocodeRequestDTO request)
        {
            try
            {
                if (string.IsNullOrEmpty(_googleApiKey))
                {
                    _logger.LogError("❌ Google Maps API Key 未設定");
                    return new ReverseGeocodeResponseDTO
                    {
                        Success = false,
                        ErrorMessage = "Google Maps API Key 未設定"
                    };
                }

                // ===== 座標四捨五入（減少快取鍵的數量）=====
                // 四捨五入到 4 位小數 ≈ 11 公尺精度
                var lat = Math.Round(request.Latitude, 4);
                var lng = Math.Round(request.Longitude, 4);

                // ===== 生成快取鍵 =====
                var cacheKey = $"geocode:{lat}:{lng}:{request.Language}";

                // ===== 先檢查快取 =====
                if (_cache.TryGetValue<ReverseGeocodeResponseDTO>(cacheKey, out var cachedResult))
                {
                    _logger.LogInformation("✅ 使用快取的地理編碼結果，座標: ({Lat}, {Lng})", lat, lng);
                    return cachedResult;
                }

                // ===== 快取未命中，呼叫 Google API =====
                _logger.LogInformation("🌍 開始反向地理編碼（快取未命中），座標: ({Lat}, {Lng})", lat, lng);

                var apiUrl = $"{GoogleGeocodingApiUrl}?latlng={lat},{lng}&key={_googleApiKey}&language={request.Language}";

                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("❌ Google API 請求失敗，HTTP 狀態碼: {StatusCode}", response.StatusCode);
                    return new ReverseGeocodeResponseDTO
                    {
                        Success = false,
                        ErrorMessage = $"API 請求失敗: {response.StatusCode}"
                    };
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var googleResponse = JsonSerializer.Deserialize<GoogleGeocodingResponse>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (googleResponse.Status != "OK")
                {
                    _logger.LogWarning("⚠️ Google API 回應狀態異常: {Status}, 訊息: {ErrorMessage}",
                        googleResponse.Status, googleResponse.Error_Message);

                    return new ReverseGeocodeResponseDTO
                    {
                        Success = false,
                        ErrorMessage = $"API 回應錯誤: {googleResponse.Status}"
                    };
                }

                if (googleResponse.Results == null || googleResponse.Results.Count == 0)
                {
                    _logger.LogWarning("⚠️ 未找到地址資訊，座標: ({Lat}, {Lng})", lat, lng);

                    return new ReverseGeocodeResponseDTO
                    {
                        Success = false,
                        ErrorMessage = "未找到地址資訊"
                    };
                }

                // ===== 解析地址資訊 =====
                var addressInfo = ParseAddressInfo(googleResponse.Results[0], request.Latitude, request.Longitude);

                var result = new ReverseGeocodeResponseDTO
                {
                    Success = true,
                    AddressInfo = addressInfo
                };

                // ===== 儲存到快取（30 天有效期）=====
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromDays(30))  // 30 天內沒被存取就移除
                    .SetAbsoluteExpiration(TimeSpan.FromDays(90))  // 絕對 90 天後過期
                    .SetPriority(CacheItemPriority.Normal)
                    .SetSize(1);

                _cache.Set(cacheKey, result, cacheOptions);

                _logger.LogInformation("✅ 反向地理編碼成功並已快取，地址: {FormattedAddress}", addressInfo.FormattedAddress);

                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "❌ Google API 網路請求失敗");
                return new ReverseGeocodeResponseDTO
                {
                    Success = false,
                    ErrorMessage = $"網路請求失敗: {ex.Message}"
                };
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "❌ Google API 回應 JSON 解析失敗");
                return new ReverseGeocodeResponseDTO
                {
                    Success = false,
                    ErrorMessage = $"回應解析失敗: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 反向地理編碼時發生錯誤");
                return new ReverseGeocodeResponseDTO
                {
                    Success = false,
                    ErrorMessage = $"處理失敗: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// 解析 Google API 回應，提取地址資訊
        /// </summary>
        private AddressInfoDTO ParseAddressInfo(GoogleGeocodingResult result, decimal latitude, decimal longitude)
        {
            var addressInfo = new AddressInfoDTO
            {
                FormattedAddress = result.Formatted_Address,
                Latitude = latitude,
                Longitude = longitude
            };

            // 解析地址組成元件
            foreach (var component in result.Address_Components)
            {
                // 國家
                if (component.Types.Contains("country"))
                {
                    addressInfo.Country = component.Long_Name;
                }

                // 城市 (優先順序: locality > administrative_area_level_2 > administrative_area_level_1)
                if (component.Types.Contains("locality"))
                {
                    addressInfo.City = component.Long_Name;
                }
                else if (component.Types.Contains("administrative_area_level_2") && string.IsNullOrEmpty(addressInfo.City))
                {
                    addressInfo.City = component.Long_Name;
                }
                else if (component.Types.Contains("administrative_area_level_1") && string.IsNullOrEmpty(addressInfo.City))
                {
                    addressInfo.City = component.Long_Name;
                }

                // 區域 (sublocality 或 administrative_area_level_3)
                if (component.Types.Contains("sublocality") || component.Types.Contains("sublocality_level_1"))
                {
                    addressInfo.District = component.Long_Name;
                }
                else if (component.Types.Contains("administrative_area_level_3") && string.IsNullOrEmpty(addressInfo.District))
                {
                    addressInfo.District = component.Long_Name;
                }

                // 地點名稱 (point_of_interest, establishment)
                if (component.Types.Contains("point_of_interest") || component.Types.Contains("establishment"))
                {
                    addressInfo.PlaceName = component.Long_Name;
                }
            }

            // 完整地址
            addressInfo.Address = result.Formatted_Address;

            // 紀錄解析結果
            _logger.LogDebug("📍 解析結果 - 國家: {Country}, 城市: {City}, 區域: {District}, 地點: {PlaceName}",
                addressInfo.Country, addressInfo.City, addressInfo.District, addressInfo.PlaceName);

            return addressInfo;
        }
    }
}