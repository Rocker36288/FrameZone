using FrameZone_WebApi.DTOs;
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

        /// <summary>
        /// 從 JWT Token 取得使用者 Email
        /// </summary>
        private string GetCurrentUserEmail()
        {
            return User.FindFirst("email")?.Value ?? string.Empty;
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

                return Ok(new
                {
                    success = true,
                    message = "此 API 尚未實作，請先實作 IPhotoService.GetPhotosByUseIdAsync"
                });
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

        #endregion
    }
}
