using FrameZone_WebApi.DTOs;
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

        public PhotosController(
            IPhotoService photoService,
            IPhotoRepository photoRepository,
            ILogger<PhotosController> logger)
        {
            _photoService = photoService;
            _photoRepository = photoRepository;
            _logger = logger;
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
        [Authorize]
        [ResponseCache(Duration = 86400)] // 快取 24 小時
        public async Task<IActionResult> GetThumbnail(
            long photoId,
            [FromQuery] int width = 300,
            [FromQuery] int height = 200,
            [FromQuery] string token = null)
        {
            try
            {
                var userId = GetCurrentUserId();

                // 驗證尺寸參數
                if (width < 50 || width > 1000) width = 200;
                if (height < 50 || height > 1000) height = 150;

                _logger.LogInformation(
                    "使用者 {UserId} 請求照片 {PhotoId} 的縮圖",
                    userId, photoId);

                // 取得照片
                var thumbnailData = await _photoRepository.GetThumbnailDataAsync(photoId);

                if (thumbnailData == null)
                {
                    _logger.LogWarning("照片不存在，PhotoId: {PhotoId}", photoId);
                    return NotFound();
                }

                // 檢查權限
                if (thumbnailData.UserId != userId)
                {
                    _logger.LogWarning("無權限存取照片，PhotoId: {PhotoId}, UserId: {UserId}", photoId, userId);
                    return Forbid();
                }

                // 優先使用預生成的縮圖
                if (thumbnailData.ThumbnailData != null && thumbnailData.ThumbnailData.Length > 0)
                {
                    _logger.LogDebug("使用預生成的縮圖，PhotoId: {PhotoId}", photoId);
                    return File(thumbnailData.ThumbnailData, "image/jpeg");
                }

                // 縮圖不存在：即時生成
                _logger.LogWarning("縮圖不存在，即時生成，PhotoId: {PhotoId}", photoId);

                // 只在需要時才載入原圖
                var photoData = await _photoRepository.GetPhotoDataAsync(photoId);

                if (photoData == null || photoData.PhotoData == null || photoData.PhotoData.Length == 0)
                {
                    _logger.LogError("原圖資料不存在，PhotoId: {PhotoId}", photoId);
                    return NotFound();
                }

                // 生成縮圖
                var thumbnail = await _photoService.GenerateThumbnailAsync(
                    photoData.PhotoData, width, height);

                // 將生成的縮圖存回資料庫，避免下次再生成
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _photoRepository.UpdateThumbnailAsync(photoId, thumbnail);
                        _logger.LogInformation("縮圖已補存，PhotoId: {PhotoId}", photoId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "補存縮圖時發生錯誤，PhotoId: {PhotoId}", photoId);
                    }
                });

                // 回傳圖片
                var contentType = GetContentType(thumbnailData.FileExtension);
                return File(thumbnail, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得縮圖時發生錯誤，PhotoId: {PhotoId}", photoId);
                return StatusCode(500);
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

        #endregion
    }
}
