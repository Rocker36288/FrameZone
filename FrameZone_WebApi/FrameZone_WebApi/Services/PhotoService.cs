using FrameZone_WebApi.DTOs;
using FrameZone_WebApi.Helpers;
using FrameZone_WebApi.Models;
using FrameZone_WebApi.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

using Openize.Heic.Decoder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using System.Diagnostics;


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
        private readonly IGeocodingService _geocodingService;
        private readonly IPhotoRepository _photoRepository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<PhotoService> _logger;
        private readonly IBackgroundGeocodingService _backgroundGeocodingService;
        private readonly IBlobStorageService _blobStorageService;


        public PhotoService(
            IExifService exifService,
            IGeocodingService geocodingService,
            IPhotoRepository photoRepository,
            IMemoryCache cache,
            ILogger<PhotoService> logger,
            IBackgroundGeocodingService backgroundGeocodingService,
            IBlobStorageService blobStorageService)
        {
            _exifService = exifService;
            _geocodingService = geocodingService;
            _photoRepository = photoRepository;
            _cache = cache;
            _logger = logger;
            _backgroundGeocodingService = backgroundGeocodingService;
            _blobStorageService = blobStorageService;
        }

        #endregion

        #region 公開方法

        public async Task<PhotoQueryResponseDTO> QueryPhotosAsync(PhotoQueryRequestDTO request, long userId)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("開始查詢照片，使用者ID: {UserId}, 頁碼: {PageNumber}, 每頁: {PageSize}", userId, request.PageNumber, request.PageSize);

                // 驗證參數
                var validationError = ValidateQueryRequest(request);
                if (validationError != null)
                {
                    _logger.LogWarning("查詢請求驗證失敗: {ValidationError}", validationError);
                    return new PhotoQueryResponseDTO
                    {
                        Success = false,
                        Message = validationError
                    };
                }

                // 呼叫 Repository 執行查詢
                var (photos, totalCount) = await _photoRepository.QueryPhotosAsync(request, userId);

                _logger.LogInformation("查詢完成，總照片數: {TotalCount}", totalCount);

                // 轉換為 DTO
                var photoItems = photos.Select(p => MapToPhotoItemDTO(p)).ToList();

                // 計算分頁資訊
                var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

                stopwatch.Stop();

                return new PhotoQueryResponseDTO
                {
                    Success = true,
                    Photos = photoItems,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalPages = totalPages,
                    HasPreviousPage = request.PageNumber > 1,
                    HasNextPage = request.PageNumber < totalPages,
                    ExecutionTimeMs = stopwatch.ElapsedMilliseconds
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "查詢照片時發生錯誤");

                return new PhotoQueryResponseDTO
                {
                    Success = false,
                    Message = $"查詢失敗: {ex.Message}",
                    ExecutionTimeMs = stopwatch.ElapsedMilliseconds
                };
            }
        }

        /// <summary>
        /// 上傳單張照片
        /// </summary>
        public async Task<PhotoUploadResponseDTO> UploadPhotoAsync(IFormFile file, long userId)
        {
            try
            {
                _logger.LogInformation("開始處理照片上傳，使用者ID: {UserId}, 檔案名稱: {FileName}",
                    userId, file?.FileName);

                // 驗證檔案
                var validationError = ValidateFile(file);
                if (validationError != null)
                {
                    _logger.LogWarning("檔案驗證失敗: {ValidationError}", validationError);
                    return new PhotoUploadResponseDTO
                    {
                        Success = false,
                        Message = validationError
                    };
                }

                // 讀取檔案並進行處理
                byte[] fileBytes;
                PhotoMetadataDTO metadata;
                string fileHash;

                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    fileBytes = memoryStream.ToArray();

                    memoryStream.Position = 0;
                    metadata = _exifService.ExtractMetadata(memoryStream, file.FileName);

                    memoryStream.Position = 0;
                    fileHash = _exifService.CalculateFileHash(memoryStream);

                    metadata.Hash = fileHash;
                }

                _logger.LogInformation("檔案處理完成 - 大小: {FileSize} bytes, Hash: {FileHash}",
                    fileBytes.Length, fileHash);

                // 檢查是否重複上傳
                var isDuplicate = await _photoRepository.ExistsPhotoByHashAsync(userId, fileHash);
                if (isDuplicate)
                {
                    _logger.LogWarning("偵測到重複照片，Hash: {FileHash}", fileHash);
                    return new PhotoUploadResponseDTO
                    {
                        Success = false,
                        Message = "此照片已經上傳過了"
                    };
                }

                metadata.AutoTags = GenerateBasicTags(metadata);

                _logger.LogInformation("自動標籤生成完成，數量: {TagCount}", metadata.AutoTags.Count);

                _logger.LogInformation("開始生成縮圖...");

                byte[] thumbnailData = null;
                int retryCount = 0;
                int maxRetries = PhotoConstants.MAX_RETRY_COUNT;

                while (thumbnailData == null && retryCount < maxRetries)
                {
                    try
                    {
                        thumbnailData = await GenerateThumbnailAsync(
                            fileBytes, 
                            PhotoConstants.THUMBNAIL_WIDTH,
                            PhotoConstants.THUMBNAIL_HEIGHT
                        );

                        if (thumbnailData != null && thumbnailData.Length > 0)
                        {
                            _logger.LogInformation("縮圖生成成功，大小: {ThumbnailSize} bytes", thumbnailData.Length);
                        }
                        else
                        {
                            throw new Exception("生成的縮圖為空");
                        }
                    }
                    catch (Exception ex)
                    {
                        retryCount++;
                        _logger.LogError(ex, "縮圖生成失敗（第 {RetryCount}/{MaxRetries} 次），檔案: {FileName}",
                            retryCount, maxRetries, file.FileName);

                        if (retryCount >= maxRetries)
                        {
                            // 所有重試都失敗：使用預設縮圖（1x1 透明圖）
                            _logger.LogWarning("縮圖生成失敗達上限，使用預設縮圖，檔案: {FileName}", file.FileName);
                            thumbnailData = CreateDefaultThumbnail();
                        }
                        else
                        {
                            // 等待後重試
                            await Task.Delay(500 * retryCount);
                        }
                    }
                }

                var photo = new Photo
                {
                    UserId = userId,
                    FileName = Path.GetFileNameWithoutExtension(file.FileName),
                    FileExtension = Path.GetExtension(file.FileName).TrimStart('.').ToLower(),
                    FileSize = file.Length,
                    PhotoData = null,
                    ThumbnailData = null,
                    Hash = fileHash,
                    UploadedAt = DateTime.UtcNow,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
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
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // 儲存到資料庫
                _logger.LogInformation("開始儲存照片到資料庫...");

                var uploadedPhoto = await _photoRepository.UploadPhotoWithDetailsAsync(
                    photo,
                    photoMetadata,
                    metadata.AutoTags,
                    PhotoConstants.SOURCE_EXIF,
                    location: null
                );

                _logger.LogInformation("照片上傳成功，PhotoId: {PhotoId}", uploadedPhoto.PhotoId);

                try
                {
                    _logger.LogInformation("📤 開始上傳到 Blob Storage，PhotoId: {PhotoId}", uploadedPhoto.PhotoId);

                    // 1️⃣ 上傳原圖到 Blob Storage
                    string originalBlobUrl;
                    using (var originalStream = new MemoryStream(fileBytes))
                    {
                        originalBlobUrl = await _blobStorageService.UploadPhotoAsync(
                            originalStream,
                            file.FileName,
                            userId,
                            uploadedPhoto.PhotoId,
                            uploadedPhoto.UploadedAt,
                            $"image/{uploadedPhoto.FileExtension}");
                    }

                    _logger.LogInformation("✅ 原圖上傳成功，URL: {BlobUrl}", originalBlobUrl);

                    // 2️⃣ 上傳縮圖到 Blob Storage
                    string thumbnailBlobUrl;
                    using (var thumbnailStream = new MemoryStream(thumbnailData))
                    {
                        thumbnailBlobUrl = await _blobStorageService.UploadThumbnailAsync(
                            thumbnailStream,
                            file.FileName,
                            userId,
                            uploadedPhoto.PhotoId,
                            uploadedPhoto.UploadedAt,
                            $"image/{uploadedPhoto.FileExtension}");
                    }

                    _logger.LogInformation("✅ 縮圖上傳成功，URL: {ThumbnailUrl}", thumbnailBlobUrl);

                    // 3️⃣ 記錄原圖的 PhotoStorage（主要儲存）
                    var photoStorage = new PhotoStorage
                    {
                        PhotoId = uploadedPhoto.PhotoId,
                        ProviderId = 1, // Azure Blob Storage（需要在 PhotoStorageProvider 表中有這筆資料）
                        StoragePath = _blobStorageService.GeneratePhotoBlobPath(
                            userId,
                            uploadedPhoto.PhotoId,
                            uploadedPhoto.UploadedAt,
                            uploadedPhoto.FileExtension),
                        BucketName = null, // 可選
                        Region = null,     // 可選
                        AccessUrl = originalBlobUrl,
                        IsPrimary = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _photoRepository.AddPhotoStorageAsync(photoStorage);

                    _logger.LogInformation("✅ PhotoStorage 記錄新增成功，StorageId: {StorageId}", photoStorage.StorageId);

                    // 4️⃣ 記錄縮圖的 PhotoStorage（次要儲存）
                    var thumbnailStorage = new PhotoStorage
                    {
                        PhotoId = uploadedPhoto.PhotoId,
                        ProviderId = 1, // Azure Blob Storage
                        StoragePath = _blobStorageService.GenerateThumbnailBlobPath(
                            userId,
                            uploadedPhoto.PhotoId,
                            uploadedPhoto.UploadedAt,
                            uploadedPhoto.FileExtension),
                        BucketName = null,
                        Region = null,
                        AccessUrl = thumbnailBlobUrl,
                        IsPrimary = false, // 縮圖不是主要儲存
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _photoRepository.AddPhotoStorageAsync(thumbnailStorage);

                    _logger.LogInformation("✅ 縮圖 PhotoStorage 記錄新增成功");

                    // 5️⃣ 生成前端可用的 SAS Token URL
                    var photoUrlWithSas = await _blobStorageService.GetPhotoUrlAsync(photoStorage.StoragePath, useSasToken: true);
                    var thumbnailUrlWithSas = await _blobStorageService.GetThumbnailUrlAsync(thumbnailStorage.StoragePath, useSasToken: true);

                    _logger.LogInformation("✅ Blob Storage 上傳完成");

                    // ⚠️ 注意：將 BlobUrl 和 ThumbnailUrl 存到變數中，稍後返回給前端
                    // 暫存這兩個 URL，稍後在返回 DTO 時使用
                    metadata.BlobUrl = photoUrlWithSas;      // 需要在 PhotoMetadataDTO 新增這個屬性
                    metadata.ThumbnailUrl = thumbnailUrlWithSas;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Blob Storage 上傳失敗，PhotoId: {PhotoId}", uploadedPhoto.PhotoId);

                    // ⚠️ 重要決策：如果 Blob 上傳失敗，是否要回滾資料庫？
                    // 選項 A：回滾（刪除已建立的 Photo 記錄）
                    // 選項 B：繼續（資料庫有記錄但沒有 Blob，之後可以重試上傳）
                    // 這裡建議選項 A：回滾

                    await _photoRepository.SoftDeletePhotoAsync(uploadedPhoto.PhotoId);

                    return new PhotoUploadResponseDTO
                    {
                        Success = false,
                        Message = "照片上傳到儲存服務失敗，請稍後再試"
                    };
                }


                // 🚀 觸發背景地理編碼任務（Fire and Forget）
                if (metadata.GPSLatitude.HasValue && metadata.GPSLongitude.HasValue)
                {
                    _logger.LogInformation(
                        "🌍 觸發背景地理編碼任務，PhotoId: {PhotoId}, GPS: ({Latitude}, {Longitude})",
                        uploadedPhoto.PhotoId, metadata.GPSLatitude, metadata.GPSLongitude);

                    // 使用 Task.Run 確保不會阻塞當前請求
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _backgroundGeocodingService.ProcessGeocodingAsync(
                                uploadedPhoto.PhotoId,
                                userId,
                                metadata.GPSLatitude.Value,
                                metadata.GPSLongitude.Value
                            );
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "背景地理編碼任務執行失敗，PhotoId: {PhotoId}", uploadedPhoto.PhotoId);
                        }
                    });
                }

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
                        BlobUrl = metadata.BlobUrl,
                        ThumbnailUrl = metadata.ThumbnailUrl
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "上傳照片時發生錯誤，檔案: {FileName}", file?.FileName);
                return new PhotoUploadResponseDTO
                {
                    Success = false,
                    Message = $"上傳失敗: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// 建立預設縮圖（當無法生成縮圖時使用）
        /// 產生一個 1x1 的透明 PNG 圖片
        /// </summary>
        private byte[] CreateDefaultThumbnail()
        {
            try
            {
                // 建立 1x1 透明圖片
                using var image = new Image<Rgba32>(1, 1);
                image[0, 0] = new Rgba32(200, 200, 200, 255); // 淡灰色

                using var outputStream = new MemoryStream();
                image.SaveAsJpeg(outputStream, new JpegEncoder { Quality = 70 });

                return outputStream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立預設縮圖時發生錯誤");

                // 如果連預設縮圖都無法建立，返回一個最小的 JPEG
                return new byte[] {
            0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46,
            0x49, 0x46, 0x00, 0x01, 0x01, 0x00, 0x00, 0x01,
            0x00, 0x01, 0x00, 0x00, 0xFF, 0xD9
        };
            }
        }

        /// <summary>
        /// 生成基本標籤（時間、相機）
        /// ❌ 不包含地點標籤（移至背景處理）
        /// </summary>
        private List<string> GenerateBasicTags(PhotoMetadataDTO metadata)
        {
            var tags = new List<string>();

            // 時間標籤
            if (metadata.DateTaken.HasValue)
            {
                var date = metadata.DateTaken.Value;
                tags.Add($"{date.Year}");
            }

            // 相機標籤
            if (!string.IsNullOrEmpty(metadata.CameraMake))
            {
                tags.Add(metadata.CameraMake.Trim());
            }

            return tags;
        }

        /// <summary>
        /// 批次上傳照片
        /// </summary>
        public async Task<BatchUploadResponseDTO> UploadPhotoBatchAsync(List<IFormFile> files, long userId)
        {
            try
            {
                _logger.LogInformation($"開始批次上傳，共 {files.Count} 個檔案");

                var results = new List<BatchUploadResultDTO>();
                int successCount = 0;
                int failedCount = 0;

                foreach (var file in files)
                {
                    try
                    {
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

        /// <summary>
        /// 測試 EXIF 解析
        /// </summary>
        public async Task<PhotoMetadataDTO> TestExifAsync(IFormFile file)
        {
            try
            {
                _logger.LogInformation($"測試 EXIF 解析: {file.FileName}");

                var validationError = ValidateFile(file);
                if (validationError != null)
                {
                    throw new Exception(validationError);
                }

                PhotoMetadataDTO metadata;
                using (var stream = file.OpenReadStream())
                {
                    metadata = _exifService.ExtractMetadata(stream, file.FileName);
                }

                _logger.LogInformation("EXIF 測試完成");
                return await Task.FromResult(metadata);
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
                // 由 Repository 組出 Detail DTO（避免 Service 直接碰 DbContext）
                var detail = await _photoRepository.GetPhotoDetailAsync(photoId);

                if (detail == null)
                    throw new KeyNotFoundException($"照片不存在，PhotoId: {photoId}");

                // 權限驗證：只有照片擁有者能看
                if (detail.UserId != userId)
                    throw new UnauthorizedAccessException("無權限查看此照片");

                return detail;
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
        public async Task<bool> DeletePhotoAsync(long photoId, long userId)
        {
            try
            {
                _logger.LogInformation($"刪除照片，PhotoId: {photoId}, UserId: {userId}");

                // 先查詢照片
                var ownerUserId = await _photoRepository.GetPhotoOwnerUserIdAsync(photoId);

                if (ownerUserId == null)
                    throw new KeyNotFoundException($"照片不存在，PhotoId: {photoId}");

                if (ownerUserId.Value != userId)
                    throw new UnauthorizedAccessException("無權限刪除此照片");


                // 執行軟刪除
                var result = await _photoRepository.SoftDeletePhotoAsync(photoId);

                if (result)
                {
                    _logger.LogInformation($"✅ 照片刪除成功，PhotoId: {photoId}");
                }

                return result;
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
        private string ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return "請選擇要上傳的檔案";
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!PhotoConstants.ALLOWED_IMAGE_EXTENSIONS.Contains(fileExtension))
            {
                return $"不支援的檔案格式，僅支援: {string.Join(", ", PhotoConstants.ALLOWED_IMAGE_EXTENSIONS)}";
            }

            if (file.Length > PhotoConstants.MAX_FILE_SIZE_BYTES)
            {
                return $"檔案大小不能超過 {PhotoConstants.MAX_FILE_SIZE_MB} MB";
            }

            return null;
        }

        /// <summary>
        /// 處理地理位置編碼
        /// </summary>
        private async Task<PhotoLocation> ProcessGeolocationAsync(PhotoMetadataDTO metadata)
        {
            _logger.LogInformation($"🌍 照片包含 GPS 座標，開始反向地理編碼: ({metadata.GPSLatitude}, {metadata.GPSLongitude})");

            try
            {
                var geocodingResult = await _geocodingService.ReverseGeocodeAsync(
                    metadata.GPSLatitude.Value,
                    metadata.GPSLongitude.Value,
                    "zh-TW"
                );

                if (geocodingResult.Success && geocodingResult.AddressInfo != null)
                {
                    var addressInfo = geocodingResult.AddressInfo;

                    // 根據地址資訊重新生成標籤（包含地點標籤）
                    metadata.AutoTags = _exifService.GenerateAutoTags(metadata, addressInfo);

                    _logger.LogInformation($"✅ 反向地理編碼成功: {addressInfo.Country} - {addressInfo.City} - {addressInfo.District}");
                    _logger.LogInformation($"🏷️ 已加入地點標籤，總標籤數量: {metadata.AutoTags.Count}");

                    return new PhotoLocation
                    {
                        Latitude = metadata.GPSLatitude.Value,
                        Longitude = metadata.GPSLongitude.Value,
                        Country = addressInfo.Country,
                        City = addressInfo.City,
                        District = addressInfo.District,
                        PlaceName = addressInfo.PlaceName,
                        Address = addressInfo.Address
                    };
                }
                else
                {
                    _logger.LogWarning($"⚠️ 反向地理編碼失敗: {geocodingResult.ErrorMessage}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 反向地理編碼時發生錯誤");
                return null;
            }
        }

        /// <summary>
        /// 建立 Photo 模型
        /// </summary>
        private Photo CreatePhotoModel(long userId, PhotoMetadataDTO metadata, byte[] fileBytes, string fileHash)
        {
            return new Photo
            {
                UserId = userId,
                FileName = metadata.FileName,
                FileExtension = metadata.FileExtension,
                FileSize = metadata.FileSize,
                PhotoData = fileBytes, // TODO: 之後替換成雲端儲存
                Hash = fileHash
            };
        }

        /// <summary>
        /// 建立 PhotoMetadata 模型
        /// </summary>
        private PhotoMetadatum CreatePhotoMetadataModel(PhotoMetadataDTO metadata)
        {
            return new PhotoMetadatum
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
                Height = metadata.Height
            };
        }


        /// <summary>
        /// 生成縮圖
        /// </summary>
        public async Task<byte[]> GenerateThumbnailAsync(byte[] imageData, int width, int height)
        {
            var imageHash = Convert.ToBase64String(
                System.Security.Cryptography.SHA256.HashData(imageData)
            ).Substring(0, 16);

            var cacheKey = $"thumbnail_{imageHash}_{width}x{height}";

            if (_cache.TryGetValue<byte[]>(cacheKey, out var cachedThumbnail))
            {
                _logger.LogInformation("從快取取得縮圖: {CacheKey}", cacheKey);
                return cachedThumbnail;
            }

            const int maxRetries = 3;
            int retryCount = 0;

            while (retryCount < maxRetries)
            {
                try
                {
                    _logger.LogInformation("生成縮圖: {Width}x{Height} (嘗試 {RetryCount}/{MaxRetries})",
                        width, height, retryCount + 1, maxRetries);

                    // 判斷是否為 HEIC 格式
                    bool isHeic = IsHeicFormat(imageData);

                    Image<Rgba32> image;

                    if (isHeic)
                    {
                        var asm = typeof(MetadataExtractor.ImageMetadataReader).Assembly;
                        _logger.LogInformation("MetadataExtractor loaded: {FullName}", asm.FullName);
                        _logger.LogInformation("MetadataExtractor path: {Path}", asm.Location);

                        var heicAsm = typeof(Openize.Heic.Decoder.HeicImage).Assembly;
                        _logger.LogInformation("Openize.HEIC loaded: {FullName}", heicAsm.FullName);
                        _logger.LogInformation("Openize.HEIC path: {Path}", heicAsm.Location);



                        // HEIC 格式：使用 Openize.HEIC 解碼
                        _logger.LogInformation("🎨 偵測到 HEIC 格式，使用 Openize.HEIC 解碼");
                        image = await DecodeHeicToImageSharpAsync(imageData);
                    }
                    else
                    {
                        // 其他格式：直接用 ImageSharp 載入
                        _logger.LogInformation("🎨 使用 ImageSharp 直接載入圖片");
                        using var inputStream = new MemoryStream(imageData);
                        image = await Image.LoadAsync<Rgba32>(inputStream);
                    }

                    // 使用 ImageSharp 處理縮圖
                    using (image)
                    {
                        // 計算縮圖尺寸（保持寬高比）
                        var (thumbnailWidth, thumbnailHeight) = CalculateThumbnailSize(
                            image.Width,
                            image.Height,
                            width,
                            height
                        );

                        _logger.LogInformation("原始尺寸: {OriginalWidth}x{OriginalHeight}, 縮圖尺寸: {ThumbnailWidth}x{ThumbnailHeight}",
                            image.Width, image.Height, thumbnailWidth, thumbnailHeight);

                        // 調整大小
                        image.Mutate(x => x
                            .Resize(new ResizeOptions
                            {
                                Size = new Size(thumbnailWidth, thumbnailHeight),
                                Mode = ResizeMode.Max,
                                Sampler = KnownResamplers.Lanczos3
                            })
                            .GaussianSharpen(PhotoConstants.THUMBNAIL_SHARPEN_STRENGTH)
                        );

                        // 儲存為 JPEG
                        using var outputStream = new MemoryStream();
                        var encoder = new JpegEncoder { Quality = PhotoConstants.THUMBNAIL_JPEG_QUALITY };
                        await image.SaveAsync(outputStream, encoder);

                        var thumbnail = outputStream.ToArray();

                        // 存入快取
                        var cacheOptions = new MemoryCacheEntryOptions()
                            .SetSlidingExpiration(TimeSpan.FromHours(PhotoConstants.THUMBNAIL_CACHE_HOURS))
                            .SetSize(thumbnail.Length);

                        _cache.Set(cacheKey, thumbnail, cacheOptions);

                        _logger.LogInformation("✅ 縮圖生成成功: {Width}x{Height}, 大小: {Size} bytes",
                            thumbnailWidth, thumbnailHeight, thumbnail.Length);

                        return thumbnail;
                    }
                }
                catch (Exception ex)
                {
                    retryCount++;
                    _logger.LogError(ex, "❌ 生成縮圖時發生錯誤（第 {RetryCount}/{MaxRetries} 次）",
                        retryCount, maxRetries);

                    if (retryCount < maxRetries)
                    {
                        // 等待後重試
                        await Task.Delay(TimeSpan.FromMilliseconds(PhotoConstants.BASE_RETRY_DELAY_MS * retryCount));
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ 縮圖生成失敗達上限，使用預設縮圖");
                        return await GenerateDefaultThumbnailAsync(width, height);
                    }
                }
            }

            // 不應該到達這裡，但作為保護
            return await GenerateDefaultThumbnailAsync(width, height);
        }

        /// <summary>
        /// 使用 Openize.HEIC 解碼 HEIC 並轉換為 ImageSharp 格式
        /// </summary>
        private async Task<Image<Rgba32>> DecodeHeicToImageSharpAsync(byte[] heicData)
        {
            try
            {
                _logger.LogInformation("開始 HEIC 解碼...");

                // 使用 Openize.HEIC 解碼
                using var inputStream = new MemoryStream(heicData);

                var heicImage = HeicImage.Load(inputStream);

                int width = (int)heicImage.Width;
                int height = (int)heicImage.Height;

                _logger.LogInformation("✅ HEIC 解碼成功，尺寸: {Width}x{Height}", width, height);

                byte[] pixelData = heicImage.GetByteArray(
                    Openize.Heic.Decoder.PixelFormat.Rgba32,
                    new System.Drawing.Rectangle(0, 0, width, height)
                );

                _logger.LogInformation("📊 像素資料大小: {Size} bytes ({Width}x{Height}x3)", pixelData.Length, width, height);

                // 建立 ImageSharp Image
                var image = new Image<Rgba32>(width, height);

                // 填充像素資料
                int pixelIndex = 0;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        byte r = pixelData[pixelIndex++];
                        byte g = pixelData[pixelIndex++];
                        byte b = pixelData[pixelIndex++];
                        byte a = pixelData[pixelIndex++];

                        image[x, y] = new Rgba32(r, g, b, a);
                    }
                }

                _logger.LogInformation("✅ HEIC 轉換為 ImageSharp 完成");

                return image;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ HEIC 解碼失敗");
                throw;
            }
        }

        /// <summary>
        /// 判斷是否為 HEIC 格式   
        /// </summary>
        private bool IsHeicFormat(byte[] data)
        {
            // 檢查檔案頭 (Magic Number)
            // HEIC 檔案以 "ftyp" 開頭（在 offset 4）
            if (data.Length >= 12)
            {
                try
                {
                    string header = System.Text.Encoding.ASCII.GetString(data, 4, 4);
                    if (header == "ftyp")
                    {
                        // 進一步檢查 brand
                        string brand = System.Text.Encoding.ASCII.GetString(data, 8, 4);

                        // HEIC/HEIF 的各種 brand
                        if (brand.StartsWith("heic") || brand.StartsWith("heix") ||
                            brand.StartsWith("hevc") || brand.StartsWith("hevx") ||
                            brand.StartsWith("heim") || brand.StartsWith("heis") ||
                            brand.StartsWith("mif1") || brand.StartsWith("msf1"))
                        {
                            _logger.LogInformation("✅ 偵測到 HEIC 格式，brand: {Brand}", brand);
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ 檢查 HEIC 格式時發生錯誤");
                }
            }

            return false;
        }

        private (int width, int height) CalculateThumbnailSize(int originalWidth, int originalHeight, int maxWidth, int maxHeight)
        {
            double aspectRatio = (double)originalWidth / originalHeight;

            int thumbnailWidth;
            int thumbnailHeight;

            if (originalWidth > originalHeight)
            {
                // 橫向圖片
                thumbnailWidth = maxWidth;
                thumbnailHeight = (int)(maxWidth / aspectRatio);

                if (thumbnailHeight > maxHeight)
                {
                    thumbnailHeight = maxHeight;
                    thumbnailWidth = (int)(maxHeight * aspectRatio);
                }
            }
            else
            {
                // 直向圖片
                thumbnailHeight = maxHeight;
                thumbnailWidth = (int)(maxHeight * aspectRatio);

                if (thumbnailWidth > maxWidth)
                {
                    thumbnailWidth = maxWidth;
                    thumbnailHeight = (int)(maxWidth / aspectRatio);
                }
            }

            // 確保至少 1 像素
            thumbnailWidth = Math.Max(1, thumbnailWidth);
            thumbnailHeight = Math.Max(1, thumbnailHeight);

            return (thumbnailWidth, thumbnailHeight);
        }

        /// <summary>
        /// 生成預設縮圖（純色背景）
        /// 當無法處理原始圖片時使用
        /// </summary>
        private async Task<byte[]> GenerateDefaultThumbnailAsync(int width, int height)
        {
            try
            {
                _logger.LogInformation("🎨 生成預設縮圖: {Width}x{Height}", width, height);

                using var image = new Image<Rgba32>(width, height);

                // 填充灰色背景 #E8E8E8
                image.Mutate(x => x.BackgroundColor(Color.FromRgb(232, 232, 232)));

                // 儲存為 JPEG
                using var outputStream = new MemoryStream();
                var encoder = new JpegEncoder { Quality = 85 };
                await image.SaveAsync(outputStream, encoder);

                _logger.LogInformation("✅ 預設縮圖生成成功");

                return outputStream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 生成預設縮圖時發生錯誤");

                // 返回最小的有效 JPEG (1x1 灰色像素)
                return new byte[] {
                    0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46,
                    0x49, 0x46, 0x00, 0x01, 0x01, 0x01, 0x00, 0x60,
                    0x00, 0x60, 0x00, 0x00, 0xFF, 0xC0, 0x00, 0x11,
                    0x08, 0x00, 0x01, 0x00, 0x01, 0x03, 0x01, 0x22,
                    0x00, 0x02, 0x11, 0x01, 0x03, 0x11, 0x01, 0xFF,
                    0xD9
                };
            }
        }

        /// <summary>
        /// 取得標籤階層（用於 Sidebar）
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>標籤階層回應</returns>
        public async Task<TagHierarchyResponseDTO> GetTagHierarchyAsync(long userId)
        {
            try
            {
                _logger.LogInformation("🏷️ 開始取得標籤階層，UserId: {UserId}", userId);

                // 從 Repository 取得標籤階層
                var categories = await _photoRepository.GetTagHierarchyAsync(userId);

                if (categories == null || !categories.Any())
                {
                    _logger.LogWarning("⚠️ 沒有找到任何分類");
                    return new TagHierarchyResponseDTO
                    {
                        Success = true,
                        Message = "目前沒有任何分類",
                        Categories = new List<CategoryWithTagsDTO>(),
                        TotalPhotoCount = 0
                    };
                }

                // 計算總照片數量
                var totalPhotoCount = await _photoRepository.GetUserPhotoCountAsync(userId);

                _logger.LogInformation("✅ 標籤階層取得成功，共 {CategoryCount} 個分類", categories.Count);

                return new TagHierarchyResponseDTO
                {
                    Success = true,
                    Message = "標籤階層取得成功",
                    Categories = categories,
                    TotalPhotoCount = totalPhotoCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 取得標籤階層時發生錯誤");
                return new TagHierarchyResponseDTO
                {
                    Success = false,
                    Message = $"取得標籤階層失敗: {ex.Message}",
                    Categories = new List<CategoryWithTagsDTO>(),
                    TotalPhotoCount = 0
                };
            }
        }

        /// <summary>
        /// 根據標籤篩選照片（便捷方法，實際使用 QueryPhotosAsync）
        /// </summary>
        /// <param name="request">查詢請求（包含 TagIds）</param>
        /// <param name="userId">使用者 ID</param>
        /// <returns>查詢結果</returns>
        public async Task<PhotoQueryResponseDTO> GetPhotosByTagsAsync(PhotoQueryRequestDTO request, long userId)
        {
            try
            {
                _logger.LogInformation("🔍 根據標籤篩選照片，TagIds: {TagIds}, UserId: {UserId}",
                    request.TagIds != null ? string.Join(", ", request.TagIds) : "無",
                    userId);

                // 驗證 TagIds
                if (request.TagIds == null || !request.TagIds.Any())
                {
                    _logger.LogWarning("⚠️ TagIds 為空，返回所有照片");
                    // 如果沒有標籤篩選，返回所有照片
                    request.TagIds = null;
                }

                // 使用現有的 QueryPhotosAsync 方法
                var result = await QueryPhotosAsync(request, userId);

                _logger.LogInformation("✅ 標籤篩選完成，找到 {Count} 張照片", result.TotalCount);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 根據標籤篩選照片時發生錯誤");
                return new PhotoQueryResponseDTO
                {
                    Success = false,
                    Message = $"標籤篩選失敗: {ex.Message}",
                    Photos = new List<PhotoItemDTO>(),
                    TotalCount = 0
                };
            }
        }

        /// <summary>
        /// 建立自訂標籤
        /// </summary>
        /// <param name="request">建立自訂標籤請求</param>
        /// <param name="userId">使用者 ID</param>
        /// <returns>建立結果</returns>
        public async Task<CreateCustomTagResponseDTO> CreateCustomTagAsync(CreateCustomTagRequestDTO request, long userId)
        {
            try
            {
                _logger.LogInformation("🏷️ 建立自訂標籤，TagName: {TagName}, UserId: {UserId}",
                    request.TagName, userId);

                // 驗證請求
                if (string.IsNullOrWhiteSpace(request.TagName))
                {
                    _logger.LogWarning("⚠️ 標籤名稱不能為空");
                    return new CreateCustomTagResponseDTO
                    {
                        Success = false,
                        Message = "標籤名稱不能為空"
                    };
                }

                // 移除前後空白
                request.TagName = request.TagName.Trim();

                // 檢查標籤名稱長度
                if (request.TagName.Length > 50)
                {
                    _logger.LogWarning("⚠️ 標籤名稱過長: {Length} 字元", request.TagName.Length);
                    return new CreateCustomTagResponseDTO
                    {
                        Success = false,
                        Message = "標籤名稱不能超過 50 個字元"
                    };
                }

                // 取得「自訂標籤」分類
                var customCategory = await _photoRepository.GetCategoryByCodeAsync(PhotoConstants.TAG_TYPE_CUSTOM);

                if (customCategory == null)
                {
                    _logger.LogError("❌ 找不到「自訂標籤」分類");
                    return new CreateCustomTagResponseDTO
                    {
                        Success = false,
                        Message = "系統錯誤：找不到自訂標籤分類"
                    };
                }

                // 如果有父標籤，驗證父標籤是否存在且屬於自訂分類
                if (request.ParentTagId.HasValue)
                {
                    var parentTag = await _photoRepository.GetTagByIdAsync(request.ParentTagId.Value);

                    if (parentTag == null)
                    {
                        _logger.LogWarning("⚠️ 父標籤不存在，ParentTagId: {ParentTagId}", request.ParentTagId.Value);
                        return new CreateCustomTagResponseDTO
                        {
                            Success = false,
                            Message = "父標籤不存在"
                        };
                    }

                    if (parentTag.CategoryId != customCategory.CategoryId)
                    {
                        _logger.LogWarning("⚠️ 父標籤不屬於自訂分類");
                        return new CreateCustomTagResponseDTO
                        {
                            Success = false,
                            Message = "父標籤必須屬於自訂標籤分類"
                        };
                    }
                }

                // 建立標籤
                var newTag = await _photoRepository.CreateCustomTagAsync(
                    request.TagName,
                    customCategory.CategoryId,
                    request.ParentTagId,
                    userId
                );

                _logger.LogInformation("✅ 自訂標籤建立成功，TagId: {TagId}", newTag.TagId);

                // 轉換為 DTO
                var tagDto = new TagTreeNodeDTO
                {
                    TagId = newTag.TagId,
                    TagName = newTag.TagName,
                    TagType = newTag.TagType,
                    CategoryId = newTag.CategoryId,
                    ParentTagId = newTag.ParentTagId,
                    PhotoCount = 0, // 新建立的標籤照片數量為 0
                    DisplayOrder = newTag.DisplayOrder,
                    Children = new List<TagTreeNodeDTO>()
                };

                return new CreateCustomTagResponseDTO
                {
                    Success = true,
                    Message = "標籤建立成功",
                    Tag = tagDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 建立自訂標籤時發生錯誤");
                return new CreateCustomTagResponseDTO
                {
                    Success = false,
                    Message = $"建立標籤失敗: {ex.Message}"
                };
            }
        }

        #endregion

        #region 私有方法 - 查詢相關

        /// <summary>
        /// 驗證查詢請求參數
        /// </summary>
        private string ValidateQueryRequest(PhotoQueryRequestDTO request)
        {
            if (request.PageNumber < 1)
            {
                return "頁碼必須大於 0";
            }

            if (request.PageSize <= 0 || request.PageSize > PhotoConstants.MAX_PAGE_SIZE)
            {
                return $"每頁筆數必須介於 1 到 {PhotoConstants.MAX_PAGE_SIZE} 之間";
            }

            // 驗證日期範圍
            if (request.StartDate.HasValue && request.EndDate.HasValue)
            {
                if (request.StartDate > request.EndDate)
                {
                    return "開始日期不能晚於結束日期";
                }
            }

            // 驗證年份
            if (request.Years != null && request.Years.Any())
            {
                var currentYear = DateTime.UtcNow.Year;
                if (request.Years.Any(y => y < 1900 || y > currentYear + 1))
                {
                    return $"年份必須在 1900 到 {currentYear + 1} 之間";
                }
            }

            // 驗證月份
            if (request.Months != null && request.Months.Any())
            {
                if (request.Months.Any(m => m < 1 || m > 12))
                {
                    return "月份必須介於 1 到 12 之間";
                }
            }

            // 驗證 ISO 範圍
            if (request.MinISO.HasValue && request.MaxISO.HasValue)
            {
                if (request.MinISO > request.MaxISO)
                {
                    return "最小 ISO 不能大於最大 ISO";
                }
            }

            // 驗證光圈範圍
            if (request.MinAperture.HasValue && request.MaxAperture.HasValue)
            {
                if (request.MinAperture > request.MaxAperture)
                {
                    return "最小光圈不能大於最大光圈";
                }
            }

            // 驗證焦距範圍
            if (request.MinFocalLength.HasValue && request.MaxFocalLength.HasValue)
            {
                if (request.MinFocalLength > request.MaxFocalLength)
                {
                    return "最小焦距不能大於最大焦距";
                }
            }

            // 驗證檔案大小範圍
            if (request.MinFileSize.HasValue && request.MaxFileSize.HasValue)
            {
                if (request.MinFileSize > request.MaxFileSize)
                {
                    return "最小檔案大小不能大於最大檔案大小";
                }
            }

            // 驗證排序欄位
            var validSortBy = new[] {  "DateTaken", "UploadedAt", "FileName", "FileSize" };
            if (!validSortBy.Contains(request.SortBy, StringComparer.OrdinalIgnoreCase))
            {
                return $"無效的排序欄位，允許的值: {string.Join(", ", validSortBy)}";
            }

            // 驗證排序方向
            var validSortOrder = new[] { "asc", "desc" };
            if (!validSortOrder.Contains(request.SortOrder, StringComparer.OrdinalIgnoreCase))
            {
                return "排序方向必須是 'asc' 或 'desc'";
            }

            return null;
        }

        /// <summary>
        /// 將 Photo 模型轉換為 PhotoItemDTO
        /// </summary>
        private PhotoItemDTO MapToPhotoItemDTO(Photo photo)
        {
            var metadata = photo.PhotoMetadata?.FirstOrDefault();
            var location = photo.PhotoLocations?.FirstOrDefault();

            // 組合地點描述
            var locationParts = new List<string>();
            if (!string.IsNullOrEmpty(location?.Country))
                locationParts.Add(location.Country);
            if (!string.IsNullOrEmpty(location?.City))
                locationParts.Add(location.City);
            if (!string.IsNullOrEmpty(location?.District))
                locationParts.Add(location.District);
            var locationDescription = locationParts.Any() ? string.Join(" - ", locationParts) : null;

            // 組合相機描述
            var cameraParts = new List<string>();
            if (!string.IsNullOrEmpty(metadata?.CameraMake))
                cameraParts.Add(metadata.CameraMake);
            if (!string.IsNullOrEmpty(metadata?.CameraModel))
                cameraParts.Add(metadata.CameraModel);
            var cameraDescription = cameraParts.Any() ? string.Join(" - ", cameraParts) : null;

            // 整理標籤
            var exifTags = photo.PhotoPhotoTags?
                .Where(ppt => ppt.SourceId == PhotoConstants.SOURCE_ID_EXIF)
                .Select(ppt => ppt.Tag?.TagName)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList() ?? new List<string>();

            var manualTags = photo.PhotoPhotoTags?
                .Where(ppt => ppt.SourceId == PhotoConstants.SOURCE_ID_MANUAL)
                .Select(ppt => ppt.Tag?.TagName)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList() ?? new List<string>();

            var aiTags = photo.PhotoPhotoTags?
                .Where(ppt => ppt.SourceId == PhotoConstants.SOURCE_ID_AI)
                .Select(ppt => ppt.Tag?.TagName)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList() ?? new List<string>();

            var allTags = new List<string>();
            allTags.AddRange(exifTags);
            allTags.AddRange(manualTags);
            allTags.AddRange(aiTags);

            var categories = photo.PhotoPhotoCategories?
                .Select(ppc => ppc.Category?.CategoryName)
                .Where(name => !string.IsNullOrEmpty(name))
                .Distinct()
                .ToList() ?? new List<string>();

            return new PhotoItemDTO
            {
                // 基本資訊
                PhotoId = photo.PhotoId,
                FileName = photo.FileName,
                FileExtension = photo.FileExtension,
                FileSize = photo.FileSize,
                ThumbnailUrl = null, // TODO: 之後實作縮圖功能
                PreviewUrl = null,   // TODO: 之後實作預覽功能

                // 時間資訊
                DateTaken = metadata?.DateTaken,
                UploadedAt = photo.UploadedAt,

                // 地點資訊
                Location = locationDescription,
                Country = location?.Country,
                City = location?.City,
                District = location?.District,
                PlaceName = location?.PlaceName,
                Latitude = location?.Latitude,
                Longitude = location?.Longitude,

                // 相機資訊
                Camera = cameraDescription,
                CameraMake = metadata?.CameraMake,
                CameraModel = metadata?.CameraModel,
                LensModel = metadata?.LensModel,

                // 拍攝參數
                ISO = metadata?.Iso,
                Aperture = metadata?.Aperture,
                ShutterSpeed = metadata?.ShutterSpeed,
                FocalLength = metadata?.FocalLength,

                // 標籤與分類
                Tags = allTags,
                ExifTags = exifTags,
                ManualTags = manualTags,
                AiTags = aiTags,
                Categories = categories,

                // 圖片尺寸
                Width = metadata?.Width,
                Height = metadata?.Height
            };

        }

        #endregion
    }
}