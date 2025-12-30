using FrameZone_WebApi.DTOs.Member;

namespace FrameZone_WebApi.Services.Member
{
    /// <summary>
    /// 使用者活動記錄服務介面
    /// </summary>
    public interface IUserLogService
    {
        #region 查詢日誌

        /// <summary>
        /// 分頁查詢使用者活動記錄
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="queryDto">查詢參數</param>
        /// <returns>分頁日誌資料</returns>
        Task<UserLogPagedResponseDto> GetUserLogsAsync(long userId, UserLogQueryDto queryDto);

        /// <summary>
        /// 取得使用者活動統計資料
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>統計資料</returns>
        Task<UserLogStatsResponseDto> GetUserLogStatsAsync(long userId);

        /// <summary>
        /// 取得單筆日誌詳細資料
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="logId">日誌 ID</param>
        /// <returns>日誌詳細資料</returns>
        Task<UserLogPagedResponseDto> GetUserLogByIdAsync(long userId, long logId);

        /// <summary>
        /// 取得最近登入記錄
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="count">取得數量</param>
        /// <returns>登入記錄列表</returns>
        Task<UserLogPagedResponseDto> GetRecentLoginLogsAsync(long userId, int count = 5);

        #endregion

        #region 匯出功能

        /// <summary>
        /// 匯出使用者活動記錄為 CSV
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="queryDto">查詢參數</param>
        /// <returns>CSV 檔案內容（byte array）</returns>
        Task<byte[]> ExportUserLogsToCsvAsync(long userId, UserLogQueryDto queryDto);

        #endregion
    }
}