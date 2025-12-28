using FrameZone_WebApi.DTOs.Member;
using FrameZone_WebApi.Repositories;
using System.Text;

namespace FrameZone_WebApi.Services.Member
{
    /// <summary>
    /// 使用者活動記錄服務實作
    /// </summary>
    public class UserLogService : IUserLogService
    {
        private readonly IUserLogRepository _userLogRepository;
        private readonly ILogger<UserLogService> _logger;

        public UserLogService(
            IUserLogRepository userLogRepository,
            ILogger<UserLogService> logger)
        {
            _userLogRepository = userLogRepository;
            _logger = logger;
        }

        #region 查詢日誌

        /// <summary>
        /// 分頁查詢使用者活動記錄
        /// </summary>
        public async Task<UserLogPagedResponseDto> GetUserLogsAsync(long userId, UserLogQueryDto queryDto)
        {
            try
            {
                _logger.LogInformation(
                    "查詢使用者活動記錄：UserId={UserId}, PageNumber={PageNumber}, PageSize={PageSize}",
                    userId, queryDto.PageNumber, queryDto.PageSize
                );

                // 驗證分頁參數
                if (queryDto.PageNumber < 1)
                {
                    queryDto.PageNumber = 1;
                }

                if (queryDto.PageSize < 1 || queryDto.PageSize > 100)
                {
                    queryDto.PageSize = 10;
                }

                // 驗證日期範圍
                if (queryDto.StartDate.HasValue && queryDto.EndDate.HasValue)
                {
                    if (queryDto.StartDate > queryDto.EndDate)
                    {
                        return new UserLogPagedResponseDto
                        {
                            Success = false,
                            Message = "開始日期不能大於結束日期"
                        };
                    }
                }

                // 呼叫 Repository 查詢
                var pagedData = await _userLogRepository.GetUserLogsPagedAsync(userId, queryDto);

                return new UserLogPagedResponseDto
                {
                    Success = true,
                    Message = "查詢成功",
                    Data = pagedData
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查詢使用者活動記錄失敗：UserId={UserId}", userId);

                return new UserLogPagedResponseDto
                {
                    Success = false,
                    Message = "查詢失敗，請稍後再試"
                };
            }
        }

        /// <summary>
        /// 取得使用者活動統計資料
        /// </summary>
        public async Task<UserLogStatsResponseDto> GetUserLogStatsAsync(long userId)
        {
            try
            {
                _logger.LogInformation("取得使用者活動統計：UserId={UserId}", userId);

                var stats = await _userLogRepository.GetUserLogStatsAsync(userId);

                return new UserLogStatsResponseDto
                {
                    Success = true,
                    Message = "取得統計資料成功",
                    Data = stats
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得使用者活動統計失敗：UserId={UserId}", userId);

                return new UserLogStatsResponseDto
                {
                    Success = false,
                    Message = "取得統計資料失敗，請稍後再試"
                };
            }
        }

        /// <summary>
        /// 取得單筆日誌詳細資料
        /// </summary>
        public async Task<UserLogPagedResponseDto> GetUserLogByIdAsync(long userId, long logId)
        {
            try
            {
                _logger.LogInformation(
                    "取得單筆日誌詳細資料：UserId={UserId}, LogId={LogId}",
                    userId, logId
                );

                var log = await _userLogRepository.GetUserLogByIdAsync(userId, logId);

                if (log == null)
                {
                    return new UserLogPagedResponseDto
                    {
                        Success = false,
                        Message = "找不到指定的日誌記錄"
                    };
                }

                // 將單筆資料包裝成分頁格式（為了統一回應格式）
                var pagedData = new PagedData<UserLogDto>
                {
                    Items = new List<UserLogDto> { log },
                    TotalCount = 1,
                    PageNumber = 1,
                    PageSize = 1,
                    TotalPages = 1
                };

                return new UserLogPagedResponseDto
                {
                    Success = true,
                    Message = "取得日誌詳細資料成功",
                    Data = pagedData
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "取得單筆日誌詳細資料失敗：UserId={UserId}, LogId={LogId}",
                    userId, logId
                );

                return new UserLogPagedResponseDto
                {
                    Success = false,
                    Message = "取得日誌詳細資料失敗，請稍後再試"
                };
            }
        }

        /// <summary>
        /// 取得最近登入記錄
        /// </summary>
        public async Task<UserLogPagedResponseDto> GetRecentLoginLogsAsync(long userId, int count = 5)
        {
            try
            {
                _logger.LogInformation(
                    "取得最近登入記錄：UserId={UserId}, Count={Count}",
                    userId, count
                );

                // 驗證數量
                if (count < 1 || count > 20)
                {
                    count = 5;
                }

                var logs = await _userLogRepository.GetRecentLoginLogsAsync(userId, count);

                // 包裝成分頁格式
                var pagedData = new PagedData<UserLogDto>
                {
                    Items = logs,
                    TotalCount = logs.Count,
                    PageNumber = 1,
                    PageSize = count,
                    TotalPages = 1
                };

                return new UserLogPagedResponseDto
                {
                    Success = true,
                    Message = "取得最近登入記錄成功",
                    Data = pagedData
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得最近登入記錄失敗：UserId={UserId}", userId);

                return new UserLogPagedResponseDto
                {
                    Success = false,
                    Message = "取得最近登入記錄失敗，請稍後再試"
                };
            }
        }

        #endregion

        #region 匯出功能

        /// <summary>
        /// 匯出使用者活動記錄為 CSV
        /// </summary>
        public async Task<byte[]> ExportUserLogsToCsvAsync(long userId, UserLogQueryDto queryDto)
        {
            try
            {
                _logger.LogInformation("匯出使用者活動記錄：UserId={UserId}", userId);

                // 取得所有符合條件的資料（不分頁）
                var allLogsQuery = new UserLogQueryDto
                {
                    PageNumber = 1,
                    PageSize = 10000, // 設定一個較大的數字以取得所有資料
                    ActionType = queryDto.ActionType,
                    ActionCategory = queryDto.ActionCategory,
                    StartDate = queryDto.StartDate,
                    EndDate = queryDto.EndDate,
                    Status = queryDto.Status,
                    Severity = queryDto.Severity
                };

                var pagedData = await _userLogRepository.GetUserLogsPagedAsync(userId, allLogsQuery);
                var logs = pagedData.Items;

                // 建立 CSV 內容
                var csv = new StringBuilder();

                // CSV 標題列
                csv.AppendLine("日誌ID,使用者ID,狀態,操作類型,操作類別,操作描述,目標類型,目標ID,IP位址,裝置類型,系統名稱,嚴重性,錯誤訊息,執行時間(ms),執行者,建立時間");

                // 資料列
                foreach (var log in logs)
                {
                    csv.AppendLine(
                        $"{log.LogId}," +
                        $"{log.UserId}," +
                        $"\"{EscapeCsvField(log.Status)}\"," +
                        $"\"{EscapeCsvField(log.ActionType)}\"," +
                        $"\"{EscapeCsvField(log.ActionCategory)}\"," +
                        $"\"{EscapeCsvField(log.ActionDescription)}\"," +
                        $"\"{EscapeCsvField(log.TargetType)}\"," +
                        $"{log.TargetId}," +
                        $"\"{EscapeCsvField(log.IpAddress)}\"," +
                        $"\"{EscapeCsvField(log.DeviceType)}\"," +
                        $"\"{EscapeCsvField(log.SystemName)}\"," +
                        $"\"{EscapeCsvField(log.Severity)}\"," +
                        $"\"{EscapeCsvField(log.ErrorMessage)}\"," +
                        $"{log.ExecutionTime}," +
                        $"\"{EscapeCsvField(log.PerformedBy)}\"," +
                        $"{log.CreatedAt:yyyy-MM-dd HH:mm:ss}"
                    );
                }

                // 轉換為 byte array（使用 UTF-8 with BOM，Excel 才能正確顯示中文）
                var preamble = Encoding.UTF8.GetPreamble();
                var content = Encoding.UTF8.GetBytes(csv.ToString());
                var result = new byte[preamble.Length + content.Length];

                Array.Copy(preamble, 0, result, 0, preamble.Length);
                Array.Copy(content, 0, result, preamble.Length, content.Length);

                _logger.LogInformation(
                    "匯出使用者活動記錄成功：UserId={UserId}, 筆數={Count}",
                    userId, logs.Count
                );

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "匯出使用者活動記錄失敗：UserId={UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// 處理 CSV 欄位特殊字元
        /// </summary>
        private string EscapeCsvField(string? field)
        {
            if (string.IsNullOrEmpty(field))
            {
                return string.Empty;
            }

            // 如果包含雙引號，需要轉義
            if (field.Contains("\""))
            {
                field = field.Replace("\"", "\"\"");
            }

            // 如果包含逗號、換行符號或雙引號，需要用雙引號包起來
            if (field.Contains(",") || field.Contains("\n") || field.Contains("\r") || field.Contains("\""))
            {
                return field;
            }

            return field;
        }

        #endregion
    }
}