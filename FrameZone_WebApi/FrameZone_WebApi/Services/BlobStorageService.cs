using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using FrameZone_WebApi.Configuration;
using FrameZone_WebApi.DTOs;
using FrameZone_WebApi.Helpers;
using Microsoft.Extensions.Options;

namespace FrameZone_WebApi.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly AzureBlobStorageSettings _settings;
        private readonly IConfiguration _configuration;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _photoContainerClient;
        private readonly BlobContainerClient _thumbnailContainerClient;
        private readonly ILogger<BlobStorageService> _logger;

        public BlobStorageService(
            IOptions<AzureBlobStorageSettings> settings,
            IConfiguration configuration,
            ILogger<BlobStorageService> logger)
        {
            _settings = settings.Value;
            _configuration = configuration;
            _logger = logger;

            // 優先從 IOptions<AzureBlobStorageSettings> 讀取（appsettings.json: AzureBlobStorage:ConnectionString）
            // 保留對舊 Key 的相容（AzureStorageConnectionString），避免其他環節尚未完全移除 Key Vault 時造成中斷
            var connectionString = _settings.ConnectionString;

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = _configuration["AzureStorageConnectionString"];
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                _logger.LogError("❌ Azure Blob Storage 設定驗證失敗: ConnectionString 未設定（AzureBlobStorage:ConnectionString）");
                throw new InvalidOperationException("Azure Blob Storage 設定錯誤: ConnectionString 未設定");
            }

            _logger.LogInformation("✅ Azure Storage ConnectionString 已從 appsettings 載入，長度: {Length}", connectionString.Length);

            // 初始化 BlobServiceClient
            _blobServiceClient = new BlobServiceClient(connectionString);

            // 初始化容器 Client
            _photoContainerClient = _blobServiceClient.GetBlobContainerClient(_settings.ContainerName);
            _thumbnailContainerClient = _blobServiceClient.GetBlobContainerClient(_settings.ThumbnailContainerName);

            _logger.LogInformation(
                "✅ BlobStorageService 初始化完成，容器: {PhotoContainer}, {ThumbnailContainer}",
                _settings.ContainerName,
                _settings.ThumbnailContainerName);
        }

        #region 上傳操作

        /// <summary>
        /// 上傳照片原圖到 Blob Storage
        /// </summary>
        public async Task<string> UploadPhotoAsync(
            Stream fileStream,
            string fileName,
            long userId,
            long photoId,
            DateTime uploadDate,
            string contentType = "image/jpeg")
        {
            try
            {
                // 取得副檔名
                var extension = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();

                // 生成 Blob 路徑
                var blobPath = GeneratePhotoBlobPath(userId, photoId, uploadDate, extension);

                _logger.LogInformation(
                    "📤 開始上傳原圖，PhotoId: {PhotoId}, 路徑: {BlobPath}",
                    photoId, blobPath);

                // 上傳到 Blob Storage
                var blobUrl = await UploadAsync(
                    fileStream,
                    blobPath,
                    _settings.ContainerName,
                    contentType);

                _logger.LogInformation(
                    "✅ 原圖上傳成功，PhotoId: {PhotoId}, URL: {BlobUrl}",
                    photoId, blobUrl);

                return blobUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 上傳原圖失敗，PhotoId: {PhotoId}", photoId);
                throw;
            }
        }

        /// <summary>
        /// 上傳縮圖到 Blob Storage
        /// </summary>
        public async Task<string> UploadThumbnailAsync(
            Stream thumbnailStream,
            string fileName,
            long userId,
            long photoId,
            DateTime uploadDate,
            string contentType = "image/jpeg")
        {
            try
            {
                // 取得副檔名
                var extension = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();

                // 生成 Blob 路徑
                var blobPath = GenerateThumbnailBlobPath(userId, photoId, uploadDate, extension);

                _logger.LogInformation(
                    "📤 開始上傳縮圖，PhotoId: {PhotoId}, 路徑: {BlobPath}",
                    photoId, blobPath);

                // 上傳到 Blob Storage
                var blobUrl = await UploadAsync(
                    thumbnailStream,
                    blobPath,
                    _settings.ThumbnailContainerName,
                    contentType);

                _logger.LogInformation(
                    "✅ 縮圖上傳成功，PhotoId: {PhotoId}, URL: {BlobUrl}",
                    photoId, blobUrl);

                return blobUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 上傳縮圖失敗，PhotoId: {PhotoId}", photoId);
                throw;
            }
        }

        /// <summary>
        /// 通用上傳方法
        /// </summary>
        public async Task<string> UploadAsync(
            Stream stream,
            string blobPath,
            string containerName,
            string contentType = "image/jpeg")
        {
            try
            {
                // 取得容器 Client
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

                // 確保容器存在
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

                // 取得 Blob Client
                var blobClient = containerClient.GetBlobClient(blobPath);

                // 重置串流位置
                if (stream.CanSeek)
                {
                    stream.Position = 0;
                }

                // 設定上傳選項
                var uploadOptions = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = contentType,
                        CacheControl = BlobStorageConstants.CACHE_CONTROL_ONE_YEAR // 快取 1 年
                    },
                    // 設定存取層級（初期都是 Hot）
                    AccessTier = ParseAccessTier(_settings.DefaultAccessTier ?? BlobStorageConstants.DEFAULT_ACCESS_TIER)
                };

                // 上傳到 Blob Storage
                var response = await blobClient.UploadAsync(stream, uploadOptions);

                // 返回 Blob URL
                var blobUrl = blobClient.Uri.ToString();

                _logger.LogDebug(
                    "✅ Blob 上傳成功，路徑: {BlobPath}, 大小: {Size} bytes",
                    blobPath, stream.Length);

                return blobUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Blob 上傳失敗，路徑: {BlobPath}", blobPath);
                throw;
            }
        }

        #endregion

        #region 下載操作

        /// <summary>
        /// 下載照片原圖
        /// </summary>
        public async Task<Stream> DownloadPhotoAsync(string blobPath)
        {
            return await DownloadAsync(blobPath, _settings.ContainerName);
        }

        /// <summary>
        /// 下載縮圖
        /// </summary>
        public async Task<Stream> DownloadThumbnailAsync(string blobPath)
        {
            return await DownloadAsync(blobPath, _settings.ThumbnailContainerName);
        }

        /// <summary>
        /// 通用下載方法
        /// </summary>
        public async Task<Stream> DownloadAsync(string blobPath, string containerName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(blobPath);

                // 檢查 Blob 是否存在
                if (!await blobClient.ExistsAsync())
                {
                    _logger.LogWarning("⚠️ Blob 不存在: {BlobPath}", blobPath);
                    throw new FileNotFoundException($"Blob 不存在: {blobPath}");
                }

                // 下載到記憶體串流
                var memoryStream = new MemoryStream();
                await blobClient.DownloadToAsync(memoryStream);
                memoryStream.Position = 0;

                _logger.LogDebug("✅ Blob 下載成功: {BlobPath}", blobPath);

                return memoryStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Blob 下載失敗: {BlobPath}", blobPath);
                throw;
            }
        }

        #endregion

        #region 刪除操作

        /// <summary>
        /// 刪除照片原圖
        /// </summary>
        public async Task<bool> DeletePhotoAsync(string blobPath)
        {
            return await DeleteAsync(blobPath, _settings.ContainerName);
        }

        /// <summary>
        /// 刪除縮圖
        /// </summary>
        public async Task<bool> DeleteThumbnailAsync(string blobPath)
        {
            return await DeleteAsync(blobPath, _settings.ThumbnailContainerName);
        }

        /// <summary>
        /// 通用刪除方法
        /// </summary>
        public async Task<bool> DeleteAsync(string blobPath, string containerName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(blobPath);

                // 刪除 Blob（包含所有快照）
                var response = await blobClient.DeleteIfExistsAsync(
                    DeleteSnapshotsOption.IncludeSnapshots);

                if (response.Value)
                {
                    _logger.LogInformation("✅ Blob 刪除成功: {BlobPath}", blobPath);
                    return true;
                }
                else
                {
                    _logger.LogWarning("⚠️ Blob 不存在（刪除操作跳過）: {BlobPath}", blobPath);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Blob 刪除失敗: {BlobPath}", blobPath);
                return false;
            }
        }

        #endregion

        #region 檢查操作

        /// <summary>
        /// 檢查 Blob 是否存在
        /// </summary>
        public async Task<bool> ExistsAsync(string blobPath, string containerName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(blobPath);

                return await blobClient.ExistsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 檢查 Blob 存在時發生錯誤: {BlobPath}", blobPath);
                return false;
            }
        }

        /// <summary>
        /// 檢查照片原圖是否存在
        /// </summary>
        public async Task<bool> PhotoExistsAsync(string blobPath)
        {
            return await ExistsAsync(blobPath, _settings.ContainerName);
        }

        /// <summary>
        /// 檢查縮圖是否存在
        /// </summary>
        public async Task<bool> ThumbnailExistsAsync(string blobPath)
        {
            return await ExistsAsync(blobPath, _settings.ThumbnailContainerName);
        }

        #endregion

        #region URL 生成

        /// <summary>
        /// 生成照片的存取 URL
        /// </summary>
        public async Task<string> GetPhotoUrlAsync(string blobPath, bool useSasToken = true)
        {
            if (useSasToken)
            {
                return await GenerateSasUrlAsync(blobPath, _settings.ContainerName);
            }
            else
            {
                return GetDirectUrl(blobPath, _settings.ContainerName);
            }
        }

        /// <summary>
        /// 生成縮圖的存取 URL
        /// </summary>
        public async Task<string> GetThumbnailUrlAsync(string blobPath, bool useSasToken = true)
        {
            if (useSasToken)
            {
                return await GenerateSasUrlAsync(blobPath, _settings.ThumbnailContainerName);
            }
            else
            {
                return GetDirectUrl(blobPath, _settings.ThumbnailContainerName);
            }
        }

        /// <summary>
        /// 生成 SAS Token URL（具有時效性的安全 URL）
        /// </summary>
        public async Task<string> GenerateSasUrlAsync(
            string blobPath,
            string containerName,
            int? expiryMinutes = null)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(blobPath);

                // 檢查 Blob 是否存在
                if (!await blobClient.ExistsAsync())
                {
                    _logger.LogWarning("⚠️ 生成 SAS URL 失敗，Blob 不存在: {BlobPath}", blobPath);
                    throw new FileNotFoundException($"Blob 不存在: {blobPath}");
                }

                // 設定 SAS Token 權限和有效期限
                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = containerName,
                    BlobName = blobPath,
                    Resource = "b", // "b" = Blob
                    StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5), // 提前 5 分鐘防止時間差
                    ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(PhotoConstants.SAS_TOKEN_DEFAULT_EXPIRY_MINUTES)
                };

                // 設定權限（只允許讀取）
                sasBuilder.SetPermissions(BlobSasPermissions.Read);

                // 生成 SAS Token
                var sasToken = blobClient.GenerateSasUri(sasBuilder);

                // 如果啟用 CDN，替換為 CDN URL
                if (_settings.UseCdn && !string.IsNullOrWhiteSpace(_settings.CdnEndpoint))
                {
                    var cdnUrl = sasToken.ToString()
                        .Replace(blobClient.Uri.GetLeftPart(UriPartial.Authority), _settings.CdnEndpoint);

                    _logger.LogDebug("✅ 生成 CDN SAS URL: {BlobPath}", blobPath);
                    return cdnUrl;
                }

                _logger.LogDebug("✅ 生成 SAS URL: {BlobPath}", blobPath);
                return sasToken.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 生成 SAS URL 失敗: {BlobPath}", blobPath);
                throw;
            }
        }

        /// <summary>
        /// 取得直接 URL（不使用 SAS Token）
        /// 注意：容器必須設定為公開存取才能使用
        /// </summary>
        private string GetDirectUrl(string blobPath, string containerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobPath);

            // 如果啟用 CDN，返回 CDN URL
            if (_settings.UseCdn && !string.IsNullOrWhiteSpace(_settings.CdnEndpoint))
            {
                return $"{_settings.CdnEndpoint}/{containerName}/{blobPath}";
            }

            return blobClient.Uri.ToString();
        }

        #endregion

        #region 路徑生成

        /// <summary>
        /// 生成照片原圖的 Blob 路徑
        /// 格式：{userId}/{year}/{month}/{photoId}_original.{extension}
        /// </summary>
        public string GeneratePhotoBlobPath(
            long userId,
            long photoId,
            DateTime uploadDate,
            string fileExtension)
        {
            // 確保副檔名不含點
            fileExtension = fileExtension.TrimStart('.').ToLowerInvariant();

            // 生成路徑：1001/2024/12/550001_original.jpg
            return $"{userId}/{uploadDate:yyyy}/{uploadDate:MM}/{photoId}_original.{fileExtension}";
        }

        /// <summary>
        /// 生成縮圖的 Blob 路徑
        /// 格式：{userId}/{year}/{month}/{photoId}_thumbnail.{extension}
        /// </summary>
        public string GenerateThumbnailBlobPath(
            long userId,
            long photoId,
            DateTime uploadDate,
            string fileExtension)
        {
            // 確保副檔名不含點
            fileExtension = fileExtension.TrimStart('.').ToLowerInvariant();

            // 生成路徑：1001/2024/12/550001_thumbnail.jpg
            return $"{userId}/{uploadDate:yyyy}/{uploadDate:MM}/{photoId}_thumbnail.{fileExtension}";
        }

        #endregion

        #region 容器管理

        /// <summary>
        /// 確保容器存在（應用程式啟動時呼叫）
        /// </summary>
        public async Task<bool> EnsureContainersExistAsync()
        {
            try
            {
                _logger.LogInformation("🔍 開始檢查容器是否存在...");

                // 建立原圖容器
                var photoContainerResponse = await _photoContainerClient.CreateIfNotExistsAsync(
                    PublicAccessType.None); // 私有存取

                if (photoContainerResponse != null)
                {
                    _logger.LogInformation(
                        "✅ 原圖容器已建立: {ContainerName}",
                        _settings.ContainerName);
                }
                else
                {
                    _logger.LogInformation(
                        "ℹ️ 原圖容器已存在: {ContainerName}",
                        _settings.ContainerName);
                }

                // 建立縮圖容器
                var thumbnailContainerResponse = await _thumbnailContainerClient.CreateIfNotExistsAsync(
                    PublicAccessType.None); // 私有存取

                if (thumbnailContainerResponse != null)
                {
                    _logger.LogInformation(
                        "✅ 縮圖容器已建立: {ContainerName}",
                        _settings.ThumbnailContainerName);
                }
                else
                {
                    _logger.LogInformation(
                        "ℹ️ 縮圖容器已存在: {ContainerName}",
                        _settings.ThumbnailContainerName);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 確保容器存在時發生錯誤");
                return false;
            }
        }

        /// <summary>
        /// 取得容器的使用統計資訊
        /// </summary>
        public async Task<BlobContainerStatsDto> GetContainerStatsAsync(string containerName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

                var stats = new BlobContainerStatsDto
                {
                    ContainerName = containerName,
                    StatsDate = DateTime.UtcNow
                };

                // 列舉所有 Blob
                await foreach (var blobItem in containerClient.GetBlobsAsync())
                {
                    stats.TotalBlobs++;
                    stats.TotalSizeBytes += blobItem.Properties.ContentLength ?? 0;
                }

                _logger.LogInformation(
                    "📊 容器統計 - {ContainerName}: {Count} 個檔案, {Size:F2} GB",
                    containerName, stats.TotalBlobs, stats.TotalSizeGB);

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 取得容器統計資訊失敗: {ContainerName}", containerName);
                throw;
            }
        }

        #endregion


        #region 批次操作

        /// <summary>
        /// 批次上傳照片
        /// </summary>
        public async Task<List<BlobUploadResultDto>> UploadBatchAsync(
            List<BlobUploadRequestDto> uploadRequests)
        {
            var results = new List<BlobUploadResultDto>();

            // 使用 SemaphoreSlim 控制並行數量
            var semaphore = new SemaphoreSlim(_settings.MaxConcurrentUploads);

            var tasks = uploadRequests.Select(async request =>
            {
                await semaphore.WaitAsync();
                try
                {
                    var blobUrl = await UploadAsync(
                        request.Stream,
                        request.BlobPath,
                        request.ContainerName,
                        request.ContentType);

                    return new BlobUploadResultDto
                    {
                        Success = true,
                        BlobPath = request.BlobPath,
                        BlobUrl = blobUrl,
                        UploadedAt = DateTime.UtcNow,
                        FileSizeBytes = request.Stream.Length
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ 批次上傳失敗: {BlobPath}", request.BlobPath);

                    return new BlobUploadResultDto
                    {
                        Success = false,
                        BlobPath = request.BlobPath,
                        ErrorMessage = ex.Message
                    };
                }
                finally
                {
                    semaphore.Release();
                }
            });

            results = (await Task.WhenAll(tasks)).ToList();

            var successCount = results.Count(r => r.Success);
            var failedCount = results.Count(r => !r.Success);

            _logger.LogInformation(
                "📊 批次上傳完成 - 成功: {Success}, 失敗: {Failed}, 總計: {Total}",
                successCount, failedCount, results.Count);

            return results;
        }

        /// <summary>
        /// 批次刪除照片
        /// </summary>
        public async Task<BatchDeleteResultDto> DeleteBatchAsync(
            List<string> blobPaths,
            string containerName)
        {
            var result = new BatchDeleteResultDto
            {
                TotalCount = blobPaths.Count
            };

            // 使用區域變數來累計
            int successCount = 0;
            int failedCount = 0;


            var tasks = blobPaths.Select(async blobPath =>
            {
                var success = await DeleteAsync(blobPath, containerName);

                if (success)
                {
                    Interlocked.Increment(ref successCount);
                }
                else
                {
                    Interlocked.Increment(ref failedCount);
                    result.FailedPaths.Add(blobPath);
                    result.FailedDetails[blobPath] = "刪除失敗或 Blob 不存在";
                }
            });

            await Task.WhenAll(tasks);

            _logger.LogInformation(
                "📊 批次刪除完成 - 成功: {Success}, 失敗: {Failed}, 總計: {Total}",
                result.SuccessCount, result.FailedCount, result.TotalCount);

            return result;
        }

        #endregion

        #region 私有輔助方法

        /// <summary>
        /// 解析存取層級字串
        /// </summary>
        private AccessTier ParseAccessTier(string tierString)
        {
            return tierString.ToLowerInvariant() switch
            {
                "hot" => AccessTier.Hot,
                "cool" => AccessTier.Cool,
                "cold" => AccessTier.Cold,
                "archive" => AccessTier.Archive,
                _ => AccessTier.Hot // 預設為 Hot
            };
        }

        #endregion



    }
}
