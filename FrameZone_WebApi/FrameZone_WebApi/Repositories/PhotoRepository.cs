using FrameZone_WebApi.DTOs;
using FrameZone_WebApi.Helpers;
using FrameZone_WebApi.Models;
using FrameZone_WebApi.Services;
using Microsoft.EntityFrameworkCore;

namespace FrameZone_WebApi.Repositories
{
    public partial class PhotoRepository : IPhotoRepository
    {
        #region 依賴注入

        private readonly AAContext _context;
        private readonly ITagCategorizationService _tagCategorizationService;
        private readonly ILogger<PhotoRepository> _logger;

        public PhotoRepository(
            AAContext context,
            ITagCategorizationService tagCategorizationService,
            ILogger<PhotoRepository> logger)
        {
            _context = context;
            _tagCategorizationService = tagCategorizationService;
            _logger = logger;
        }

        #endregion

        #region Photo 表操作

        public async Task<Photo> AddPhotoAsync(Photo photo)
        {
            try
            {
                _logger.LogInformation($"新增照片: {photo.FileName}");

                await _context.Photos.AddAsync(photo);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"照片新增成功，PhotoId: {photo.PhotoId}");
                return photo;
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "新增照片時發生錯誤");
                throw;
            }
        }

        /// <summary>
        /// 根據 PhotoId 查詢照片
        /// </summary>
        public async Task<Photo> GetPhotoByIdAsync(long photoId)
        {
            try
            {
                return await _context.Photos
                    .Where(p => p.PhotoId == photoId && p.IsDeleted == false)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"查詢照片時發生錯誤，PhotoId: {photoId}");
                throw;
            }
        }

        /// <summary>
        /// 只查照片擁有者 UserId（避免載入 PhotoData/ThumbnailData）
        /// </summary>
        public async Task<long?> GetPhotoOwnerUserIdAsync(long photoId)
        {
            return await _context.Photos
                .AsNoTracking()
                .Where(p => p.PhotoId == photoId && p.IsDeleted == false)
                .Select(p => (long?)p.UserId)
                .FirstOrDefaultAsync();
        }


        /// <summary>
        /// 根據 Hash 查詢照片
        /// </summary>
        public async Task<Photo> GetPhotoByHashAsync(long userId, string hash)
        {
            try
            {
                _logger.LogInformation($"查詢照片 Hash，UserId: {userId}, Hash: {hash}");

                // 確保參數不為 null
                if (string.IsNullOrEmpty(hash))
                {
                    _logger.LogWarning("Hash 參數為空");
                    return null;
                }

                var result = await _context.Photos
                    .Where(p => p.UserId == userId &&
                               p.Hash == hash &&
                               p.IsDeleted == false)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                _logger.LogInformation($"查詢完成，結果: {(result != null ? $"找到 PhotoId={result.PhotoId}" : "未找到")}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"根據 Hash 查詢照片時發生錯誤，UserId: {userId}, Hash: {hash}");
                throw;
            }
        }

        /// <summary>
        /// 更新照片資訊
        /// </summary>
        public async Task<bool> UpdatePhotoAsync(Photo photo)
        {
            try
            {
                _logger.LogInformation($"更新照片，PhotoId: {photo.PhotoId}");

                _context.Photos.Update(photo);
                var result = await _context.SaveChangesAsync();

                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新照片時發生錯誤，PhotoId: {photo.PhotoId}");
                throw;
            }
        }

        /// <summary>
        /// 軟刪除照片
        /// </summary>
        public async Task<bool> SoftDeletePhotoAsync(long photoId)
        {
            try
            {
                _logger.LogInformation($"軟刪除照片，PhotoId: {photoId}");

                // 使用 ExecuteUpdateAsync 直接更新，不載入實體
                var affectedRows = await _context.Photos
                    .Where(p => p.PhotoId == photoId && p.IsDeleted == false)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(p => p.IsDeleted, true)
                        .SetProperty(p => p.DeletedAt, DateTime.UtcNow)
                        .SetProperty(p => p.UpdatedAt, DateTime.UtcNow)
                    );

                if (affectedRows == 0)
                {
                    _logger.LogWarning($"照片不存在或已刪除，PhotoId: {photoId}");
                    return false;
                }

                _logger.LogInformation($"✅ 軟刪除成功，PhotoId: {photoId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"軟刪除照片時發生錯誤，PhotoId: {photoId}");
                throw;
            }
        }

        /// <summary>
        /// 查詢使用者的所有照片
        /// </summary>
        public async Task<List<Photo>> GetPhotosByUserIdAsync(long userId, int pageIndex = 1, int pageSize = 20)
        {
            try
            {
                return await _context.Photos
                    .AsNoTracking()
                    .Where(p => p.UserId == userId && p.IsDeleted == false)
                    .OrderByDescending(p => p.UpdatedAt)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new Photo
                    {
                        PhotoId = p.PhotoId,
                        UserId = p.UserId,
                        FileName = p.FileName,
                        FileExtension = p.FileExtension,
                        FileSize = p.FileSize,
                        UploadedAt = p.UploadedAt,
                        UpdatedAt = p.UpdatedAt
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"查詢使用者照片時發生錯誤，UserId: {userId}");
                throw;
            }
        }
        public async Task<int> GetUserPhotoCountAsync(long userId)
        {
            return await _context.Photos
                .AsNoTracking()
                .Where(p => p.UserId == userId && p.IsDeleted == false)
                .CountAsync();
        }

        #endregion

        #region PhotoMetadata 表操作

        /// <summary>
        /// 新增照片元數據 (EXIF 資訊)
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public async Task<PhotoMetadatum> AddPhotoMetadataAsync(PhotoMetadatum metadata)
        {
            try
            {
                _logger.LogInformation($"新增照片元數據，PhotoId: {metadata.PhotoId}");

                await _context.PhotoMetadata.AddAsync(metadata);
                await _context.SaveChangesAsync();
                return metadata;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"新增照片元數據時發生錯誤，PhotoId: {metadata.PhotoId}");
                throw;
            }
        }

        /// <summary>
        /// 根據 PhotoId 查詢元數據
        /// </summary>
        public async Task<PhotoMetadatum> GetPhotoMetadataByPhotoIdAsync(long photoId)
        {
            try
            {
                return await _context.PhotoMetadata
                    .Where(m => m.PhotoId == photoId)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"查詢照片元數據時發生錯誤，PhotoId: {photoId}");
                throw;
            }
        }

        #endregion

        #region PhotoTag 表操作

        /// <summary>
        /// 根據標籤名稱和類型查詢標籤
        /// </summary>
        public async Task<PhotoTag> GetTagByNameAsync(string tagName, string tagType)
        {
            try
            {
                return await _context.PhotoTags
                    .AsNoTracking()
                    .Where(t => t.TagName == tagName && t.TagType == tagType && t.IsActive == true)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"查詢標籤時發生錯誤，TagName: {tagName}, TagType: {tagType}");
                throw;
            }
        }

        /// <summary>
        /// 建立新標籤
        /// </summary>
        public async Task<PhotoTag> CreateTagAsync(PhotoTag tag)
        {
            try
            {
                _logger.LogInformation($"建立新標籤: {tag.TagName}, 類型: {tag.TagType}");

                tag.CreatedAt = DateTime.UtcNow;
                tag.UpdatedAt = DateTime.UtcNow;
                tag.IsActive = true;

                await _context.PhotoTags.AddAsync(tag);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"標籤建立成功，TagId: {tag.TagId}");
                return tag;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"建立標籤時發生錯誤，TagName: {tag.TagName}");
                throw;
            }
        }

        /// <summary>
        /// 取得或建立標籤
        /// </summary>
        public async Task<PhotoTag> GetOrCreateTagAsync(
            string tagName,
            string tagType,
            int categoryId,
            int? parentTagId = null,
            long? userId = null)
        {
            try
            {
                // 先查詢標籤是否已存在
                var existingTag = await _context.PhotoTags
                    .AsNoTracking()
                    .Where(t => t.TagName == tagName
                             && t.TagType == tagType
                             && t.CategoryId == categoryId
                             && t.IsActive == true)
                    .FirstOrDefaultAsync();

                if (existingTag != null)
                {
                    _logger.LogInformation($"標籤已存在，TagId: {existingTag.TagId}, TagName: {tagName}");
                    return existingTag;
                }

                // 標籤不存在，建立新標籤
                var newTag = new PhotoTag
                {
                    CategoryId = categoryId,
                    TagName = tagName,
                    TagType = tagType,
                    ParentTagId = parentTagId,
                    DisplayOrder = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _context.PhotoTags.AddAsync(newTag);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"建立新標籤，TagId: {newTag.TagId}, TagName: {tagName}, CategoryId: {categoryId}");
                return newTag;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"取得或建立標籤時發生錯誤，TagName: {tagName}");
                throw;
            }
        }

        /// <summary>
        /// 搜尋標籤
        /// 支援關鍵字搜尋、標籤類型篩選、分類篩選
        /// </summary>
        public async Task<List<TagItemDTO>> SearchTagsAsync(
            string keyword,
            long userId,
            bool includeSystemTags = true,
            bool includeUserTags = true,
            int? categoryId = null,
            int limit = 20)
        {
            try
            {
                _logger.LogInformation($"🔍 開始搜尋標籤，Keyword: {keyword}, UserId: {userId}, Limit: {limit}");

                // 建立基礎查詢
                var query = _context.PhotoTags
                    .AsNoTracking()
                    .Where(t => t.IsActive == true);

                // 關鍵字模糊搜尋
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    query = query.Where(t => EF.Functions.Like(t.TagName, $"%{keyword}%"));
                }

                // 標籤類型篩選
                var tagTypes = new List<string>();
                if (includeSystemTags)
                {
                    tagTypes.Add(PhotoConstants.TAG_TYPE_SYSTEM);
                }
                if (includeUserTags)
                {
                    tagTypes.Add(PhotoConstants.TAG_TYPE_USER);
                    tagTypes.Add(PhotoConstants.TAG_TYPE_CUSTOM);
                }

                if (tagTypes.Any())
                {
                    query = query.Where(t => tagTypes.Contains(t.TagType));
                }

                // 分類篩選
                if (categoryId.HasValue)
                {
                    query = query.Where(t => t.CategoryId == categoryId.Value);
                }

                // JOIN 分類資訊並統計照片數量
                var results = await query
                    .Join(_context.PhotoCategories,
                        tag => tag.CategoryId,
                        category => category.CategoryId,
                        (tag, category) => new { Tag = tag, Category = category })
                    .GroupJoin(
                        _context.PhotoPhotoTags
                            .Join(_context.Photos,
                                pt => pt.PhotoId,
                                p => p.PhotoId,
                                (pt, p) => new { pt, p })
                            .Where(x => x.p.UserId == userId && x.p.IsDeleted == false),
                        tc => tc.Tag.TagId,
                        pt => pt.pt.TagId,
                        (tc, photoTags) => new { tc.Tag, tc.Category, PhotoCount = photoTags.Count() })
                    .Select(x => new TagItemDTO
                    {
                        TagId = x.Tag.TagId,
                        TagName = x.Tag.TagName,
                        TagType = x.Tag.TagType,
                        CategoryId = x.Category.CategoryId,
                        CategoryName = x.Category.CategoryName,
                        ParentTagId = x.Tag.ParentTagId,
                        PhotoCount = x.PhotoCount,
                        DisplayOrder = x.Tag.DisplayOrder,
                        IsUserCreated = x.Tag.TagType == PhotoConstants.TAG_TYPE_CUSTOM
                    })
                    .OrderByDescending(t => t.PhotoCount)
                    .ThenBy(t => t.TagName)
                    .Take(limit)
                    .ToListAsync();

                // 查詢父標籤名稱
                var parentTagIds = results.Where(r => r.ParentTagId.HasValue).Select(r => r.ParentTagId.Value).Distinct().ToList();
                if (parentTagIds.Any())
                {
                    var parentTags = await _context.PhotoTags
                        .AsNoTracking()
                        .Where(t => parentTagIds.Contains(t.TagId))
                        .ToDictionaryAsync(t => t.TagId, t => t.TagName);

                    foreach (var result in results.Where(r => r.ParentTagId.HasValue))
                    {
                        if (parentTags.TryGetValue(result.ParentTagId.Value, out var parentName))
                        {
                            result.ParentTagName = parentName;
                        }
                    }
                }

                _logger.LogInformation($"✅ 標籤搜尋完成，找到 {results.Count} 個標籤");
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ 搜尋標籤時發生錯誤，Keyword: {keyword}");
                throw;
            }
        }

        #endregion

        #region PhotoPhotoTag 表操作

        public async Task<PhotoPhotoTag> AddPhotoTagAsync(PhotoPhotoTag photoPhotoTag)
        {
            try
            {
                _logger.LogInformation($"新增照片標籤關聯，PhotoId: {photoPhotoTag.PhotoId}, TagId: {photoPhotoTag.TagId}");

                photoPhotoTag.AddedAt = DateTime.UtcNow;
                photoPhotoTag.CreatedAt = DateTime.UtcNow;

                await _context.PhotoPhotoTags.AddAsync(photoPhotoTag);
                await _context.SaveChangesAsync();

                return photoPhotoTag;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"新增照片標籤關聯時發生錯誤，PhotoId: {photoPhotoTag.PhotoId}, TagId: {photoPhotoTag.TagId}");
                throw;
            }
        }

        /// <summary>
        /// 批次新增照片標籤
        /// </summary>
        public async Task<int> AddPhotoTagsBatchAsync(long photoId, List<int> tagIds, int sourceId, decimal? confidence = null)
        {
            try
            {
                _logger.LogInformation($"批次新增照片標籤，PhotoId: {photoId}, 標籤數量: {tagIds.Count}");

                var photoPhotoTags = tagIds.Select(tagId => new PhotoPhotoTag
                {
                    PhotoId = photoId,
                    TagId = tagId,
                    SourceId = sourceId,
                    Confidence = confidence,
                    AddedBy = null,
                    AddedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                await _context.PhotoPhotoTags.AddRangeAsync(photoPhotoTags);
                var result = await _context.SaveChangesAsync();

                _logger.LogInformation($"批次新增完成，成功: {result} 筆");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"批次新增照片標籤時發生錯誤，PhotoId: {photoId}");
                throw;
            }
        }

        /// <summary>
        /// 查詢照片的所有標籤
        /// </summary>
        public async Task<List<PhotoTag>> GetPhotoTagsByPhotoIdAsync(long photoId)
        {
            try
            {
                return await _context.PhotoPhotoTags
                    .Where(pt => pt.PhotoId == photoId)
                    .Include(pt => pt.Tag)
                    .Where(pt => pt.Tag.IsActive == true)
                    .Select(pt => pt.Tag)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"查詢照片標籤時發生錯誤，PhotoId: {photoId}");
                throw;
            }
        }

        /// <summary>
        /// 取得照片標籤詳細資訊
        /// 返回該照片的所有標籤，並按來源分類（EXIF、MANUAL、AI、GEOCODING）
        /// </summary>
        public async Task<PhotoTagsDetailDTO> GetPhotoTagsWithDetailsAsync(long photoId)
        {
            try
            {
                _logger.LogInformation($"🏷️ 開始查詢照片標籤詳細資訊，PhotoId: {photoId}");

                // 查詢照片的所有標籤關聯
                var photoTags = await _context.PhotoPhotoTags
                    .AsNoTracking()
                    .Where(pt => pt.PhotoId == photoId)
                    .Include(pt => pt.Tag)
                        .ThenInclude(t => t.Category)
                    .Include(pt => pt.Source)
                    .Where(pt => pt.Tag.IsActive == true)
                    .OrderBy(pt => pt.Tag.TagName)
                    .ToListAsync();

                // 組裝 DTO
                var allTags = photoTags.Select(pt => new PhotoTagItemDTO
                {
                    TagId = pt.TagId,
                    TagName = pt.Tag.TagName,
                    TagType = pt.Tag.TagType,
                    CategoryName = pt.Tag.Category?.CategoryName,
                    SourceId = pt.SourceId,
                    SourceName = pt.Source?.SourceName,
                    Confidence = pt.Confidence,
                    AddedAt = pt.AddedAt,
                    CanRemove = pt.Source?.SourceCode == PhotoConstants.SOURCE_MANUAL
                }).ToList();

                // 按來源分類
                var result = new PhotoTagsDetailDTO
                {
                    PhotoId = photoId,
                    AllTags = allTags,
                    ExifTags = allTags.Where(t => t.SourceName == PhotoConstants.SOURCE_EXIF).ToList(),
                    GeocodingTags = allTags.Where(t => t.SourceName == PhotoConstants.SOURCE_GEOCODING).ToList(),
                    ManualTags = allTags.Where(t => t.SourceName == PhotoConstants.SOURCE_MANUAL).ToList(),
                    AiTags = allTags.Where(t => t.SourceName == PhotoConstants.SOURCE_AI).ToList(),
                    TotalCount = allTags.Count
                };

                _logger.LogInformation($"✅ 照片標籤查詢完成，共 {result.TotalCount} 個標籤");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ 查詢照片標籤詳細資訊時發生錯誤，PhotoId: {photoId}");
                throw;
            }
        }

        /// <summary>
        /// 移除照片標籤
        /// </summary>
        public async Task<bool> RemovePhotoTagAsync(long photoId, int tagId)
        {
            try
            {
                _logger.LogInformation($"移除照片標籤，PhotoId: {photoId}, TagId: {tagId}");

                var photoTag = await _context.PhotoPhotoTags
                    .Where(pt => pt.PhotoId == photoId && pt.TagId == tagId)
                    .FirstOrDefaultAsync();

                if (photoTag == null)
                {
                    _logger.LogWarning($"照片標籤關聯不存在");
                    return false;
                }

                _context.PhotoPhotoTags.Remove(photoTag);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"移除照片標籤時發生錯誤，PhotoId: {photoId}, TagId: {tagId}");
                throw;
            }
        }

        #endregion

        #region PhotoLocation 表操作

        /// <summary>
        /// 新增照片地點資訊
        /// </summary>
        public async Task<PhotoLocation> AddPhotoLocationAsync(PhotoLocation location)
        {
            try
            {
                _logger.LogInformation($"新增照片地點，PhotoId: {location.PhotoId}");

                location.CreatedAt = DateTime.Now;
                location.UpdatedAt = DateTime.Now;

                await _context.PhotoLocations.AddAsync(location);
                await _context.SaveChangesAsync();

                return location;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"新增照片地點時發生錯誤，PhotoId: {location.PhotoId}");
                throw;
            }
        }

        public async Task<PhotoLocation> GetPhotoLocationByPhotoIdAsync(long photoId)
        {
            try
            {
                return await _context.PhotoLocations
                    .Where(l => l.PhotoId == photoId)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"查詢照片地點時發生錯誤，PhotoId: {photoId}");
                throw;
            }
        }

        #endregion

        #region ClassificationSource 表操作

        public async Task<PhotoClassificationSource> GetClassificationSourceByCodeAsync(string sourceCode)
        {
            try
            {
                return await _context.PhotoClassificationSources
                    .Where(s => s.SourceCode == sourceCode && s.IsActive == true)
                    .FirstOrDefaultAsync();
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, $"查詢分類來源時發生錯誤，SourceCode: {sourceCode}");
                throw;
            }
        }

        #endregion

        #region PhotoCategory 表操作

        /// <summary>
        /// 根據分類代碼查詢分類
        /// </summary>
        public async Task<PhotoCategory> GetCategoryByCodeAsync(string categoryCode)
        {
            try
            {
                return await _context.PhotoCategories
                    .AsNoTracking()
                    .Where(c => c.CategoryCode == categoryCode && c.IsActive == true)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"根據代碼查詢分類時發生錯誤，CategoryCode: {categoryCode}");
                throw;
            }
        }

        /// <summary>
        /// 根據分類 ID 查詢分類
        /// </summary>
        public async Task<PhotoCategory> GetCategoryByIdAsync(int categoryId)
        {
            try
            {
                return await _context.PhotoCategories
                    .AsNoTracking()
                    .Where(c => c.CategoryId == categoryId && c.IsActive == true)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"根據 ID 查詢分類時發生錯誤，CategoryId: {categoryId}");
                throw;
            }
        }

        /// <summary>
        /// 根據分類類型查詢所有分類
        /// </summary>
        public async Task<List<PhotoCategory>> GetCategoriesByTypeAsync(string categoryTypeCode)
        {
            try
            {
                var categoryType = await _context.PhotoCategoryTypes
                    .Where(t => t.TypeCode == categoryTypeCode && t.IsActive == true)
                    .FirstOrDefaultAsync();

                if (categoryType == null)
                    return new List<PhotoCategory>();

                return await _context.PhotoCategories
                    .Where(c => c.CategoryTypeId == categoryType.CategoryTypeId && c.IsActive == true)
                    .OrderBy(c => c.DisplayOrder)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"根據類型查詢分類時發生錯誤，TypeCode: {categoryTypeCode}");
                throw;
            }
        }

        /// <summary>
        /// 取得或建立分類
        /// </summary>
        public async Task<PhotoCategory> GetOrCreateCategoryAsync(
            string categoryName,
            string categoryCode,
            int categoryTypeId,
            int? parentCategoryId = null,
            long? userId = null)
        {
            try
            {
                // 先查詢分類是否已存在
                var existingCategory = await _context.PhotoCategories
                    .AsNoTracking()
                    .Where(c => c.CategoryCode == categoryCode
                             && c.CategoryTypeId == categoryTypeId
                             && c.IsActive == true)
                    .FirstOrDefaultAsync();

                if (existingCategory != null)
                {
                    _logger.LogInformation($"分類已存在，CategoryId: {existingCategory.CategoryId}, CategoryName: {categoryName}");
                    return existingCategory;
                }

                // 分類不存在，建立新分類
                var newCategory = new PhotoCategory
                {
                    CategoryTypeId = categoryTypeId,
                    UserId = userId,
                    CategoryName = categoryName,
                    CategoryCode = categoryCode,
                    ParentCategoryId = parentCategoryId,
                    DisplayOrder = 0,
                    IsComingSoon = false,
                    IsDefaultExpanded = false,
                    MaxItemsToShow = 10,
                    UiType = "flat",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _context.PhotoCategories.AddAsync(newCategory);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"建立新分類，CategoryId: {newCategory.CategoryId}, CategoryName: {categoryName}");
                return newCategory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"取得或建立分類時發生錯誤，CategoryName: {categoryName}");
                throw;
            }
        }

        /// <summary>
        /// 取得分類樹（包含照片數量統計）
        /// </summary>
        public async Task<List<CategoryTreeNodeDTO>> GetCategoryTreeWithCountsAsync(long userId, int? parentCategoryId = null)
        {
            try
            {
                _logger.LogInformation($"開始查詢分類樹，UserId: {userId}, ParentCategoryId: {parentCategoryId}");

                // 查詢分類
                var categories = await _context.PhotoCategories
                    .Where(c => c.ParentCategoryId == parentCategoryId && c.IsActive == true)
                    .OrderBy(c => c.DisplayOrder)
                    .ThenBy(c => c.CategoryName)
                    .ToListAsync();

                var result = new List<CategoryTreeNodeDTO>();

                foreach (var category in categories)
                {
                    // 統計照片數量（透過 PhotoPhotoCategory）
                    var photoCount = await _context.PhotoPhotoCategories
                        .Where(pc => pc.CategoryId == category.CategoryId)
                        .Join(_context.Photos,
                              pc => pc.PhotoId,
                              p => p.PhotoId,
                              (pc, p) => new { pc, p })
                        .Where(x => x.p.UserId == userId && x.p.IsDeleted == false)
                        .CountAsync();

                    var node = new CategoryTreeNodeDTO
                    {
                        CategoryId = category.CategoryId,
                        CategoryName = category.CategoryName,
                        CategoryCode = category.CategoryCode,
                        CategoryTypeId = category.CategoryTypeId,
                        ParentCategoryId = category.ParentCategoryId,
                        PhotoCount = photoCount,
                        IsDefaultExpanded = category.IsDefaultExpanded,
                        IsComingSoon = category.IsComingSoon,
                        UiType = category.UiType,
                        DisplayOrder = category.DisplayOrder,
                        Children = new List<CategoryTreeNodeDTO>()
                    };

                    // 遞迴查詢子分類
                    node.Children = await GetCategoryTreeWithCountsAsync(userId, category.CategoryId);

                    result.Add(node);
                }

                _logger.LogInformation($"分類樹查詢完成，數量: {result.Count}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"查詢分類樹時發生錯誤，UserId: {userId}");
                throw;
            }
        }

        /// <summary>
        /// 取得可用分類列表
        /// 返回系統分類和用戶自定義分類，用於標籤建立時選擇分類
        /// </summary>
        public async Task<AvailableCategoriesResponseDTO> GetCategoryListAsync(long userId)
        {
            try
            {
                _logger.LogInformation($"📂 開始查詢可用分類列表，UserId: {userId}");

                // 查詢系統分類（UserId IS NULL）
                var systemCategories = await _context.PhotoCategories
                    .AsNoTracking()
                    .Where(c => c.UserId == null && c.IsActive == true)
                    .GroupJoin(_context.PhotoTags.Where(t => t.IsActive == true),
                        category => category.CategoryId,
                        tag => tag.CategoryId,
                        (category, tags) => new { Category = category, TagCount = tags.Count() })
                    .Select(x => new CategoryItemDTO
                    {
                        CategoryId = x.Category.CategoryId,
                        CategoryName = x.Category.CategoryName,
                        CategoryCode = x.Category.CategoryCode,
                        IsUserDefined = false,
                        TagCount = x.TagCount,
                        DisplayOrder = x.Category.DisplayOrder
                    })
                    .OrderBy(c => c.DisplayOrder)
                    .ThenBy(c => c.CategoryName)
                    .ToListAsync();

                // 查詢用戶自定義分類（UserId = userId）
                var userCategories = await _context.PhotoCategories
                    .AsNoTracking()
                    .Where(c => c.UserId == userId && c.IsActive == true)
                    .GroupJoin(_context.PhotoTags.Where(t => t.IsActive == true),
                        category => category.CategoryId,
                        tag => tag.CategoryId,
                        (category, tags) => new { Category = category, TagCount = tags.Count() })
                    .Select(x => new CategoryItemDTO
                    {
                        CategoryId = x.Category.CategoryId,
                        CategoryName = x.Category.CategoryName,
                        CategoryCode = x.Category.CategoryCode,
                        IsUserDefined = true,
                        TagCount = x.TagCount,
                        DisplayOrder = x.Category.DisplayOrder
                    })
                    .OrderBy(c => c.DisplayOrder)
                    .ThenBy(c => c.CategoryName)
                    .ToListAsync();

                var result = new AvailableCategoriesResponseDTO
                {
                    Success = true,
                    Message = "查詢成功",
                    SystemCategories = systemCategories,
                    UserCategories = userCategories
                };

                _logger.LogInformation($"✅ 分類列表查詢完成，系統分類: {systemCategories.Count}，用戶分類: {userCategories.Count}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ 查詢可用分類列表時發生錯誤，UserId: {userId}");
                return new AvailableCategoriesResponseDTO
                {
                    Success = false,
                    Message = $"查詢失敗：{ex.Message}",
                    SystemCategories = new List<CategoryItemDTO>(),
                    UserCategories = new List<CategoryItemDTO>()
                };
            }
        }

        #endregion

        #region PhotoPhotoCategory 表操作

        /// <summary>
        /// 批次新增照片與分類的關聯
        /// </summary>
        public async Task<int> AddPhotoCategoriesAsync(
            long photoId,
            List<int> categoryIds,
            int sourceId,
            decimal? confidence = null)
        {
            try
            {
                if (categoryIds == null || categoryIds.Count == 0)
                    return 0;

                _logger.LogInformation($"批次新增照片分類關聯，PhotoId: {photoId}, 數量: {categoryIds.Count}");

                var photoCategories = categoryIds.Select(categoryId => new PhotoPhotoCategory
                {
                    PhotoId = photoId,
                    CategoryId = categoryId,
                    SourceId = sourceId,
                    Confidence = confidence,
                    AssignedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }).ToList();

                await _context.PhotoPhotoCategories.AddRangeAsync(photoCategories);
                var result = await _context.SaveChangesAsync();

                _logger.LogInformation($"照片分類關聯新增成功，數量: {result}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"批次新增照片分類關聯時發生錯誤，PhotoId: {photoId}");
                throw;
            }
        }

        /// <summary>
        /// 查詢照片的所有分類
        /// </summary>
        public async Task<List<PhotoCategory>> GetPhotoCategoriesByPhotoIdAsync(long photoId)
        {
            try
            {
                return await _context.PhotoPhotoCategories
                    .Where(pc => pc.PhotoId == photoId)
                    .Include(pc => pc.Category)
                    .Select(pc => pc.Category)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"查詢照片分類時發生錯誤，PhotoId: {photoId}");
                throw;
            }
        }

        #endregion

        #region 進階查詢

        /// <summary>
        /// 查詢照片完整資訊
        /// </summary>
        public async Task<PhotoDetailDTO> GetPhotoDetailAsync(long photoId)
        {
            try
            {
                var photo = await _context.Photos
                    .Where(p => p.PhotoId == photoId && p.IsDeleted == false)
                    .Include(p => p.PhotoMetadata)
                    .Include(p => p.PhotoPhotoTags)
                        .ThenInclude(pt => pt.Tag)
                    .Include(p => p.PhotoPhotoTags)
                        .ThenInclude(pt => pt.Source)
                    .Include(p => p.PhotoLocations)
                    .FirstOrDefaultAsync();

                if (photo == null)
                {
                    return null;
                }

                var metadata = photo.PhotoMetadata.FirstOrDefault();
                var location = photo.PhotoLocations.FirstOrDefault();

                var exifTags = photo.PhotoPhotoTags
                    .Where(pt => pt.Source.SourceCode == PhotoConstants.SOURCE_EXIF)
                    .Select(pt => pt.Tag.TagName)
                    .ToList();

                var manualTags = photo.PhotoPhotoTags
                    .Where(pt => pt.Source.SourceCode == PhotoConstants.SOURCE_MANUAL)
                    .Select(pt => pt.Tag.TagName)
                    .ToList();

                var aiTags = photo.PhotoPhotoTags
                    .Where(pt => pt.Source.SourceCode == PhotoConstants.SOURCE_AI)
                    .Select(pt => pt.Tag.TagName)
                    .ToList();

                return new PhotoDetailDTO
                {
                    PhotoId = photo.PhotoId,
                    UserId = photo.UserId,
                    FileName = photo.FileName,
                    FileExtension = photo.FileExtension,
                    FileSize = photo.FileSize,
                    BlobUrl = null,
                    ThumbnailUrl = null,
                    UploadedAt = photo.UploadedAt,
                    Metadata = metadata != null ? new PhotoMetadataDTO
                    {
                        FileName = photo.FileName,
                        FileSize = photo.FileSize,
                        FileExtension = photo.FileExtension,
                        DateTaken = metadata.DateTaken,
                        GPSLatitude = metadata.Gpslatitude,
                        GPSLongitude = metadata.Gpslongitude,
                        CameraMake = metadata.CameraMake,
                        CameraModel = metadata.CameraModel,
                        Hash = photo.Hash
                    } : null,
                    ExifTags = exifTags,
                    ManualTags = manualTags,
                    AiTags = aiTags,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"查詢照片完整資訊時發生錯誤，PhotoId: {photoId}");
                throw;
            }
        }

        /// <summary>
        /// 根據標籤查詢照片
        /// </summary>
        public async Task<List<Photo>> GetPhotosByTagIdAsync(long userId, int tagId, int pageIndex = 1, int pageSize = 20)
        {
            try
            {
                return await _context.PhotoPhotoTags
                    .Where(pt => pt.TagId == tagId)
                    .Include(pt => pt.Photo)
                    .Where(pt => pt.Photo.UserId == userId && pt.Photo.IsDeleted == false)
                    .Select(pt => pt.Photo)
                    .OrderByDescending(p => p.UploadedAt)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"根據標籤查詢照片時發生錯誤，TagId: {tagId}");
                throw;
            }
        }

        // 根據日期範圍查詢照片
        public async Task<List<Photo>> GetPhotosByDateRangeAsync(long userId, DateTime? startDate, DateTime? endDate, int pageIndex = 1, int pageSize = 20)
        {
            try
            {
                IQueryable<Photo> query = _context.Photos
                    .Where(p => p.UserId == userId && p.IsDeleted == false)
                    .Include(p => p.PhotoMetadata);

                if (startDate.HasValue)
                {
                    query = query.Where(p =>
                        (p.PhotoMetadata.Any() && p.PhotoMetadata.First().DateTaken >= startDate) ||
                        (!p.PhotoMetadata.Any() && p.UploadedAt >= startDate));
                }

                if (endDate.HasValue)
                {
                    query = query.Where(p =>
                    (p.PhotoMetadata.Any() && p.PhotoMetadata.First().DateTaken <= endDate) ||
                    (!p.PhotoMetadata.Any() && p.UploadedAt < endDate));
                }

                return await query
                    .OrderByDescending(p => p.UploadedAt)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"根據日期範圍查詢照片時發生錯誤");
                throw;
            }
        }

        /// <summary>
        /// 查詢照片（支援多條件篩選、分頁、排序）
        /// </summary>
        public async Task<(List<Photo> Photos, int TotalCount)> QueryPhotosAsync(
            PhotoQueryRequestDTO request,
            long userId)
        {
            try
            {
                _logger.LogInformation("開始查詢照片，UserId: {UserId}, PageNumber: {PageNumber}, PageSize: {PageSize}",
                    userId, request.PageNumber, request.PageSize);

                // ===== 基礎查詢（不做投影）=====
                var baseQuery = _context.Photos
                    .AsNoTracking()
                    .Where(p => p.UserId == userId && p.IsDeleted == false);

                // ===== 套用所有篩選條件 =====
                #region 時間篩選

                // 日期範圍
                if (request.StartDate.HasValue)
                {
                    baseQuery = baseQuery.Where(p =>
                        p.PhotoMetadata.Any(m => m.DateTaken >= request.StartDate));
                }

                if (request.EndDate.HasValue)
                {
                    baseQuery = baseQuery.Where(p =>
                        p.PhotoMetadata.Any(m => m.DateTaken <= request.EndDate));
                }

                // 年份篩選 (多選)
                if (request.Years != null && request.Years.Any())
                {
                    baseQuery = baseQuery.Where(p =>
                        p.PhotoMetadata.Any(m =>
                            m.DateTaken.HasValue && request.Years.Contains(m.DateTaken.Value.Year)));
                }

                // 月份篩選 (多選)
                if (request.Months != null && request.Months.Any())
                {
                    baseQuery = baseQuery.Where(p =>
                        p.PhotoMetadata.Any(m =>
                            m.DateTaken.HasValue && request.Months.Contains(m.DateTaken.Value.Month)));
                }

                #endregion

                #region 分類與標籤篩選

                // 分類 ID 篩選
                if (request.CategoryIds != null && request.CategoryIds.Any())
                {
                    baseQuery = baseQuery.Where(p =>
                        p.PhotoPhotoCategories.Any(ppc => request.CategoryIds.Contains(ppc.CategoryId)));
                }

                // 標籤 ID 篩選
                if (request.TagIds != null && request.TagIds.Any())
                {
                    baseQuery = baseQuery.Where(p =>
                        p.PhotoPhotoTags.Any(ppt => request.TagIds.Contains(ppt.TagId)));
                }

                // 標籤名稱篩選（使用導航屬性，避免巢狀子查詢）
                if (request.TagNames != null && request.TagNames.Any())
                {
                    baseQuery = baseQuery.Where(p =>
                        p.PhotoPhotoTags.Any(ppt => request.TagNames.Contains(ppt.Tag.TagName)));
                }

                #endregion

                #region 地點篩選

                // 國家
                if (!string.IsNullOrEmpty(request.Country))
                {
                    baseQuery = baseQuery.Where(p =>
                        p.PhotoLocations.Any(l => l.Country == request.Country));
                }

                // 城市
                if (!string.IsNullOrEmpty(request.City))
                {
                    baseQuery = baseQuery.Where(p =>
                        p.PhotoLocations.Any(l => l.City == request.City));
                }

                // 行政區
                if (!string.IsNullOrEmpty(request.District))
                {
                    baseQuery = baseQuery.Where(p =>
                        p.PhotoLocations.Any(l => l.District == request.District));
                }

                // 地點名稱
                if (!string.IsNullOrEmpty(request.PlaceName))
                {
                    baseQuery = baseQuery.Where(p =>
                        p.PhotoLocations.Any(l => l.PlaceName.Contains(request.PlaceName)));
                }

                // 是否有 GPS 資訊
                if (request.HasLocation.HasValue && request.HasLocation.Value)
                {
                    baseQuery = baseQuery.Where(p =>
                        p.PhotoLocations.Any(l => l.Latitude != null && l.Longitude != null));
                }

                #endregion

                #region 相機篩選

                // 相機品牌
                if (!string.IsNullOrEmpty(request.CameraMake))
                {
                    baseQuery = baseQuery.Where(p =>
                        p.PhotoMetadata.Any(m => m.CameraMake == request.CameraMake));
                }

                // 相機型號
                if (!string.IsNullOrEmpty(request.CameraModel))
                {
                    baseQuery = baseQuery.Where(p =>
                        p.PhotoMetadata.Any(m => m.CameraModel == request.CameraModel));
                }

                // 鏡頭型號
                if (!string.IsNullOrEmpty(request.LensModel))
                {
                    baseQuery = baseQuery.Where(p =>
                        p.PhotoMetadata.Any(m => m.LensModel == request.LensModel));
                }

                #endregion

                #region 拍攝參數篩選

                // ISO 範圍
                if (request.MinISO.HasValue)
                {
                    baseQuery = baseQuery.Where(p =>
                        p.PhotoMetadata.Any(m => m.Iso >= request.MinISO));
                }

                if (request.MaxISO.HasValue)
                {
                    baseQuery = baseQuery.Where(p =>
                        p.PhotoMetadata.Any(m => m.Iso <= request.MaxISO));
                }

                // 光圈範圍
                if (request.MinAperture.HasValue)
                {
                    baseQuery = baseQuery.Where(p =>
                        p.PhotoMetadata.Any(m => m.Aperture >= request.MinAperture));
                }

                if (request.MaxAperture.HasValue)
                {
                    baseQuery = baseQuery.Where(p =>
                        p.PhotoMetadata.Any(m => m.Aperture <= request.MaxAperture));
                }

                // 焦距範圍
                if (request.MinFocalLength.HasValue)
                {
                    baseQuery = baseQuery.Where(p =>
                        p.PhotoMetadata.Any(m => m.FocalLength >= request.MinFocalLength));
                }

                if (request.MaxFocalLength.HasValue)
                {
                    baseQuery = baseQuery.Where(p =>
                        p.PhotoMetadata.Any(m => m.FocalLength <= request.MaxFocalLength));
                }

                #endregion

                #region 檔案屬性篩選

                // 檔案名稱關鍵字（模糊搜尋）
                if (!string.IsNullOrEmpty(request.FileNameKeyword))
                {
                    baseQuery = baseQuery.Where(p => p.FileName.Contains(request.FileNameKeyword));
                }

                // 檔案大小範圍
                if (request.MinFileSize.HasValue)
                {
                    baseQuery = baseQuery.Where(p => p.FileSize >= request.MinFileSize);
                }

                if (request.MaxFileSize.HasValue)
                {
                    baseQuery = baseQuery.Where(p => p.FileSize <= request.MaxFileSize);
                }

                // 檔案副檔名
                if (request.FileExtensions != null && request.FileExtensions.Any())
                {
                    baseQuery = baseQuery.Where(p => request.FileExtensions.Contains(p.FileExtension));
                }

                #endregion

                #region 進階篩選

                // 是否有 EXIF
                if (request.HasExif.HasValue && request.HasExif.Value)
                {
                    baseQuery = baseQuery.Where(p => p.PhotoMetadata.Any());
                }

                // 是否已分類
                if (request.IsCategorized.HasValue && request.IsCategorized.Value)
                {
                    baseQuery = baseQuery.Where(p => p.PhotoPhotoCategories.Any());
                }

                // 是否有標籤
                if (request.HasTags.HasValue && request.HasTags.Value)
                {
                    baseQuery = baseQuery.Where(p => p.PhotoPhotoTags.Any());
                }

                // 上傳時間範圍
                if (request.UploadedAfter.HasValue)
                {
                    baseQuery = baseQuery.Where(p => p.UploadedAt >= request.UploadedAfter);
                }

                if (request.UploadedBefore.HasValue)
                {
                    baseQuery = baseQuery.Where(p => p.UploadedAt <= request.UploadedBefore);
                }

                #endregion

                // ===== 計算總筆數（輕量查詢，不包含投影）=====
                var totalCount = await baseQuery.CountAsync();

                _logger.LogInformation("篩選後共 {TotalCount} 張照片", totalCount);

                // ===== 套用排序 =====
                baseQuery = ApplySorting(baseQuery, request.SortBy, request.SortOrder);

                // ===== 分頁 + 投影（最後才做投影，避免影響 Count）=====
                var skip = (request.PageNumber - 1) * request.PageSize;
                var photos = await baseQuery
                    .Skip(skip)
                    .Take(request.PageSize)
                    .Select(p => new Photo
                    {
                        // 基本資訊
                        PhotoId = p.PhotoId,
                        FileName = p.FileName,
                        FileExtension = p.FileExtension,
                        FileSize = p.FileSize,
                        UploadedAt = p.UploadedAt,
                        // ❌ 不載入 PhotoData (varbinary)

                        // PhotoMetadata - 只載入需要的欄位
                        PhotoMetadata = p.PhotoMetadata
                            .Select(m => new PhotoMetadatum
                            {
                                MetadataId = m.MetadataId,
                                PhotoId = m.PhotoId,
                                DateTaken = m.DateTaken,
                                CameraMake = m.CameraMake,
                                CameraModel = m.CameraModel,
                                LensModel = m.LensModel,
                                Iso = m.Iso,
                                Aperture = m.Aperture,
                                ShutterSpeed = m.ShutterSpeed,
                                FocalLength = m.FocalLength,
                                Width = m.Width,
                                Height = m.Height
                            })
                            .ToList(),

                        // PhotoLocations - 只載入需要的欄位
                        PhotoLocations = p.PhotoLocations
                            .Select(l => new PhotoLocation
                            {
                                LocationId = l.LocationId,
                                PhotoId = l.PhotoId,
                                Country = l.Country,
                                City = l.City,
                                District = l.District,
                                PlaceName = l.PlaceName,
                                Latitude = l.Latitude,
                                Longitude = l.Longitude
                            })
                            .ToList(),

                        // PhotoPhotoTags - 只載入 TagName
                        PhotoPhotoTags = p.PhotoPhotoTags
                            .Select(ppt => new PhotoPhotoTag
                            {
                                PhotoId = ppt.PhotoId,
                                TagId = ppt.TagId,
                                SourceId = ppt.SourceId,
                                Tag = new PhotoTag
                                {
                                    TagId = ppt.Tag.TagId,
                                    TagName = ppt.Tag.TagName
                                }
                            })
                            .ToList(),

                        // PhotoPhotoCategories - 只載入 CategoryName
                        PhotoPhotoCategories = p.PhotoPhotoCategories
                            .Select(ppc => new PhotoPhotoCategory
                            {
                                PhotoId = ppc.PhotoId,
                                CategoryId = ppc.CategoryId,
                                SourceId = ppc.SourceId,
                                Category = new PhotoCategory
                                {
                                    CategoryId = ppc.Category.CategoryId,
                                    CategoryName = ppc.Category.CategoryName
                                }
                            })
                            .ToList()
                    })
                    .ToListAsync();

                _logger.LogInformation("查詢完成，回傳 {PhotoCount} 張照片", photos.Count);

                return (photos, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查詢照片時發生錯誤");
                throw;
            }
        }

        /// <summary>
        /// 套用排序邏輯
        /// </summary>
        private IQueryable<Photo> ApplySorting(
            IQueryable<Photo> query,
            string sortBy,
            string sortOrder)
        {
            var isAscending = sortOrder.Equals("asc", StringComparison.OrdinalIgnoreCase);

            return sortBy.ToLower() switch
            {
                "datetaken" => isAscending
                    ? query.OrderBy(p => p.PhotoMetadata.OrderBy(m => m.DateTaken).FirstOrDefault().DateTaken)
                    : query.OrderByDescending(p => p.PhotoMetadata.OrderByDescending(m => m.DateTaken).FirstOrDefault().DateTaken),

                "uploadedat" => isAscending
                    ? query.OrderBy(p => p.UploadedAt)
                    : query.OrderByDescending(p => p.UploadedAt),

                "filename" => isAscending
                    ? query.OrderBy(p => p.FileName)
                    : query.OrderByDescending(p => p.FileName),

                "filesize" => isAscending
                    ? query.OrderBy(p => p.FileSize)
                    : query.OrderByDescending(p => p.FileSize),

                // 預設：依拍攝時間降冪
                _ => query.OrderByDescending(p => p.PhotoMetadata.OrderByDescending(m => m.DateTaken).FirstOrDefault().DateTaken)
            };
        }

        #endregion

        #region Transaction 相關

        public async Task<Photo> UploadPhotoWithDetailsAsync(
            Photo photo,
            PhotoMetadatum metadata,
            List<string> tagNames,
            string sourceCode,
            PhotoLocation location = null)
        {
            PhotoClassificationSource source = null;
            var tagIds = new List<int>();

            try
            {
                _logger.LogInformation("開始上傳照片，檔案: {FileName}", photo.FileName);

                source = await GetClassificationSourceByCodeAsync(sourceCode);
                if (source == null)
                {
                    throw new Exception($"找不到分類來源: {sourceCode}");
                }

                if (tagNames != null && tagNames.Count > 0)
                {
                    var tagCache = new Dictionary<string, PhotoTag>();

                    foreach (var tagName in tagNames)
                    {
                        PhotoTag tag;
                        string cacheKey = $"{tagName}|{PhotoConstants.TAG_TYPE_SYSTEM}";

                        if (tagCache.ContainsKey(cacheKey))
                        {
                            tag = tagCache[cacheKey];
                        }
                        else
                        {
                            int determinedCategoryId = await _tagCategorizationService.DetermineCategoryIdAsync(
                                tagName,
                                PhotoConstants.TAG_TYPE_SYSTEM
                            );

                            tag = await GetOrCreateTagAsync(
                                tagName: tagName,
                                tagType: PhotoConstants.TAG_TYPE_SYSTEM,
                                categoryId: determinedCategoryId,
                                parentTagId: null,
                                userId: photo.UserId
                            );
                            tagCache[cacheKey] = tag;
                            _context.Entry(tag).State = EntityState.Detached;
                        }

                        tagIds.Add(tag.TagId);
                    }

                    _logger.LogInformation("標籤準備完成，數量: {TagCount}", tagIds.Count);
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // 新增 Photo
                    photo.CreatedAt = DateTime.UtcNow;
                    photo.UpdatedAt = DateTime.UtcNow;
                    photo.UploadedAt = DateTime.UtcNow;
                    photo.IsDeleted = false;

                    await _context.Photos.AddAsync(photo);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Photo 新增成功，PhotoId: {PhotoId}", photo.PhotoId);

                    // 新增 PhotoMetadata
                    metadata.PhotoId = photo.PhotoId;
                    metadata.CreatedAt = DateTime.UtcNow;
                    metadata.UpdatedAt = DateTime.UtcNow;

                    await _context.PhotoMetadata.AddAsync(metadata);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"PhotoMetadata 新增成功");

                    if (tagIds.Count > 0)
                    {
                        await AddPhotoTagsBatchAsync(photo.PhotoId, tagIds, source.SourceId);
                        _logger.LogInformation("標籤關聯新增成功，數量: {TagCount}", tagIds.Count);
                    }

                    // 有 GPS，新增 PhotoLocation
                    if (location != null)
                    {
                        location.PhotoId = photo.PhotoId;
                        location.SourceId = source.SourceId;
                        location.SetBy = photo.UserId;
                        location.CreatedAt = DateTime.UtcNow;
                        location.UpdatedAt = DateTime.UtcNow;

                        await _context.PhotoLocations.AddAsync(location);
                        await _context.SaveChangesAsync();

                        _logger.LogInformation($"PhotoLocation 新增成功");
                    }

                    // Commit Transaction
                    await transaction.CommitAsync();

                    _logger.LogInformation("照片上傳完成，PhotoId: {PhotoId}", photo.PhotoId);

                    return photo;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    _logger.LogError(ex, "Transaction 執行失敗，已回滾");
                    throw;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "上傳照片時發生錯誤");
                throw;
            }


        }

        #endregion

        public async Task<bool> ExistsPhotoByHashAsync(long userId, string hash)
        {
            return await _context.Photos
                .AsNoTracking()
                .AnyAsync(p => p.UserId == userId && p.Hash == hash && p.IsDeleted == false);
        }

        #region 縮圖優化相關

        /// <summary>
        /// 只取得縮圖資料
        /// </summary>
        public async Task<ThumbnailDataDTO> GetThumbnailDataAsync(long photoId)
        {
            try
            {
                _logger.LogInformation("查詢縮圖資料，PhotoId: {PhotoId}", photoId);

                var result = await _context.Photos
                    .AsNoTracking()
                    .Where(p => p.PhotoId == photoId && p.IsDeleted == false)
                    .Select(p => new ThumbnailDataDTO
                    {
                        PhotoId = p.PhotoId,
                        UserId = p.UserId,
                        ThumbnailData = p.ThumbnailData,  // 只載入縮圖
                        FileExtension = p.FileExtension,
                        FileName = p.FileName
                    })
                    .FirstOrDefaultAsync();

                if (result != null)
                {
                    _logger.LogDebug("縮圖資料查詢成功，PhotoId: {PhotoId}, 有縮圖: {HasThumbnail}",
                        photoId, result.ThumbnailData != null && result.ThumbnailData.Length > 0);
                }
                else
                {
                    _logger.LogWarning("照片不存在，PhotoId: {PhotoId}", photoId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查詢縮圖資料時發生錯誤，PhotoId: {PhotoId}", photoId);
                throw;
            }
        }

        /// <summary>
        /// 只取得照片原圖資料
        /// </summary>
        public async Task<PhotoDataDTO> GetPhotoDataAsync(long photoId)
        {
            try
            {
                _logger.LogInformation("查詢原圖資料（用於生成縮圖），PhotoId: {PhotoId}", photoId);

                var result = await _context.Photos
                    .AsNoTracking()
                    .Where(p => p.PhotoId == photoId && p.IsDeleted == false)
                    .Select(p => new PhotoDataDTO
                    {
                        PhotoId = p.PhotoId,
                        UserId = p.UserId,
                        PhotoData = p.PhotoData,  // 載入原圖
                        FileExtension = p.FileExtension
                    })
                    .FirstOrDefaultAsync();

                if (result != null)
                {
                    _logger.LogDebug("原圖資料查詢成功，PhotoId: {PhotoId}, 大小: {Size} bytes",
                        photoId, result.PhotoData?.Length ?? 0);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查詢原圖資料時發生錯誤，PhotoId: {PhotoId}", photoId);
                throw;
            }
        }

        /// <summary>
        /// 更新縮圖資料
        /// </summary>
        public async Task<bool> UpdateThumbnailAsync(long photoId, byte[] thumbnailData)
        {
            try
            {
                _logger.LogInformation("更新縮圖資料，PhotoId: {PhotoId}, 大小: {Size} bytes",
                    photoId, thumbnailData?.Length ?? 0);

                var photo = await _context.Photos
                    .Where(p => p.PhotoId == photoId && p.IsDeleted == false)
                    .FirstOrDefaultAsync();

                if (photo == null)
                {
                    _logger.LogWarning("照片不存在，無法更新縮圖，PhotoId: {PhotoId}", photoId);
                    return false;
                }

                photo.ThumbnailData = thumbnailData;
                photo.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("縮圖更新成功，PhotoId: {PhotoId}", photoId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新縮圖時發生錯誤，PhotoId: {PhotoId}", photoId);
                throw;
            }
        }

        /// <summary>
        /// 取得標籤階層（用於 Sidebar 顯示）
        /// 包含分類 + 標籤的完整階層結構，並計算每個標籤的照片數量
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>分類與標籤列表</returns>
        public async Task<List<CategoryWithTagsDTO>> GetTagHierarchyAsync(long userId, string? aiSource = null)
        {
            try
            {
                _logger.LogInformation("開始取得標籤階層，UserId: {UserId}", userId);

                // 1️⃣ 取得所有啟用的分類（按照 DisplayOrder 排序）
                var categories = await _context.PhotoCategories
                    .AsNoTracking()
                    .Where(c => c.IsActive && c.UserId == null) // 只取系統分類
                    .OrderBy(c => c.DisplayOrder)
                    .Select(c => new CategoryWithTagsDTO
                    {
                        CategoryId = c.CategoryId,
                        CategoryName = c.CategoryName,
                        CategoryCode = c.CategoryCode,
                        Icon = GetCategoryIcon(c.CategoryCode), // 根據 CategoryCode 設定圖示
                        DisplayOrder = c.DisplayOrder,
                        IsDefaultExpanded = c.IsDefaultExpanded,
                        UiType = c.UiType,
                        IsComingSoon = c.IsComingSoon,
                        Tags = new List<TagTreeNodeDTO>()
                    })
                    .ToListAsync();

                _logger.LogInformation("✅ 取得 {Count} 個分類", categories.Count);

                // 2️⃣ 取得每個標籤的照片數量
                var tagPhotoCounts = await GetTagPhotoCountsAsync(userId);
                var aiTagCounts = await GetAiTagPhotoCountsAsync(userId, aiSource);
                var aiTagIdSet = aiTagCounts.Keys.ToHashSet();

                // 3️⃣ 針對每個分類，取得其標籤並建立階層結構
                foreach (var category in categories)
                {
                    var isAiCategory = category.CategoryCode == "AI";

                    var tagsQuery = _context.PhotoTags
                        .AsNoTracking()
                        .Where(t => t.CategoryId == category.CategoryId && t.IsActive);

                    if (isAiCategory && !string.IsNullOrWhiteSpace(aiSource) && !aiSource.Equals("All", StringComparison.OrdinalIgnoreCase))
                    {
                        tagsQuery = tagsQuery.Where(t => aiTagIdSet.Contains(t.TagId));
                    }

                    // 取得該分類下的所有標籤
                    var tags = await tagsQuery
                        .OrderBy(t => t.DisplayOrder)
                        .ThenBy(t => t.TagName)
                        .Select(t => new TagTreeNodeDTO
                        {
                            TagId = t.TagId,
                            TagName = t.TagName,
                            TagType = t.TagType,
                            CategoryId = t.CategoryId,
                            ParentTagId = t.ParentTagId,

                            // ✅ AI 分類用 aiTagCounts，其它分類用原本 tagPhotoCounts
                            PhotoCount = isAiCategory
                                ? (aiTagCounts.ContainsKey(t.TagId) ? aiTagCounts[t.TagId] : 0)
                                : (tagPhotoCounts.ContainsKey(t.TagId) ? tagPhotoCounts[t.TagId] : 0),

                            DisplayOrder = t.DisplayOrder,
                            Children = new List<TagTreeNodeDTO>()
                        })
                        .ToListAsync();

                    category.Tags = BuildTagHierarchy(tags);
                }

                _logger.LogInformation("✅ 標籤階層建立完成");
                return categories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 取得標籤階層時發生錯誤");
                throw;
            }
        }

        private async Task<Dictionary<int, int>> GetAiTagPhotoCountsAsync(long userId, string? aiSource)
        {
            var source = string.IsNullOrWhiteSpace(aiSource) || aiSource.Equals("All", StringComparison.OrdinalIgnoreCase)
                ? null
                : aiSource.Trim();

            var q = _context.PhotoAiclassificationSuggestions
                .AsNoTracking()
                .Where(s => s.Photo.UserId == userId && s.Photo.IsDeleted == false)
                .Where(s => s.TagId != null); // ✅ 避免 int?

            if (source != null)
                q = q.Where(s => s.Source == source);

            var tagCounts = await q
                .GroupBy(s => s.TagId!.Value) // ✅ 用 int 當 key
                .Select(g => new
                {
                    TagId = g.Key,
                    Count = g.Select(x => x.PhotoId).Distinct().Count()
                })
                .ToDictionaryAsync(x => x.TagId, x => x.Count);

            return tagCounts;
        }




        /// <summary>
        /// 取得每個標籤的照片數量
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>標籤ID -> 照片數量的對應表</returns>
        public async Task<Dictionary<int, int>> GetTagPhotoCountsAsync(long userId)
        {
            try
            {
                _logger.LogInformation("開始計算標籤照片數量，UserId: {UserId}", userId);

                var tagCounts = await _context.PhotoPhotoTags
                    .AsNoTracking()
                    .Where(ppt => ppt.Photo.UserId == userId && ppt.Photo.IsDeleted == false)
                    .GroupBy(ppt => ppt.TagId)
                    .Select(g => new
                    {
                        TagId = g.Key,
                        Count = g.Count()
                    })
                    .ToDictionaryAsync(x => x.TagId, x => x.Count);

                _logger.LogInformation("✅ 計算完成，共 {Count} 個標籤有照片", tagCounts.Count);
                return tagCounts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 計算標籤照片數量時發生錯誤");
                throw;
            }
        }

        /// <summary>
        /// 建立自訂標籤
        /// </summary>
        /// <param name="tagName">標籤名稱</param>
        /// <param name="categoryId">分類 ID</param>
        /// <param name="parentTagId">父標籤 ID（可選）</param>
        /// <param name="userId">使用者 ID</param>
        /// <returns>新建立的標籤</returns>
        public async Task<PhotoTag> CreateCustomTagAsync(string tagName, int categoryId, int? parentTagId, long userId)
        {
            try
            {
                _logger.LogInformation("建立自訂標籤，TagName: {TagName}, CategoryId: {CategoryId}, UserId: {UserId}",
                    tagName, categoryId, userId);

                // 1️⃣ 檢查標籤是否已存在
                var existingTag = await _context.PhotoTags
                    .Where(t => t.TagName == tagName && t.CategoryId == categoryId)
                    .FirstOrDefaultAsync();

                if (existingTag != null)
                {
                    _logger.LogWarning("⚠️ 標籤已存在，TagId: {TagId}", existingTag.TagId);
                    return existingTag;
                }

                // 2️⃣ 如果有父標籤，驗證父標籤是否存在
                if (parentTagId.HasValue)
                {
                    var parentTag = await _context.PhotoTags
                        .Where(t => t.TagId == parentTagId.Value)
                        .FirstOrDefaultAsync();

                    if (parentTag == null)
                    {
                        _logger.LogError("❌ 父標籤不存在，ParentTagId: {ParentTagId}", parentTagId.Value);
                        throw new ArgumentException($"父標籤不存在: {parentTagId.Value}");
                    }

                    // 確保父標籤屬於同一分類
                    if (parentTag.CategoryId != categoryId)
                    {
                        _logger.LogError("❌ 父標籤與目標分類不一致");
                        throw new ArgumentException("父標籤必須屬於同一分類");
                    }
                }

                // 3️⃣ 建立新標籤
                var newTag = new PhotoTag
                {
                    CategoryId = categoryId,
                    TagName = tagName,
                    TagType = PhotoConstants.TAG_TYPE_CUSTOM, // "CUSTOM"
                    ParentTagId = parentTagId,
                    DisplayOrder = 0, // 自訂標籤預設排序為 0
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _context.PhotoTags.AddAsync(newTag);
                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ 自訂標籤建立成功，TagId: {TagId}", newTag.TagId);
                return newTag;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 建立自訂標籤時發生錯誤");
                throw;
            }
        }

        #endregion

        #region 私有輔助方法

        /// <summary>
        /// 建立標籤階層結構（遞迴）
        /// </summary>
        /// <param name="allTags">所有標籤的平面列表</param>
        /// <returns>階層結構的標籤列表（只包含根節點）</returns>
        private List<TagTreeNodeDTO> BuildTagHierarchy(List<TagTreeNodeDTO> allTags)
        {
            // 建立標籤字典（用於快速查找）
            var tagDict = allTags.ToDictionary(t => t.TagId);

            // 找出所有根節點（沒有父標籤的標籤）
            var rootTags = allTags.Where(t => t.ParentTagId == null).ToList();

            // 遞迴建立每個根節點的子樹
            foreach (var rootTag in rootTags)
            {
                BuildChildrenRecursive(rootTag, tagDict);
            }

            return rootTags;
        }

        /// <summary>
        /// 遞迴建立子標籤結構
        /// </summary>
        /// <param name="parentTag">父標籤</param>
        /// <param name="tagDict">所有標籤的字典</param>
        private void BuildChildrenRecursive(TagTreeNodeDTO parentTag, Dictionary<int, TagTreeNodeDTO> tagDict)
        {
            // 找出所有子標籤
            var children = tagDict.Values
                .Where(t => t.ParentTagId == parentTag.TagId)
                .OrderBy(t => t.DisplayOrder)
                .ThenBy(t => t.TagName)
                .ToList();

            parentTag.Children = children;

            // 遞迴處理每個子標籤
            foreach (var child in children)
            {
                BuildChildrenRecursive(child, tagDict);
            }
        }

        /// <summary>
        /// 根據分類代碼取得對應的圖示名稱
        /// </summary>
        /// <param name="categoryCode">分類代碼</param>
        /// <returns>圖示名稱（Tabler Icons）</returns>
        private static string GetCategoryIcon(string categoryCode)
        {
            return categoryCode switch
            {
                PhotoConstants.CATEGORY_TIME => "calendar",            // 📅 時間
                PhotoConstants.CATEGORY_CAMERA => "camera",            // 📷 相機
                PhotoConstants.CATEGORY_LOCATION => "map-pin",         // 📍 地點
                PhotoConstants.CATEGORY_SCENE => "photo",              // 🎬 場景
                PhotoConstants.TAG_TYPE_CUSTOM => "tag",               // 🏷️ 自訂
                PhotoConstants.CATEGORY_GENERAL => "folder",           // 📁 一般
                _ => "tag"
            };
        }


        public async Task<PhotoTag> GetTagByIdAsync(int tagId)
        {
            try
            {
                _logger.LogInformation("查詢標籤，TagId: {TagId}", tagId);

                return await _context.PhotoTags
                    .AsNoTracking()
                    .Where(t => t.TagId == tagId && t.IsActive)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查詢標籤時發生錯誤，TagId: {TagId}", tagId);
                throw;
            }
        }

        #endregion

        #region PhotoStorage 表操作

        /// <summary>
        /// 新增照片儲存記錄
        /// 記錄照片在 Blob Storage 的儲存位置
        /// </summary>
        public async Task<PhotoStorage> AddPhotoStorageAsync(PhotoStorage storage)
        {
            try
            {
                _logger.LogInformation(
                    "💾 開始新增儲存記錄，PhotoId: {PhotoId}, ProviderId: {ProviderId}",
                    storage.PhotoId, storage.ProviderId);

                storage.CreatedAt = DateTime.UtcNow;
                storage.UpdatedAt = DateTime.UtcNow;

                await _context.PhotoStorages.AddAsync(storage);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "✅ 儲存記錄新增成功，StorageId: {StorageId}, 路徑: {StoragePath}",
                    storage.StorageId, storage.StoragePath);

                return storage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ 新增儲存記錄失敗，PhotoId: {PhotoId}",
                    storage.PhotoId);
                throw;
            }
        }

        /// <summary>
        /// 根據 PhotoId 查詢主要儲存位置
        /// </summary>
        public async Task<PhotoStorage> GetPrimaryStorageByPhotoIdAsync(long photoId)
        {
            try
            {
                _logger.LogDebug("🔍 查詢主要儲存位置，PhotoId: {PhotoId}", photoId);

                var storage = await _context.PhotoStorages
                    .AsNoTracking()
                    .Where(s => s.PhotoId == photoId && s.IsPrimary)
                    .FirstOrDefaultAsync();

                if (storage == null)
                {
                    _logger.LogWarning("⚠️ 找不到主要儲存位置，PhotoId: {PhotoId}", photoId);
                }
                else
                {
                    _logger.LogDebug(
                        "✅ 找到主要儲存位置，StorageId: {StorageId}, 路徑: {StoragePath}",
                        storage.StorageId, storage.StoragePath);
                }

                return storage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 查詢主要儲存位置失敗，PhotoId: {PhotoId}", photoId);
                throw;
            }
        }

        /// <summary>
        /// 根據 PhotoId 查詢所有儲存位置
        /// 支援多個儲存位置（例如：主要儲存 + 備份）
        /// </summary>
        public async Task<List<PhotoStorage>> GetAllStoragesByPhotoIdAsync(long photoId)
        {
            try
            {
                _logger.LogDebug("🔍 查詢所有儲存位置，PhotoId: {PhotoId}", photoId);

                var storages = await _context.PhotoStorages
                    .AsNoTracking()
                    .Where(s => s.PhotoId == photoId)
                    .OrderByDescending(s => s.IsPrimary) // 主要儲存排在最前面
                    .ThenBy(s => s.CreatedAt)
                    .ToListAsync();

                _logger.LogDebug(
                    "✅ 找到 {Count} 個儲存位置，PhotoId: {PhotoId}",
                    storages.Count, photoId);

                return storages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 查詢所有儲存位置失敗，PhotoId: {PhotoId}", photoId);
                throw;
            }
        }

        /// <summary>
        /// 更新照片儲存記錄
        /// 例如：變更存取層級、更新 AccessURL
        /// </summary>
        public async Task<bool> UpdatePhotoStorageAsync(PhotoStorage storage)
        {
            try
            {
                _logger.LogInformation(
                    "🔄 開始更新儲存記錄，StorageId: {StorageId}",
                    storage.StorageId);

                storage.UpdatedAt = DateTime.UtcNow;

                _context.PhotoStorages.Update(storage);
                var rowsAffected = await _context.SaveChangesAsync();

                if (rowsAffected > 0)
                {
                    _logger.LogInformation(
                        "✅ 儲存記錄更新成功，StorageId: {StorageId}",
                        storage.StorageId);
                    return true;
                }
                else
                {
                    _logger.LogWarning(
                        "⚠️ 儲存記錄更新失敗（無資料變更），StorageId: {StorageId}",
                        storage.StorageId);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ 更新儲存記錄失敗，StorageId: {StorageId}",
                    storage.StorageId);
                throw;
            }
        }

        /// <summary>
        /// 刪除照片儲存記錄
        /// 實體刪除，用於清理儲存記錄
        /// </summary>
        public async Task<bool> DeletePhotoStorageAsync(long storageId)
        {
            try
            {
                _logger.LogInformation("🗑️ 開始刪除儲存記錄，StorageId: {StorageId}", storageId);

                var storage = await _context.PhotoStorages
                    .Where(s => s.StorageId == storageId)
                    .FirstOrDefaultAsync();

                if (storage == null)
                {
                    _logger.LogWarning("⚠️ 儲存記錄不存在，StorageId: {StorageId}", storageId);
                    return false;
                }

                _context.PhotoStorages.Remove(storage);
                var rowsAffected = await _context.SaveChangesAsync();

                if (rowsAffected > 0)
                {
                    _logger.LogInformation(
                        "✅ 儲存記錄刪除成功，StorageId: {StorageId}, 路徑: {StoragePath}",
                        storageId, storage.StoragePath);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 刪除儲存記錄失敗，StorageId: {StorageId}", storageId);
                throw;
            }
        }

        /// <summary>
        /// 根據 StorageId 查詢儲存記錄
        /// </summary>
        public async Task<PhotoStorage> GetStorageByIdAsync(long storageId)
        {
            try
            {
                _logger.LogDebug("🔍 查詢儲存記錄，StorageId: {StorageId}", storageId);

                var storage = await _context.PhotoStorages
                    .AsNoTracking()
                    .Where(s => s.StorageId == storageId)
                    .FirstOrDefaultAsync();

                if (storage == null)
                {
                    _logger.LogWarning("⚠️ 儲存記錄不存在，StorageId: {StorageId}", storageId);
                }
                else
                {
                    _logger.LogDebug(
                        "✅ 找到儲存記錄，PhotoId: {PhotoId}, 路徑: {StoragePath}",
                        storage.PhotoId, storage.StoragePath);
                }

                return storage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 查詢儲存記錄失敗，StorageId: {StorageId}", storageId);
                throw;
            }
        }

        /// <summary>
        /// 檢查照片是否有儲存記錄
        /// </summary>
        public async Task<bool> HasStorageRecordAsync(long photoId)
        {
            try
            {
                _logger.LogDebug("🔍 檢查儲存記錄是否存在，PhotoId: {PhotoId}", photoId);

                var exists = await _context.PhotoStorages
                    .AsNoTracking()
                    .AnyAsync(s => s.PhotoId == photoId);

                _logger.LogDebug(
                    "✅ 儲存記錄檢查完成，PhotoId: {PhotoId}, 存在: {Exists}",
                    photoId, exists);

                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 檢查儲存記錄時發生錯誤，PhotoId: {PhotoId}", photoId);
                throw;
            }
        }

        #endregion
    }
}