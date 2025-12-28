using FrameZone_WebApi.DTOs;
using FrameZone_WebApi.DTOs.Member;
using FrameZone_WebApi.Helpers;
using FrameZone_WebApi.Models;
using FrameZone_WebApi.Repositories;
using FrameZone_WebApi.Repositories.Member;
using FrameZone_WebApi.Services;
using Microsoft.AspNetCore.Http;

namespace FrameZone_WebApi.Services.Member
{
    /// <summary>
    /// 會員個人資料服務實作
    /// </summary>
    public class MemberProfileService : IMemberProfileService
    {
        private readonly IMemberProfileRepository _repository;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IUserLogRepository _userLogRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<MemberProfileService> _logger;

        public MemberProfileService(
            IMemberProfileRepository repository,
            IBlobStorageService blobStorageService,
            IUserLogRepository userLogRepository,
            IHttpContextAccessor httpContextAccessor,
            ILogger<MemberProfileService> logger)
        {
            _repository = repository;
            _blobStorageService = blobStorageService;
            _userLogRepository = userLogRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        #region 取得個人資料

        /// <summary>
        /// 取得使用者的個人資料（不含圖片檔案）
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>個人資料 Response DTO</returns>
        public async Task<GetProfileResponseDto> GetProfileAsync(long userId)
        {
            try
            {
                // 從資料庫取得使用者完整資料
                var user = await _repository.GetUserWithProfileAsync(userId);

                if (user == null)
                {
                    return new GetProfileResponseDto
                    {
                        Success = false,
                        Message = "找不到使用者資料"
                    };
                }

                // 轉換為 DTO 格式
                var profileDto = UserProfileDto.FromEntity(
                    user,
                    user.UserProfile,
                    user.UserPrivateInfo
                );

                if (!string.IsNullOrWhiteSpace(profileDto.Avatar))
                {
                    profileDto.Avatar = await GenerateSasUrlForImage(profileDto.Avatar, "avatars");
                }

                if (!string.IsNullOrWhiteSpace(profileDto.CoverImage))
                {
                    profileDto.CoverImage = await GenerateSasUrlForImage(profileDto.CoverImage, "covers");
                }

                return new GetProfileResponseDto
                {
                    Success = true,
                    Message = "取得個人資料成功",
                    Data = profileDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得個人資料時發生錯誤，UserId: {UserId}", userId);

                return new GetProfileResponseDto
                {
                    Success = false,
                    Message = "取得個人資料失敗，請稍後再試"
                };
            }
        }

        #endregion

        #region 更新個人資料

        /// <summary>
        /// 更新使用者的個人資料
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="dto">更新資料 DTO</param>
        /// <returns>更新結果 Response DTO</returns>
        public async Task<UpdateProfileResponseDto> UpdateProfileAsync(
            long userId,
            UpdateUserProfileDto dto)
        {
            try
            {
                // ========== 1. 業務邏輯驗證 ==========
                var validationErrors = ValidateUpdateProfileDto(dto);
                if (validationErrors.Any())
                {
                    return new UpdateProfileResponseDto
                    {
                        Success = false,
                        Message = "資料驗證失敗",
                        Errors = validationErrors
                    };
                }

                // ========== 2. 開始資料庫交易 ==========
                await _repository.BeginTransactionAsync();

                try
                {
                    // ========== 3. 取得使用者資料 ==========
                    var user = await _repository.GetUserWithProfileAsync(userId);
                    if (user == null)
                    {
                        await _repository.RollbackTransactionAsync();
                        return new UpdateProfileResponseDto
                        {
                            Success = false,
                            Message = "找不到使用者資料"
                        };
                    }

                    var userProfile = await _repository.GetOrCreateUserProfileAsync(userId);
                    var userPrivateInfo = await _repository.GetOrCreateUserPrivateInfoAsync(userId);

                    // 紀錄舊的圖片 URL（用於稍後刪除）
                    string? oldAvatarUrl = userProfile.Avatar;
                    string? oldCoverImageUrl = userProfile.CoverImage;

                    // ========== 4. 更新 User 基本資訊 ==========
                    if (!string.IsNullOrWhiteSpace(dto.Phone))
                    {
                        user.Phone = dto.Phone;
                        await _repository.UpdateUserAsync(user);
                    }

                    // ========== 5. 更新 UserProfile 公開資訊 ==========
                    if (!string.IsNullOrWhiteSpace(dto.DisplayName))
                        userProfile.DisplayName = dto.DisplayName;

                    if (dto.Bio != null)
                        userProfile.Bio = string.IsNullOrWhiteSpace(dto.Bio) ? null : dto.Bio;

                    if (dto.Website != null)
                        userProfile.Website = string.IsNullOrWhiteSpace(dto.Website) ? null : dto.Website;

                    if (dto.Location != null)
                        userProfile.Location = string.IsNullOrWhiteSpace(dto.Location) ? null : dto.Location;

                    // ========== 6. 處理頭像上傳 ==========
                    if (dto.AvatarFile != null)
                    {
                        var avatarUploadResult = await UploadAvatarAsync(userId, dto.AvatarFile);
                        if (avatarUploadResult.Success)
                        {
                            userProfile.Avatar = avatarUploadResult.BlobUrl;
                        }
                        else
                        {
                            await _repository.RollbackTransactionAsync();
                            return new UpdateProfileResponseDto
                            {
                                Success = false,
                                Message = avatarUploadResult.ErrorMessage ?? "頭像上傳失敗"
                            };
                        }
                    }

                    // ========== 7. 處理頭像移除 ==========
                    if (dto.RemoveAvatar && !string.IsNullOrWhiteSpace(userProfile.Avatar))
                    {
                        userProfile.Avatar = null;
                    }

                    // ========== 8. 處理封面圖片上傳 ==========
                    if (dto.CoverImageFile != null)
                    {
                        var coverUploadResult = await UploadCoverImageAsync(userId, dto.CoverImageFile);
                        if (coverUploadResult.Success)
                        {
                            userProfile.CoverImage = coverUploadResult.BlobUrl;
                        }
                        else
                        {
                            await _repository.RollbackTransactionAsync();
                            return new UpdateProfileResponseDto
                            {
                                Success = false,
                                Message = coverUploadResult.ErrorMessage ?? "封面圖片上傳失敗"
                            };
                        }
                    }

                    // ========== 9. 處理封面圖片移除 ==========
                    if (dto.RemoveCoverImage && !string.IsNullOrWhiteSpace(userProfile.CoverImage))
                    {
                        userProfile.CoverImage = null;
                    }

                    await _repository.UpdateUserProfileAsync(userProfile);

                    // ========== 10. 更新 UserPrivateInfo 私密資訊 ==========
                    if (!string.IsNullOrWhiteSpace(dto.RealName))
                        userPrivateInfo.RealName = dto.RealName;

                    if (!string.IsNullOrWhiteSpace(dto.Gender))
                        userPrivateInfo.Gender = dto.Gender;

                    if (dto.BirthDate.HasValue)
                        userPrivateInfo.BirthDate = dto.BirthDate.Value;

                    if (!string.IsNullOrWhiteSpace(dto.Country))
                        userPrivateInfo.Country = dto.Country;

                    if (!string.IsNullOrWhiteSpace(dto.City))
                        userPrivateInfo.City = dto.City;

                    if (!string.IsNullOrWhiteSpace(dto.PostalCode))
                        userPrivateInfo.PostalCode = dto.PostalCode;

                    if (!string.IsNullOrWhiteSpace(dto.FullAddress))
                        userPrivateInfo.FullAddress = dto.FullAddress;

                    await _repository.UpdateUserPrivateInfoAsync(userPrivateInfo);

                    // ========== 11. 儲存變更 ==========
                    await _repository.SaveChangesAsync();

                    // ========== 12. 提交交易 ==========
                    await _repository.CommitTransactionAsync();

                    // ========== 13. 刪除舊圖片（交易成功後執行） ==========
                    await DeleteOldImagesAsync(oldAvatarUrl, oldCoverImageUrl, dto);

                    // ========== 14. 記錄操作日誌 ==========
                    await LogUserActionAsync(
                        userId: userId,
                        actionType: "UpdateProfile",
                        actionCategory: "Profile",
                        status: "Success",
                        description: "更新個人資料成功"
                    );

                    return new UpdateProfileResponseDto
                    {
                        Success = true,
                        Message = "個人資料更新成功"
                    };
                }
                catch (Exception innerEx)
                {
                    // 發生錯誤時回滾交易
                    await _repository.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新個人資料時發生錯誤，UserId: {UserId}", userId);

                // 記錄失敗日誌
                await LogUserActionAsync(
                    userId: userId,
                    actionType: "UpdateProfile",
                    actionCategory: "Profile",
                    status: "Failure",
                    description: "更新個人資料失敗",
                    errorMessage: ex.Message
                );

                return new UpdateProfileResponseDto
                {
                    Success = false,
                    Message = "更新個人資料失敗，請稍後再試"
                };
            }
        }

        #endregion

        #region 驗證方法

        /// <summary>
        /// 驗證更新個人資料的業務邏輯
        /// Data Annotations 已在 DTO 執行基本驗證
        /// 這裡執行更複雜的業務邏輯驗證
        /// </summary>
        private List<string> ValidateUpdateProfileDto(UpdateUserProfileDto dto)
        {
            var errors = new List<string>();

            // 驗證性別值
            if (!string.IsNullOrWhiteSpace(dto.Gender) && !MemberConstants.IsValidGender(dto.Gender))
            {
                errors.Add(MemberConstants.GetInvalidGenderMessage());
            }

            // 驗證圖片檔案
            if (dto.AvatarFile != null)
            {
                var avatarValidation = ValidateImageFile(
                    dto.AvatarFile,
                    MemberConstants.AVATAR_MAX_SIZE_BYTES,
                    "頭像"
                );
                if (avatarValidation != null)
                    errors.Add(avatarValidation);
            }

            if (dto.CoverImageFile != null)
            {
                var coverValidation = ValidateImageFile(
                    dto.CoverImageFile,
                    MemberConstants.COVER_IMAGE_MAX_SIZE_BYTES,
                    "封面圖片"
                );
                if (coverValidation != null)
                    errors.Add(coverValidation);
            }

            // 驗證邏輯衝突：不能同時上傳和移除同一個圖片
            if (dto.AvatarFile != null && dto.RemoveAvatar)
            {
                errors.Add("無法同時上傳和移除頭像");
            }

            if (dto.CoverImageFile != null && dto.RemoveCoverImage)
            {
                errors.Add("無法同時上傳和移除封面圖片");
            }

            return errors;
        }

        /// <summary>
        /// 驗證圖片檔案（大小、格式）
        /// </summary>
        private string? ValidateImageFile(IFormFile file, long maxSizeBytes, string fieldName)
        {
            // 檢查檔案大小
            if (file.Length > maxSizeBytes)
            {
                var maxSizeMB = maxSizeBytes / 1024 / 1024;
                return $"{fieldName}大小不能超過 {maxSizeMB} MB";
            }

            // 檢查檔案格式
            if (!MemberConstants.IsValidImageExtension(file.FileName))
            {
                return MemberConstants.GetUnsupportedImageFormatMessage();
            }

            return null;
        }

        #endregion

        #region 圖片上傳方法

        /// <summary>
        /// 上傳頭像到 Azure Blob Storage
        /// </summary>
        private async Task<BlobUploadResultDto> UploadAvatarAsync(long userId, IFormFile file)
        {
            try
            {
                // 取得副檔名
                var extension = Path.GetExtension(file.FileName);

                // 生成檔名（使用 MemberConstants 的命名規則）
                var fileName = MemberConstants.GenerateAvatarFileName(userId, extension);

                // 上傳到 Blob Storage
                using var stream = file.OpenReadStream();
                var blobUrl = await _blobStorageService.UploadAsync(
                    stream: stream,
                    blobPath: fileName,
                    containerName: MemberConstants.AVATAR_CONTAINER,
                    contentType: file.ContentType
                );

                _logger.LogInformation(
                    "頭像上傳成功，UserId: {UserId}, FileName: {FileName}, BlobUrl: {BlobUrl}",
                    userId, fileName, blobUrl
                );

                return new BlobUploadResultDto
                {
                    Success = true,
                    BlobPath = fileName,
                    BlobUrl = blobUrl,
                    UploadedAt = DateTime.UtcNow,
                    FileSizeBytes = file.Length
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "頭像上傳失敗，UserId: {UserId}", userId);

                return new BlobUploadResultDto
                {
                    Success = false,
                    ErrorMessage = "頭像上傳失敗，請稍後再試"
                };
            }
        }

        /// <summary>
        /// 上傳封面圖片到 Azure Blob Storage
        /// </summary>
        private async Task<BlobUploadResultDto> UploadCoverImageAsync(long userId, IFormFile file)
        {
            try
            {
                // 取得副檔名
                var extension = Path.GetExtension(file.FileName);

                // 生成檔名（使用 MemberConstants 的命名規則）
                var fileName = MemberConstants.GenerateCoverImageFileName(userId, extension);

                // 上傳到 Blob Storage
                using var stream = file.OpenReadStream();
                var blobUrl = await _blobStorageService.UploadAsync(
                    stream: stream,
                    blobPath: fileName,
                    containerName: MemberConstants.COVER_IMAGE_CONTAINER,
                    contentType: file.ContentType
                );

                _logger.LogInformation(
                    "封面圖片上傳成功，UserId: {UserId}, FileName: {FileName}, BlobUrl: {BlobUrl}",
                    userId, fileName, blobUrl
                );

                return new BlobUploadResultDto
                {
                    Success = true,
                    BlobPath = fileName,
                    BlobUrl = blobUrl,
                    UploadedAt = DateTime.UtcNow,
                    FileSizeBytes = file.Length
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "封面圖片上傳失敗，UserId: {UserId}", userId);

                return new BlobUploadResultDto
                {
                    Success = false,
                    ErrorMessage = "封面圖片上傳失敗，請稍後再試"
                };
            }
        }

        #endregion

        #region 圖片刪除方法

        /// <summary>
        /// 刪除舊圖片（在資料庫交易成功後執行）
        /// </summary>
        private async Task DeleteOldImagesAsync(
            string? oldAvatarUrl,
            string? oldCoverImageUrl,
            UpdateUserProfileDto dto)
        {
            // 刪除舊頭像（如果有上傳新頭像或移除頭像）
            if ((dto.AvatarFile != null || dto.RemoveAvatar) && !string.IsNullOrWhiteSpace(oldAvatarUrl))
            {
                await DeleteImageFromBlobAsync(oldAvatarUrl, MemberConstants.AVATAR_CONTAINER, "頭像");
            }

            // 刪除舊封面圖片（如果有上傳新封面或移除封面）
            if ((dto.CoverImageFile != null || dto.RemoveCoverImage) && !string.IsNullOrWhiteSpace(oldCoverImageUrl))
            {
                await DeleteImageFromBlobAsync(oldCoverImageUrl, MemberConstants.COVER_IMAGE_CONTAINER, "封面圖片");
            }
        }

        /// <summary>
        /// 從 Blob Storage 刪除圖片
        /// </summary>
        private async Task DeleteImageFromBlobAsync(string blobUrl, string containerName, string imageType)
        {
            try
            {
                // 從完整 URL 提取檔名
                var blobFileName = ExtractBlobFileNameFromUrl(blobUrl);

                if (!string.IsNullOrWhiteSpace(blobFileName))
                {
                    var deleted = await _blobStorageService.DeleteAsync(blobFileName, containerName);

                    if (deleted)
                    {
                        _logger.LogInformation(
                            "成功刪除舊{ImageType}，FileName: {FileName}",
                            imageType, blobFileName
                        );
                    }
                    else
                    {
                        _logger.LogWarning(
                            "刪除舊{ImageType}失敗或檔案不存在，FileName: {FileName}",
                            imageType, blobFileName
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                // 刪除失敗不影響主要流程，只記錄錯誤
                _logger.LogError(ex, "刪除舊{ImageType}時發生錯誤，BlobUrl: {BlobUrl}", imageType, blobUrl);
            }
        }

        /// <summary>
        /// 從完整 Blob URL 提取檔名
        /// 例如：https://xxx.blob.core.windows.net/avatars/avatar_123_20241227.jpg → avatar_123_20241227.jpg
        /// </summary>
        private string? ExtractBlobFileNameFromUrl(string blobUrl)
        {
            try
            {
                var uri = new Uri(blobUrl);
                // 取得路徑的最後一段（檔名）
                return Path.GetFileName(uri.LocalPath);
            }
            catch
            {
                _logger.LogWarning("無法從 URL 提取檔名：{BlobUrl}", blobUrl);
                return null;
            }
        }

        #endregion

        #region UserLog 記錄

        /// <summary>
        /// 記錄使用者操作日誌
        /// </summary>
        private async Task LogUserActionAsync(
            long userId,
            string actionType,
            string actionCategory,
            string status,
            string description,
            string? errorMessage = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                string ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                string userAgent = httpContext?.Request.Headers["User-Agent"].ToString() ?? "Unknown";

                var userLog = new UserLog
                {
                    UserId = userId,
                    Status = status,
                    ActionType = actionType,
                    ActionCategory = actionCategory,
                    ActionDescription = description,
                    TargetType = "UserProfile",
                    TargetId = userId,
                    Ipaddress = ipAddress,
                    UserAgent = userAgent,
                    DeviceType = GetDeviceType(userAgent),
                    SystemName = "FrameZone",
                    Severity = status == "Success" ? "Info" : "Warning",
                    ErrorMessage = errorMessage,
                    CreatedAt = DateTime.UtcNow
                };

                // 寫入 UserLog 到資料庫
                await _userLogRepository.CreateUserLogAsync(userLog);

                _logger.LogInformation(
                    "記錄 UserLog 成功：UserId={UserId}, ActionType={ActionType}, Status={Status}",
                    userId, actionType, status
                );
            }
            catch (Exception ex)
            {
                // 記錄日誌失敗不影響主要流程
                _logger.LogError(ex, "記錄 UserLog 時發生錯誤");
            }
        }

        /// <summary>
        /// 從 User-Agent 判斷裝置類型
        /// </summary>
        private string GetDeviceType(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return "Unknown";

            userAgent = userAgent.ToLower();

            if (userAgent.Contains("mobile") ||
                userAgent.Contains("android") ||
                userAgent.Contains("iphone") ||
                userAgent.Contains("ipad"))
            {
                if (userAgent.Contains("android"))
                    return "Android";
                if (userAgent.Contains("iphone") || userAgent.Contains("ipad"))
                    return "iOS";

                return "Mobile";
            }

            if (userAgent.Contains("tablet") || userAgent.Contains("ipad"))
                return "Tablet";

            return "Desktop";
        }

        #endregion

        /// <summary>
        /// 為圖片 URL 生成 SAS Token
        /// </summary>
        /// <param name="imageUrl">原始圖片 URL</param>
        /// <param name="containerName">容器名稱</param>
        /// <returns>帶有 SAS Token 的 URL</returns>
        private async Task<string> GenerateSasUrlForImage(string imageUrl, string containerName)
        {
            try
            {
                // 如果 URL 為空，直接返回
                if (string.IsNullOrWhiteSpace(imageUrl))
                {
                    return imageUrl;
                }

                // 如果已包含 SAS Token (有問號參數)，直接返回
                if (imageUrl.Contains("?"))
                {
                    _logger.LogDebug("URL 已包含參數，可能已有 SAS Token: {ImageUrl}", imageUrl);
                    return imageUrl;
                }

                // 從 URL 中提取 Blob 名稱
                // URL 格式: https://xxx.blob.core.windows.net/containers/blobname.ext
                var uri = new Uri(imageUrl);
                var segments = uri.Segments; // 例如: ["/", "covers/", "cover_10016_xxx.HEIC"]

                // 取得最後一個 segment (檔名)
                var blobName = segments[segments.Length - 1].TrimEnd('/');

                _logger.LogInformation(
                    "🔐 生成 SAS URL - Container: {Container}, BlobName: {BlobName}",
                    containerName, blobName);

                // 使用 BlobStorageService 生成 SAS URL
                var sasUrl = await _blobStorageService.GenerateSasUrlAsync(blobName, containerName);

                _logger.LogInformation("✅ SAS URL 生成成功: {SasUrl}", sasUrl);

                return sasUrl;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ 生成 SAS URL 失敗，返回原始 URL: {ImageUrl}", imageUrl);
                return imageUrl; // 失敗時返回原始 URL
            }
        }
    }
}