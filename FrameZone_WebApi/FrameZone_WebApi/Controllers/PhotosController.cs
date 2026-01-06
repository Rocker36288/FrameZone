using FrameZone_WebApi.DTOs;
using FrameZone_WebApi.DTOs.AI;
using FrameZone_WebApi.Models;
using FrameZone_WebApi.Repositories;
using FrameZone_WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FrameZone_WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PhotosController : ControllerBase
    {
        private readonly IPhotoService _photoService;
        private readonly IPhotoRepository _photoRepository;
        private readonly ILogger<PhotosController> _logger;
        private readonly IBlobStorageService _blobStorageService;

        public PhotosController(
            IPhotoService photoService,
            IPhotoRepository photoRepository,
            ILogger<PhotosController> logger,
            IBlobStorageService blobStorageService)
        {
            _photoService = photoService;
            _photoRepository = photoRepository;
            _logger = logger;
            _blobStorageService = blobStorageService;
        }


        /// <summary>
        /// 從 JWT Token 的 Claims 中取得當前使用者的 UserId
        /// </summary>
        /// <returns>UserId</returns>
        private long GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            }

            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("無法從 JWT Token 取得 UserId");
                throw new UnauthorizedAccessException("無效的驗證 Token");
            }

            _logger.LogDebug($"從 JWT Token 取得 UserId: {userId}");
            return userId;
        }

        /// <summary>
        /// 從 JWT Token 取得使用者帳號
        /// </summary>
        private string GetCurrentUserAccount()
        {
            return User.FindFirst(ClaimTypes.Name)?.Value
                ?? User.FindFirst("account")?.Value
                ?? "未知使用者";
        }

        #region API 端點

        /// <summary>
        /// 測試 EXIF 解析功能
        /// </summary>
        /// <param name="file">照片檔案</param>
        /// <returns>EXIF 資訊</returns>
        /// 
        [AllowAnonymous]
        [HttpPost("test-exif")]
        [RequestSizeLimit(104_857_600)]
        public async Task<IActionResult> TestExif(IFormFile file)
        {
            try
            {
                var metadata = await _photoService.TestExifAsync(file);

                return Ok(new
                {
                    success = true,
                    metadata
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "測試 EXIF 時發生錯誤");
                return StatusCode(500, new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// 上傳單張照片
        /// </summary>
        /// <param name="file">照片檔案</param>
        /// <returns>上傳結果</returns>
        [HttpPost("upload")]
        [RequestSizeLimit(104_857_600)]
        public async Task<IActionResult> UploadPhoto(IFormFile file)
        {
            try
            {
                var userId = GetCurrentUserId();
                var account = GetCurrentUserAccount();

                _logger.LogInformation($"使用者 {account} (ID: {userId}) 正在上傳照片: {file.FileName}");

                // 呼叫 Service 處理上傳
                var result = await _photoService.UploadPhotoAsync(file, userId);

                // 根據結果返回對應的 HTTP 狀態碼
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "上傳照片時發生錯誤");
                return StatusCode(500, new PhotoUploadResponseDTO
                {
                    Success = false,
                    Message = $"伺服器錯誤: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 批次上傳照片
        /// </summary>
        [HttpPost("batch-upload")]
        [RequestSizeLimit(209_715_200)]
        public async Task<IActionResult> BatchUpload(List<IFormFile> files)
        {
            try
            {
                // 檢查是否有檔案
                if (files == null || files.Count == 0)
                {
                    return BadRequest(new BatchUploadResponseDTO
                    {
                        Success = false,
                        TotalFiles = 0,
                        SuccessCount = 0,
                        FailedCount = 0,
                        Results = new List<BatchUploadResultDTO>()
                    });
                }

                var userId = GetCurrentUserId();

                _logger.LogInformation($"使用者 {userId} 正在批次上傳 {files.Count} 張照片");


                var result = await _photoService.UploadPhotoBatchAsync(files, userId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批次上傳照片時發生錯誤");
                return StatusCode(500, new BatchUploadResponseDTO
                {
                    Success = false,
                    TotalFiles = files?.Count ?? 0,
                    SuccessCount = 0,
                    FailedCount = files?.Count ?? 0,
                    Results = new List<BatchUploadResultDTO>()
                });
            }
        }

        /// <summary>
        /// 檢查照片是否已上傳
        /// </summary>
        /// <param name="hash">檔案 Hash</param>
        /// <returns>是否存在</returns>
        [HttpGet("check-duplicate/{hash}")]
        public async Task<IActionResult> CheckDuplicateByHash(string hash)
        {
            try
            {
                var userId = GetCurrentUserId();

                var existingPhoto = await _photoRepository.GetPhotoByHashAsync(userId, hash);

                return Ok(new
                {
                    exists = existingPhoto != null,
                    photoId = existingPhoto?.PhotoId,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"檢查重複照片時發生錯誤，Hash: {hash}");
                return StatusCode(500, new
                {
                    exists = false,
                    error = ex.Message
                });
            }

        }

        /// <summary>
        /// 取得照片詳細資訊
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <returns>照片詳細資訊</returns>
        [HttpGet("{photoId:long}")]
        public async Task<IActionResult> GetPhotoById(long photoId)
        {
            try
            {
                var userId = GetCurrentUserId();

                var photoDetail = await _photoService.GetPhotoByIdAsync(photoId, userId);

                return Ok(new
                {
                    success = true,
                    data = photoDetail
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, $"無權限查看照片，PhotoId: {photoId}");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"查詢照片時發生錯誤，PhotoId: {photoId}");

                if (ex.Message.Contains("不存在"))
                {
                    return NotFound(new
                    {
                        success = false,
                        error = "照片不存在"
                    });
                }
                return StatusCode(500, new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// 刪除照片 (軟刪除)
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <returns>刪除結果</returns>
        [HttpDelete("{photoId:long}")]
        public async Task<IActionResult> DeletePhoto(long photoId)
        {
            try
            {
                var userId = GetCurrentUserId();

                var result = await _photoService.DeletePhotoAsync(photoId, userId);

                if (result)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "照片已刪除"
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        error = "刪除失敗"
                    });
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, $"無權限刪除照片，PhotoId: {photoId}");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"刪除照片時發生錯誤，PhotoId: {photoId}");

                if (ex.Message.Contains("不存在"))
                {
                    return NotFound(new
                    {
                        success = false,
                        error = "照片不存在"
                    });
                }

                return StatusCode(500, new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        // ===== 縮圖 URL 統一處理 =====
        private static string BuildThumbnailUrl(long photoId, int width = 300, int height = 200)
            => $"/api/photos/{photoId}/thumbnail?width={width}&height={height}";

        private static void EnsurePhotoThumbnailUrls(IList<PhotoItemDTO>? photos, int width = 300, int height = 200)
        {
            if (photos == null) return;

            foreach (var p in photos)
            {
                if (string.IsNullOrWhiteSpace(p.ThumbnailUrl))
                    p.ThumbnailUrl = BuildThumbnailUrl(p.PhotoId, width, height);
            }
        }

        /// <summary>
        /// 取得使用者的照片列表
        /// </summary>
        /// <param name="pageIndex">頁碼</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <returns>照片列表</returns>
        [HttpGet("list")]
        public async Task<IActionResult> GetPhotosList(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                // 驗證分頁參數
                if (pageIndex < 1) pageIndex = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                var userId = GetCurrentUserId();
                var account = GetCurrentUserAccount();

                _logger.LogInformation(
                    "使用者 {Account} (ID: {UserId}) 請求照片列表，頁碼: {PageIndex}, 每頁: {PageSize}",
                    account, userId, pageIndex, pageSize);

                // 使用 QueryPhotosAsync，只傳入基本分頁參數
                var queryRequest = new PhotoQueryRequestDTO
                {
                    PageNumber = pageIndex,
                    PageSize = pageSize,
                    SortBy = "UploadedAt",  // 預設依上傳時間排序
                    SortOrder = "desc"       // 新到舊
                };

                var result = await _photoService.QueryPhotosAsync(queryRequest, userId);

                if (!result.Success)
                {
                    _logger.LogWarning("照片列表查詢失敗: {Message}", result.Message);
                    return BadRequest(new { success = false, error = result.Message });
                }

                EnsurePhotoThumbnailUrls(result.Photos);

                var response = new
                {
                    success = true,
                    data = result.Photos.Select(p => new
                    {
                        photoId = p.PhotoId,
                        fileName = p.FileName,
                        thumbnailUrl = p.ThumbnailUrl,
                        uploadedAt = p.UploadedAt,
                        dateTaken = p.DateTaken,
                        tags = p.Tags ?? new List<string>()
                    }).ToList(),
                    totalCount = result.TotalCount,
                    pageIndex = result.PageNumber,
                    pageSize = result.PageSize
                };

                _logger.LogInformation(
                    "照片列表查詢成功，共 {TotalCount} 張照片，本頁 {CurrentCount} 張",
                    result.TotalCount, result.Photos.Count);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查詢照片列表時發生錯誤");

                return StatusCode(500, new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// 查詢照片（支援多條件篩選、分頁、排序）
        /// </summary>
        [HttpPost("query")]
        [ProducesResponseType(typeof(PhotoQueryResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PhotoQueryResponseDTO), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> QueryPhotos([FromBody] PhotoQueryRequestDTO request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var account = GetCurrentUserAccount();

                _logger.LogInformation(
                    "使用者 {Account} (ID: {UserId}) 開始查詢照片，頁碼: {PageNumber}, 每頁: {PageSize}",
                    account, userId, request.PageNumber, request.PageSize);

                if (request.TagIds != null && request.TagIds.Any())
                {
                    _logger.LogInformation("🏷️ 標籤篩選: {TagIds}", string.Join(", ", request.TagIds));
                }

                var result = await _photoService.QueryPhotosAsync(request, userId);

                if (!result.Success)
                {
                    _logger.LogWarning("查詢失敗: {Message}", result.Message);
                    return BadRequest(result);
                }

                EnsurePhotoThumbnailUrls(result.Photos);

                _logger.LogInformation(
                    "查詢成功，共 {TotalCount} 張照片，執行時間: {ExecutionTime}ms",
                    result.TotalCount, result.ExecutionTimeMs);

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "未授權的查詢請求");
                return Unauthorized(new PhotoQueryResponseDTO { Success = false, Message = "請先登入" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查詢照片時發生錯誤");
                return StatusCode(500, new PhotoQueryResponseDTO { Success = false, Message = $"伺服器錯誤: {ex.Message}" });
            }
        }

        /// <summary>
        /// 取得照片縮圖
        /// </summary>
        [HttpGet("{photoId}/thumbnail")]
        public async Task<IActionResult> GetThumbnail(long photoId)
        {
            try
            {
                var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                _logger.LogInformation("使用者 {UserId} 請求照片 {PhotoId} 的縮圖", userId, photoId);

                // 取得照片基本資訊
                var photo = await _photoRepository.GetPhotoByIdAsync(photoId);
                if (photo == null)
                {
                    _logger.LogWarning("照片不存在，PhotoId: {PhotoId}", photoId);
                    return NotFound(new { message = "照片不存在" });
                }

                // 檢查權限
                if (photo.UserId != userId)
                {
                    _logger.LogWarning("無權限存取照片，PhotoId: {PhotoId}, UserId: {UserId}", photoId, userId);
                    return Forbid();
                }

                // 從 PhotoStorage 取得縮圖路徑
                var storages = await _photoRepository.GetAllStoragesByPhotoIdAsync(photoId);
                var thumbnailStorage = storages.FirstOrDefault(s => !s.IsPrimary); // 縮圖的 IsPrimary = false

                if (thumbnailStorage == null)
                {
                    _logger.LogWarning("找不到縮圖儲存記錄，PhotoId: {PhotoId}", photoId);
                    return NotFound(new { message = "縮圖不存在" });
                }

                // 生成 SAS Token URL（帶時效性）
                var thumbnailUrl = await _blobStorageService.GetThumbnailUrlAsync(
                    thumbnailStorage.StoragePath,
                    useSasToken: true);

                _logger.LogInformation("✅ 縮圖 URL 生成成功，PhotoId: {PhotoId}", photoId);

                // 重定向到 Blob Storage URL
                return Redirect(thumbnailUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得縮圖時發生錯誤，PhotoId: {PhotoId}", photoId);
                return StatusCode(500, new { message = "取得縮圖失敗" });
            }
        }

        /// <summary>
        /// 取得照片原圖（從 Blob Storage）
        /// </summary>
        [HttpGet("{photoId}/photo")]
        public async Task<IActionResult> GetPhoto(long photoId)
        {
            try
            {
                var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                _logger.LogInformation("使用者 {UserId} 請求照片 {PhotoId} 的原圖", userId, photoId);

                // 1️⃣ 檢查照片是否存在並驗證權限
                var ownerUserId = await _photoRepository.GetPhotoOwnerUserIdAsync(photoId);

                if (ownerUserId == null)
                {
                    _logger.LogWarning("照片不存在，PhotoId: {PhotoId}", photoId);
                    return NotFound(new { message = "照片不存在" });
                }

                if (ownerUserId != userId)
                {
                    _logger.LogWarning("無權限存取照片，PhotoId: {PhotoId}, UserId: {UserId}", photoId, userId);
                    return Forbid();
                }

                // 2️⃣ 從 PhotoStorage 取得原圖路徑
                var primaryStorage = await _photoRepository.GetPrimaryStorageByPhotoIdAsync(photoId);

                if (primaryStorage == null)
                {
                    _logger.LogWarning("找不到原圖儲存記錄，PhotoId: {PhotoId}", photoId);
                    return NotFound(new { message = "原圖不存在" });
                }

                // 3️⃣ 生成 SAS Token URL（帶時效性，60分鐘有效）
                var photoUrl = await _blobStorageService.GetPhotoUrlAsync(
                    primaryStorage.StoragePath,
                    useSasToken: true);

                _logger.LogInformation("✅ 原圖 URL 生成成功，PhotoId: {PhotoId}", photoId);

                // 4️⃣ 重定向到 Blob Storage URL
                return Redirect(photoUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 取得原圖時發生錯誤，PhotoId: {PhotoId}", photoId);
                return StatusCode(500, new { message = "取得原圖失敗" });
            }
        }

        /// <summary>
        /// 根據副檔名取得 Content-Type
        /// </summary>
        private string GetContentType(string fileExtension)
        {
            return fileExtension.ToLower() switch
            {
                "jpg" or "jpeg" => "image/jpeg",
                "png" => "image/png",
                "gif" => "image/gif",
                "bmp" => "image/bmp",
                "webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }

        /// <summary>
        /// 取得標籤階層（用於 Sidebar）
        /// </summary>
        /// <returns>標籤階層回應</returns>
        [HttpGet("tags/hierarchy")]
        [ProducesResponseType(typeof(TagHierarchyResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTagHierarchy()
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("🏷️ API: 取得標籤階層，UserId: {UserId}", userId);

                var result = await _photoService.GetTagHierarchyAsync(userId);

                if (!result.Success)
                {
                    _logger.LogWarning("⚠️ 取得標籤階層失敗: {Message}", result.Message);
                    return StatusCode(500, result);
                }

                _logger.LogInformation("✅ 標籤階層取得成功，分類數量: {Count}", result.Categories.Count);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("⚠️ 未授權訪問: {Message}", ex.Message);
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 取得標籤階層時發生錯誤");
                return StatusCode(500, new
                {
                    success = false,
                    message = "取得標籤階層失敗",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// 建立自訂標籤
        /// </summary>
        /// <param name="request">建立自訂標籤請求</param>
        /// <returns>建立結果</returns>
        [HttpPost("tags/custom")]
        [ProducesResponseType(typeof(CreateCustomTagResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCustomTag([FromBody] CreateCustomTagRequestDTO request)
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("🏷️ API: 建立自訂標籤，UserId: {UserId}, TagName: {TagName}",
                    userId, request.TagName);

                // 驗證請求
                if (string.IsNullOrWhiteSpace(request.TagName))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "標籤名稱不能為空"
                    });
                }

                var result = await _photoService.CreateCustomTagAsync(request, userId);

                if (!result.Success)
                {
                    _logger.LogWarning("⚠️ 建立自訂標籤失敗: {Message}", result.Message);
                    return BadRequest(result);
                }

                _logger.LogInformation("✅ 自訂標籤建立成功，TagId: {TagId}", result.Tag.TagId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("⚠️ 未授權訪問: {Message}", ex.Message);
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 建立自訂標籤時發生錯誤");
                return StatusCode(500, new
                {
                    success = false,
                    message = "建立標籤失敗",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// 批次添加標籤到多張照片
        /// </summary>
        /// <param name="request">批次添加標籤請求</param>
        /// <returns>批次添加結果</returns>
        [HttpPost("tags/batch-add")]
        [ProducesResponseType(typeof(BatchAddTagsResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> BatchAddTags([FromBody] BatchAddTagsRequestDTO request)
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation(
                    "🏷️ API: 批次添加標籤，UserId: {UserId}, 照片數: {PhotoCount}, 現有標籤: {ExistingTagCount}, 新標籤: {NewTagCount}",
                    userId,
                    request.PhotoIds?.Count ?? 0,
                    request.ExistingTagIds?.Count ?? 0,
                    request.NewTags?.Count ?? 0);

                // 驗證請求
                if (request.PhotoIds == null || !request.PhotoIds.Any())
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "照片 ID 列表不能為空"
                    });
                }

                if ((request.ExistingTagIds == null || !request.ExistingTagIds.Any()) &&
                    (request.NewTags == null || !request.NewTags.Any()))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "必須提供至少一個現有標籤或新標籤"
                    });
                }

                var result = await _photoService.BatchAddTagsToPhotosAsync(request, userId);

                if (!result.Success)
                {
                    _logger.LogWarning("⚠️ 批次添加標籤失敗: {Message}", result.Message);
                    return BadRequest(result);
                }

                _logger.LogInformation(
                    "✅ 批次添加標籤成功，總照片: {TotalPhotos}, 成功: {SuccessCount}, 失敗: {FailedCount}, 新建標籤: {CreatedTagCount}",
                    result.TotalPhotos,
                    result.SuccessCount,
                    result.FailedCount,
                    result.CreatedTags?.Count ?? 0);

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("⚠️ 未授權訪問: {Message}", ex.Message);
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 批次添加標籤時發生錯誤");
                return StatusCode(500, new
                {
                    success = false,
                    message = "批次添加標籤失敗",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// 搜尋標籤
        /// </summary>
        /// <param name="request">搜尋標籤請求</param>
        /// <returns>搜尋結果</returns>
        [HttpGet("tags/search")]
        [ProducesResponseType(typeof(SearchTagsResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchTags([FromQuery] SearchTagsRequestDTO request)
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation(
                    "🏷️ API: 搜尋標籤，UserId: {UserId}, 關鍵字: {Keyword}, 限制: {Limit}",
                    userId,
                    request.Keyword ?? "(無)",
                    request.Limit);

                var result = await _photoService.SearchTagsAsync(request, userId);

                if (!result.Success)
                {
                    _logger.LogWarning("⚠️ 搜尋標籤失敗: {Message}", result.Message);
                    return BadRequest(result);
                }

                _logger.LogInformation(
                    "✅ 搜尋標籤成功，找到 {TotalCount} 個標籤，回傳 {ReturnCount} 個",
                    result.TotalCount,
                    result.Tags?.Count ?? 0);

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("⚠️ 未授權訪問: {Message}", ex.Message);
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 搜尋標籤時發生錯誤");
                return StatusCode(500, new
                {
                    success = false,
                    message = "搜尋標籤失敗",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// 獲取可用分類列表
        /// </summary>
        /// <returns>系統分類和用戶自定義分類列表</returns>
        [HttpGet("categories/available")]
        [ProducesResponseType(typeof(AvailableCategoriesResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAvailableCategories()
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation(
                    "🏷️ API: 獲取可用分類列表，UserId: {UserId}",
                    userId);

                var result = await _photoService.GetAvailableCategoriesAsync(userId);

                if (!result.Success)
                {
                    _logger.LogWarning("⚠️ 獲取可用分類失敗: {Message}", result.Message);
                    return BadRequest(result);
                }

                _logger.LogInformation(
                    "✅ 成功獲取可用分類，系統分類: {SystemCount}, 用戶分類: {UserCount}",
                    result.SystemCategories?.Count ?? 0,
                    result.UserCategories?.Count ?? 0);

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("⚠️ 未授權訪問: {Message}", ex.Message);
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 獲取可用分類時發生錯誤");
                return StatusCode(500, new
                {
                    success = false,
                    message = "獲取可用分類失敗",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// 獲取單張照片的所有標籤
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <returns>照片的標籤詳細資訊</returns>
        [HttpGet("{photoId:long}/tags")]
        [ProducesResponseType(typeof(PhotoTagsDetailDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPhotoTags(long photoId)
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation(
                    "🏷️ API: 獲取照片標籤，UserId: {UserId}, PhotoId: {PhotoId}",
                    userId,
                    photoId);

                var result = await _photoService.GetPhotoTagsAsync(photoId, userId);

                _logger.LogInformation(
                    "✅ 成功獲取照片標籤，PhotoId: {PhotoId}, 總標籤數: {TotalTags}",
                    photoId,
                    result.AllTags?.Count ?? 0);

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("⚠️ 未授權訪問: {Message}", ex.Message);
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("⚠️ 照片不存在: {Message}", ex.Message);
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 獲取照片標籤時發生錯誤");
                return StatusCode(500, new
                {
                    success = false,
                    message = "獲取照片標籤失敗",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// 從單張照片移除指定標籤
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <param name="tagId">標籤 ID</param>
        /// <returns>移除結果</returns>
        [HttpDelete("{photoId:long}/tags/{tagId:int}")]
        [ProducesResponseType(typeof(RemoveTagResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemovePhotoTag(long photoId, int tagId)
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation(
                    "🏷️ API: 移除照片標籤，UserId: {UserId}, PhotoId: {PhotoId}, TagId: {TagId}",
                    userId,
                    photoId,
                    tagId);

                var result = await _photoService.RemoveTagFromPhotoAsync(photoId, tagId, userId);

                if (!result.Success)
                {
                    _logger.LogWarning("⚠️ 移除照片標籤失敗: {Message}", result.Message);

                    // 根據錯誤訊息判斷返回的狀態碼
                    if (result.Message.Contains("不存在"))
                    {
                        return NotFound(result);
                    }
                    else if (result.Message.Contains("不屬於") || result.Message.Contains("無權限"))
                    {
                        return Forbid();
                    }

                    return BadRequest(result);
                }

                _logger.LogInformation("✅ 成功移除照片標籤，PhotoId: {PhotoId}, TagId: {TagId}", photoId, tagId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("⚠️ 未授權訪問: {Message}", ex.Message);
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 移除照片標籤時發生錯誤");
                return StatusCode(500, new
                {
                    success = false,
                    message = "移除標籤失敗",
                    error = ex.Message
                });
            }
        }

        #endregion

        #region AI 功能

        // ==================== AI 照片分析 ====================

        /// <summary>
        /// 分析單張照片
        /// </summary>
        /// <param name="request">分析請求參數</param>
        /// <returns>完整的 AI 分析結果</returns>
        /// <remarks>
        /// 執行照片的完整 AI 分析，包含三個階段：
        /// 1. Azure Vision 物件識別和場景分析
        /// 2. Google Places 景點識別（如果有 GPS）
        /// 3. Claude 語義整合和標籤建議
        /// </remarks>
        [HttpPost("ai/analyze")]
        public async Task<IActionResult> AnalyzePhoto([FromBody] PhotoAIAnalysisRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                request.UserId = userId;

                _logger.LogInformation("📸 收到照片 AI 分析請求 PhotoId={PhotoId}, UserId={UserId}",
                    request.PhotoId, userId);

                var result = await _photoService.AnalyzePhotoWithAIAsync(request);

                if (result.Status == "Success")
                {
                    return Ok(result);
                }
                else if (result.ErrorMessage?.Contains("不存在") == true)
                {
                    return NotFound(result);
                }
                else if (result.ErrorMessage?.Contains("無權限") == true)
                {
                    return StatusCode(403, result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 照片 AI 分析發生錯誤 PhotoId={PhotoId}", request.PhotoId);
                return StatusCode(500, new { success = false, message = "系統錯誤", error = ex.Message });
            }
        }

        /// <summary>
        /// 批次分析多張照片
        /// </summary>
        /// <param name="request">批次分析請求</param>
        /// <returns>批次分析結果</returns>
        /// <remarks>
        /// 一次分析多張照片，支援兩種模式：
        /// - 同步模式：等待所有照片分析完成後返回（適合少量照片，1-10 張）
        /// - 非同步模式：立即返回任務 ID，背景執行（適合大量照片，>10 張）
        /// </remarks>
        [HttpPost("ai/batch-analyze")]
        public async Task<IActionResult> BatchAnalyzePhotos([FromBody] BatchPhotoAIAnalysisRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                request.UserId = userId;

                _logger.LogInformation("📦 收到批次 AI 分析請求 PhotoCount={Count}, UserId={UserId}",
                    request.PhotoIds.Count, userId);

                var result = await _photoService.BatchAnalyzePhotosAsync(request);

                if (result.Errors.Any())
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 批次 AI 分析發生錯誤");
                return StatusCode(500, new { success = false, message = "系統錯誤", error = ex.Message });
            }
        }

        // ==================== 查詢 AI 分析結果 ====================

        /// <summary>
        /// 取得照片的 AI 分析狀態（輕量級查詢）
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <returns>分析狀態摘要</returns>
        /// <remarks>
        /// 快速查詢照片是否已分析過，以及 AI 建議的摘要資訊。
        /// 這是一個輕量級的查詢，不會返回完整的分析結果。
        /// </remarks>
        [HttpGet("{photoId}/ai/status")]
        public async Task<IActionResult> GetPhotoAIStatus(long photoId)
        {
            try
            {
                _logger.LogInformation("🔍 查詢照片 AI 狀態 PhotoId={PhotoId}", photoId);

                var result = await _photoService.GetPhotoAIStatusAsync(photoId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 查詢 AI 狀態失敗 PhotoId={PhotoId}", photoId);
                return StatusCode(500, new { success = false, message = "系統錯誤", error = ex.Message });
            }
        }

        /// <summary>
        /// 取得照片的完整 AI 分析結果
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <returns>完整的分析結果</returns>
        /// <remarks>
        /// 返回照片的完整 AI 分析結果，包含：
        /// - Azure Vision 分析摘要
        /// - Google Places 景點資訊
        /// - Claude 語義分析結果
        /// - 所有 AI 標籤建議（包含已採用和待處理）
        /// </remarks>
        [HttpGet("{photoId}/ai/analysis")]
        public async Task<IActionResult> GetPhotoAIAnalysis(long photoId)
        {
            try
            {
                _logger.LogInformation("🔍 查詢照片完整 AI 分析 PhotoId={PhotoId}", photoId);

                var result = await _photoService.GetPhotoAIAnalysisAsync(photoId);

                if (result == null)
                {
                    return NotFound(new { success = false, message = "照片沒有 AI 分析記錄" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 查詢 AI 分析失敗 PhotoId={PhotoId}", photoId);
                return StatusCode(500, new { success = false, message = "系統錯誤", error = ex.Message });
            }
        }

        /// <summary>
        /// 取得照片的待處理 AI 建議
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <param name="minConfidence">最低信心分數過濾（可選）</param>
        /// <returns>待處理的標籤建議列表</returns>
        /// <remarks>
        /// 返回照片的所有待處理 AI 建議（尚未被使用者採用的標籤）。
        /// 可以使用 minConfidence 參數過濾低信心分數的建議。
        /// </remarks>
        [HttpGet("{photoId}/ai/suggestions")]
        public async Task<IActionResult> GetPendingAISuggestions(
            long photoId,
            [FromQuery] double? minConfidence = null)
        {
            try
            {
                _logger.LogInformation("💡 查詢待處理 AI 建議 PhotoId={PhotoId}, MinConfidence={MinConfidence}",
                    photoId, minConfidence);

                var result = await _photoService.GetPendingAISuggestionsAsync(photoId, minConfidence);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 查詢待處理建議失敗 PhotoId={PhotoId}", photoId);
                return StatusCode(500, new { success = false, message = "系統錯誤", error = ex.Message });
            }
        }

        // ==================== 套用 AI 建議 ====================

        /// <summary>
        /// 套用 AI 標籤建議到照片
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <param name="request">套用請求（包含要套用的建議 ID）</param>
        /// <returns>套用結果</returns>
        /// <remarks>
        /// 將 AI 建議的標籤實際套用到照片上。使用者可以選擇：
        /// - 套用所有建議（不指定 suggestionIds）
        /// - 套用特定建議（指定 suggestionIds）
        /// - 按信心分數過濾（設定 minConfidence）
        /// </remarks>
        [HttpPost("{photoId}/ai/apply-tags")]
        public async Task<IActionResult> ApplyAITags(
            long photoId,
            [FromBody] ApplyAITagsRequestDto request)
        {
            try
            {
                request.PhotoId = photoId;

                _logger.LogInformation("✏️ 套用 AI 標籤 PhotoId={PhotoId}, SuggestionCount={Count}",
                    photoId, request.SuggestionIds.Count);

                var result = await _photoService.ApplyAITagsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 套用 AI 標籤失敗 PhotoId={PhotoId}", photoId);
                return StatusCode(500, new { success = false, message = "系統錯誤", error = ex.Message });
            }
        }

        // ==================== AI 使用統計 ====================

        /// <summary>
        /// 取得使用者的 AI 使用統計
        /// </summary>
        /// <returns>AI 使用統計資訊</returns>
        /// <remarks>
        /// 返回目前登入使用者的 AI 功能使用統計，包含：
        /// - 總分析次數
        /// - 成功/失敗次數
        /// - 使用的配額
        /// - 平均處理時間
        /// </remarks>
        [HttpGet("ai/stats")]
        public async Task<IActionResult> GetUserAIStats()
        {
            try
            {
                var userId = GetCurrentUserId();

                _logger.LogInformation("📊 查詢使用者 AI 統計 UserId={UserId}", userId);

                var result = await _photoService.GetUserAIStatsAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 查詢使用者 AI 統計失敗");
                return StatusCode(500, new { success = false, message = "系統錯誤", error = ex.Message });
            }
        }

        #endregion

        /// <summary>
        /// 測試 Azure Blob Storage 連線
        /// </summary>
        [AllowAnonymous]
        [HttpGet("test-blob-connection")]
        public async Task<IActionResult> TestBlobConnection()
        {
            try
            {
                _logger.LogInformation("🧪 開始測試 Blob Storage 連線...");

                // 測試容器是否存在
                var result = await _blobStorageService.EnsureContainersExistAsync();

                if (result)
                {
                    _logger.LogInformation("✅ Blob Storage 連線測試成功");
                    return Ok(new
                    {
                        success = true,
                        message = "Azure Blob Storage 連線成功",
                        timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    _logger.LogError("❌ Blob Storage 連線測試失敗");
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "容器初始化失敗"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Blob Storage 連線測試發生錯誤");
                return StatusCode(500, new
                {
                    success = false,
                    message = $"連線測試失敗: {ex.Message}",
                    detail = ex.ToString()
                });
            }
        }

        /// <summary>
        /// 測試 Blob Storage 上傳功能（使用測試檔案）
        /// </summary>
        [AllowAnonymous]
        [HttpPost("test-blob-upload")]
        public async Task<IActionResult> TestBlobUpload()
        {
            try
            {
                _logger.LogInformation("🧪 開始測試 Blob 上傳功能...");

                // 建立一個測試用的小圖片（1x1 透明 PNG）
                byte[] testImageBytes = Convert.FromBase64String(
                    "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==");

                var testFileName = $"test_{DateTime.UtcNow:yyyyMMddHHmmss}.png";
                var testUserId = 9999; // 測試用的 UserId
                var testPhotoId = DateTime.UtcNow.Ticks; // 使用時間戳作為測試 PhotoId

                // 上傳到 Blob Storage
                string blobUrl;
                using (var stream = new MemoryStream(testImageBytes))
                {
                    blobUrl = await _blobStorageService.UploadPhotoAsync(
                        stream,
                        testFileName,
                        testUserId,
                        testPhotoId,
                        DateTime.UtcNow,
                        "image/png");
                }

                _logger.LogInformation("✅ 測試檔案上傳成功: {BlobUrl}", blobUrl);

                // 生成 SAS Token URL
                var blobPath = _blobStorageService.GeneratePhotoBlobPath(
                    testUserId,
                    testPhotoId,
                    DateTime.UtcNow,
                    "png");

                var sasUrl = await _blobStorageService.GetPhotoUrlAsync(blobPath, useSasToken: true);

                return Ok(new
                {
                    success = true,
                    message = "Blob 上傳測試成功",
                    blobUrl = blobUrl,
                    sasUrl = sasUrl,
                    blobPath = blobPath,
                    testInfo = new
                    {
                        userId = testUserId,
                        photoId = testPhotoId,
                        fileName = testFileName
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Blob 上傳測試失敗");
                return StatusCode(500, new
                {
                    success = false,
                    message = $"上傳測試失敗: {ex.Message}",
                    detail = ex.ToString()
                });
            }
        }
    }
}