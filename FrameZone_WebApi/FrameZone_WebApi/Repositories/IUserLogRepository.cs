using FrameZone_WebApi.Models;

namespace FrameZone_WebApi.Repositories
{
    public interface IUserLogRepository
    {
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
    }
}
