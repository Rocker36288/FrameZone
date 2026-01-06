namespace FrameZone_WebApi.DTOs.AI
{
    /// <summary>
    /// 使用者 AI 分析統計 DTO
    /// </summary>
    public class UserAIAnalysisStatsDto
    {
        /// <summary>
        /// 總分析次數
        /// </summary>
        public int TotalAnalysisCount { get; set; }

        /// <summary>
        /// 成功次數
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// 失敗次數
        /// </summary>
        public int FailedCount { get; set; }

        /// <summary>
        /// 總使用 Quota
        /// </summary>
        public int TotalQuotaUsed { get; set; }

        /// <summary>
        /// 平均處理時間（毫秒）
        /// </summary>
        public int? AverageProcessingTime { get; set; }

        /// <summary>
        /// 成功率（百分比）
        /// </summary>
        public double SuccessRate => TotalAnalysisCount > 0
            ? (double)SuccessCount / TotalAnalysisCount * 100
            : 0;
    }

    /// <summary>
    /// 照片 AI 分析統計 DTO
    /// </summary>
    public class PhotoAIAnalysisStatsDto
    {
        /// <summary>
        /// 照片 ID
        /// </summary>
        public long PhotoId { get; set; }

        /// <summary>
        /// 建議總數
        /// </summary>
        public int TotalSuggestions { get; set; }

        /// <summary>
        /// 已採用數量
        /// </summary>
        public int AdoptedCount { get; set; }

        /// <summary>
        /// 待處理數量
        /// </summary>
        public int PendingCount { get; set; }

        /// <summary>
        /// 平均信心分數 (0.0 - 1.0)
        /// </summary>
        public double? AverageConfidence { get; set; }

        /// <summary>
        /// 採用率（百分比）
        /// </summary>
        public double AdoptionRate => TotalSuggestions > 0
            ? (double)AdoptedCount / TotalSuggestions * 100
            : 0;
    }
}