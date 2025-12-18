using FrameZone_WebApi.DTOs;
using FrameZone_WebApi.Models;
using FrameZone_WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace FrameZone_WebApi.Services
{
    /// <summary>
    /// 照片服務實作
    /// 負責處理照片上傳、EXIF 解析、自動分類、標籤生成等業務邏輯
    /// </summary>
    public class PhotoService : IPhotoService
    {
        #region 依賴注入

        private readonly IExifService _exifService;
        private readonly IPhotoRepository _photoRepository;
        private readonly ILogger<PhotoService> _logger;

        /// <summary>
        /// 建構子 - 依賴注入
        /// </summary>
        public PhotoService(
            IExifService exifService,
            IPhotoRepository photoRepository,
            ILogger<PhotoService> logger)
        {
            _exifService = exifService;
            _photoRepository = photoRepository;
            _logger = logger;
        }

        #endregion

        #region 常數定義

        /// <summary>
        /// 允許的檔案副檔名
        /// </summary>
        private static readonly string[] AllowedExtensions = new[]
        {
            ".jpg", ".jpeg", ".png", ".heic", ".gif", ".bmp"
        };

        /// <summary>
        /// 檔案大小限制 (50MB)
        /// </summary>
        private const long MaxFileSize = 50 * 1024 * 1024; // 50MB in bytes

        #endregion

        #region 公開方法

        /// <summary>
        /// 上傳單張照片
        /// </summary>
        /// <param name="file">上傳的檔案</param>
        /// <param name="userId">使用者 ID</param>
        /// <returns>上傳結果</returns>
        public async Task<PhotoUploadResponseDTO> UploadPhotoAsync(IFormFile file, long userId)
        {
            try
            {
                _logger.LogInformation($"開始處理照片上傳，使用者ID: {userId}, 檔案名稱: {file?.FileName}");

                // 驗證檔案
                var validationError = ValidateFile(file);
                if (validationError != null)
                {
                    _logger.LogWarning($"檔案驗證失敗: {validationError}");
                    return new PhotoUploadResponseDTO
                    {
                        Success = false,
                        Message = validationError
                    };
                }

                // 讀取檔案內容到記憶體
                // TODO: 之後改成雲端儲存，上傳到 Blob Storage
                byte[] fileBytes;
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    fileBytes = memoryStream.ToArray();
                }

                _logger.LogInformation($"檔案讀取完成，大小: {fileBytes.Length} bytes");


                // 提取 EXIF 元數據
                PhotoMetadataDtos metadata;
                using (var stream = new MemoryStream(fileBytes))
                {
                    metadata = _exifService.ExtractMetadata(stream, file.FileName);
                }

                _logger.LogInformation($"EXIF 解析完成，自動標籤數量: {metadata.AutoTags.Count}");


                // 計算檔案 Hash
                string fileHash;
                using (var stream = new MemoryStream(fileBytes))
                {
                    fileHash = _exifService.CalculateFileHash(stream);
                }

                _logger.LogInformation($"檔案 Hash: {fileHash}");

                // ⭐ 檢查是否重複上傳（加強 log）
                _logger.LogInformation($"開始檢查重複照片，UserId: {userId}, Hash: {fileHash}");

                Photo existingPhoto = null;
                try
                {
                    var exists = await _photoRepository.ExistsPhotoByHashAsync(userId, fileHash);
                    if (exists)
                    {
                        return new PhotoUploadResponseDTO { Success = false, Message = "此照片已經上傳過了" };
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ GetPhotoByHashAsync 執行時發生錯誤");
                    // 如果檢查失敗，為了安全起見，繼續上傳
                }

                if (existingPhoto != null)
                {
                    _logger.LogWarning($"⚠️ 偵測到重複照片，已存在的照片ID: {existingPhoto.PhotoId}，檔案名稱: {file.FileName}");
                    return new PhotoUploadResponseDTO
                    {
                        Success = false,
                        Message = "此照片已經上傳過了"
                    };
                }

                _logger.LogInformation("✅ 無重複照片，繼續處理上傳...");

                // 建立 Photo Model
                var photo = new Photo
                {
                    UserId = userId,
                    FileName = metadata.FileName,
                    FileExtension = metadata.FileExtension,
                    FileSize = metadata.FileSize,
                    PhotoData = fileBytes, // TODO: 之後替換成雲端儲存 URL
                    Hash = fileHash
                };

                var photoMetadata = new PhotoMetadatum
                {
                    CameraMake = metadata.CameraMake,
                    CameraModel = metadata.CameraModel,
                    DateTaken = metadata.DateTaken,
                    Gpslatitude = metadata.GPSLatitude,
                    Gpslongitude = metadata.GPSLongitude,
                    FocalLength = metadata.FocalLength,
                    Aperture = metadata.Aperture,
                    ShutterSpeed = metadata.ShutterSpeed,
                    Iso = metadata.ISO,
                    ExposureMode = metadata.ExposureMode,
                    WhiteBalance = metadata.WhiteBalance,
                    LensModel = metadata.LensModel,
                    Orientation = metadata.Orientation,
                    Width = metadata.Width,
                    Height = metadata.Height,
                };

                PhotoLocation location = null;
                if (metadata.GPSLatitude.HasValue && metadata.GPSLongitude.HasValue)
                {
                    location = new PhotoLocation
                    {
                        Latitude = metadata.GPSLatitude.Value,
                        Longitude = metadata.GPSLongitude.Value,
                        // TODO: 反向地理編碼
                    };
                }

                _logger.LogInformation("開始儲存照片到資料庫...");

                var uploadedPhoto = await _photoRepository.UploadPhotoWithDetailsAsync(
                    photo, photoMetadata, metadata.AutoTags, "EXIF", location);

                _logger.LogInformation($"✅ 照片上傳成功，PhotoId: {uploadedPhoto.PhotoId}");


                // 返回結果
                return new PhotoUploadResponseDTO
                {
                    Success = true,
                    Message = "照片上傳成功",
                    Data = new PhotoUploadDataDTO
                    {
                        PhotoId = uploadedPhoto.PhotoId,
                        FileName = metadata.FileName,
                        FileSize = metadata.FileSize,
                        Metadata = metadata,
                        AutoTags = metadata.AutoTags,
                        BlobUrl = null // TODO: 之後替換成雲端儲存 URL
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ 上傳照片時發生錯誤，檔案: {file?.FileName}");
                return new PhotoUploadResponseDTO
                {
                    Success = false,
                    Message = $"上傳失敗: {ex.Message}"
                };
            }
        }

        public async Task<BatchUploadResponseDTO> UploadPhotoBatchAsync(List<IFormFile> files, long userId)
        {
            try
            {
                _logger.LogInformation($"開始批次上傳，共 {files.Count} 個檔案");

                var results = new List<BatchUploadResultDTO>();
                int successCount = 0;
                int failedCount = 0;

                // 逐一處理每張照片
                foreach(var file in files)
                {
                    try
                    {
                        // 呼叫單張上傳方法
                        var uploadResult = await UploadPhotoAsync(file, userId);

                        if (uploadResult.Success)
                        {
                            successCount++;
                            results.Add(new BatchUploadResultDTO
                            {
                                FileName = file.FileName,
                                Success = true,
                                PhotoId = uploadResult.Data?.PhotoId
                            });
                        }
                        else
                        {
                            failedCount++;
                            results.Add(new BatchUploadResultDTO
                            {
                                FileName = file.FileName,
                                Success = false,
                                Error = uploadResult.Message
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"處理檔案 {file.FileName} 時發生錯誤");
                        failedCount++;
                        results.Add(new BatchUploadResultDTO
                        {
                            FileName = file.FileName,
                            Success = false,
                            Error = ex.Message
                        });
                    }
                }
                _logger.LogInformation($"批次上傳完成，成功: {successCount}, 失敗: {failedCount}");

                return new BatchUploadResponseDTO
                {
                    Success = successCount > 0,
                    TotalFiles = files.Count,
                    SuccessCount = successCount,
                    FailedCount = failedCount,
                    Results = results
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批次上傳時發生錯誤");
                return new BatchUploadResponseDTO
                {
                    Success = false,
                    TotalFiles = files.Count,
                    SuccessCount = 0,
                    FailedCount = files.Count,
                    Results = new List<BatchUploadResultDTO>()
                };
            }
        }

        public async Task<PhotoMetadataDtos> TestExifAsync(IFormFile file)
        {
            try
            {
                _logger.LogInformation($"測試 EXIF 解析: {file.FileName}");

                // 驗證檔案
                var validationError = ValidateFile(file);
                if (validationError != null)
                {
                    throw new Exception(validationError);
                }

                // 提取 EXIF 資訊
                PhotoMetadataDtos metadata;
                using (var stream = file.OpenReadStream())
                {
                    metadata = _exifService.ExtractMetadata(stream, file.FileName);
                }

                _logger.LogInformation("EXIF 測試完成");
                return metadata;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "測試 EXIF 時發生錯誤");
                throw;
            }
        }


        /// <summary>
        /// 取得照片詳細資訊
        /// </summary>
        public async Task<PhotoDetailDTO> GetPhotoByIdAsync(long photoId, long userId)
        {
            try
            {
                _logger.LogInformation($"查詢照片，PhotoId: {photoId}, UserId: {userId}");


                throw new NotImplementedException("等 Repository 實做");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查詢照片時發生錯誤");
                throw;
            }
        }
        
        /// <summary>
        /// 刪除照片 (軟刪除)
        /// </summary>
        /// TODO: 如果使用雲端儲存，也要刪除 Blob
        public async Task<bool> DeletePhotoAsync(long photoId, long userId)
        {
            try
            {
                _logger.LogInformation($"刪除照片，PhotoId: {photoId}, UserId: {userId}");

                throw new NotImplementedException("等 Repository 完成後實作");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除照片時發生錯誤");
                throw;
            }
        }

        #endregion

        #region 私有輔助方法

        /// <summary>
        /// 驗證檔案
        /// </summary>
        /// <param name="file">要驗證的檔案</param>
        /// <returns>錯誤訊息，null 表示驗證通過</returns>
        private string ValidateFile(IFormFile file)
        {
            // 檢查檔案是否為空
            if (file == null || file.Length == 0)
            {
                return "請選擇要上傳的檔案";
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            // 檢查檔案格式
            if (!AllowedExtensions.Contains(fileExtension))
            {
                return $"不支援的檔案格式，僅支援: {string.Join(", ", AllowedExtensions)}";
            }

            // 檢查檔案大小
            if (file.Length > MaxFileSize)
            {
                return $"檔案大小不能超過 {MaxFileSize / (1024 * 1024)} MB";
            }

            // 驗證通過
            return null;
        }

        #endregion
    }
}
