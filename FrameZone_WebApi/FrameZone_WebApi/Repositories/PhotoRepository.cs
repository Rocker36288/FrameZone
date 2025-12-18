using FrameZone_WebApi.DTOs;
using FrameZone_WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FrameZone_WebApi.Repositories
{
    public class PhotoRepository : IPhotoRepository
    {
        #region 依賴注入

        private readonly AAContext _context;
        private readonly ILogger<PhotoRepository> _logger;

        public PhotoRepository(
            AAContext context, 
            ILogger<PhotoRepository> logger)
        {
            _context = context;
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

                var photo = await GetPhotoByIdAsync(photoId);
                if (photo == null)
                {
                    _logger.LogWarning($"照片不存在，PhotoId: {photoId}");
                    return false;
                }

                photo.IsDeleted = true;
                photo.DeletedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
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
                    .Where(p => p.UserId == userId && p.IsDeleted == false)
                    .OrderByDescending(p => p.UpdatedAt)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"查詢使用者照片時發生錯誤，UserId: {userId}");
                throw;
            }
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
            catch(Exception ex)
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
        public async Task<PhotoTag> GetOrCreateTagAsync(string tagName, string tagType, long? userId = null)
        {
            try
            {
                var existingTag = await GetTagByNameAsync(tagName, tagType);
                if (existingTag != null)
                {
                    return existingTag;
                }

                var newTag = new PhotoTag
                {
                    TagName = tagName,
                    TagType = tagType,
                    UserId = userId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                return await CreateTagAsync(newTag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"取得或建立標籤時發生錯誤，TagName: {tagName}, TagType: {tagType}");
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
                    .Where(pt => pt.Source.SourceCode == "EXIF")
                    .Select(pt => pt.Tag.TagName)
                    .ToList();

                var manualTags = photo.PhotoPhotoTags
                    .Where(pt => pt.Source.SourceCode == "MANUAL")
                    .Select(pt => pt.Tag.TagName)
                    .ToList();

                var aiTags = photo.PhotoPhotoTags
                    .Where(pt => pt.Source.SourceCode == "AI")
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
                    Metadata = metadata != null ? new PhotoMetadataDtos
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
            catch(Exception ex)
            {
                _logger.LogError(ex, $"根據標籤查詢照片時發生錯誤，TagId: {tagId}");
                throw;
            }
        }

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
                _logger.LogInformation($"開始上傳照片，檔案: {photo.FileName}");

                source = await GetClassificationSourceByCodeAsync(sourceCode);
                if (source == null)
                {
                    throw new Exception($"找不到分類來源: {sourceCode}");
                }

                if (tagNames != null && tagNames.Count > 0)
                {
                    var tagCache = new Dictionary<string, PhotoTag>();

                    foreach ( var tagName in tagNames )
                    {
                        PhotoTag tag;
                        string cacheKey = $"{tagName}|SYSTEM";

                        if (tagCache.ContainsKey(cacheKey))
                        {
                            tag = tagCache[cacheKey];
                        }
                        else
                        {
                            tag = await GetOrCreateTagAsync(tagName, "SYSTEM", photo.UserId);
                            tagCache[cacheKey] = tag;
                            _context.Entry(tag).State = EntityState.Detached;
                        }

                        tagIds.Add(tag.TagId);
                    }

                    _logger.LogInformation($"標籤準備完成，數量: {tagIds.Count}");
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

                    _logger.LogInformation($"Photo 新增成功，PhotoId: {photo.PhotoId}");

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
                        _logger.LogInformation($"標籤關聯新增成功，數量: {tagIds.Count}");
                    }

                    // 有 GPS，新增 PhotoLocation
                    if (location != null)
                    {
                        location.PhotoId = photo.PhotoId;
                        location.CreatedAt = DateTime.UtcNow;
                        location.UpdatedAt = DateTime.UtcNow;

                        await _context.PhotoLocations.AddAsync(location);
                        await _context.SaveChangesAsync();

                        _logger.LogInformation($"PhotoLocation 新增成功");
                    }

                    // Commit Transaction
                    await transaction.CommitAsync();

                    _logger.LogInformation($"照片上傳完成，PhotoId: {photo.PhotoId}");

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

    }
}
