using FrameZone_WebApi.Constants;
using FrameZone_WebApi.DTOs.AI;
using FrameZone_WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FrameZone_WebApi.Repositories
{
    /// <summary>
    /// PhotoRepository 的 AI 功能實作擴充
    /// </summary>
    public partial class PhotoRepository
    {
        #region PhotoAIClassificationLog 表操作

        /// <summary>
        /// 新增 AI 分析記錄
        /// </summary>
        public async Task<PhotoAiclassificationLog> AddAILogAsync(PhotoAiclassificationLog log)
        {
            try
            {
                _logger.LogInformation($"📝 新增 AI 分析記錄，PhotoId: {log.PhotoId}, AIModel: {log.Aimodel}");

                await _context.PhotoAiclassificationLogs.AddAsync(log);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ AI 分析記錄新增成功，LogId: {log.LogId}");
                return log;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ 新增 AI 分析記錄時發生錯誤，PhotoId: {log.PhotoId}");
                throw;
            }
        }

        /// <summary>
        /// 取得照片的最新 AI 分析記錄
        /// </summary>
        public async Task<PhotoAiclassificationLog?> GetLatestAILogAsync(long photoId)
        {
            try
            {
                _logger.LogInformation($"🔍 查詢最新 AI 分析記錄，PhotoId: {photoId}");

                var log = await _context.PhotoAiclassificationLogs
                    .AsNoTracking()
                    .Where(l => l.PhotoId == photoId)
                    .OrderByDescending(l => l.CreatedAt)
                    .FirstOrDefaultAsync();

                _logger.LogInformation(log != null
                    ? $"✅ 找到 AI 記錄，LogId: {log.LogId}, Status: {log.Status}"
                    : $"ℹ️ 無 AI 分析記錄");

                return log;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ 查詢最新 AI 分析記錄時發生錯誤，PhotoId: {photoId}");
                throw;
            }
        }

        /// <summary>
        /// 取得照片的最新 AI 分析記錄（依指定模型）
        /// </summary>
        public async Task<PhotoAiclassificationLog?> GetLatestAILogByModelAsync(long photoId, string aiModel)
        {
            try
            {
                _logger.LogInformation($"🔍 查詢最新 AI 分析記錄，PhotoId: {photoId}, AIModel: {aiModel}");

                var log = await _context.PhotoAiclassificationLogs
                    .AsNoTracking()
                    .Where(l => l.PhotoId == photoId && l.Aimodel == aiModel)
                    .OrderByDescending(l => l.CreatedAt)
                    .FirstOrDefaultAsync();

                return log;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ 查詢最新 AI 分析記錄時發生錯誤，PhotoId: {photoId}, AIModel: {aiModel}");
                throw;
            }
        }

        /// <summary>
        /// 取得照片的 AI 分析歷史記錄
        /// </summary>
        public async Task<List<PhotoAiclassificationLog>> GetAILogHistoryAsync(long photoId, int pageSize = 10)
        {
            try
            {
                _logger.LogInformation($"🔍 查詢 AI 分析歷史，PhotoId: {photoId}, PageSize: {pageSize}");

                var logs = await _context.PhotoAiclassificationLogs
                    .AsNoTracking()
                    .Where(l => l.PhotoId == photoId)
                    .OrderByDescending(l => l.CreatedAt)
                    .Take(pageSize)
                    .ToListAsync();

                _logger.LogInformation($"✅ 找到 {logs.Count} 筆 AI 分析歷史");
                return logs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ 查詢 AI 分析歷史時發生錯誤，PhotoId: {photoId}");
                throw;
            }
        }

        /// <summary>
        /// 根據 LogId 查詢 AI 分析記錄
        /// </summary>
        public async Task<PhotoAiclassificationLog?> GetAILogByIdAsync(long logId)
        {
            try
            {
                return await _context.PhotoAiclassificationLogs
                    .AsNoTracking()
                    .Where(l => l.LogId == logId)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ 根據 LogId 查詢 AI 分析記錄時發生錯誤，LogId: {logId}");
                throw;
            }
        }

        /// <summary>
        /// 檢查照片是否已有 AI 分析記錄
        /// </summary>
        public async Task<bool> HasAIAnalysisAsync(long photoId)
        {
            try
            {
                return await _context.PhotoAiclassificationLogs
                    .AsNoTracking()
                    .AnyAsync(l => l.PhotoId == photoId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ 檢查 AI 分析記錄時發生錯誤，PhotoId: {photoId}");
                throw;
            }
        }

        /// <summary>
        /// 取得使用者的 AI 分析統計
        /// </summary>
        public async Task<UserAIAnalysisStatsDto> GetUserAIAnalysisStatsAsync(long userId)
        {
            try
            {
                _logger.LogInformation($"📊 計算使用者 AI 分析統計，UserId: {userId}");

                var logs = await _context.PhotoAiclassificationLogs
                    .AsNoTracking()
                    .Where(l => l.Photo.UserId == userId)
                    .Select(l => new
                    {
                        l.Status,
                        l.QuotaUsed,
                        ProcessingTimeMs = (int?)l.ProcessingTimeMs
                    })
                    .ToListAsync();

                if (logs.Count == 0)
                {
                    return new UserAIAnalysisStatsDto();
                }

                return new UserAIAnalysisStatsDto
                {
                    TotalAnalysisCount = logs.Count,
                    SuccessCount = logs.Count(l => l.Status == AIAnalysisConstants.Status.Success),
                    FailedCount = logs.Count(l => l.Status == AIAnalysisConstants.Status.Failed),
                    TotalQuotaUsed = logs.Sum(l => l.QuotaUsed),
                    AverageProcessingTime = (int?)logs.Average(l => (double)(l.ProcessingTimeMs ?? 0))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ 計算使用者 AI 分析統計時發生錯誤，UserId: {userId}");
                throw;
            }
        }

        #endregion

        #region PhotoAIClassificationSuggestion 表操作

        private static string NormTag(string s) => (s ?? "").Trim().ToLowerInvariant();

        private async Task<Dictionary<(string normName, int categoryId), int>> EnsurePhotoTagsAsync(
            IEnumerable<(string tagName, int categoryId)> inputs,
            CancellationToken ct = default)
        {
            var keys = inputs
                .Select(x => (normName: NormTag(x.tagName), categoryId: x.categoryId, raw: (x.tagName ?? "").Trim()))
                .Where(x => !string.IsNullOrWhiteSpace(x.raw))
                .Distinct()
                .ToList();

            if (keys.Count == 0)
                return new Dictionary<(string, int), int>();

            // 先查已存在的 tags（用 TagName 篩一輪，再用 categoryId 在記憶體精準配對）
            var nameSet = keys.Select(k => k.raw).Distinct().ToList();

            var existing = await _context.PhotoTags
                .Where(t => nameSet.Contains(t.TagName))
                .Select(t => new { t.TagId, t.TagName, t.CategoryId })
                .ToListAsync(ct);

            var map = existing.ToDictionary(
                x => (NormTag(x.TagName), x.CategoryId),
                x => x.TagId
            );

            // 缺的就補建
            var toAdd = new List<PhotoTag>();

            foreach (var k in keys)
            {
                var key = (k.normName, k.categoryId);
                if (map.ContainsKey(key)) continue;

                var entity = new PhotoTag
                {
                    TagName = k.raw,
                    CategoryId = k.categoryId,
                    TagType = "AI",              
                    DisplayOrder = 0,            
                    IsActive = true,             
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow  
                };

                toAdd.Add(entity);
            }

            if (toAdd.Count > 0)
            {
                _context.PhotoTags.AddRange(toAdd);
                await _context.SaveChangesAsync(ct);

                foreach (var t in toAdd)
                    map[(NormTag(t.TagName), t.CategoryId)] = t.TagId;
            }

            return map;
        }

        /// <summary>
        /// 批次新增 AI 標籤建議
        /// </summary>
        public async Task<List<PhotoAiclassificationSuggestion>> AddAISuggestionsAsync(
            List<PhotoAiclassificationSuggestion> suggestions)
        {
            try
            {
                _logger.LogInformation($"📝 批次新增 AI 建議，數量: {suggestions?.Count ?? 0}");

                if (suggestions == null || suggestions.Count == 0)
                {
                    _logger.LogInformation($"ℹ️ 無建議需要新增");
                    return new List<PhotoAiclassificationSuggestion>();
                }

                var result = await AddAISuggestionsInternalAsync(suggestions);

                _logger.LogInformation($"✅ 成功新增 {result.Count} 筆 AI 建議");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ 批次新增 AI 建議時發生錯誤");
                throw;
            }
        }

        /// <summary>
        /// 批次新增 AI 標籤建議（新增前會確保 PhotoTags 存在並回填 TagId）
        /// </summary>
        private async Task<List<PhotoAiclassificationSuggestion>> AddAISuggestionsInternalAsync(
            List<PhotoAiclassificationSuggestion> suggestions,
            CancellationToken ct = default)
        {
            if (suggestions == null || suggestions.Count == 0)
                return suggestions ?? new List<PhotoAiclassificationSuggestion>();

            // 1) 基本清理 + 去重（同張照片同分類同 tagName，保留最高分那筆）
            var cleaned = suggestions
                .Where(s => !string.IsNullOrWhiteSpace(s.TagName))
                .Select(s =>
                {
                    s.TagName = s.TagName!.Trim();
                    return s;
                })
                .GroupBy(s => new { s.PhotoId, s.CategoryId, Norm = NormTag(s.TagName!) })
                .Select(g => g.OrderByDescending(x => x.ConfidenceScore).First())
                .ToList();

            if (cleaned.Count == 0)
                return new List<PhotoAiclassificationSuggestion>();

            // 2) 確保 PhotoTags 存在，並回填 TagId 到 suggestion
            //    ⚠️ 這裡假設 CategoryId 是 int；如果你的 suggestion.CategoryId 是 long，請在這裡 (int) 轉型
            var tagInputs = cleaned.Select(s => (s.TagName!, (int)s.CategoryId)).ToList();
            var tagMap = await EnsurePhotoTagsAsync(tagInputs, ct);

            foreach (var s in cleaned)
            {
                var key = (NormTag(s.TagName!), (int)s.CategoryId);
                s.TagId = tagMap[key];                 // ✅ 讓 Suggestion.TagId 不再是 NULL
                if (s.CreatedAt == default)            // 如果是 DateTime
                    s.CreatedAt = DateTime.UtcNow;
                s.IsAdopted = s.IsAdopted;             // 視你的預設需求（通常新增時 false）
            }

            // 3) 建立「照片-標籤」關聯（Sidebar 的 photoCount 通常靠這張表）
            //    ⚠️ 你要把 PhotoPhotoTag 換成你專案真正的關聯 entity
            var pairs = cleaned
                .Select(s => new { s.PhotoId, TagId = (int)s.TagId })
                .Distinct()
                .ToList();

            var photoIds = pairs.Select(x => x.PhotoId).Distinct().ToList();
            var tagIds = pairs.Select(x => x.TagId).Distinct().ToList();

            // 先查已存在的關聯避免重複
            var existingLinks = await _context.PhotoPhotoTags   // <-- 這裡換成你的關聯 DbSet
                .Where(x => photoIds.Contains(x.PhotoId) && tagIds.Contains(x.TagId))
                .Select(x => new { x.PhotoId, x.TagId })
                .ToListAsync(ct);

            var linkSet = existingLinks.Select(x => (x.PhotoId, x.TagId)).ToHashSet();

            var aiSourceId = await _context.PhotoClassificationSources
                .Where(s => s.SourceCode == "AI")
                .Select(s => s.SourceId)
                .FirstOrDefaultAsync(ct);

            if (aiSourceId == 0)
                throw new InvalidOperationException("AI 來源尚未在資料庫中設定");

            var toLink = pairs
                .Where(p => !linkSet.Contains((p.PhotoId, p.TagId)))
                .Select(p => new PhotoPhotoTag
                {
                    PhotoId = p.PhotoId,
                    TagId = p.TagId,
                    SourceId = aiSourceId,
                    AddedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                })
                .ToList();

            if (toLink.Count > 0)
                _context.PhotoPhotoTags.AddRange(toLink);

            // 4) 寫入 AI suggestion（如果你會重跑分析，這裡也可以再做一次 DB 去重）
            await _context.PhotoAiclassificationSuggestions.AddRangeAsync(cleaned, ct);

            await _context.SaveChangesAsync(ct);
            return cleaned;
        }

        /// <summary>
        /// 取得指定 Log 的所有建議
        /// </summary>
        public async Task<List<PhotoAiclassificationSuggestion>> GetAISuggestionsByLogIdAsync(long logId)
        {
            try
            {
                _logger.LogInformation($"🔍 查詢 AI 建議，LogId: {logId}");

                var suggestions = await _context.PhotoAiclassificationSuggestions
                    .AsNoTracking()
                    .Include(s => s.Category)
                    .Where(s => s.LogId == logId)
                    .OrderByDescending(s => s.ConfidenceScore)
                    .ToListAsync();

                _logger.LogInformation($"✅ 找到 {suggestions.Count} 筆 AI 建議");
                return suggestions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ 查詢 AI 建議時發生錯誤，LogId: {logId}");
                throw;
            }
        }

        /// <summary>
        /// 取得照片的待處理建議（未採用）
        /// </summary>
        public async Task<List<PhotoAiclassificationSuggestion>> GetPendingSuggestionsAsync(long photoId, decimal? minConfidence = null)
        {
            try
            {
                _logger.LogInformation($"🔍 查詢待處理 AI 建議，PhotoId: {photoId}, MinConfidence: {minConfidence}");

                var query = _context.PhotoAiclassificationSuggestions
                    .AsNoTracking()
                    .Include(s => s.Category)
                    .Include(s => s.Log)
                    .Where(s => s.Log.PhotoId == photoId && s.IsAdopted == false);

                if (minConfidence.HasValue)
                {
                    query = query.Where(s => s.ConfidenceScore >= minConfidence.Value);
                }

                var suggestions = await query
                    .OrderByDescending(s => s.ConfidenceScore)
                    .ToListAsync();

                _logger.LogInformation($"✅ 找到 {suggestions.Count} 筆待處理建議");
                return suggestions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ 查詢待處理建議時發生錯誤，PhotoId: {photoId}");
                throw;
            }
        }

        /// <summary>
        /// 取得照片的所有 AI 建議
        /// </summary>
        public async Task<List<PhotoAiclassificationSuggestion>> GetAllAISuggestionsAsync(long photoId)
        {
            try
            {
                var suggestions = await _context.PhotoAiclassificationSuggestions
                    .AsNoTracking()
                    .Include(s => s.Category)
                    .Include(s => s.Log)
                    .Where(s => s.Log.PhotoId == photoId)
                    .OrderByDescending(s => s.CreatedAt)
                    .ThenByDescending(s => s.ConfidenceScore)
                    .ToListAsync();

                return suggestions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ 查詢所有 AI 建議時發生錯誤，PhotoId: {photoId}");
                throw;
            }
        }

        /// <summary>
        /// 根據建議 ID 查詢建議
        /// </summary>
        public async Task<PhotoAiclassificationSuggestion?> GetAISuggestionByIdAsync(long suggestionId)
        {
            try
            {
                return await _context.PhotoAiclassificationSuggestions
                    .AsNoTracking()
                    .Include(s => s.Category)
                    .Where(s => s.SuggestionId == suggestionId)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ 根據 ID 查詢建議時發生錯誤，SuggestionId: {suggestionId}");
                throw;
            }
        }

        /// <summary>
        /// 標記單一建議為已採用
        /// </summary>
        public async Task<bool> AdoptSuggestionAsync(long suggestionId)
        {
            try
            {
                _logger.LogInformation($"✏️ 標記建議為已採用，SuggestionId: {suggestionId}");

                var affectedRows = await _context.PhotoAiclassificationSuggestions
                    .Where(s => s.SuggestionId == suggestionId)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(s => s.IsAdopted, true));

                if (affectedRows == 0)
                {
                    _logger.LogWarning($"⚠️ 建議不存在，SuggestionId: {suggestionId}");
                    return false;
                }

                _logger.LogInformation($"✅ 建議已標記為已採用");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ 標記建議為已採用時發生錯誤，SuggestionId: {suggestionId}");
                throw;
            }
        }

        /// <summary>
        /// 批次標記建議為已採用
        /// </summary>
        public async Task<int> BulkAdoptSuggestionsAsync(List<long> suggestionIds)
        {
            try
            {
                _logger.LogInformation($"✏️ 批次標記建議為已採用，數量: {suggestionIds.Count}");

                var affectedRows = await _context.PhotoAiclassificationSuggestions
                    .Where(s => suggestionIds.Contains(s.SuggestionId))
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(s => s.IsAdopted, true));

                _logger.LogInformation($"✅ 成功標記 {affectedRows} 筆建議");
                return affectedRows;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ 批次標記建議為已採用時發生錯誤");
                throw;
            }
        }

        /// <summary>
        /// 取得照片的 AI 分析統計
        /// </summary>
        public async Task<PhotoAIAnalysisStatsDto> GetPhotoAIAnalysisStatsAsync(long photoId)
        {
            try
            {
                _logger.LogInformation($"📊 計算照片 AI 分析統計，PhotoId: {photoId}");

                var stats = await _context.PhotoAiclassificationSuggestions
                    .AsNoTracking()
                    .Where(s => s.Log.PhotoId == photoId)
                    .GroupBy(s => 1)
                    .Select(g => new PhotoAIAnalysisStatsDto
                    {
                        PhotoId = photoId,
                        TotalSuggestions = g.Count(),
                        AdoptedCount = g.Count(s => s.IsAdopted),
                        PendingCount = g.Count(s => !s.IsAdopted),
                        AverageConfidence = g.Average(s => (double)s.ConfidenceScore)
                    })
                    .FirstOrDefaultAsync();

                return stats ?? new PhotoAIAnalysisStatsDto { PhotoId = photoId };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ 計算照片 AI 分析統計時發生錯誤，PhotoId: {photoId}");
                throw;
            }
        }

        #endregion

        #region AI 標籤套用相關

        /// <summary>
        /// 檢查照片是否已有指定分類
        /// </summary>
        public async Task<bool> HasPhotoCategoryAsync(long photoId, int categoryId)
        {
            try
            {
                return await _context.PhotoPhotoCategories
                    .AsNoTracking()
                    .AnyAsync(pc => pc.PhotoId == photoId && pc.CategoryId == categoryId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ 檢查照片分類時發生錯誤，PhotoId: {photoId}, CategoryId: {categoryId}");
                throw;
            }
        }

        /// <summary>
        /// 檢查照片是否已有指定標籤
        /// </summary>
        public async Task<bool> HasPhotoTagAsync(long photoId, int tagId)
        {
            try
            {
                return await _context.PhotoPhotoTags
                    .AsNoTracking()
                    .AnyAsync(pt => pt.PhotoId == photoId && pt.TagId == tagId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ 檢查照片標籤時發生錯誤，PhotoId: {photoId}, TagId: {tagId}");
                throw;
            }
        }

        /// <summary>
        /// 為照片新增單一分類
        /// </summary>
        public async Task<bool> AddPhotoCategoryAsync(long photoId, int categoryId, int sourceId, decimal? confidence = null, long? assignedBy = null)
        {
            try
            {
                _logger.LogInformation($"📝 新增照片分類，PhotoId: {photoId}, CategoryId: {categoryId}, SourceId: {sourceId}");

                // 檢查是否已存在
                var exists = await HasPhotoCategoryAsync(photoId, categoryId);
                if (exists)
                {
                    _logger.LogInformation($"ℹ️ 照片分類已存在，跳過");
                    return false;
                }

                var photoCategory = new PhotoPhotoCategory
                {
                    PhotoId = photoId,
                    CategoryId = categoryId,
                    SourceId = sourceId,
                    Confidence = confidence,
                    AssignedBy = assignedBy,
                    AssignedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _context.PhotoPhotoCategories.AddAsync(photoCategory);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ 照片分類新增成功");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ 新增照片分類時發生錯誤，PhotoId: {photoId}, CategoryId: {categoryId}");
                throw;
            }
        }

        /// <summary>
        /// 為照片新增單一標籤
        /// </summary>
        public async Task<bool> AddPhotoTagAsync(long photoId, int tagId, int sourceId, decimal? confidence = null, long? addedBy = null)
        {
            try
            {
                _logger.LogInformation($"📝 新增照片標籤，PhotoId: {photoId}, TagId: {tagId}, SourceId: {sourceId}");

                // 檢查是否已存在
                var exists = await HasPhotoTagAsync(photoId, tagId);
                if (exists)
                {
                    _logger.LogInformation($"ℹ️ 照片標籤已存在，跳過");
                    return false;
                }

                var photoTag = new PhotoPhotoTag
                {
                    PhotoId = photoId,
                    TagId = tagId,
                    SourceId = sourceId,
                    Confidence = confidence,
                    AddedBy = addedBy,
                    AddedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.PhotoPhotoTags.AddAsync(photoTag);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ 照片標籤新增成功");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ 新增照片標籤時發生錯誤，PhotoId: {photoId}, TagId: {tagId}");
                throw;
            }
        }

        /// <summary>
        /// 批次新增照片分類（支援重複檢查）
        /// </summary>
        public async Task<int> BulkAddPhotoCategoriesAsync(List<PhotoPhotoCategory> photoCategories)
        {
            try
            {
                _logger.LogInformation($"📝 批次新增照片分類，數量: {photoCategories.Count}");

                // 取得已存在的分類組合
                var photoIds = photoCategories.Select(pc => pc.PhotoId).Distinct().ToList();
                var categoryIds = photoCategories.Select(pc => pc.CategoryId).Distinct().ToList();

                var existingPairs = await _context.PhotoPhotoCategories
                    .AsNoTracking()
                    .Where(pc => photoIds.Contains(pc.PhotoId) && categoryIds.Contains(pc.CategoryId))
                    .Select(pc => new { pc.PhotoId, pc.CategoryId })
                    .ToHashSetAsync();

                // 過濾掉已存在的
                var newCategories = photoCategories
                    .Where(pc => !existingPairs.Contains(new { pc.PhotoId, pc.CategoryId }))
                    .ToList();

                if (newCategories.Count == 0)
                {
                    _logger.LogInformation($"ℹ️ 所有分類已存在，無需新增");
                    return 0;
                }

                await _context.PhotoPhotoCategories.AddRangeAsync(newCategories);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ 批次新增成功，實際新增: {newCategories.Count} 筆，跳過: {photoCategories.Count - newCategories.Count} 筆");
                return newCategories.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ 批次新增照片分類時發生錯誤");
                throw;
            }
        }

        /// <summary>
        /// 批次新增照片標籤（支援重複檢查）
        /// </summary>
        public async Task<int> BulkAddPhotoTagsAsync(List<PhotoPhotoTag> photoTags)
        {
            try
            {
                _logger.LogInformation($"📝 批次新增照片標籤，數量: {photoTags.Count}");

                // 取得已存在的標籤組合
                var photoIds = photoTags.Select(pt => pt.PhotoId).Distinct().ToList();
                var tagIds = photoTags.Select(pt => pt.TagId).Distinct().ToList();

                var existingPairs = await _context.PhotoPhotoTags
                    .AsNoTracking()
                    .Where(pt => photoIds.Contains(pt.PhotoId) && tagIds.Contains(pt.TagId))
                    .Select(pt => new { pt.PhotoId, pt.TagId })
                    .ToHashSetAsync();

                // 過濾掉已存在的
                var newTags = photoTags
                    .Where(pt => !existingPairs.Contains(new { pt.PhotoId, pt.TagId }))
                    .ToList();

                if (newTags.Count == 0)
                {
                    _logger.LogInformation($"ℹ️ 所有標籤已存在，無需新增");
                    return 0;
                }

                await _context.PhotoPhotoTags.AddRangeAsync(newTags);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ 批次新增成功，實際新增: {newTags.Count} 筆，跳過: {photoTags.Count - newTags.Count} 筆");
                return newTags.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ 批次新增照片標籤時發生錯誤");
                throw;
            }
        }

        #endregion
    }
}