using FrameZone_WebApi.DTOs.Member;
using FrameZone_WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FrameZone_WebApi.Repositories
{
    /// <summary>
    /// 使用者日誌 Repository 實作
    /// </summary>
    public class UserLogRepository : IUserLogRepository
    {
        private readonly AAContext _context;
        private readonly ILogger<UserLogRepository> _logger;

        public UserLogRepository(AAContext context, ILogger<UserLogRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region 建立日誌

        /// <summary>
        /// 建立使用者操作日誌
        /// </summary>
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

        #endregion

        #region 查詢日誌

        /// <summary>
        /// 分頁查詢使用者操作日誌
        /// </summary>
        public async Task<PagedData<UserLogDto>> GetUserLogsPagedAsync(long userId, UserLogQueryDto queryDto)
        {
            try
            {
                // 建立基本查詢
                var query = _context.UserLogs
                    .Where(log => log.UserId == userId)
                    .AsQueryable();

                // 套用篩選條件
                if (!string.IsNullOrWhiteSpace(queryDto.ActionType))
                {
                    query = query.Where(log => log.ActionType == queryDto.ActionType);
                }

                if (!string.IsNullOrWhiteSpace(queryDto.ActionCategory))
                {
                    query = query.Where(log => log.ActionCategory == queryDto.ActionCategory);
                }

                if (!string.IsNullOrWhiteSpace(queryDto.Status))
                {
                    query = query.Where(log => log.Status == queryDto.Status);
                }

                if (!string.IsNullOrWhiteSpace(queryDto.Severity))
                {
                    query = query.Where(log => log.Severity == queryDto.Severity);
                }

                // 日期範圍篩選
                if (queryDto.StartDate.HasValue)
                {
                    query = query.Where(log => log.CreatedAt >= queryDto.StartDate.Value);
                }

                if (queryDto.EndDate.HasValue)
                {
                    // 結束日期包含當天整天
                    var endDate = queryDto.EndDate.Value.Date.AddDays(1);
                    query = query.Where(log => log.CreatedAt < endDate);
                }

                // 計算總筆數
                var totalCount = await query.CountAsync();

                // 計算總頁數
                var totalPages = (int)Math.Ceiling(totalCount / (double)queryDto.PageSize);

                // 套用排序（最新的在前）
                query = query.OrderByDescending(log => log.CreatedAt);

                // 套用分頁
                var logs = await query
                    .Skip((queryDto.PageNumber - 1) * queryDto.PageSize)
                    .Take(queryDto.PageSize)
                    .Select(log => new UserLogDto
                    {
                        LogId = log.LogId,
                        UserId = log.UserId,
                        Status = log.Status,
                        ActionType = log.ActionType,
                        ActionCategory = log.ActionCategory,
                        ActionDescription = log.ActionDescription,
                        TargetType = log.TargetType,
                        TargetId = log.TargetId,
                        OldValue = log.OldValue,
                        NewValue = log.NewValue,
                        IpAddress = log.Ipaddress,
                        UserAgent = log.UserAgent,
                        DeviceType = log.DeviceType,
                        SystemName = log.SystemName,
                        Severity = log.Severity,
                        ErrorMessage = log.ErrorMessage,
                        ExecutionTime = log.ExcutionTime,
                        PerformedBy = log.PerformedBy,
                        CreatedAt = log.CreatedAt
                    })
                    .ToListAsync();

                // 組裝分頁資料
                return new PagedData<UserLogDto>
                {
                    Items = logs,
                    TotalCount = totalCount,
                    PageNumber = queryDto.PageNumber,
                    PageSize = queryDto.PageSize,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查詢使用者日誌失敗：UserId={UserId}", userId);

                // 返回空的分頁資料
                return new PagedData<UserLogDto>
                {
                    Items = new List<UserLogDto>(),
                    TotalCount = 0,
                    PageNumber = queryDto.PageNumber,
                    PageSize = queryDto.PageSize,
                    TotalPages = 0
                };
            }
        }

        /// <summary>
        /// 取得使用者日誌統計資料
        /// </summary>
        public async Task<UserLogStatsDto> GetUserLogStatsAsync(long userId)
        {
            try
            {
                var logs = await _context.UserLogs
                    .Where(log => log.UserId == userId)
                    .ToListAsync();

                // 計算統計
                var stats = new UserLogStatsDto
                {
                    TotalLogs = logs.Count,
                    SuccessCount = logs.Count(log => log.Status == "Success"),
                    FailureCount = logs.Count(log => log.Status == "Failure"),
                    LastLoginAt = logs
                        .Where(log => log.ActionType == "Login" && log.Status == "Success")
                        .OrderByDescending(log => log.CreatedAt)
                        .Select(log => log.CreatedAt)
                        .FirstOrDefault(),
                    LastActivityAt = logs
                        .OrderByDescending(log => log.CreatedAt)
                        .Select(log => log.CreatedAt)
                        .FirstOrDefault()
                };

                // 依操作類型分組統計
                stats.ActionTypeStats = logs
                    .GroupBy(log => log.ActionType)
                    .ToDictionary(g => g.Key, g => g.Count());

                // 依操作類別分組統計
                stats.ActionCategoryStats = logs
                    .GroupBy(log => log.ActionCategory)
                    .ToDictionary(g => g.Key, g => g.Count());

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得使用者日誌統計失敗：UserId={UserId}", userId);

                // 返回空的統計資料
                return new UserLogStatsDto
                {
                    TotalLogs = 0,
                    SuccessCount = 0,
                    FailureCount = 0,
                    ActionTypeStats = new Dictionary<string, int>(),
                    ActionCategoryStats = new Dictionary<string, int>()
                };
            }
        }

        /// <summary>
        /// 取得單筆日誌詳細資料
        /// </summary>
        public async Task<UserLogDto?> GetUserLogByIdAsync(long userId, long logId)
        {
            try
            {
                var log = await _context.UserLogs
                    .Where(l => l.LogId == logId && l.UserId == userId)
                    .Select(log => new UserLogDto
                    {
                        LogId = log.LogId,
                        UserId = log.UserId,
                        Status = log.Status,
                        ActionType = log.ActionType,
                        ActionCategory = log.ActionCategory,
                        ActionDescription = log.ActionDescription,
                        TargetType = log.TargetType,
                        TargetId = log.TargetId,
                        OldValue = log.OldValue,
                        NewValue = log.NewValue,
                        IpAddress = log.Ipaddress,
                        UserAgent = log.UserAgent,
                        DeviceType = log.DeviceType,
                        SystemName = log.SystemName,
                        Severity = log.Severity,
                        ErrorMessage = log.ErrorMessage,
                        ExecutionTime = log.ExcutionTime,
                        PerformedBy = log.PerformedBy,
                        CreatedAt = log.CreatedAt
                    })
                    .FirstOrDefaultAsync();

                return log;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得單筆日誌失敗：LogId={LogId}, UserId={UserId}", logId, userId);
                return null;
            }
        }

        /// <summary>
        /// 取得使用者最近的登入記錄
        /// </summary>
        public async Task<List<UserLogDto>> GetRecentLoginLogsAsync(long userId, int count = 5)
        {
            try
            {
                var logs = await _context.UserLogs
                    .Where(log => log.UserId == userId && log.ActionType == "Login")
                    .OrderByDescending(log => log.CreatedAt)
                    .Take(count)
                    .Select(log => new UserLogDto
                    {
                        LogId = log.LogId,
                        UserId = log.UserId,
                        Status = log.Status,
                        ActionType = log.ActionType,
                        ActionCategory = log.ActionCategory,
                        ActionDescription = log.ActionDescription,
                        IpAddress = log.Ipaddress,
                        UserAgent = log.UserAgent,
                        DeviceType = log.DeviceType,
                        SystemName = log.SystemName,
                        Severity = log.Severity,
                        ErrorMessage = log.ErrorMessage,
                        CreatedAt = log.CreatedAt
                    })
                    .ToListAsync();

                return logs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得最近登入記錄失敗：UserId={UserId}", userId);
                return new List<UserLogDto>();
            }
        }

        #endregion

        #region 清理日誌

        /// <summary>
        /// 刪除指定天數之前的日誌（清理舊資料）
        /// </summary>
        public async Task<int> DeleteOldLogsAsync(int daysToKeep = 90)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);

                var oldLogs = await _context.UserLogs
                    .Where(log => log.CreatedAt < cutoffDate)
                    .ToListAsync();

                if (oldLogs.Any())
                {
                    _context.UserLogs.RemoveRange(oldLogs);
                    var deletedCount = await _context.SaveChangesAsync();

                    _logger.LogInformation(
                        "清理舊日誌完成：刪除 {Count} 筆記錄（保留 {Days} 天內的資料）",
                        deletedCount,
                        daysToKeep
                    );

                    return deletedCount;
                }

                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理舊日誌失敗");
                return 0;
            }
        }

        #endregion
    }
}