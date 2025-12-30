using FrameZone.API.DTOs.Member;
using FrameZone.API.Repositories.Interfaces;
using FrameZone.API.Services.Interfaces;
using FrameZone_WebApi.Models;
using FrameZone_WebApi.Repositories;

namespace FrameZone.API.Services
{
    /// <summary>
    /// 會員隱私設定 Service 實作
    /// </summary>
    public class MemberPrivacyService : IMemberPrivacyService
    {
        private readonly IMemberPrivacyRepository _privacyRepository;
        private readonly IUserLogRepository _userLogRepository;
        private readonly ILogger<MemberPrivacyService> _logger;

        public MemberPrivacyService(
            IMemberPrivacyRepository privacyRepository,
            IUserLogRepository userLogRepository,
            ILogger<MemberPrivacyService> logger)
        {
            _privacyRepository = privacyRepository;
            _userLogRepository = userLogRepository;
            _logger = logger;
        }

        /// <summary>
        /// 取得使用者的隱私設定
        /// </summary>
        public async Task<PrivacySettingsResponseDto> GetPrivacySettingsAsync(long userId)
        {
            try
            {
                var settings = await _privacyRepository.GetPrivacySettingsByUserIdAsync(userId);

                var settingDtos = settings.Select(s => new PrivacySettingDto
                {
                    PrivacyId = s.PrivacyId,
                    UserId = s.UserId,
                    FieldName = s.FieldName,
                    Visibility = s.Visibility
                }).ToList();

                return new PrivacySettingsResponseDto
                {
                    Success = true,
                    Message = "取得隱私設定成功",
                    Data = settingDtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得隱私設定失敗，UserId: {UserId}", userId);

                return new PrivacySettingsResponseDto
                {
                    Success = false,
                    Message = "取得隱私設定失敗",
                    Data = new List<PrivacySettingDto>()
                };
            }
        }

        /// <summary>
        /// 批次更新隱私設定
        /// </summary>
        public async Task<PrivacySettingsResponseDto> BatchUpdatePrivacySettingsAsync(
            long userId,
            BatchUpdatePrivacySettingsDto dto)
        {
            try
            {
                // 驗證輸入
                if (dto.Settings == null || !dto.Settings.Any())
                {
                    return new PrivacySettingsResponseDto
                    {
                        Success = false,
                        Message = "沒有要更新的設定",
                        Data = new List<PrivacySettingDto>()
                    };
                }

                // 驗證可見性值
                var validVisibilities = new[] { "Public", "FriendsOnly", "Private" };
                var invalidSettings = dto.Settings
                    .Where(s => !validVisibilities.Contains(s.Visibility))
                    .ToList();

                if (invalidSettings.Any())
                {
                    return new PrivacySettingsResponseDto
                    {
                        Success = false,
                        Message = "可見性設定值無效，只能是 Public、FriendsOnly 或 Private",
                        Data = new List<PrivacySettingDto>()
                    };
                }

                // 轉換為實體
                var settingsToUpdate = dto.Settings.Select(s => new UserPrivacySetting
                {
                    UserId = userId,
                    FieldName = s.FieldName,
                    Visibility = s.Visibility
                }).ToList();

                // 批次更新
                var success = await _privacyRepository.BatchUpdatePrivacySettingsAsync(userId, settingsToUpdate);

                if (!success)
                {
                    return new PrivacySettingsResponseDto
                    {
                        Success = false,
                        Message = "更新隱私設定失敗",
                        Data = new List<PrivacySettingDto>()
                    };
                }

                // 記錄活動日誌
                await LogPrivacyUpdateAsync(userId, dto.Settings.Count);

                // 重新取得更新後的設定
                var updatedSettings = await _privacyRepository.GetPrivacySettingsByUserIdAsync(userId);
                var settingDtos = updatedSettings.Select(s => new PrivacySettingDto
                {
                    PrivacyId = s.PrivacyId,
                    UserId = s.UserId,
                    FieldName = s.FieldName,
                    Visibility = s.Visibility
                }).ToList();

                return new PrivacySettingsResponseDto
                {
                    Success = true,
                    Message = "隱私設定已更新",
                    Data = settingDtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批次更新隱私設定失敗，UserId: {UserId}", userId);

                return new PrivacySettingsResponseDto
                {
                    Success = false,
                    Message = "更新隱私設定時發生錯誤",
                    Data = new List<PrivacySettingDto>()
                };
            }
        }

        /// <summary>
        /// 記錄隱私設定更新日誌
        /// </summary>
        private async Task LogPrivacyUpdateAsync(long userId, int settingsCount)
        {
            try
            {
                var log = new UserLog
                {
                    UserId = userId,
                    Status = "Success",
                    ActionType = "Update",
                    ActionCategory = "Privacy",
                    ActionDescription = $"更新了 {settingsCount} 個隱私設定",
                    SystemName = "Member",
                    Severity = "Info",
                    PerformedBy = "User",
                    CreatedAt = DateTime.Now
                };

                await _userLogRepository.CreateUserLogAsync(log);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "記錄隱私設定更新日誌失敗，UserId: {UserId}", userId);
                // 不影響主要流程，只記錄錯誤
            }
        }
    }
}