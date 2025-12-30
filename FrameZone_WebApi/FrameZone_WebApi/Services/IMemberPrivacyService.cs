using FrameZone.API.DTOs.Member;

namespace FrameZone.API.Services.Interfaces
{
    /// <summary>
    /// 會員隱私設定 Service 介面
    /// </summary>
    public interface IMemberPrivacyService
    {
        /// <summary>
        /// 取得使用者的隱私設定
        /// </summary>
        Task<PrivacySettingsResponseDto> GetPrivacySettingsAsync(long userId);

        /// <summary>
        /// 批次更新隱私設定
        /// </summary>
        Task<PrivacySettingsResponseDto> BatchUpdatePrivacySettingsAsync(long userId, BatchUpdatePrivacySettingsDto dto);
    }
}