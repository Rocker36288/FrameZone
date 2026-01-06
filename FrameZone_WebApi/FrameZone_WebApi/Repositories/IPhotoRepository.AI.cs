using FrameZone_WebApi.DTOs.AI;
using FrameZone_WebApi.Models;

namespace FrameZone_WebApi.Repositories
{
    /// <summary>
    /// IPhotoRepository 的 AI 功能擴充
    /// </summary>
    public partial interface IPhotoRepository
    {
        #region PhotoAIClassificationLog 表操作

        /// <summary>
        /// 新增 AI 分析記錄
        /// </summary>
        /// <param name="log">AI 分析記錄物件</param>
        /// <returns>新增後的記錄</returns>
        Task<PhotoAiclassificationLog> AddAILogAsync(PhotoAiclassificationLog log);

        /// <summary>
        /// 取得照片的最新 AI 分析記錄
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <returns>最新的 AI 分析記錄，不存在則返回 null</returns>
        Task<PhotoAiclassificationLog?> GetLatestAILogAsync(long photoId);

        /// <summary>
        /// 取得照片的最新 AI 分析記錄（依指定模型）
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <param name="aiModel">AI 模型名稱（Azure, Google, Claude）</param>
        /// <returns>最新的 AI 分析記錄，不存在則返回 null</returns>
        Task<PhotoAiclassificationLog?> GetLatestAILogByModelAsync(long photoId, string aiModel);

        /// <summary>
        /// 取得照片的 AI 分析歷史記錄
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <param name="pageSize">每頁筆數（預設 10）</param>
        /// <returns>AI 分析記錄列表（按時間倒序）</returns>
        Task<List<PhotoAiclassificationLog>> GetAILogHistoryAsync(long photoId, int pageSize = 10);

        /// <summary>
        /// 根據 LogId 查詢 AI 分析記錄
        /// </summary>
        /// <param name="logId">Log ID</param>
        /// <returns>AI 分析記錄，不存在則返回 null</returns>
        Task<PhotoAiclassificationLog?> GetAILogByIdAsync(long logId);

        /// <summary>
        /// 檢查照片是否已有 AI 分析記錄
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <returns>是否存在 AI 分析記錄</returns>
        Task<bool> HasAIAnalysisAsync(long photoId);

        /// <summary>
        /// 取得使用者的 AI 分析統計（總數、成功數、失敗數）
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>AI 分析統計資訊</returns>
        Task<UserAIAnalysisStatsDto> GetUserAIAnalysisStatsAsync(long userId);

        #endregion

        #region PhotoAIClassificationSuggestion 表操作

        /// <summary>
        /// 批次新增 AI 標籤建議
        /// </summary>
        /// <param name="suggestions">AI 建議列表</param>
        /// <returns>新增後的建議列表</returns>
        Task<List<PhotoAiclassificationSuggestion>> AddAISuggestionsAsync(List<PhotoAiclassificationSuggestion> suggestions);

        /// <summary>
        /// 取得指定 Log 的所有建議
        /// </summary>
        /// <param name="logId">AI 分析 Log ID</param>
        /// <returns>AI 建議列表</returns>
        Task<List<PhotoAiclassificationSuggestion>> GetAISuggestionsByLogIdAsync(long logId);

        /// <summary>
        /// 取得照片的待處理建議（未採用）
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <param name="minConfidence">最低信心分數（可選）</param>
        /// <returns>待處理的 AI 建議列表</returns>
        Task<List<PhotoAiclassificationSuggestion>> GetPendingSuggestionsAsync(long photoId, decimal? minConfidence = null);

        /// <summary>
        /// 取得照片的所有 AI 建議（包含已採用和待處理）
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <returns>所有 AI 建議列表</returns>
        Task<List<PhotoAiclassificationSuggestion>> GetAllAISuggestionsAsync(long photoId);

        /// <summary>
        /// 根據建議 ID 查詢建議
        /// </summary>
        /// <param name="suggestionId">建議 ID</param>
        /// <returns>AI 建議物件，不存在則返回 null</returns>
        Task<PhotoAiclassificationSuggestion?> GetAISuggestionByIdAsync(long suggestionId);

        /// <summary>
        /// 標記單一建議為已採用
        /// </summary>
        /// <param name="suggestionId">建議 ID</param>
        /// <returns>是否標記成功</returns>
        Task<bool> AdoptSuggestionAsync(long suggestionId);

        /// <summary>
        /// 批次標記建議為已採用
        /// </summary>
        /// <param name="suggestionIds">建議 ID 列表</param>
        /// <returns>成功標記的數量</returns>
        Task<int> BulkAdoptSuggestionsAsync(List<long> suggestionIds);

        /// <summary>
        /// 取得照片的 AI 分析統計（建議總數、已採用數、待處理數、平均信心分數）
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <returns>AI 分析統計資訊</returns>
        Task<PhotoAIAnalysisStatsDto> GetPhotoAIAnalysisStatsAsync(long photoId);

        #endregion

        #region AI 標籤套用相關

        /// <summary>
        /// 檢查照片是否已有指定分類（任何來源）
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <param name="categoryId">分類 ID</param>
        /// <returns>是否已存在</returns>
        Task<bool> HasPhotoCategoryAsync(long photoId, int categoryId);

        /// <summary>
        /// 檢查照片是否已有指定標籤（任何來源）
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <param name="tagId">標籤 ID</param>
        /// <returns>是否已存在</returns>
        Task<bool> HasPhotoTagAsync(long photoId, int tagId);

        /// <summary>
        /// 為照片新增單一分類（支援指定來源和信心度）
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <param name="categoryId">分類 ID</param>
        /// <param name="sourceId">來源 ID</param>
        /// <param name="confidence">信心度（AI 建議時使用）</param>
        /// <param name="assignedBy">指派者 ID（可選）</param>
        /// <returns>是否新增成功</returns>
        Task<bool> AddPhotoCategoryAsync(long photoId, int categoryId, int sourceId, decimal? confidence = null, long? assignedBy = null);

        /// <summary>
        /// 為照片新增單一標籤（支援指定來源和信心度）
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <param name="tagId">標籤 ID</param>
        /// <param name="sourceId">來源 ID</param>
        /// <param name="confidence">信心度（AI 建議時使用）</param>
        /// <param name="addedBy">新增者 ID（可選）</param>
        /// <returns>是否新增成功</returns>
        Task<bool> AddPhotoTagAsync(long photoId, int tagId, int sourceId, decimal? confidence = null, long? addedBy = null);

        /// <summary>
        /// 批次新增照片分類（支援重複檢查）
        /// </summary>
        /// <param name="photoCategories">照片分類列表</param>
        /// <returns>實際新增的數量</returns>
        Task<int> BulkAddPhotoCategoriesAsync(List<PhotoPhotoCategory> photoCategories);

        /// <summary>
        /// 批次新增照片標籤（支援重複檢查）
        /// </summary>
        /// <param name="photoTags">照片標籤列表</param>
        /// <returns>實際新增的數量</returns>
        Task<int> BulkAddPhotoTagsAsync(List<PhotoPhotoTag> photoTags);

        #endregion
    }
}