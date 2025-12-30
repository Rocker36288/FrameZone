using FrameZone.API.Repositories.Interfaces;
using FrameZone_WebApi.Models;
using Google;
using Microsoft.EntityFrameworkCore;

namespace FrameZone.API.Repositories
{
    /// <summary>
    /// 會員隱私設定 Repository 實作
    /// </summary>
    public class MemberPrivacyRepository : IMemberPrivacyRepository
    {
        private readonly AAContext _context;
        private readonly ILogger<MemberPrivacyRepository> _logger;

        public MemberPrivacyRepository(
            AAContext context,
            ILogger<MemberPrivacyRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 取得使用者的所有隱私設定
        /// </summary>
        public async Task<List<UserPrivacySetting>> GetPrivacySettingsByUserIdAsync(long userId)
        {
            try
            {
                return await _context.UserPrivacySettings
                    .Where(p => p.UserId == userId)
                    .OrderBy(p => p.FieldName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得使用者隱私設定失敗，UserId: {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// 取得使用者的單一欄位隱私設定
        /// </summary>
        public async Task<UserPrivacySetting?> GetPrivacySettingAsync(long userId, string fieldName)
        {
            try
            {
                return await _context.UserPrivacySettings
                    .FirstOrDefaultAsync(p => p.UserId == userId && p.FieldName == fieldName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得隱私設定失敗，UserId: {UserId}, FieldName: {FieldName}", userId, fieldName);
                throw;
            }
        }

        /// <summary>
        /// 建立隱私設定
        /// </summary>
        public async Task<UserPrivacySetting> CreatePrivacySettingAsync(UserPrivacySetting setting)
        {
            try
            {
                setting.CreatedAt = DateTime.Now;
                setting.UpdatedAt = DateTime.Now;

                await _context.UserPrivacySettings.AddAsync(setting);
                await _context.SaveChangesAsync();

                return setting;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立隱私設定失敗，UserId: {UserId}, FieldName: {FieldName}",
                    setting.UserId, setting.FieldName);
                throw;
            }
        }

        /// <summary>
        /// 更新隱私設定
        /// </summary>
        public async Task<UserPrivacySetting> UpdatePrivacySettingAsync(UserPrivacySetting setting)
        {
            try
            {
                setting.UpdatedAt = DateTime.Now;

                _context.UserPrivacySettings.Update(setting);
                await _context.SaveChangesAsync();

                return setting;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新隱私設定失敗，PrivacyId: {PrivacyId}", setting.PrivacyId);
                throw;
            }
        }

        /// <summary>
        /// 批次更新隱私設定（使用交易）
        /// </summary>
        public async Task<bool> BatchUpdatePrivacySettingsAsync(long userId, List<UserPrivacySetting> settings)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 取得現有的所有隱私設定
                var existingSettings = await _context.UserPrivacySettings
                    .Where(p => p.UserId == userId)
                    .ToListAsync();

                var now = DateTime.Now;

                foreach (var setting in settings)
                {
                    var existing = existingSettings.FirstOrDefault(e => e.FieldName == setting.FieldName);

                    if (existing != null)
                    {
                        // 更新現有設定
                        existing.Visibility = setting.Visibility;
                        existing.UpdatedAt = now;
                        _context.UserPrivacySettings.Update(existing);
                    }
                    else
                    {
                        // 建立新設定
                        setting.UserId = userId;
                        setting.CreatedAt = now;
                        setting.UpdatedAt = now;
                        await _context.UserPrivacySettings.AddAsync(setting);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "批次更新隱私設定失敗，UserId: {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// 刪除隱私設定
        /// </summary>
        public async Task<bool> DeletePrivacySettingAsync(long privacyId)
        {
            try
            {
                var setting = await _context.UserPrivacySettings.FindAsync(privacyId);

                if (setting == null)
                {
                    return false;
                }

                _context.UserPrivacySettings.Remove(setting);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除隱私設定失敗，PrivacyId: {PrivacyId}", privacyId);
                throw;
            }
        }

        /// <summary>
        /// 儲存變更
        /// </summary>
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}