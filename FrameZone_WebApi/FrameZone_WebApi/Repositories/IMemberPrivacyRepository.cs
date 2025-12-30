using FrameZone_WebApi.Models;

namespace FrameZone.API.Repositories.Interfaces
{
    /// <summary>
    /// 會員隱私設定 Repository 介面
    /// </summary>
    public interface IMemberPrivacyRepository
    {
        /// <summary>
        /// 取得使用者的所有隱私設定
        /// </summary>
        Task<List<UserPrivacySetting>> GetPrivacySettingsByUserIdAsync(long userId);

        /// <summary>
        /// 取得使用者的單一欄位隱私設定
        /// </summary>
        Task<UserPrivacySetting?> GetPrivacySettingAsync(long userId, string fieldName);

        /// <summary>
        /// 建立隱私設定
        /// </summary>
        Task<UserPrivacySetting> CreatePrivacySettingAsync(UserPrivacySetting setting);

        /// <summary>
        /// 更新隱私設定
        /// </summary>
        Task<UserPrivacySetting> UpdatePrivacySettingAsync(UserPrivacySetting setting);

        /// <summary>
        /// 批次更新隱私設定（使用交易）
        /// </summary>
        Task<bool> BatchUpdatePrivacySettingsAsync(long userId, List<UserPrivacySetting> settings);

        /// <summary>
        /// 刪除隱私設定
        /// </summary>
        Task<bool> DeletePrivacySettingAsync(long privacyId);

        /// <summary>
        /// 儲存變更
        /// </summary>
        Task<int> SaveChangesAsync();
    }
}