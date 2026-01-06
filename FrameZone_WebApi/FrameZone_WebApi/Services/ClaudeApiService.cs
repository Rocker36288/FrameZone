using FrameZone_WebApi.Constants;
using FrameZone_WebApi.DTOs.AI;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FrameZone_WebApi.Services
{
    /// <summary>
    /// （保留類名/介面不變）改為呼叫 Azure OpenAI Responses API 的服務實作
    /// </summary>
    public class ClaudeApiService : IClaudeApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ClaudeApiService> _logger;
        private readonly IConfiguration _configuration;

        private readonly string _apiKey;
        private readonly string _deploymentName;

        // BaseAddress 會設定成 endpoint 的 base（例如 https://xxx.openai.azure.com/ 或 https://xxx.openai.azure.com/openai/v1/）
        private readonly string _baseUrl;

        // 相對路徑：可能是 "openai/v1/responses" 或 "responses"
        private readonly string _responsesPath;

        // JSON 序列化選項（camelCase）
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            //PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public ClaudeApiService(
            HttpClient httpClient,
            ILogger<ClaudeApiService> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;

            // 讀設定：優先讀你 appsettings.json 的 AIServices:AzureOpenAI:*，也支援 AI:AzureOpenAI:*（避免你之後改結構）
            _apiKey = GetRequiredConfig(
                "AIServices:AzureOpenAI:ApiKey",
                "AI:AzureOpenAI:ApiKey",
                "AzureOpenAI:ApiKey");

            var endpoint = GetRequiredConfig(
                "AIServices:AzureOpenAI:Endpoint",
                "AI:AzureOpenAI:Endpoint",
                "AzureOpenAI:Endpoint");

            _deploymentName = GetRequiredConfig(
                "AIServices:AzureOpenAI:Deployment",
                "AI:AzureOpenAI:Deployment",
                "AzureOpenAI:Deployment");

            // 正規化 endpoint / baseUrl / responsesPath
            // 支援使用者填：
            // 1) https://xxx.openai.azure.com/
            // 2) https://xxx.openai.azure.com/openai/v1/
            // 3) https://xxx.openai.azure.com/openai/v1/responses
            var endpointStr = endpoint.Trim();
            if (!endpointStr.EndsWith("/")) endpointStr += "/";

            // 若 endpoint 已包含 /openai/v1/，就把 BaseAddress 設為那層，path 只用 "responses"
            if (endpointStr.Contains("/openai/v1/", StringComparison.OrdinalIgnoreCase))
            {
                // endpoint 可能是 .../openai/v1/responses/
                var idx = endpointStr.IndexOf("/openai/v1/", StringComparison.OrdinalIgnoreCase);
                _baseUrl = endpointStr[..(idx + "/openai/v1/".Length)];
                _responsesPath = "responses";
            }
            else
            {
                _baseUrl = endpointStr;                 // 例如 https://xxx.openai.azure.com/
                _responsesPath = "openai/v1/responses"; // v1 GA 的 Responses API
            }

            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Azure OpenAI：api-key header
            _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);

            _httpClient.Timeout = TimeSpan.FromSeconds(120);
        }

        #region 核心分析功能

        /// <summary>
        /// 分析單張照片並提供智能分類建議
        /// </summary>
        public async Task<ClaudeAnalysisResultDto> AnalyzeSinglePhotoAsync(PhotoAnalysisContextDto request)
        {
            var startTime = DateTime.UtcNow;

            try
            {
                _logger.LogInformation("🧠 開始分析照片 PhotoId={PhotoId}", request.PhotoId);

                // 驗證輸入
                if (!ValidateAnalysisRequest(request, out var validationError))
                    return CreateErrorResult(validationError ?? "請求無效");

                // 建立 Azure OpenAI Responses API 請求
                var azureRequest = BuildAzureResponsesRequest(request);

                // 發送請求
                var (ok, outputText, errorType, errorMessage, rawBody) = await SendAzureOpenAiRequestAsync(azureRequest);

                if (!ok)
                {
                    var msg = string.IsNullOrWhiteSpace(errorMessage) ? rawBody ?? "未知錯誤" : errorMessage;
                    _logger.LogError("❌ Azure OpenAI 請求失敗 PhotoId={PhotoId}, Type={Type}, Error={Error}",
                        request.PhotoId, errorType, msg);

                    return CreateErrorResult($"Azure OpenAI 錯誤: {msg}", rawResponseText: rawBody, errorCode: errorType);
                }

                if (string.IsNullOrWhiteSpace(outputText))
                    return CreateErrorResult("Azure OpenAI 回應沒有 output_text", rawResponseText: rawBody);

                // 解析 JSON（沿用你原本的 ExtractJsonFromText 防呆）
                var semanticOutput = ExtractJsonFromText(outputText);
                if (semanticOutput == null)
                {
                    _logger.LogWarning("⚠️ 無法從 Azure OpenAI 回應中解析語義分析結果 PhotoId={PhotoId}", request.PhotoId);
                    return CreateErrorResult("無法解析 Azure OpenAI 的分析結果", rawResponseText: outputText);
                }

                var processingTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;

                _logger.LogInformation(
                    "✅ 照片分析成功 PhotoId={PhotoId}, 信心分數={Confidence}, 處理時間={Ms}ms",
                    request.PhotoId,
                    semanticOutput.ConfidenceScore,
                    processingTime);

                return new ClaudeAnalysisResultDto
                {
                    Success = true,
                    PhotoId = request.PhotoId,
                    SemanticOutput = semanticOutput,
                    RawResponse = outputText,
                    TokenUsage = null, // Azure Responses usage 若你需要可再映射；先不強依賴 DTO 結構
                    ProcessingTimeMs = processingTime
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 分析照片時發生例外 PhotoId={PhotoId}", request.PhotoId);
                return CreateErrorResult($"系統錯誤: {ex.Message}");
            }
        }

        #endregion

        #region Prompt 建構（沿用你原本的內容）

        /// <summary>
        /// 建立 Azure OpenAI Responses API request body
        /// </summary>
        private object BuildAzureResponsesRequest(PhotoAnalysisContextDto context)
        {
            var content = new List<object>
            {
                new { type = "input_text", text = BuildAnalysisPrompt(context) }
            };

            // 照片縮圖（base64 data URL）
            if (!string.IsNullOrEmpty(context.ThumbnailBase64))
            {
                var mime = context.ThumbnailMediaType ?? "image/jpeg";
                content.Add(new
                {
                    type = "input_image",
                    image_url = $"data:{mime};base64,{context.ThumbnailBase64}"
                });
            }

            var req = new Dictionary<string, object?>
            {
                ["model"] = _deploymentName,
                ["instructions"] = BuildSystemPrompt(context),
                ["input"] = new object[] { new { role = "user", content } },
                ["max_output_tokens"] = context.Options.MaxTokens,
                ["text"] = new { format = new { type = "json_object" } },
                ["store"] = false
            };

            // 若你未來換成支援 temperature 的模型，再打開
            // if (ModelSupportsTemperature(_deploymentName))
            //     req["temperature"] = context.Options.Temperature;

            return req;
        }

        /// <summary>
        /// 建立系統提示
        /// </summary>
        private string BuildSystemPrompt(PhotoAnalysisContextDto context)
        {
            var prompt = new StringBuilder();

            prompt.AppendLine("你是一個專業的照片分類助手，擅長分析照片內容並提供智能分類建議。");
            prompt.AppendLine();

            prompt.AppendLine("🏛️ **【核心任務】地標與建築物識別**");
            prompt.AppendLine();
            prompt.AppendLine("這是你最重要的任務！請用以下步驟仔細分析照片：");
            prompt.AppendLine();

            prompt.AppendLine("**步驟1：建築物特徵識別**");
            prompt.AppendLine("仔細觀察照片中的建築物，注意以下特徵：");
            prompt.AppendLine("  1️⃣ 整體外形：是否為超高層建築、塔狀結構、特殊造型");
            prompt.AppendLine("  2️⃣ 顏色與材質：玻璃帷幕顏色（綠色/藍色/金色）、金屬材質、石材");
            prompt.AppendLine("  3️⃣ 結構特點：分節式結構、斜撐、獨特頂部、天線");
            prompt.AppendLine("  4️⃣ 高度感：是否明顯高於周圍建築、是否穿透雲層");
            prompt.AppendLine("  5️⃣ 環境線索：周圍的城市天際線、山脈背景、其他知名建築");
            prompt.AppendLine();

            prompt.AppendLine("**步驟2：知名地標比對**");
            prompt.AppendLine();
            prompt.AppendLine("🇹🇼 **台灣地標（重點識別）**");
            prompt.AppendLine("  • 台北101：綠色玻璃帷幕、8段分節式結構（每段8層樓，象徵竹節）、高508米、斜撐外露、頂部尖塔、常有LED燈光秀");
            prompt.AppendLine("  • 台北101特徵：如果看到綠色/藍綠色的超高層建築，分段式結構，幾乎可以確定是台北101");
            prompt.AppendLine("  • 中正紀念堂：藍白色建築、八角形屋頂、大型廣場");
            prompt.AppendLine("  • 圓山大飯店：紅色中式宮殿建築、14層樓高、金色屋頂");
            prompt.AppendLine("  • 台北車站：現代化大型車站建築");
            prompt.AppendLine();

            prompt.AppendLine("🌍 **國際知名地標**");
            prompt.AppendLine("  • 艾菲爾鐵塔（法國）：鐵製鏤空結構、三層平台、324米高");
            prompt.AppendLine("  • 東京晴空塔（日本）：白色/銀色、634米高、三角形橫截面");
            prompt.AppendLine("  • 東京鐵塔（日本）：紅白相間、類似艾菲爾鐵塔但較小");
            prompt.AppendLine("  • 自由女神像（美國）：綠色銅像、手持火炬、位於小島");
            prompt.AppendLine("  • 雪梨歌劇院（澳洲）：白色帆船形建築、位於海港");
            prompt.AppendLine("  • 金門大橋（美國）：橘紅色懸索橋、雙塔結構");
            prompt.AppendLine("  • 倫敦眼（英國）：巨型摩天輪、泰晤士河畔");
            prompt.AppendLine("  • 比薩斜塔（義大利）：白色大理石、明顯傾斜");
            prompt.AppendLine("  • 泰姬瑪哈陵（印度）：白色大理石、對稱結構、圓頂");
            prompt.AppendLine("  • 布爾吉·哈里發塔（杜拜）：828米、Y形橫截面、世界最高建築");
            prompt.AppendLine();

            prompt.AppendLine("**步驟3：即使局部或遠景也要識別**");
            prompt.AppendLine("  ⚠️ 重要：即使建築物被雲層遮擋、只拍到一部分、或者在遠景中，也請嘗試識別！");
            prompt.AppendLine("  ⚠️ 提示：看到綠色/藍綠色的超高層分節式建築，99%是台北101！");
            prompt.AppendLine("  ⚠️ 提示：如果照片在台灣拍攝，出現超高層建築，首先考慮台北101");
            prompt.AppendLine();

            prompt.AppendLine("**步驟4：標籤生成要求**");
            prompt.AppendLine("如果識別出知名地標，必須在 suggestedTags 中包含：");
            prompt.AppendLine("  1. 地標的完整名稱（例如：台北101、艾菲爾鐵塔）");
            prompt.AppendLine("  2. 建築類型標籤（例如：摩天大樓、塔、橋樑、紀念碑）");
            prompt.AppendLine("  3. 地理位置標籤（例如：台北、信義區、巴黎、紐約）");
            prompt.AppendLine("  4. 視覺特徵標籤（例如：綠色玻璃帷幕、分節式結構、超高層建築）");
            prompt.AppendLine();

            prompt.AppendLine("你的任務：");
            prompt.AppendLine("1. 仔細觀察照片中的所有元素（建築物、景觀、物體、人物、活動等）");
            prompt.AppendLine("2. **優先分析建築物特徵**（外形、高度、顏色、結構、分段方式、材質）");

            prompt.AppendLine("3. 分析照片的視覺內容和場景");
            prompt.AppendLine("4. 判斷照片是否在旅遊景點拍攝");
            prompt.AppendLine("5. 推理照片的深層語義和故事脈絡");
            prompt.AppendLine("6. 為照片生成準確的標籤和描述");
            prompt.AppendLine();
            prompt.AppendLine("輸出格式要求：");
            prompt.AppendLine("請以 JSON 格式回應，結構如下：");

            //prompt.AppendLine("{");
            //prompt.AppendLine("  \"isTouristSpot\": true/false,  // 是否為旅遊景點");
            //prompt.AppendLine("  \"spotName\": \"景點名稱\",  // 如果是旅遊景點，給出景點名稱");
            //prompt.AppendLine("  \"confidence\": 0.0-1.0,  // 判斷的信心分數");
            //prompt.AppendLine("  \"suggestedTags\": [\"標籤1\", \"標籤2\"],  // 建議的標籤列表（3-8個）");
            //prompt.AppendLine("  \"description\": \"照片描述\",  // 人類可讀的照片描述");
            //prompt.AppendLine("  \"category\": \"類別\",  // 建議的相簿分類（風景/美食/人物/建築/活動/其他）");
            //prompt.AppendLine("  \"reasoning\": \"判斷理由\"  // 簡短說明判斷的依據");
            //prompt.AppendLine("}");
            prompt.AppendLine("{");
            prompt.AppendLine("  \"is_tourist_spot\": true/false,");
            prompt.AppendLine("  \"spot_name\": \"景點名稱\",");
            prompt.AppendLine("  \"spot_type\": \"景點類型(可選)\",");
            prompt.AppendLine("  \"confidence\": 0.0-1.0,");
            prompt.AppendLine("  \"suggested_tags\": [\"標籤1\", \"標籤2\"],");
            prompt.AppendLine("  \"description\": \"照片描述\",");
            prompt.AppendLine("  \"reasoning\": \"判斷理由\",");
            prompt.AppendLine("  \"suggested_album_categories\": [\"建築\"],");
            prompt.AppendLine("  \"recommended_place_id\": \"(可選)\",");
            prompt.AppendLine("  \"historical_context\": \"(可選)\"");
            prompt.AppendLine("}");
            prompt.AppendLine();

            prompt.AppendLine("重要原則：");
            prompt.AppendLine("- 標籤應該具體且有用，避免過於籠統（例如「東京」比「亞洲」更有用）");
            prompt.AppendLine("- 只有在有充分證據時才將照片標記為「旅遊景點」");
            prompt.AppendLine("- 如果資訊不足，寧可保守判斷，不要過度推測");
            prompt.AppendLine("- 描述應該自然流暢，像是在跟朋友分享照片故事");

            if (context.Options.IncludeHistoricalContext)
            {
                prompt.AppendLine("- 如果是知名景點，可以簡短提及歷史或文化背景");
            }

            return prompt.ToString();
        }

        /// <summary>
        /// 建立分析提示文字
        /// </summary>
        private string BuildAnalysisPrompt(PhotoAnalysisContextDto context)
        {
            var prompt = new StringBuilder();

            prompt.AppendLine("請分析以下照片並提供分類建議：");
            prompt.AppendLine();

            // 檢查是否有完整的 EXIF 資訊
            bool hasLocation = context.Exif?.Latitude.HasValue == true && context.Exif?.Longitude.HasValue == true;
            bool hasDateTime = context.Exif?.DateTaken.HasValue == true;

            if (hasLocation || hasDateTime)
            {
                prompt.AppendLine("📍 EXIF 資訊：");

                if (hasDateTime)
                    prompt.AppendLine($"  拍攝時間: {context.Exif!.DateTaken!.Value:yyyy-MM-dd HH:mm:ss}");

                if (hasLocation)
                    prompt.AppendLine($"  GPS 座標: {context.Exif!.Latitude}, {context.Exif.Longitude}");

                if (!string.IsNullOrEmpty(context.Exif?.CameraMake) || !string.IsNullOrEmpty(context.Exif?.CameraModel))
                    prompt.AppendLine($"  相機: {context.Exif?.CameraMake} {context.Exif?.CameraModel}".Trim());

                prompt.AppendLine();
            }
            else
            {
                prompt.AppendLine("⚠️ 注意：此照片缺少 EXIF 資訊（無 GPS 座標或拍攝時間）");
                prompt.AppendLine("請僅根據視覺內容進行分析，不要嘗試猜測拍攝地點。");
                prompt.AppendLine();
            }

            // Azure Vision 分析結果
            if (context.AzureVision != null)
            {
                prompt.AppendLine("👁️ 視覺分析結果（Azure Computer Vision）：");

                if (context.AzureVision.Objects.Any())
                    prompt.AppendLine($"  辨識物體: {string.Join(", ", context.AzureVision.Objects)}");

                if (context.AzureVision.Tags.Any())
                    prompt.AppendLine($"  標籤: {string.Join(", ", context.AzureVision.Tags)}");

                if (context.AzureVision.DominantColors.Any())
                    prompt.AppendLine($"  主要顏色: {string.Join(", ", context.AzureVision.DominantColors)}");

                if (!string.IsNullOrEmpty(context.AzureVision.Caption))
                    prompt.AppendLine($"  照片描述: {context.AzureVision.Caption}");

                if (context.AzureVision.IsAdultContent || context.AzureVision.IsRacyContent)
                    prompt.AppendLine("  ⚠️ 內容警告: 此照片包含敏感內容");

                prompt.AppendLine();
            }

            // Google Places 分析結果
            if (context.GooglePlaces != null)
            {
                if (context.GooglePlaces.Geocode != null)
                {
                    prompt.AppendLine("🌍 地理位置資訊（Google Places）：");

                    var geo = context.GooglePlaces.Geocode;
                    if (!string.IsNullOrEmpty(geo.Country)) prompt.AppendLine($"  國家: {geo.Country}");
                    if (!string.IsNullOrEmpty(geo.City)) prompt.AppendLine($"  城市: {geo.City}");
                    if (!string.IsNullOrEmpty(geo.District)) prompt.AppendLine($"  地區: {geo.District}");
                    if (!string.IsNullOrEmpty(geo.FullAddress)) prompt.AppendLine($"  地址: {geo.FullAddress}");

                    prompt.AppendLine();
                }

                if (context.GooglePlaces.TouristSpot != null)
                {
                    prompt.AppendLine("🎯 景點識別結果（Google Places 智能分析）：");

                    var spot = context.GooglePlaces.TouristSpot;
                    prompt.AppendLine($"  景點名稱: {spot.SpotName}");
                    prompt.AppendLine($"  信心分數: {spot.Confidence:F2}");
                    prompt.AppendLine($"  距離: {spot.DistanceMeters:F0} 公尺");

                    if (spot.SpotTypes.Any())
                        prompt.AppendLine($"  景點類型: {string.Join(", ", spot.SpotTypes)}");

                    if (spot.Rating.HasValue)
                        prompt.AppendLine($"  評分: {spot.Rating:F1}/5.0 ({spot.ReviewCount} 則評論)");

                    prompt.AppendLine($"  判斷依據: {spot.Evidence}");
                    prompt.AppendLine();
                }
                else if (context.GooglePlaces.CandidateSpots.Any())
                {
                    prompt.AppendLine("📍 附近的候選景點（Google Places）：");

                    foreach (var candidate in context.GooglePlaces.CandidateSpots.Take(5))
                    {
                        prompt.AppendLine($"  - {candidate.Name}");
                        var ratingText = candidate.Rating.HasValue ? $", 評分: {candidate.Rating:F1}/5.0" : "";
                        prompt.AppendLine($"    距離: {candidate.DistanceMeters:F0} 公尺{ratingText}");
                        prompt.AppendLine($"    類型: {string.Join(", ", candidate.Types)}");
                    }

                    prompt.AppendLine();
                    prompt.AppendLine("提示：這些是附近的景點，但無法確定照片是在哪個景點拍攝的。");
                    prompt.AppendLine("請根據照片的視覺內容和這些候選景點資訊，判斷最可能的景點。");
                    prompt.AppendLine();
                }
            }

            // 分析指引
            prompt.AppendLine("---");
            prompt.AppendLine("請綜合以上資訊，回答以下問題：");
            prompt.AppendLine("1. 這張照片是在旅遊景點拍攝的嗎？");
            prompt.AppendLine("2. 如果是，景點名稱是什麼？你的信心分數是多少？");
            prompt.AppendLine("3. 這張照片應該標記什麼標籤？（3-8個，從具體到抽象）");
            prompt.AppendLine("4. 這張照片應該歸類到哪個相簿分類？");
            prompt.AppendLine("5. 如何用一句話描述這張照片？");
            prompt.AppendLine();
            prompt.AppendLine("請以 JSON 格式回應，不要包含任何 markdown 標記。");

            return prompt.ToString();
        }

        #endregion

        #region HTTP 請求處理（Azure OpenAI Responses API）

        private async Task<(bool Ok, string? OutputText, string? ErrorType, string? ErrorMessage, string? RawBody)>
            SendAzureOpenAiRequestAsync(object requestBody)
        {
            try
            {
                var json = JsonSerializer.Serialize(requestBody, _jsonOptions);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogDebug("📤 發送請求到 Azure OpenAI Responses API, Deployment={Deployment}", _deploymentName);

                var resp = await _httpClient.PostAsync(_responsesPath, httpContent);
                var body = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    // 嘗試解析 error 物件：{ "error": { "message": "...", "type": "..." } }
                    TryParseAzureError(body, out var errorType, out var errorMessage);
                    return (false, null, errorType, errorMessage, body);
                }

                var outputText = ExtractOutputTextFromResponses(body);
                return (true, outputText, null, null, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 發送 Azure OpenAI 請求時發生例外");
                return (false, null, "api_error", ex.Message, null);
            }
        }

        private static bool TryParseAzureError(string? body, out string? errorType, out string? errorMessage)
        {
            errorType = null;
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(body)) return false;

            try
            {
                using var doc = JsonDocument.Parse(body);
                if (!doc.RootElement.TryGetProperty("error", out var err)) return false;

                if (err.TryGetProperty("type", out var t) && t.ValueKind == JsonValueKind.String)
                    errorType = t.GetString();

                if (err.TryGetProperty("message", out var m) && m.ValueKind == JsonValueKind.String)
                    errorMessage = m.GetString();

                return errorType != null || errorMessage != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 從 Responses API 回應抽出 output_text
        /// </summary>
        private static string? ExtractOutputTextFromResponses(string responseJson)
        {
            using var doc = JsonDocument.Parse(responseJson);

            // 有些回應可能提供 output_text 便捷欄位
            if (doc.RootElement.TryGetProperty("output_text", out var ot) && ot.ValueKind == JsonValueKind.String)
                return ot.GetString();

            // 標準：output[] -> message -> content[] -> output_text
            if (!doc.RootElement.TryGetProperty("output", out var output) || output.ValueKind != JsonValueKind.Array)
                return null;

            foreach (var item in output.EnumerateArray())
            {
                if (item.TryGetProperty("type", out var type) && type.GetString() != "message") continue;
                if (item.TryGetProperty("role", out var role) && role.GetString() != "assistant") continue;
                if (!item.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array) continue;

                foreach (var part in content.EnumerateArray())
                {
                    if (part.TryGetProperty("type", out var pt) && pt.GetString() == "output_text" &&
                        part.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String)
                    {
                        return text.GetString();
                    }
                }
            }

            return null;
        }

        #endregion

        #region 回應解析（沿用你的 JSON 抽取）

        private TouristSpotSemanticOutputDto? ExtractJsonFromText(string text)
        {
            try
            {
                var cleanText = (text ?? string.Empty).Trim();

                // 1) 去掉 ```json / ``` 包裹
                if (cleanText.Contains("```json", StringComparison.OrdinalIgnoreCase))
                {
                    var startIndex = cleanText.IndexOf("```json", StringComparison.OrdinalIgnoreCase) + 7;
                    var endIndex = cleanText.IndexOf("```", startIndex, StringComparison.OrdinalIgnoreCase);
                    if (endIndex > startIndex)
                        cleanText = cleanText.Substring(startIndex, endIndex - startIndex).Trim();
                }
                else if (cleanText.Contains("```", StringComparison.OrdinalIgnoreCase))
                {
                    var startIndex = cleanText.IndexOf("```", StringComparison.OrdinalIgnoreCase) + 3;
                    var endIndex = cleanText.IndexOf("```", startIndex, StringComparison.OrdinalIgnoreCase);
                    if (endIndex > startIndex)
                        cleanText = cleanText.Substring(startIndex, endIndex - startIndex).Trim();
                }

                // 2) 抽出第一個 { 到最後一個 } 的 JSON
                var jsonStart = cleanText.IndexOf('{');
                var jsonEnd = cleanText.LastIndexOf('}');
                if (jsonStart < 0 || jsonEnd <= jsonStart) return null;

                var jsonText = cleanText.Substring(jsonStart, jsonEnd - jsonStart + 1);

                // 3) 容錯解析 + 欄位對應（同時支援 snake_case / camelCase）
                using var doc = JsonDocument.Parse(jsonText, new JsonDocumentOptions
                {
                    AllowTrailingCommas = true,
                    CommentHandling = JsonCommentHandling.Skip
                });

                if (doc.RootElement.ValueKind != JsonValueKind.Object) return null;

                var root = doc.RootElement;

                var dto = new TouristSpotSemanticOutputDto
                {
                    IsTouristSpot = GetBool(root, "is_tourist_spot", "isTouristSpot"),
                    SpotName = GetString(root, "spot_name", "spotName"),
                    SpotType = GetString(root, "spot_type", "spotType"),
                    Confidence = GetDouble(root, "confidence", "confidence_score", "confidenceScore"),
                    Description = GetString(root, "description"),
                    RecommendedPlaceId = GetString(root, "recommended_place_id", "recommendedPlaceId"),
                    Reasoning = GetString(root, "reasoning"),
                    HistoricalContext = GetString(root, "historical_context", "historicalContext"),
                    SuggestedTags = GetStringList(root, "suggested_tags", "suggestedTags") ?? new List<string>()
                };

                var cats = GetStringList(root, "suggested_album_categories", "suggestedAlbumCategories");
                if (cats is { Count: > 0 })
                {
                    dto.SuggestedAlbumCategories = cats;
                }
                else
                {
                    // 你的 prompt 是 category: "建築"（單字串），DTO 目前是 list，所以做個轉換
                    var category = GetString(root, "category");
                    if (!string.IsNullOrWhiteSpace(category))
                        dto.SuggestedAlbumCategories = new List<string> { category! };
                }

                return dto;
            }
            catch (JsonException ex)
            {
                _logger.LogWarning("⚠️ JSON 解析失敗: {Message}", ex.Message);
                return null;
            }
        }

        private static bool GetBool(JsonElement obj, params string[] keys)
        {
            foreach (var k in keys)
            {
                if (!obj.TryGetProperty(k, out var v)) continue;

                if (v.ValueKind == JsonValueKind.True) return true;
                if (v.ValueKind == JsonValueKind.False) return false;

                if (v.ValueKind == JsonValueKind.String && bool.TryParse(v.GetString(), out var b))
                    return b;

                if (v.ValueKind == JsonValueKind.Number && v.TryGetInt32(out var n))
                    return n != 0;
            }
            return false;
        }

        private static string? GetString(JsonElement obj, params string[] keys)
        {
            foreach (var k in keys)
            {
                if (!obj.TryGetProperty(k, out var v)) continue;

                if (v.ValueKind == JsonValueKind.String) return v.GetString();
                // 有些模型可能回 number/bool，這裡也轉字串
                if (v.ValueKind is JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False)
                    return v.ToString();
            }
            return null;
        }

        private static double GetDouble(JsonElement obj, params string[] keys)
        {
            foreach (var k in keys)
            {
                if (!obj.TryGetProperty(k, out var v)) continue;

                if (v.ValueKind == JsonValueKind.Number && v.TryGetDouble(out var d))
                    return d;

                if (v.ValueKind == JsonValueKind.String)
                {
                    var s = v.GetString();
                    if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var ds))
                        return ds;
                    if (double.TryParse(s, NumberStyles.Any, CultureInfo.CurrentCulture, out ds))
                        return ds;
                }
            }
            return 0d;
        }

        private static List<string>? GetStringList(JsonElement obj, params string[] keys)
        {
            foreach (var k in keys)
            {
                if (!obj.TryGetProperty(k, out var v)) continue;

                if (v.ValueKind == JsonValueKind.Array)
                {
                    var list = new List<string>();
                    foreach (var item in v.EnumerateArray())
                    {
                        if (item.ValueKind == JsonValueKind.String)
                        {
                            var s = item.GetString();
                            if (!string.IsNullOrWhiteSpace(s)) list.Add(s!);
                        }
                        else if (item.ValueKind is JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False)
                        {
                            var s = item.ToString();
                            if (!string.IsNullOrWhiteSpace(s)) list.Add(s);
                        }
                    }
                    return list;
                }

                // 如果模型回傳單一字串而不是 array，也容錯
                if (v.ValueKind == JsonValueKind.String)
                {
                    var s = v.GetString();
                    if (!string.IsNullOrWhiteSpace(s)) return new List<string> { s! };
                }
            }
            return null;
        }

        #endregion

        #region 輔助方法

        private bool ValidateAnalysisRequest(PhotoAnalysisContextDto request, out string? errorMessage)
        {
            if (request.PhotoId <= 0)
            {
                errorMessage = "PhotoId 必須大於 0";
                return false;
            }

            // 如果沒有縮圖，至少要有 Azure Vision 的結果
            if (string.IsNullOrEmpty(request.ThumbnailBase64) && request.AzureVision == null)
            {
                errorMessage = "必須提供照片縮圖或 Azure Vision 分析結果";
                return false;
            }

            errorMessage = null;
            return true;
        }

        private ClaudeAnalysisResultDto CreateErrorResult(string errorMessage, string? rawResponseText = null, string? errorCode = null)
        {
            return new ClaudeAnalysisResultDto
            {
                Success = false,
                ErrorMessage = errorMessage,
                ErrorCode = errorCode,
                RawResponse = rawResponseText
            };
        }

        private string GetRequiredConfig(params string[] keys)
        {
            foreach (var k in keys)
            {
                var v = _configuration[k];
                if (!string.IsNullOrWhiteSpace(v))
                    return v!;
            }

            throw new InvalidOperationException($"必要設定未提供：{string.Join(" / ", keys)}");
        }

        #endregion

        #region 連線測試

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                _logger.LogInformation("🔌 測試 Azure OpenAI 連線...");

                var req = new
                {
                    model = _deploymentName,
                    input = new object[]
                    {
                        new
                        {
                            role = "user",
                            content = new object[]
                            {
                                new { type = "input_text", text = "請回覆「連線成功」（繁體中文），不要加任何多餘內容。" }
                            }
                        }
                    },
                    max_output_tokens = 50,
                    temperature = 0,
                    text = new { format = new { type = "text" } },
                    store = false
                };

                var (ok, outputText, _, errorMessage, rawBody) = await SendAzureOpenAiRequestAsync(req);

                if (ok && !string.IsNullOrWhiteSpace(outputText))
                {
                    _logger.LogInformation("✅ Azure OpenAI 連線測試成功：{Reply}", outputText);
                    return true;
                }

                _logger.LogWarning("❌ Azure OpenAI 連線測試失敗：{Error} Body={Body}", errorMessage, rawBody);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 測試 Azure OpenAI 連線時發生例外");
                return false;
            }
        }

        #endregion

        #region 未來擴展功能（待實作）

        public Task<ClaudeAnalysisResultDto> AnalyzePhotoGroupAsync(PhotoGroupAnalysisContextDto request)
        {
            throw new NotImplementedException("照片群組分析功能尚未實作");
        }

        #endregion
    }
}