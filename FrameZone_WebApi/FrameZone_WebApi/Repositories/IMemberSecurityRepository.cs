using FrameZone_WebApi.Models;

namespace FrameZone_WebApi.Repositories.Member
{
    /// <summary>
    /// 會員安全 Repository 介面
    /// 負責所有安全相關的資料庫存取操作
    /// </summary>
    public interface IMemberSecurityRepository
    {
        // ============================================================================
        // 使用者查詢
        // ============================================================================

        /// <summary>
        /// 根據 UserId 取得使用者
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>使用者實體</returns>
        Task<User?> GetUserByIdAsync(long userId);

        /// <summary>
        /// 更新使用者資料
        /// </summary>
        /// <param name="user">使用者實體</param>
        Task UpdateUserAsync(User user);

        // ============================================================================
        // UserSession (登入裝置管理)
        // ============================================================================

        /// <summary>
        /// 取得使用者所有活躍的 Session
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>Session 列表</returns>
        Task<List<UserSession>> GetActiveSessionsAsync(long userId);

        /// <summary>
        /// 根據 SessionId 取得 Session
        /// </summary>
        /// <param name="sessionId">Session ID</param>
        /// <param name="userId">使用者 ID（用於驗證）</param>
        /// <returns>Session 實體</returns>
        Task<UserSession?> GetSessionByIdAsync(long sessionId, long userId);

        /// <summary>
        /// 取得使用者除了指定 Session 外的所有活躍 Session
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="currentSessionId">目前 Session ID</param>
        /// <returns>Session 列表</returns>
        Task<List<UserSession>> GetOtherActiveSessionsAsync(long userId, long currentSessionId);

        /// <summary>
        /// 更新 Session
        /// </summary>
        /// <param name="session">Session 實體</param>
        Task UpdateSessionAsync(UserSession session);

        /// <summary>
        /// 批次更新 Session
        /// </summary>
        /// <param name="sessions">Session 列表</param>
        Task UpdateSessionsAsync(List<UserSession> sessions);

        // ============================================================================
        // UserSecurityStatus (帳號安全狀態)
        // ============================================================================

        /// <summary>
        /// 取得使用者安全狀態
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>安全狀態實體</returns>
        Task<UserSecurityStatus?> GetSecurityStatusAsync(long userId);

        /// <summary>
        /// 建立使用者安全狀態
        /// </summary>
        /// <param name="securityStatus">安全狀態實體</param>
        Task CreateSecurityStatusAsync(UserSecurityStatus securityStatus);

        /// <summary>
        /// 更新使用者安全狀態
        /// </summary>
        /// <param name="securityStatus">安全狀態實體</param>
        Task UpdateSecurityStatusAsync(UserSecurityStatus securityStatus);

        // ============================================================================
        // UserLog (操作日誌)
        // ============================================================================

        /// <summary>
        /// 取得使用者最後一次登入記錄
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>登入記錄</returns>
        Task<UserLog?> GetLastLoginLogAsync(long userId);

        /// <summary>
        /// 取得使用者最後一次密碼變更記錄
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>密碼變更記錄</returns>
        Task<UserLog?> GetLastPasswordChangeLogAsync(long userId);

        /// <summary>
        /// 取得 Session 建立後的第一筆日誌（用於取得 IP）
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="sessionCreatedAt">Session 建立時間</param>
        /// <returns>日誌記錄</returns>
        Task<UserLog?> GetLogAfterSessionCreatedAsync(long userId, DateTime sessionCreatedAt);

        /// <summary>
        /// 建立操作日誌
        /// </summary>
        /// <param name="log">日誌實體</param>
        Task CreateLogAsync(UserLog log);

        // ============================================================================
        // 儲存與交易
        // ============================================================================

        /// <summary>
        /// 儲存變更
        /// </summary>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// 開始資料庫交易
        /// </summary>
        Task BeginTransactionAsync();

        /// <summary>
        /// 提交交易
        /// </summary>
        Task CommitTransactionAsync();

        /// <summary>
        /// 回滾交易
        /// </summary>
        Task RollbackTransactionAsync();
    }
}