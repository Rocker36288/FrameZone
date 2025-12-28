using FrameZone_WebApi.DTOs.Member;
using FrameZone_WebApi.Models;

namespace FrameZone_WebApi.Repositories
{
    /// <summary>
    /// 使用者日誌 Repository 介面
    /// </summary>
    public interface IUserLogRepository
    {
        #region 建立日誌

        /// <summary>
        /// 建立使用者操作日誌
        /// </summary>
        /// <param name="userLog">使用者日誌實體</param>
        /// <returns>是否建立成功</returns>
        Task<bool> CreateUserLogAsync(UserLog userLog);

        /// <summary>
        /// 批次建立使用者操作日誌
        /// </summary>
        /// <param name="userLogs">使用者日誌實體列表</param>
        /// <returns>成功建立的數量</returns>
        Task<int> CreateUserLogsBatchAsync(List<UserLog> userLogs);

        #endregion

        #region 查詢日誌

        /// <summary>
        /// 分頁查詢使用者操作日誌
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="queryDto">查詢參數</param>
        /// <returns>分頁日誌資料</returns>
        Task<PagedData<UserLogDto>> GetUserLogsPagedAsync(long userId, UserLogQueryDto queryDto);

        /// <summary>
        /// 取得使用者日誌統計資料
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>統計資料</returns>
        Task<UserLogStatsDto> GetUserLogStatsAsync(long userId);

        /// <summary>
        /// 取得單筆日誌詳細資料
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="logId">日誌 ID</param>
        /// <returns>日誌詳細資料</returns>
        Task<UserLogDto?> GetUserLogByIdAsync(long userId, long logId);

        /// <summary>
        /// 取得使用者最近的登入記錄
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="count">取得數量</param>
        /// <returns>登入記錄列表</returns>
        Task<List<UserLogDto>> GetRecentLoginLogsAsync(long userId, int count = 5);

        #endregion

        #region 清理日誌

        /// <summary>
        /// 刪除指定天數之前的日誌（清理舊資料）
        /// </summary>
        /// <param name="daysToKeep">保留天數</param>
        /// <returns>刪除的記錄數</returns>
        Task<int> DeleteOldLogsAsync(int daysToKeep = 90);

        #endregion
    }
}