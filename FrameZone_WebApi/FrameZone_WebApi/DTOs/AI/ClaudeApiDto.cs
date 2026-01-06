using System.Text.Json;
using System.Text.Json.Serialization;
using FrameZone_WebApi.Constants;

namespace FrameZone_WebApi.DTOs.AI
{
    #region === Claude API Request DTOs ===

    /// <summary>
    /// Claude API 分析請求（Anthropic Messages API）
    /// API 文件：https://docs.anthropic.com/en/api/messages
    /// 這個 DTO 對應 Claude API 的完整請求格式
    /// </summary>
    public class ClaudeAnalysisRequestDto
    {
        /// <summary>
        /// 使用的模型（必填）
        /// 使用 AIConstants.Claude.Models 中定義的模型常數
        /// </summary>
        [JsonPropertyName("model")]
        public string Model { get; set; } = AIConstants.Claude.Models.Default;

        /// <summary>
        /// 最大生成 token 數（必填，範圍：1-8192）
        /// 控制 Claude 回應的最大長度
        /// </summary>
        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; } = AIConstants.Claude.TokenLimits.DefaultMaxTokens;

        /// <summary>
        /// 系統提示（可選，用於設定角色/任務）
        /// 定義 Claude 的角色、能力和輸出格式要求
        /// </summary>
        [JsonPropertyName("system")]
        public string? System { get; set; }

        /// <summary>
        /// 對話訊息列表（必填，至少一則 user 訊息）
        /// 支援多輪對話和圖文混合內容
        /// </summary>
        [JsonPropertyName("messages")]
        public List<ClaudeMessageDto> Messages { get; set; } = new();

        /// <summary>
        /// 溫度參數 (0.0 - 1.0)，控制輸出隨機性
        /// 較低的值產生較一致的輸出，較高的值產生較多樣的輸出
        /// </summary>
        [JsonPropertyName("temperature")]
        public double? Temperature { get; set; }

        /// <summary>
        /// Top-P 參數 (0.0 - 1.0)，核採樣閾值
        /// 控制候選 token 的累積機率閾值
        /// </summary>
        [JsonPropertyName("top_p")]
        public double? TopP { get; set; }

        /// <summary>
        /// Top-K 參數，限制候選 token 數量
        /// 從最可能的 K 個 token 中採樣
        /// </summary>
        [JsonPropertyName("top_k")]
        public int? TopK { get; set; }

        /// <summary>
        /// 停止序列列表（遇到即停止生成）
        /// 當輸出包含這些序列時，Claude 會停止生成
        /// </summary>
        [JsonPropertyName("stop_sequences")]
        public List<string>? StopSequences { get; set; }

        /// <summary>
        /// 串流模式（注意：true 時回應格式為 SSE，需另外處理）
        /// 目前系統使用非串流模式
        /// </summary>
        [JsonPropertyName("stream")]
        public bool Stream { get; set; } = false;

        /// <summary>
        /// 工具定義列表（可選，用於 Function Calling）
        /// 未來可用於讓 Claude 主動呼叫外部工具
        /// </summary>
        [JsonPropertyName("tools")]
        public List<ClaudeToolDto>? Tools { get; set; }

        /// <summary>
        /// 工具選擇策略（auto, any, tool）
        /// 控制 Claude 如何使用提供的工具
        /// </summary>
        [JsonPropertyName("tool_choice")]
        public object? ToolChoice { get; set; }

