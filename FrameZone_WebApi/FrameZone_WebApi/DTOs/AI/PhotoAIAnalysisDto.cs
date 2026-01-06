using FrameZone_WebApi.DTOs.AI;
using System.Text.Json.Serialization;

namespace FrameZone_WebApi.DTOs.AI
{
    #region === AI 分析請求 ===

    /// <summary>
    /// 照片 AI 分析請求
    /// </summary>
    public class PhotoAIAnalysisRequestDto
    {
        /// <summary>
        /// 照片 ID
        /// </summary>
        public long PhotoId { get; set; }

        /// <summary>
        /// 使用者 ID
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 是否使用縮圖（節省成本）
        /// </summary>
        public bool UseThumbnail { get; set; } = true;

        /// <summary>
        /// 最低信心分數過濾（0.0-1.0）
        /// </summary>
        public double MinConfidenceScore { get; set; } = 0.9;

        /// <summary>
        /// 是否啟用景點識別
        /// </summary>
        public bool EnableTouristSpotDetection { get; set; } = true;

        /// <summary>
        /// 是否啟用物件/場景識別
        /// </summary>
        public bool EnableObjectDetection { get; set; } = true;

        /// <summary>
        /// 景點搜尋半徑（公尺）
        /// </summary>
        public int PlaceSearchRadius { get; set; } = 500;

        /// <summary>
        /// 強制重新分析（即使已有分析記錄）
        /// </summary>
        public bool ForceReanalysis { get; set; } = false;
    }

    #endregion

    #region === AI 分析回應 ===

    /// <summary>
    /// 照片 AI 分析完整回應
    /// </summary>
    public class PhotoAIAnalysisResponseDto
    {
        /// <summary>
        /// 分析 Log ID（對應 PhotoAIClassificationLog.LogId）
        /// </summary>
        public long? LogId { get; set; }

        /// <summary>
        /// 照片 ID
        /// </summary>
        public long PhotoId { get; set; }

        /// <summary>
        /// 分析狀態（Success, Failed, Pending）
        /// </summary>
        public string Status { get; set; } = "Success";

        /// <summary>
        /// 分析時間
        /// </summary>
        public DateTimeOffset AnalyzedAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Azure Vision 分析結果
        /// </summary>
        public AzureVisionSummaryDto? AzureVisionResult { get; set; }

        /// <summary>
        /// Google Places 分析結果
        /// </summary>
        public GooglePlacesSummaryDto? GooglePlacesResult { get; set; }

        /// <summary>
        /// Claude 語義分析結果
        /// </summary>
        public ClaudeSemanticSummaryDto? ClaudeSemanticResult { get; set; }

        /// <summary>
        /// AI 標籤建議列表
        /// </summary>
        public List<AITagSuggestionDto> TagSuggestions { get; set; } = new();

        /// <summary>
        /// 總處理時間（毫秒）
        /// </summary>
        public int TotalProcessingTimeMs { get; set; }

        /// <summary>
        /// 使用的 Quota（次數）
        /// </summary>
        public int QuotaUsed { get; set; } = 1;

        /// <summary>
        /// 錯誤訊息（如果有）
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 詳細錯誤資訊
        /// </summary>
        public List<string> Errors { get; set; } = new();

        /// <summary>
        /// 警告訊息
        /// </summary>
        public List<string> Warnings { get; set; } = new();
    }

    /// <summary>
    /// Azure Vision 分析摘要
    /// </summary>
    public class AzureVisionSummaryDto
    {
        /// <summary>
        /// 分析成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 偵測到的物件數量
        /// </summary>
        public int ObjectCount { get; set; }

        /// <summary>
        /// 偵測到的標籤數量
        /// </summary>
        public int TagCount { get; set; }

        /// <summary>
        /// 主要物件列表（前 5 個）
        /// </summary>
        public List<string> TopObjects { get; set; } = new();

        /// <summary>
        /// 主要標籤列表（前 10 個）
        /// </summary>
        public List<string> TopTags { get; set; } = new();

        /// <summary>
        /// 圖像描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 是否包含成人內容
        /// </summary>
        public bool HasAdultContent { get; set; }

        /// <summary>
        /// 處理時間（毫秒）
        /// </summary>
        public int ProcessingTimeMs { get; set; }

        /// <summary>
        /// 錯誤訊息（如果有）
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Google Places 分析摘要
    /// </summary>
    public class GooglePlacesSummaryDto
    {
        /// <summary>
        /// 分析成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 找到的景點數量
        /// </summary>
        public int PlaceCount { get; set; }

        /// <summary>
        /// 最接近的景點名稱
        /// </summary>
        public string? NearestPlaceName { get; set; }

        /// <summary>
        /// 最接近的景點距離（公尺）
        /// </summary>
        public double? NearestPlaceDistance { get; set; }

