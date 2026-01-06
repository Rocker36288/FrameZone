using FrameZone_WebApi.Constants;
using FrameZone_WebApi.DTOs.AI;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FrameZone_WebApi.Services
{
    /// <summary>
    /// Azure Computer Vision API 服務實作
    /// 這個服務負責所有與 Azure Computer Vision 4.0 的互動
    /// </summary>
    public class AzureComputerVisionService : IAzureComputerVisionService
    {
        #region 依賴注入與建構子

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AzureComputerVisionService> _logger;

        // 從 appsettings.json 讀取的 Azure 設定
        private readonly string _endpoint;
        private readonly string _apiKey;
        private readonly string _region;

        public AzureComputerVisionService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<AzureComputerVisionService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;

            // 從設定檔讀取 Azure 配置
            // 優先使用 AIConstants（如果你專案其他地方仍依賴它），但同時支援直接從 appsettings.json 讀取：
            // - AIServices:AzureComputerVision:{Endpoint,SubscriptionKey,Region}
            // - AzureComputerVision:{Endpoint,SubscriptionKey,Region}
            var endpoint = string.Empty;
            var apiKey = string.Empty;
            var region = string.Empty;

            // 1) 先嘗試 AIConstants 指定的 section/key（相容舊版）
            var azureSection = _configuration.GetSection(AIConstants.Azure.ConfigSection);
            endpoint = azureSection[AIConstants.Azure.EndpointKey] ?? string.Empty;
            apiKey = azureSection[AIConstants.Azure.ApiKeyKey] ?? string.Empty;
            region = azureSection[AIConstants.Azure.RegionKey] ?? string.Empty;

            // 2) fallback：AIServices:AzureComputerVision
            if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(apiKey))
            {
                var s = _configuration.GetSection("AIServices:AzureComputerVision");
                endpoint = endpoint.Trim().Length > 0 ? endpoint : (s["Endpoint"] ?? string.Empty);
                apiKey = apiKey.Trim().Length > 0 ? apiKey : (s["SubscriptionKey"] ?? s["ApiKey"] ?? string.Empty);
                region = region.Trim().Length > 0 ? region : (s["Region"] ?? string.Empty);
            }

            // 3) fallback：AzureComputerVision
            if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(apiKey))
            {
                var s = _configuration.GetSection("AzureComputerVision");
                endpoint = endpoint.Trim().Length > 0 ? endpoint : (s["Endpoint"] ?? string.Empty);
                apiKey = apiKey.Trim().Length > 0 ? apiKey : (s["SubscriptionKey"] ?? s["ApiKey"] ?? string.Empty);
                region = region.Trim().Length > 0 ? region : (s["Region"] ?? string.Empty);
            }

            if (string.IsNullOrWhiteSpace(endpoint))
                throw new InvalidOperationException("Azure Computer Vision Endpoint 未設定（請設定 AIServices:AzureComputerVision:Endpoint 或 AzureComputerVision:Endpoint）");

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("Azure Computer Vision API Key 未設定（請設定 AIServices:AzureComputerVision:SubscriptionKey 或 AzureComputerVision:SubscriptionKey）");

            _endpoint = endpoint;
            _apiKey = apiKey;
            _region = string.IsNullOrWhiteSpace(region) ? "eastus" : region;

            _logger.LogInformation("🔧 Azure Computer Vision Service 初始化完成，Region: {Region}", _region);

        }

        #endregion

        #region 核心分析方法

        /// <summary>
        /// 分析照片內容
        /// 這是整個服務最核心的方法，執行完整的照片分析流程
        /// </summary>
        public async Task<AzureVisionAnalysisDto> AnalyzeImageAsync(byte[] imageData, List<string> features)
        {
            try
            {
                _logger.LogInformation($"🔍 開始 Azure Vision 分析，Features: {string.Join(", ", features)}");

                // 步驟 1：驗證照片是否符合 Azure 的要求
                // 這一步很重要，因為如果照片太大，Azure 會直接拒絕，浪費 API 配額
                // 注意：ValidateImageAsync 返回三元組，第三個值表示是否建議使用縮圖
                var (isValid, errorMessage, shouldUseThumbnail) = await ValidateImageAsync(imageData);
                if (!isValid)
                {
                    _logger.LogWarning($"⚠️ 照片驗證失敗: {errorMessage}");
                    return new AzureVisionAnalysisDto
                    {
                        Success = false,
                        ErrorMessage = errorMessage,
                        ErrorCode = AIConstants.ErrorCodes.Azure.InvalidImage
                    };
                }

                // 步驟 2：建立 HTTP 客戶端
                // 使用 IHttpClientFactory 而不是直接 new HttpClient，這是 .NET 的最佳實踐
                // 原因：HttpClient 如果不正確管理會導致 socket 耗盡問題
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(AIConstants.Timeout.AzureSeconds);

                // 步驟 3：構建完整的 API URL
                // Azure API 使用查詢參數來指定要分析的特徵
                // 例如：https://xxx.cognitiveservices.azure.com/vision/v4.0/analyze?features=Objects,Tags
                var apiUrl = BuildAnalysisUrl(features);
                _logger.LogInformation($"📍 API URL: {apiUrl}");

                // 步驟 4：準備 HTTP 請求
                var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);

                // 設定必要的 HTTP Headers
                // Ocp-Apim-Subscription-Key 是 Azure Cognitive Services 的標準認證方式
                request.Headers.Add("Ocp-Apim-Subscription-Key", _apiKey);

                // Content-Type 設為 application/octet-stream 表示我們發送的是二進位資料
                request.Content = new ByteArrayContent(imageData);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                // 步驟 5：發送請求並取得回應
                _logger.LogInformation($"📤 發送請求到 Azure，照片大小: {imageData.Length / 1024.0:F2} KB");
                var startTime = DateTime.UtcNow;

                var response = await client.SendAsync(request);

                var processingTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.LogInformation($"⏱️ Azure 處理時間: {processingTime} ms");

                // 步驟 6：讀取並解析回應
                var responseContent = await response.Content.ReadAsStringAsync();

                // 步驟 7：處理不同的 HTTP 狀態碼
                if (!response.IsSuccessStatusCode)
                {
                    return await HandleErrorResponseAsync(response, responseContent);
                }

                // 步驟 8：解析成功的 JSON 回應
                // 使用 JsonSerializer 而不是 Newtonsoft.Json，這是 .NET 6+ 的標準做法
                var analysisResult = JsonSerializer.Deserialize<AzureVisionAnalysisDto>(
                    responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (analysisResult == null)
                {
                    _logger.LogError($"❌ 無法解析 Azure 回應: {responseContent}");
                    return new AzureVisionAnalysisDto
                    {
                        Success = false,
                        ErrorMessage = "無法解析 Azure 回應",
                        ErrorCode = AIConstants.ErrorCodes.General.InvalidResponse
                    };
                }

                // 步驟 9：補充額外資訊並返回
                analysisResult.Success = true;
                analysisResult.ProcessingTimeMs = processingTime;
                analysisResult.RawResponse = responseContent;

                // 記錄照片大小資訊（用於追蹤和分析）
                analysisResult.AnalyzedImageSizeMB = imageData.Length / (1024.0 * 1024.0);

                _logger.LogInformation($"✅ Azure Vision 分析成功");
                _logger.LogInformation($"📊 照片大小: {analysisResult.AnalyzedImageSizeMB:F2} MB");
                _logger.LogInformation($"📊 找到 Objects: {analysisResult.Objects?.Count ?? 0} 個");
                _logger.LogInformation($"📊 找到 Tags: {analysisResult.Tags?.Count ?? 0} 個");

                return analysisResult;
            }
            catch (HttpRequestException ex)
            {
                // 網路相關錯誤（連線失敗、DNS 解析失敗等）
                _logger.LogError(ex, "❌ Azure API 請求失敗（網路錯誤）");
                return new AzureVisionAnalysisDto
                {
                    Success = false,
                    ErrorMessage = $"網路連線錯誤: {ex.Message}",
                    ErrorCode = AIConstants.ErrorCodes.General.NetworkError
                };
            }
            catch (TaskCanceledException ex)
            {
                // 超時錯誤
                _logger.LogError(ex, "❌ Azure API 請求超時");
                return new AzureVisionAnalysisDto
                {
                    Success = false,
                    ErrorMessage = "請求超時，請稍後再試",
                    ErrorCode = AIConstants.ErrorCodes.General.Timeout
                };
            }
            catch (Exception ex)
            {
                // 其他未預期的錯誤
                _logger.LogError(ex, "❌ Azure Vision 分析時發生未預期錯誤");
                return new AzureVisionAnalysisDto
                {
                    Success = false,
                    ErrorMessage = $"系統錯誤: {ex.Message}",
                    ErrorCode = AIConstants.ErrorCodes.General.NetworkError
                };
            }
        }

        /// <summary>
        /// 從 URL 分析照片（未來擴充）
        /// 當照片已經在網路上（例如已上傳到 Azure Blob Storage）時，
        /// 使用這個方法會更有效率，因為不需要傳輸整個照片內容
        /// </summary>
        public async Task<AzureVisionAnalysisDto> AnalyzeImageFromUrlAsync(string imageUrl, List<string> features)
        {
            try
            {
                _logger.LogInformation($"🔍 開始 Azure Vision 分析（URL模式），URL: {imageUrl}");

                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(AIConstants.Timeout.AzureSeconds);

                var apiUrl = BuildAnalysisUrl(features);
                var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                request.Headers.Add("Ocp-Apim-Subscription-Key", _apiKey);

                // URL 模式使用 JSON 格式傳送，而不是二進位資料
                var requestBody = new { url = imageUrl };
                request.Content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                var startTime = DateTime.UtcNow;
                var response = await client.SendAsync(request);
                var processingTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;

                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return await HandleErrorResponseAsync(response, responseContent);
                }

                var analysisResult = JsonSerializer.Deserialize<AzureVisionAnalysisDto>(
                    responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (analysisResult != null)
                {
                    analysisResult.Success = true;
                    analysisResult.ProcessingTimeMs = processingTime;
                    analysisResult.RawResponse = responseContent;
                }

                _logger.LogInformation($"✅ Azure Vision 分析成功（URL模式）");
                return analysisResult ?? new AzureVisionAnalysisDto
                {
                    Success = false,
                    ErrorMessage = "無法解析 Azure 回應"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Azure Vision 分析失敗（URL模式）");
                return new AzureVisionAnalysisDto
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        #endregion

        #region 驗證與輔助方法

        /// <summary>
        /// 驗證照片是否符合 Azure 的要求
        /// 這個方法在發送請求前執行預先檢查，避免浪費 API 配額
        /// </summary>
        public async Task<(bool IsValid, string ErrorMessage, bool ShouldUseThumbnail)> ValidateImageAsync(byte[] imageData)
        {
            return await Task.Run(() =>
            {
                if (imageData == null || imageData.Length == 0)
                {
                    return (false, "照片資料為空", false);
                }

                var sizeInMB = imageData.Length / (1024.0 * 1024.0);

                // 如果照片超過 4MB，建議使用縮圖
                // 這不是錯誤，而是一個優化建議
                if (sizeInMB > AIConstants.Azure.Limits.MaxImageSizeMB)
                {
                    return (false,
                        $"原圖過大 ({sizeInMB:F2} MB)，建議使用縮圖進行分析",
                        true); // 第三個參數表示「應該使用縮圖」
                }

                return (true, string.Empty, false);
            });
        }

        /// <summary>
        /// 測試與 Azure 的連線
        /// 這個方法可以在系統啟動時呼叫，確保 Azure 服務配置正確
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                _logger.LogInformation("🔌 測試 Azure Computer Vision 連線");

                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(10);

                // 使用一個簡單的 GET 請求來測試端點是否可達
                // 注意：這不會消耗 API 配額
                var response = await client.GetAsync(_endpoint);

                var isConnected = response.StatusCode != System.Net.HttpStatusCode.NotFound;
                _logger.LogInformation(isConnected
                    ? "✅ Azure 連線測試成功"
                    : "⚠️ Azure 連線測試失敗");

                return isConnected;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Azure 連線測試發生錯誤");
                return false;
            }
        }

        /// <summary>
        /// 構建 Azure API 的完整 URL
        /// 這個方法將基礎端點和查詢參數組合成完整的 API URL
        /// </summary>
        private string BuildAnalysisUrl(List<string> features)
        {
            //// 移除端點 URL 尾部的斜線（如果有的話），避免重複斜線
            //var baseEndpoint = _endpoint.TrimEnd('/');

            //// // 將 features 列表轉換成逗號分隔的字串
            //// // 例如：["Objects", "Tags"] => "Objects,Tags"
            //// var featuresParam = string.Join(",", features);

            //// // 組合完整 URL
            //// // 範例結果：https://xxx.cognitiveservices.azure.com/vision/v4.0/analyze?features=Objects,Tags
            //// return $"{baseEndpoint}/{AIConstants.Azure.AnalyzeEndpoint}?features={featuresParam}";

            //var visualFeaturesParam = Uri.EscapeDataString(string.Join(",", features));
            //return $"{baseEndpoint}/vision/v3.2/analyze?visualFeatures={visualFeaturesParam}";

            var baseEndpoint = _endpoint.TrimEnd('/');
            var visualFeaturesParam = Uri.EscapeDataString(string.Join(",", features));

            var url = $"{baseEndpoint}/vision/v3.2/analyze?visualFeatures={visualFeaturesParam}";

            if (features.Any(f => f.Equals("Categories", StringComparison.OrdinalIgnoreCase)))
                url += "&details=Landmarks";

            return url;
        }

        /// <summary>
        /// 處理 Azure API 的錯誤回應
        /// 將 Azure 的錯誤訊息轉換成我們的標準格式
        /// </summary>
        private async Task<AzureVisionAnalysisDto> HandleErrorResponseAsync(
            HttpResponseMessage response,
            string responseContent)
        {
            _logger.LogWarning($"⚠️ Azure API 返回錯誤，StatusCode: {response.StatusCode}");
            _logger.LogWarning($"📄 錯誤內容: {responseContent}");

            // 根據不同的 HTTP 狀態碼返回對應的錯誤訊息
            var errorMessage = response.StatusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized => "Azure API 金鑰無效或已過期",
                System.Net.HttpStatusCode.BadRequest => "照片格式不正確或請求參數錯誤",
                System.Net.HttpStatusCode.NotFound => "Azure API 端點/版本路徑錯誤（404 Resource not found）",
                System.Net.HttpStatusCode.TooManyRequests => "API 請求頻率過高，請稍後再試",
                System.Net.HttpStatusCode.InternalServerError => "Azure 服務暫時無法使用",
                _ => $"Azure API 錯誤 ({response.StatusCode})"
            };

            var errorCode = response.StatusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized => AIConstants.ErrorCodes.Azure.Unauthorized,
                System.Net.HttpStatusCode.BadRequest => AIConstants.ErrorCodes.Azure.InvalidImage,
                System.Net.HttpStatusCode.TooManyRequests => AIConstants.ErrorCodes.Azure.RateLimitExceeded,
                _ => AIConstants.ErrorCodes.General.NetworkError
            };

            return await Task.FromResult(new AzureVisionAnalysisDto
            {
                Success = false,
                ErrorMessage = errorMessage,
                ErrorCode = errorCode,
                RawResponse = responseContent,
                HttpStatusCode = (int)response.StatusCode
            });
        }

        #endregion
    }
}