        /// <summary>
        /// 元資料（可選，用於 extended thinking 等功能）
        /// 可包含使用者 ID 等追蹤資訊
        /// </summary>
        [JsonPropertyName("metadata")]
        public ClaudeMetadataDto? Metadata { get; set; }
    }

    /// <summary>
    /// Claude 訊息 DTO
    /// 代表一則對話訊息，可以是使用者或助手的訊息
    /// 支援文字和圖片的混合內容
    /// </summary>
    public class ClaudeMessageDto
    {
        /// <summary>
        /// 角色（user 或 assistant）
        /// 使用 AIConstants.Claude.Role 中定義的常數
        /// </summary>
        [JsonPropertyName("role")]
        public string Role { get; set; } = AIConstants.Claude.Role.User;

        /// <summary>
        /// 訊息內容（支援圖文混合）
        /// 可以包含文字、圖片等多種類型的內容區塊
        /// </summary>
        [JsonPropertyName("content")]
        public List<ClaudeContentBlockDto> Content { get; set; } = new();
    }

    /// <summary>
    /// Claude 內容區塊 DTO
    /// 支援多種類型的內容：文字、圖片、思考過程、工具使用等
    /// 這是一個多型的 DTO，根據 Type 屬性決定使用哪些欄位
    /// </summary>
    public class ClaudeContentBlockDto
    {
        /// <summary>
        /// 內容類型（text, image, thinking, tool_use 等）
        /// 使用 AIConstants.Claude.ContentType 中定義的常數
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = AIConstants.Claude.ContentType.Text;

        /// <summary>
        /// 文字內容（當 type=text 時使用）
        /// 這是最常用的內容類型
        /// </summary>
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        /// <summary>
        /// 圖片來源（當 type=image 時使用）
        /// 描述圖片的來源和格式資訊
        /// </summary>
        [JsonPropertyName("source")]
        public ClaudeImageSourceDto? Source { get; set; }

        /// <summary>
        /// 思考內容（當 type=thinking 時使用）
        /// Extended Thinking 功能的輸出
        /// </summary>
        [JsonPropertyName("thinking")]
        public string? Thinking { get; set; }

        /// <summary>
        /// 工具使用 ID（當 type=tool_use 時使用）
        /// 用於追蹤工具呼叫
        /// </summary>
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>
        /// 工具名稱（當 type=tool_use 時使用）
        /// 指定要呼叫的工具
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// 工具輸入參數（當 type=tool_use 時使用）
        /// 傳遞給工具的參數
        /// </summary>
        [JsonPropertyName("input")]
        public object? Input { get; set; }

        /// <summary>
        /// 簽章（當 type=thinking 時使用，用於驗證）
        /// 驗證思考過程的真實性
        /// </summary>
        [JsonPropertyName("signature")]
        public string? Signature { get; set; }

        /// <summary>
        /// 保留未知欄位（用於未來 API 擴充）
        /// 確保向前相容性
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }

    /// <summary>
    /// 圖片來源 DTO
    /// 描述圖片的來源方式和格式資訊
    /// </summary>
    public class ClaudeImageSourceDto
    {
        /// <summary>
        /// 來源類型（base64 或 url）
        /// 使用 AIConstants.Claude.ImageSourceType 中定義的常數
        /// 目前系統只使用 base64 方式
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = AIConstants.Claude.ImageSourceType.Base64;

        /// <summary>
        /// 圖片的媒體類型（image/jpeg, image/png 等）
        /// 使用 AIConstants.Claude.ImageMediaType 中定義的常數
        /// 告訴 Claude API 圖片的格式
        /// </summary>
        [JsonPropertyName("media_type")]
        public string MediaType { get; set; } = AIConstants.Claude.ImageMediaType.Jpeg;

        /// <summary>
        /// Base64 編碼的圖片資料（當 type=base64 時使用）
        /// 不包含 data:image/jpeg;base64, 前綴，只有純粹的 base64 字串
        /// </summary>
        [JsonPropertyName("data")]
        public string? Data { get; set; }

        /// <summary>
        /// 圖片 URL（當 type=url 時使用，未來可能支援）
        /// 目前 Claude API 尚不支援 URL 方式
        /// </summary>
        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }

    /// <summary>
    /// Claude 工具定義 DTO
    /// 用於 Function Calling 功能
    /// </summary>
    public class ClaudeToolDto
    {
        /// <summary>
        /// 工具名稱
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 工具描述
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 輸入參數 schema（JSON Schema 格式）
        /// </summary>
        [JsonPropertyName("input_schema")]
        public object InputSchema { get; set; } = new { };
    }

    /// <summary>
    /// Claude 元資料 DTO
    /// 用於追蹤和除錯
    /// </summary>
    public class ClaudeMetadataDto
    {
        /// <summary>
        /// 使用者 ID（可選）
        /// 用於追蹤和分析使用情況
        /// </summary>
        [JsonPropertyName("user_id")]
        public string? UserId { get; set; }
    }

    #endregion

    #region === Claude API Response DTOs ===

    /// <summary>
    /// Claude API 成功回應（type=message）
    /// 這是 Claude API 成功呼叫後的回應格式
    /// </summary>
    public class ClaudeMessageResponseDto
    {
        /// <summary>
        /// 回應 ID
        /// 可用於追蹤和除錯
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 類型（固定為 "message"）
        /// 用於區分成功回應和錯誤回應
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = "message";

        /// <summary>
        /// 角色（固定為 "assistant"）
        /// Claude 的回應總是 assistant 角色
        /// </summary>
        [JsonPropertyName("role")]
        public string Role { get; set; } = AIConstants.Claude.Role.Assistant;

        /// <summary>
        /// 回應內容區塊列表
        /// 可能包含多個內容區塊，例如文字 + 思考過程
        /// </summary>
        [JsonPropertyName("content")]
        public List<ClaudeContentBlockDto> Content { get; set; } = new();

        /// <summary>
        /// 使用的模型
        /// 實際處理請求的模型版本
        /// </summary>
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// 停止原因（end_turn, max_tokens, stop_sequence 等）
        /// 說明為什麼 Claude 停止生成
        /// </summary>
        [JsonPropertyName("stop_reason")]
        public string? StopReason { get; set; }

        /// <summary>
        /// 停止序列（如果是因 stop_sequence 停止）
        /// 觸發停止的具體序列
        /// </summary>
        [JsonPropertyName("stop_sequence")]
        public string? StopSequence { get; set; }

        /// <summary>
        /// Token 使用統計
        /// 用於成本追蹤和優化
        /// </summary>
        [JsonPropertyName("usage")]
        public ClaudeUsageDto Usage { get; set; } = new();

        /// <summary>
        /// 保留未知欄位（用於未來 API 擴充）
        /// 確保向前相容性
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }

    /// <summary>
    /// Token 使用統計 DTO
    /// 追蹤每次 API 呼叫的 token 消耗
    /// </summary>
    public class ClaudeUsageDto
    {
        /// <summary>
        /// 輸入 token 數
        /// 包含 prompt 和上下文
        /// </summary>
        [JsonPropertyName("input_tokens")]
        public int InputTokens { get; set; }

        /// <summary>
        /// 輸出 token 數
        /// Claude 生成的內容長度
        /// </summary>
        [JsonPropertyName("output_tokens")]
        public int OutputTokens { get; set; }

        /// <summary>
        /// 快取建立 token 數（如果使用 prompt caching）
        /// 第一次快取 prompt 時的消耗
        /// </summary>
        [JsonPropertyName("cache_creation_input_tokens")]
        public int? CacheCreationInputTokens { get; set; }

        /// <summary>
        /// 快取讀取 token 數（如果使用 prompt caching）
        /// 從快取讀取 prompt 時的節省
        /// </summary>
        [JsonPropertyName("cache_read_input_tokens")]
        public int? CacheReadInputTokens { get; set; }
    }

    #endregion

    #region === Claude API Error Response DTOs ===

    /// <summary>
    /// Claude API 錯誤回應（type=error）
    /// 當 API 呼叫失敗時的回應格式
    /// </summary>
    public class ClaudeErrorResponseDto
    {
        /// <summary>
        /// 類型（固定為 "error"）
        /// 用於區分錯誤回應和成功回應
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = "error";

        /// <summary>
        /// 錯誤詳情
        /// 包含錯誤類型和訊息
        /// </summary>
        [JsonPropertyName("error")]
        public ClaudeErrorDto Error { get; set; } = new();

        /// <summary>
        /// 請求 ID（用於追蹤和客服查詢）
        /// 向 Anthropic 回報問題時需要提供
        /// </summary>
        [JsonPropertyName("request_id")]
        public string? RequestId { get; set; }
    }

    /// <summary>
    /// Claude API 錯誤詳情 DTO
    /// 描述具體的錯誤資訊
    /// </summary>
    public class ClaudeErrorDto
    {
        /// <summary>
        /// 錯誤類型
        /// invalid_request_error: 請求格式錯誤
        /// authentication_error: API Key 無效
        /// permission_error: 權限不足
        /// not_found_error: 資源不存在
        /// rate_limit_error: 超過速率限制
        /// api_error: API 內部錯誤
        /// overloaded_error: 服務過載
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// 錯誤訊息
        /// 人類可讀的錯誤說明
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }

    #endregion

    #region === Business Context DTOs (Claude Service 專用) ===

    /// <summary>
    /// 照片分析上下文 DTO
    /// 封裝分析單張照片所需的所有資訊
    /// 這是 ClaudeApiService.AnalyzeSinglePhotoAsync 方法的輸入參數
    /// 
    /// 設計原則：
    /// - 直接引用現有的完整 DTO（AzureVisionAnalysisDto, PlaceResult），避免重複定義
    /// - 讓 Service 層能夠存取完整資訊，自己決定要使用哪些欄位
    /// - 保持與其他服務的 DTO 分離，避免命名衝突和邏輯耦合
    /// </summary>
    public class PhotoAnalysisContextDto
    {
        /// <summary>
        /// 照片 ID（必填）
        /// 用於追蹤和記錄分析結果
        /// </summary>
        public long PhotoId { get; set; }

        /// <summary>
        /// 照片縮圖的 Base64 編碼（可選）
        /// 當照片過大時使用縮圖進行分析以節省成本
        /// 格式：純 base64 字串，不含 data:image/jpeg;base64, 前綴
        /// </summary>
        public string? ThumbnailBase64 { get; set; }

        /// <summary>
        /// 縮圖的媒體類型（可選）
        /// 例如：image/jpeg, image/png
        /// 使用 AIConstants.Claude.ImageMediaType 中定義的常數
        /// </summary>
        public string? ThumbnailMediaType { get; set; }

        /// <summary>
        /// EXIF 資料（可選）
        /// 包含拍攝時間、GPS 座標等資訊
        /// 注意：如果專案中已有統一的 EXIF DTO，可以替換為該類型
        /// </summary>
        public ExifDataDto? ExifData { get; set; }

        /// <summary>
        /// Azure Vision 分析結果（可選）
        /// 直接引用完整的 AzureVisionAnalysisDto，包含物體識別、標籤、描述等
        /// Service 層可以根據需要提取相關資訊傳遞給 Claude
        /// </summary>
        public AzureVisionAnalysisDto? AzureVision { get; set; }

        /// <summary>
        /// Google Places 附近景點列表（可選）
        /// 直接引用 PlaceResult，包含完整的景點資訊
        /// Service 層可以根據需要篩選和格式化這些資訊
        /// </summary>
        public List<PlaceResult> GooglePlaces { get; set; } = new();

        /// <summary>
        /// 地理編碼結果（可選）
        /// GPS 座標轉換的地址資訊
        /// 注意：如果專案中已有統一的地理編碼 DTO，可以替換為該類型
        /// </summary>
        public GeocodingResultDto? Geocoding { get; set; }

        /// <summary>
        /// 分析選項
        /// 控制分析行為的參數
        /// </summary>
        public PhotoAnalysisOptionsDto Options { get; set; } = new();
    }

    /// <summary>
    /// 照片分析選項 DTO
    /// 控制 Claude 分析的行為參數
    /// </summary>
    public class PhotoAnalysisOptionsDto
    {
        /// <summary>
        /// 最大輸出 Token 數
        /// 控制回應長度，預設使用 AIConstants 中定義的值
        /// </summary>
        public int MaxTokens { get; set; } = AIConstants.Claude.TokenLimits.DefaultMaxTokens;

        /// <summary>
        /// 溫度參數
        /// 控制輸出的隨機性，預設使用 AIConstants 中定義的值
        /// </summary>
        public double Temperature { get; set; } = AIConstants.Claude.Defaults.Temperature;

        /// <summary>
        /// 是否包含歷史/文化背景
        /// 如果是知名景點，是否要 Claude 提供背景資訊
        /// </summary>
        public bool IncludeHistoricalContext { get; set; } = true;

        /// <summary>
        /// 是否包含圖片縮圖
        /// 是否將縮圖發送給 Claude 進行視覺分析
        /// </summary>
        public bool IncludeThumbnail { get; set; } = true;

        /// <summary>
        /// 語言偏好
        /// 回應內容的語言，預設為繁體中文
        /// </summary>
        public string Language { get; set; } = "zh-TW";
    }

    /// <summary>
    /// EXIF 資料 DTO
    /// 從照片中提取的元資料
    /// 
    /// 注意：如果專案中已有統一的 EXIF DTO 定義，建議使用該定義並移除此類別
    /// 這裡保留是為了讓 ClaudeApiService 能夠獨立運作
    /// </summary>
    public class ExifDataDto
    {
        /// <summary>
        /// 拍攝時間
        /// </summary>
        public DateTime? TakenAt { get; set; }

        /// <summary>
        /// GPS 緯度
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// GPS 經度
        /// </summary>
        public double? Longitude { get; set; }

        /// <summary>
        /// 相機型號
        /// </summary>
        public string? CameraModel { get; set; }

        /// <summary>
        /// 其他 EXIF 資訊
        /// </summary>
        public Dictionary<string, string>? OtherData { get; set; }
    }

    /// <summary>
    /// 地理編碼結果 DTO
    /// GPS 座標轉換的地址資訊
    /// 
    /// 注意：如果專案中已有統一的地理編碼 DTO 定義，建議使用該定義並移除此類別
    /// 這裡保留是為了讓 ClaudeApiService 能夠獨立運作
    /// </summary>
    public class GeocodingResultDto
    {
        /// <summary>
        /// 國家
        /// </summary>
        public string? Country { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        public string? City { get; set; }

        /// <summary>
        /// 區域
        /// </summary>
        public string? District { get; set; }

        /// <summary>
        /// 完整地址
        /// </summary>
        public string? FullAddress { get; set; }
    }

    /// <summary>
    /// 照片群組分析上下文 DTO
    /// 封裝分析一組照片所需的所有資訊
    /// 這是未來群組分析功能（AnalyzePhotoGroupAsync）的輸入參數
    /// </summary>
    public class PhotoGroupAnalysisContextDto
    {
        /// <summary>
        /// 群組 ID
        /// 用於追蹤和記錄分析結果
        /// </summary>
        public string GroupId { get; set; } = string.Empty;

        /// <summary>
        /// 照片列表
        /// 群組中的所有照片及其分析結果
        /// </summary>
        public List<PhotoAnalysisContextDto> Photos { get; set; } = new();

        /// <summary>
        /// 候選景點列表
        /// 所有照片的 Google Places 結果的聯集
        /// </summary>
        public List<PlaceResult> CandidatePlaces { get; set; } = new();

        /// <summary>
        /// 分析選項
        /// </summary>
        public PhotoAnalysisOptionsDto Options { get; set; } = new();
    }

    #endregion

    #region === Business Output DTOs (Claude 分析結果專用) ===

    /// <summary>
    /// 景點語義分析輸出 DTO
    /// Claude 對照片的完整語義分析結果
    /// 這是 Claude API 回傳的 JSON 解析後的結構
    /// 
    /// 設計說明：
    /// - 這個 DTO 的結構完全對應 Claude 回傳的 JSON 格式
    /// - 與 GooglePlacesResponseDto 中的 TouristSpotIdentificationDto 是不同的概念
    /// - TouristSpotIdentificationDto 是整合 Google Places 查詢結果的資料結構
    /// - 而這個 DTO 是 Claude 的語義分析結果，關注照片內容理解和標籤建議
    /// </summary>
    public class TouristSpotSemanticOutputDto
    {
        /// <summary>
        /// 是否為知名旅遊景點
        /// Claude 根據視覺內容和上下文資訊的綜合判斷
        /// </summary>
        [JsonPropertyName("is_tourist_spot")]
        public bool IsTouristSpot { get; set; }

        /// <summary>
        /// 景點名稱
        /// 如果 Claude 判斷這是景點照片，會提供景點名稱
        /// </summary>
        [JsonPropertyName("spot_name")]
        public string? SpotName { get; set; }

        /// <summary>
        /// 景點類型（例如：古蹟、博物館、自然景觀）
        /// Claude 對景點類型的分類
        /// </summary>
        [JsonPropertyName("spot_type")]
        public string? SpotType { get; set; }

        /// <summary>
        /// 信心分數 (0.0 - 1.0)
        /// Claude 對自己判斷的信心程度
        /// </summary>
        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }

        /// <summary>
        /// 信心分數的別名
        /// 提供一個屬性讓 Service 可以用 ConfidenceScore 來存取
        /// 這樣可以保持與其他程式碼的命名一致性
        /// </summary>
        [JsonIgnore]
        public double ConfidenceScore => Confidence;

        /// <summary>
        /// 景點描述（繁體中文）
        /// Claude 生成的自然語言描述，像是在跟朋友分享照片故事
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        /// <summary>
        /// 建議的標籤列表
        /// Claude 建議為這張照片加上的標籤，通常 3-8 個
        /// 標籤應該具體且有用，例如「東京」比「亞洲」更有價值
        /// </summary>
        [JsonPropertyName("suggested_tags")]
        public List<string> SuggestedTags { get; set; } = new();

        /// <summary>
        /// 推薦的 Google Place ID（如果有多個候選）
        /// 當有多個候選景點時，Claude 推薦最可能的那一個
        /// </summary>
        [JsonPropertyName("recommended_place_id")]
        public string? RecommendedPlaceId { get; set; }

        /// <summary>
        /// 分析推理過程（用於除錯）
        /// Claude 說明自己是如何做出判斷的，用於系統優化和除錯
        /// </summary>
        [JsonPropertyName("reasoning")]
        public string? Reasoning { get; set; }

        /// <summary>
        /// 相關歷史/文化背景
        /// 如果是知名景點，Claude 提供的歷史或文化背景資訊
        /// </summary>
        [JsonPropertyName("historical_context")]
        public string? HistoricalContext { get; set; }

        /// <summary>
        /// 建議的相簿分類
        /// Claude 建議這張照片應該放在哪些相簿中
        /// 例如：風景、美食、人物、建築、活動等
        /// </summary>
        [JsonPropertyName("suggested_album_categories")]
        public List<string> SuggestedAlbumCategories { get; set; } = new();
    }

    /// <summary>
    /// Claude 分析完整結果 DTO
    /// 包含分析結果、原始回應、錯誤資訊等完整資訊
    /// 用於寫入 PhotoAIClassificationLog 和回傳給呼叫方
    /// 
    /// 設計說明：
    /// - 這是 ClaudeApiService 方法的回傳值類型
    /// - 包裝了 Claude 的分析結果和執行過程中的所有相關資訊
    /// - 支援成功和失敗兩種情況的完整資訊記錄
    /// </summary>
    public class ClaudeAnalysisResultDto
    {
        /// <summary>
        /// 照片 ID
        /// 標識這個分析結果屬於哪張照片
        /// </summary>
        public long PhotoId { get; set; }

        /// <summary>
        /// 分析成功標記
        /// true 表示成功取得分析結果，false 表示發生錯誤
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 語義分析結果
        /// Claude 的分析結論和建議
        /// 當 Success=true 時，這裡包含 Claude 的分析結果
        /// </summary>
        public TouristSpotSemanticOutputDto? SemanticOutput { get; set; }

        /// <summary>
        /// Claude API 原始回應文字
        /// 儲存 Claude 回傳的原始文字內容，用於除錯和記錄
        /// 這裡儲存的是提取的文字內容，不是完整的 MessageResponse 物件
        /// </summary>
        public string? RawResponse { get; set; }

        /// <summary>
        /// Claude API 錯誤回應（失敗時）
        /// 當 Success=false 時，這裡包含錯誤詳情
        /// </summary>
        public ClaudeErrorResponseDto? ErrorResponse { get; set; }

        /// <summary>
        /// 錯誤代碼
        /// 用於錯誤分類和處理
        /// 可以是 AIConstants.ErrorCodes.Claude 中定義的常數
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// 錯誤訊息（如果有）
        /// 人類可讀的錯誤說明
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 使用的 Prompt
        /// 記錄發送給 Claude 的完整 prompt，用於除錯和優化
        /// </summary>
        public string? UsedPrompt { get; set; }

        /// <summary>
        /// Token 使用統計
        /// 用於成本追蹤和分析
        /// </summary>
        public ClaudeUsageDto? TokenUsage { get; set; }

        /// <summary>
        /// 處理時間（毫秒）
        /// 用於效能監控和優化
        /// </summary>
        public int ProcessingTimeMs { get; set; }

        /// <summary>
        /// 分析時間
        /// 記錄何時進行的分析
        /// </summary>
        public DateTimeOffset AnalyzedAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// HTTP 狀態碼
        /// API 請求的 HTTP 狀態碼
        /// </summary>
        public int? HttpStatusCode { get; set; }
    }

    #endregion
}