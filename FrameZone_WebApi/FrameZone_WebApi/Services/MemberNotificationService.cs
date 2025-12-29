using FrameZone_WebApi.DTOs.Member;
using FrameZone_WebApi.Models;
using FrameZone_WebApi.Repositories.Member;

namespace FrameZone_WebApi.Services.Member
{
    /// <summary>
    /// 會員通知偏好設定服務實作
    /// 處理通知偏好設定相關的業務邏輯
    /// </summary>
    public class MemberNotificationService : IMemberNotificationService
    {
        private readonly IMemberNotificationRepository _repository;
        private readonly ILogger<MemberNotificationService> _logger;

        public MemberNotificationService(
            IMemberNotificationRepository repository,
            ILogger<MemberNotificationService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        // ============================================================================
        // 通知偏好設定
        // ============================================================================

        /// <summary>
        /// 取得通知偏好設定
        /// </summary>
        public async Task<GetNotificationPreferenceResponseDto> GetNotificationPreferenceAsync(long userId)
        {
            try
            {
                var preference = await _repository.GetNotificationPreferenceAsync(userId);

                // 如果不存在，建立預設設定
                if (preference == null)
                {
                    preference = CreateDefaultPreference(userId);
                    await _repository.CreateNotificationPreferenceAsync(preference);
                    await _repository.SaveChangesAsync();

                    _logger.LogInformation($"建立預設通知偏好設定: UserId={userId}");
                }

                var dto = MapToDto(preference);

                return new GetNotificationPreferenceResponseDto
                {
                    Success = true,
                    Message = "取得成功",
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"取得通知偏好設定時發生錯誤: UserId={userId}");
                return new GetNotificationPreferenceResponseDto
                {
                    Success = false,
                    Message = "系統錯誤，請稍後再試"
                };
            }
        }

        /// <summary>
        /// 更新通知偏好設定
        /// </summary>
        public async Task<UpdateNotificationPreferenceResponseDto> UpdateNotificationPreferenceAsync(long userId, UpdateNotificationPreferenceDto dto)
        {
            try
            {
                var preference = await _repository.GetNotificationPreferenceAsync(userId);

                // 如果不存在，建立新的設定
                if (preference == null)
                {
                    preference = CreateDefaultPreference(userId);
                    MapFromUpdateDto(preference, dto);
                    await _repository.CreateNotificationPreferenceAsync(preference);
                }
                else
                {
                    // 更新現有設定
                    MapFromUpdateDto(preference, dto);
                    await _repository.UpdateNotificationPreferenceAsync(preference);
                }

                await _repository.SaveChangesAsync();

                // 記錄操作日誌
                await LogNotificationPreferenceUpdateAsync(userId);

                _logger.LogInformation($"更新通知偏好設定成功: UserId={userId}");

                var resultDto = MapToDto(preference);

                return new UpdateNotificationPreferenceResponseDto
                {
                    Success = true,
                    Message = "更新成功",
                    Data = resultDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新通知偏好設定時發生錯誤: UserId={userId}");
                return new UpdateNotificationPreferenceResponseDto
                {
                    Success = false,
                    Message = "系統錯誤，請稍後再試"
                };
            }
        }

        // ============================================================================
        // 私有輔助方法
        // ============================================================================

        /// <summary>
        /// 建立預設通知偏好設定
        /// </summary>
        private UserNotificationPreference CreateDefaultPreference(long userId)
        {
            return new UserNotificationPreference
            {
                UserId = userId,
                EmailNotification = true,       // 預設開啟 Email 通知
                Smsnotification = false,        // 預設關閉 SMS 通知
                PushNotification = true,        // 預設開啟推播通知
                MarketingEmail = false,         // 預設關閉行銷郵件
                OrderUpdate = true,             // 預設開啟訂單更新
                PromotionAlert = false,         // 預設關閉促銷通知
                SystemAnnouncement = true,      // 預設開啟系統公告
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }

        /// <summary>
        /// 將 Entity 對應到 DTO
        /// </summary>
        private NotificationPreferenceDto MapToDto(UserNotificationPreference preference)
        {
            return new NotificationPreferenceDto
            {
                PreferenceId = preference.PreferenceId,
                UserId = preference.UserId,
                EmailNotification = preference.EmailNotification,
                SmsNotification = preference.Smsnotification,
                PushNotification = preference.PushNotification,
                MarketingEmail = preference.MarketingEmail,
                OrderUpdate = preference.OrderUpdate,
                PromotionAlert = preference.PromotionAlert,
                SystemAnnouncement = preference.SystemAnnouncement,
                CreatedAt = preference.CreatedAt,
                UpdatedAt = preference.UpdatedAt
            };
        }

        /// <summary>
        /// 從 UpdateDto 更新 Entity
        /// </summary>
        private void MapFromUpdateDto(UserNotificationPreference preference, UpdateNotificationPreferenceDto dto)
        {
            preference.EmailNotification = dto.EmailNotification;
            preference.Smsnotification = dto.SmsNotification;
            preference.PushNotification = dto.PushNotification;
            preference.MarketingEmail = dto.MarketingEmail;
            preference.OrderUpdate = dto.OrderUpdate;
            preference.PromotionAlert = dto.PromotionAlert;
            preference.SystemAnnouncement = dto.SystemAnnouncement;
        }

        /// <summary>
        /// 記錄通知偏好設定更新日誌
        /// </summary>
        private async Task LogNotificationPreferenceUpdateAsync(long userId)
        {
            try
            {
                var log = new UserLog
                {
                    UserId = userId,
                    Status = "Success",
                    ActionType = "NotificationPreferenceUpdate",
                    ActionCategory = "Settings",
                    ActionDescription = "使用者更新通知偏好設定",
                    SystemName = "Member",
                    Severity = "Info",
                    PerformedBy = "User",
                    CreatedAt = DateTime.Now
                };

                await _repository.CreateLogAsync(log);
                await _repository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "記錄通知偏好設定更新日誌失敗");
            }
        }
    }
}