        /// <summary>
        /// 附近景點列表（前 5 個）
        /// </summary>
        public List<string> NearbyPlaces { get; set; } = new();

        /// <summary>
        /// 處理時間（毫秒）
        /// </summary>
        public int ProcessingTimeMs { get; set; }

        /// <summary>
        /// 錯誤訊息（如果有）
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Claude 語義分析摘要
    /// </summary>
    public class ClaudeSemanticSummaryDto
    {
        /// <summary>
        /// 分析成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 是否為旅遊景點
        /// </summary>
        public bool IsTouristSpot { get; set; }

        /// <summary>
        /// 景點名稱
        /// </summary>
        public string? SpotName { get; set; }

        /// <summary>
        /// 信心分數 (0.0 - 1.0)
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// 景點描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 使用的輸入 tokens
        /// </summary>
        public int InputTokens { get; set; }

        /// <summary>
        /// 使用的輸出 tokens
        /// </summary>
        public int OutputTokens { get; set; }

        /// <summary>
        /// 處理時間（毫秒）
        /// </summary>
        public int ProcessingTimeMs { get; set; }

        /// <summary>
        /// 錯誤訊息（如果有）
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    #endregion

    #region === AI 標籤建議 ===

    /// <summary>
    /// AI 標籤建議（對應 PhotoAIClassificationSuggestion）
    /// </summary>
    public class AITagSuggestionDto
    {
        /// <summary>
        /// 建議 ID（對應 SuggestionId，套用後才有值）
        /// </summary>
        public long? SuggestionId { get; set; }

        /// <summary>
        /// Log ID（對應 PhotoAIClassificationLog）
        /// </summary>
        public long? LogId { get; set; }

        /// <summary>
        /// 分類 ID（可能是新建立的）
        /// </summary>
        public int? CategoryId { get; set; }

        /// <summary>
        /// 分類名稱
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;

        /// <summary>
        /// 分類類型（EXIF, Time, Tag, Location, Custom）
        /// </summary>
        public string CategoryType { get; set; } = string.Empty;

        /// <summary>
        /// 標籤 ID（可能是新建立的）
        /// </summary>
        public int? TagId { get; set; }

        /// <summary>
        /// 標籤名稱
        /// </summary>
        public string TagName { get; set; } = string.Empty;

        /// <summary>
        /// 信心分數 (0.0 - 1.0)
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// 是否已套用
        /// </summary>
        public bool IsAdopted { get; set; }

        /// <summary>
        /// 建議來源（Azure, Google, Claude）
        /// </summary>
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// 建議理由（用於顯示給使用者）
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// 建議時間
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }

    #endregion

    #region === 套用 AI 標籤 ===

    /// <summary>
    /// 套用 AI 標籤請求
    /// </summary>
    public class ApplyAITagsRequestDto
    {
        /// <summary>
        /// 照片 ID
        /// </summary>
        public long PhotoId { get; set; }

        /// <summary>
        /// 使用者 ID
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 要套用的建議 ID 列表（空白表示全部套用）
        /// </summary>
        public List<long> SuggestionIds { get; set; } = new();

        /// <summary>
        /// 最低信心分數（只套用高於此分數的標籤）
        /// </summary>
        public double? MinConfidence { get; set; }

        /// <summary>
        /// 是否自動套用到相同地點的照片
        /// </summary>
        public bool ApplyToSimilarPhotos { get; set; } = false;
    }

    /// <summary>
    /// 套用 AI 標籤回應
    /// </summary>
    public class ApplyAITagsResponseDto
    {
        /// <summary>
        /// 成功套用的標籤數量
        /// </summary>
        public int AppliedCount { get; set; }

        /// <summary>
        /// 跳過的標籤數量（已存在）
        /// </summary>
        public int SkippedCount { get; set; }

        /// <summary>
        /// 失敗的標籤數量
        /// </summary>
        public int FailedCount { get; set; }

        /// <summary>
        /// 已套用的標籤詳情
        /// </summary>
        public List<AppliedTagDetailDto> AppliedTags { get; set; } = new();

        /// <summary>
        /// 錯誤訊息列表
        /// </summary>
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>
    /// 已套用的標籤詳情
    /// </summary>
    public class AppliedTagDetailDto
    {
        /// <summary>
        /// 建議 ID
        /// </summary>
        public long SuggestionId { get; set; }

        /// <summary>
        /// 標籤名稱
        /// </summary>
        public string TagName { get; set; } = string.Empty;

        /// <summary>
        /// 分類名稱
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;

        /// <summary>
        /// 信心分數
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// 套用狀態（Applied, Skipped, Failed）
        /// </summary>
        public string Status { get; set; } = "Applied";

        /// <summary>
        /// 備註
        /// </summary>
        public string? Note { get; set; }
    }

    #endregion

    #region === 分析狀態查詢 ===

