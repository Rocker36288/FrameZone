using FrameZone_WebApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace FrameZone_WebApi.Repositories.Member
{
    /// <summary>
    /// 會員安全 Repository 實作
    /// 負責所有安全相關的資料庫存取操作
    /// </summary>
    public class MemberSecurityRepository : IMemberSecurityRepository
    {
        private readonly AAContext _context;
        private IDbContextTransaction? _transaction;

        public MemberSecurityRepository(AAContext context)
        {
            _context = context;
        }

        // ============================================================================
        // 使用者查詢
        // ============================================================================

        /// <summary>
        /// 根據 UserId 取得使用者
        /// </summary>
        public async Task<User?> GetUserByIdAsync(long userId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        /// <summary>
        /// 更新使用者資料
        /// </summary>
        public Task UpdateUserAsync(User user)
        {
            user.UpdatedAt = DateTime.Now;
            _context.Users.Update(user);
            return Task.CompletedTask;
        }

        // ============================================================================
        // UserSession (登入裝置管理)
        // ============================================================================

        /// <summary>
        /// 取得使用者所有活躍的 Session
        /// </summary>
        public async Task<List<UserSession>> GetActiveSessionsAsync(long userId)
        {
            return await _context.UserSessions
                .Where(s => s.UserId == userId && s.IsActive)
                .OrderByDescending(s => s.LastActivityAt)
                .ToListAsync();
        }

        /// <summary>
        /// 根據 SessionId 取得 Session
        /// </summary>
        public async Task<UserSession?> GetSessionByIdAsync(long sessionId, long userId)
        {
            return await _context.UserSessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.UserId == userId);
        }

        /// <summary>
        /// 取得使用者除了指定 Session 外的所有活躍 Session
        /// </summary>
        public async Task<List<UserSession>> GetOtherActiveSessionsAsync(long userId, long currentSessionId)
        {
            return await _context.UserSessions
                .Where(s => s.UserId == userId && s.SessionId != currentSessionId && s.IsActive)
                .ToListAsync();
        }

        /// <summary>
        /// 更新 Session
        /// </summary>
        public Task UpdateSessionAsync(UserSession session)
        {
            _context.UserSessions.Update(session);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 批次更新 Session
        /// </summary>
        public Task UpdateSessionsAsync(List<UserSession> sessions)
        {
            _context.UserSessions.UpdateRange(sessions);
            return Task.CompletedTask;
        }

        // ============================================================================
        // UserSecurityStatus (帳號安全狀態)
        // ============================================================================

        /// <summary>
        /// 取得使用者安全狀態
        /// </summary>
        public async Task<UserSecurityStatus?> GetSecurityStatusAsync(long userId)
        {
            return await _context.UserSecurityStatuses
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        /// <summary>
        /// 建立使用者安全狀態
        /// </summary>
        public async Task CreateSecurityStatusAsync(UserSecurityStatus securityStatus)
        {
            securityStatus.UpdatedAt = DateTime.Now;
            await _context.UserSecurityStatuses.AddAsync(securityStatus);
        }

        /// <summary>
        /// 更新使用者安全狀態
        /// </summary>
        public Task UpdateSecurityStatusAsync(UserSecurityStatus securityStatus)
        {
            securityStatus.UpdatedAt = DateTime.Now;
            _context.UserSecurityStatuses.Update(securityStatus);
            return Task.CompletedTask;
        }

        // ============================================================================
        // UserLog (操作日誌)
        // ============================================================================

        /// <summary>
        /// 取得使用者最後一次登入記錄
        /// </summary>
        public async Task<UserLog?> GetLastLoginLogAsync(long userId)
        {
            return await _context.UserLogs
                .Where(l => l.UserId == userId && l.ActionType == "Login" && l.Status == "Success")
                .OrderByDescending(l => l.CreatedAt)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// 取得使用者最後一次密碼變更記錄
        /// </summary>
        public async Task<UserLog?> GetLastPasswordChangeLogAsync(long userId)
        {
            return await _context.UserLogs
                .Where(l => l.UserId == userId && l.ActionType == "PasswordChange" && l.Status == "Success")
                .OrderByDescending(l => l.CreatedAt)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// 取得 Session 建立後的第一筆日誌（用於取得 IP）
        /// </summary>
        public async Task<UserLog?> GetLogAfterSessionCreatedAsync(long userId, DateTime sessionCreatedAt)
        {
            return await _context.UserLogs
                .Where(l => l.UserId == userId && l.CreatedAt >= sessionCreatedAt)
                .OrderByDescending(l => l.CreatedAt)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// 建立操作日誌
        /// </summary>
        public async Task CreateLogAsync(UserLog log)
        {
            log.CreatedAt = DateTime.Now;
            await _context.UserLogs.AddAsync(log);
        }

        // ============================================================================
        // 儲存與交易
        // ============================================================================

        /// <summary>
        /// 儲存變更
        /// </summary>
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 開始資料庫交易
        /// </summary>
        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        /// <summary>
        /// 提交交易
        /// </summary>
        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        /// <summary>
        /// 回滾交易
        /// </summary>
        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }
}