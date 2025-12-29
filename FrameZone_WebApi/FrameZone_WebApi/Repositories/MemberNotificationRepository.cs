using FrameZone_WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FrameZone_WebApi.Repositories.Member
{
    /// <summary>
    /// 會員通知偏好設定 Repository 實作
    /// 負責通知偏好設定相關的資料庫存取操作
    /// </summary>
    public class MemberNotificationRepository : IMemberNotificationRepository
    {
        private readonly AAContext _context;

        public MemberNotificationRepository(AAContext context)
        {
            _context = context;
        }

        // ============================================================================
        // UserNotificationPreference (通知偏好設定)
        // ============================================================================

        /// <summary>
        /// 根據 UserId 取得通知偏好設定
        /// </summary>
        public async Task<UserNotificationPreference?> GetNotificationPreferenceAsync(long userId)
        {
            return await _context.UserNotificationPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        /// <summary>
        /// 建立通知偏好設定
        /// </summary>
        public async Task CreateNotificationPreferenceAsync(UserNotificationPreference preference)
        {
            preference.CreatedAt = DateTime.Now;
            preference.UpdatedAt = DateTime.Now;
            await _context.UserNotificationPreferences.AddAsync(preference);
        }

        /// <summary>
        /// 更新通知偏好設定
        /// </summary>
        public Task UpdateNotificationPreferenceAsync(UserNotificationPreference preference)
        {
            preference.UpdatedAt = DateTime.Now;
            _context.UserNotificationPreferences.Update(preference);
            return Task.CompletedTask;
        }

        // ============================================================================
        // UserLog (操作日誌)
        // ============================================================================

        /// <summary>
        /// 建立操作日誌
        /// </summary>
        public async Task CreateLogAsync(UserLog log)
        {
            log.CreatedAt = DateTime.Now;
            await _context.UserLogs.AddAsync(log);
        }

        // ============================================================================
        // 儲存變更
        // ============================================================================

        /// <summary>
        /// 儲存變更
        /// </summary>
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}