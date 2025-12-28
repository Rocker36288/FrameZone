using FrameZone_WebApi.Models;

namespace FrameZone_WebApi.Repositories
{
    public class UserLogRepository : IUserLogRepository
    {
        private readonly AAContext _context;
        private readonly ILogger<UserLogRepository> _logger;

        public UserLogRepository(AAContext context, ILogger<UserLogRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 建立使用者操作日誌
        /// </summary>
        /// <param name="userLog">使用者日誌實體</param>
        /// <returns>是否建立成功</returns>
        public async Task<bool> CreateUserLogAsync(UserLog userLog)
        {
            try
            {
                _context.UserLogs.Add(userLog);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立 UserLog 失敗：{ActionType}", userLog.ActionType);
                return false;
            }
        }

        /// <summary>
        /// 批次建立使用者操作日誌
        /// </summary>
        /// <param name="userLogs">使用者日誌實體列表</param>
        /// <returns>成功建立的數量</returns>
        public async Task<int> CreateUserLogsBatchAsync(List<UserLog> userLogs)
        {
            try
            {
                _context.UserLogs.AddRange(userLogs);
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批次建立 UserLog 失敗");
                return 0;
            }
        }
    }
}
