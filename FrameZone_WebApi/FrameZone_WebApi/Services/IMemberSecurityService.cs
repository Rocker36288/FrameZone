using FrameZone_WebApi.DTOs.Member;

namespace FrameZone_WebApi.Services.Member
{
    /// <summary>
    /// 會員安全服務介面
    /// 處理密碼變更、登入裝置管理、帳號鎖定狀態等安全相關功能
    /// </summary>
    public interface IMemberSecurityService
    {
        // ============================================================================
        // 變更密碼
        // ============================================================================

        /// <summary>
        /// 變更密碼
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="dto">變更密碼 DTO</param>
        /// <returns>變更結果</returns>
        Task<ChangePasswordResponseDto> ChangePasswordAsync(long userId, ChangePasswordDto dto);

        // ============================================================================
        // 登入裝置管理
        // ============================================================================

        /// <summary>
        /// 取得使用者所有登入裝置（Session）
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="currentSessionId">目前 Session ID（用於標記目前裝置）</param>
        /// <returns>裝置列表</returns>
        Task<GetUserSessionsResponseDto> GetUserSessionsAsync(long userId, long? currentSessionId = null);

        /// <summary>
        /// 登出特定裝置（Session）
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="sessionId">要登出的 Session ID</param>
        /// <returns>登出結果</returns>
        Task<LogoutSessionResponseDto> LogoutSessionAsync(long userId, long sessionId);

        /// <summary>
        /// 登出所有其他裝置（保留目前裝置）
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="currentSessionId">目前 Session ID</param>
        /// <returns>登出結果</returns>
        Task<LogoutSessionResponseDto> LogoutOtherSessionsAsync(long userId, long currentSessionId);

        // ============================================================================
        // 帳號鎖定狀態
        // ============================================================================

        /// <summary>
        /// 取得帳號鎖定狀態
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>鎖定狀態</returns>
        Task<GetAccountLockStatusResponseDto> GetAccountLockStatusAsync(long userId);

        // ============================================================================
        // 安全性概覽（選用）
        // ============================================================================

        /// <summary>
        /// 取得安全性概覽
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>安全性概覽</returns>
        Task<GetSecurityOverviewResponseDto> GetSecurityOverviewAsync(long userId);
    }
}