    /// <summary>
    /// 照片 AI 分析狀態查詢
    /// </summary>
    public class PhotoAIAnalysisStatusDto
    {
        /// <summary>
        /// 照片 ID
        /// </summary>
        public long PhotoId { get; set; }

        /// <summary>
        /// 是否已分析過
        /// </summary>
        public bool HasAnalysis { get; set; }

        /// <summary>
        /// 最後分析時間
        /// </summary>
        public DateTimeOffset? LastAnalyzedAt { get; set; }

        /// <summary>
        /// 最後分析狀態（Success, Failed, Pending）
        /// </summary>
        public string? LastAnalysisStatus { get; set; }

        /// <summary>
        /// 建議數量
        /// </summary>
        public int SuggestionCount { get; set; }

        /// <summary>
        /// 已套用的建議數量
        /// </summary>
        public int AdoptedCount { get; set; }

        /// <summary>
        /// 待處理的建議數量
        /// </summary>
        public int PendingCount { get; set; }

        /// <summary>
        /// 平均信心分數
        /// </summary>
        public double? AverageConfidence { get; set; }

        /// <summary>
        /// 是否可以重新分析
        /// </summary>
        public bool CanReanalyze { get; set; }

        /// <summary>
        /// 錯誤訊息（如果最後一次失敗）
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    #endregion

    #region === 批次分析 ===

    /// <summary>
    /// 批次 AI 分析請求
    /// </summary>
    public class BatchPhotoAIAnalysisRequestDto
    {
        /// <summary>
        /// 照片 ID 列表
        /// </summary>
        public List<long> PhotoIds { get; set; } = new();

        /// <summary>
        /// 使用者 ID
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 分析選項（與單張分析相同）
        /// </summary>
        public PhotoAIAnalysisRequestDto Options { get; set; } = new();

        /// <summary>
        /// 是否非同步處理（建議超過 10 張時使用）
        /// </summary>
        public bool ProcessAsync { get; set; } = true;
    }

    /// <summary>
    /// 批次 AI 分析回應
    /// </summary>
    public class BatchPhotoAIAnalysisResponseDto
    {
        /// <summary>
        /// 批次任務 ID（如果是非同步處理）
        /// </summary>
        public string? BatchJobId { get; set; }

        /// <summary>
        /// 總照片數
        /// </summary>
        public int TotalPhotos { get; set; }

        /// <summary>
        /// 成功數量
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// 失敗數量
        /// </summary>
        public int FailedCount { get; set; }

        /// <summary>
        /// 跳過數量（已分析過且未強制重新分析）
        /// </summary>
        public int SkippedCount { get; set; }

        /// <summary>
        /// 是否為非同步處理
        /// </summary>
        public bool IsAsync { get; set; }

        /// <summary>
        /// 個別照片結果（同步處理時才有）
        /// </summary>
        public List<PhotoAIAnalysisResponseDto> Results { get; set; } = new();

        /// <summary>
        /// 錯誤訊息列表
        /// </summary>
        public List<string> Errors { get; set; } = new();

        /// <summary>
        /// 預估完成時間（非同步時）
        /// </summary>
        public DateTimeOffset? EstimatedCompletionTime { get; set; }
    }

    #endregion

    #region === 分析摘要（用於列表顯示） ===

    /// <summary>
    /// 照片 AI 分析摘要（用於照片列表顯示）
    /// </summary>
    public class PhotoAIAnalysisSummaryDto
    {
        /// <summary>
        /// 照片 ID
        /// </summary>
        public long PhotoId { get; set; }

        /// <summary>
        /// 是否有 AI 分析
        /// </summary>
        public bool HasAIAnalysis { get; set; }

        /// <summary>
        /// AI 標籤數量
        /// </summary>
        public int AITagCount { get; set; }

        /// <summary>
        /// 建議的主要標籤（前 3 個）
        /// </summary>
        public List<string> TopSuggestedTags { get; set; } = new();

        /// <summary>
        /// 是否識別為旅遊景點
        /// </summary>
        public bool IsTouristSpot { get; set; }

        /// <summary>
        /// 景點名稱（如果是）
        /// </summary>
        public string? TouristSpotName { get; set; }

        /// <summary>
        /// 最後分析時間
        /// </summary>
        public DateTimeOffset? LastAnalyzedAt { get; set; }
    }

    #endregion

    #region === 常數 ===

    /// <summary>
    /// AI 分析常數
    /// </summary>
    public static class AIAnalysisConstants
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
        /// 標籤來源
        /// </summary>
        public static class TagSource
        {
            public const string Azure = "Azure";
            public const string Google = "Google";
            public const string Claude = "Claude";
            public const string Combined = "Combined";
        }

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
        /// 預設值
        /// </summary>
        public static class Defaults
        {
            public const double MinConfidenceScore = 0.7;
            public const int PlaceSearchRadius = 500;
            public const int MaxBatchSize = 50;
            public const int AsyncThreshold = 10;
        }
    }

    #endregion
}