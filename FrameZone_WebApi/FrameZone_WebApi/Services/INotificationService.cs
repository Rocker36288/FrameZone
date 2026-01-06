using FrameZone_WebApi.DTOs;

namespace FrameZone_WebApi.Services.Interfaces
{
    /// <summary>
    /// 通知服務介面
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// 取得使用者未讀通知數量
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>未讀數量統計</returns>
        Task<ServiceResult<UnreadCountDto>> GetUnreadCountAsync(long userId);

        // <summary>
        /// 取得使用者通知清單（分頁）
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="query">查詢參數</param>
        /// <returns>分頁通知清單</returns>
        Task<ServiceResult<NotificationPagedResultDto>> GetNotificationsAsync(long userId, NotificationQueryDto query);

        // <summary>
        /// 取得單筆通知詳細資訊
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="recipientId">通知接收者 ID</param>
        /// <returns>通知詳細資訊</returns>
        Task<ServiceResult<NotificationDto>> GetNotificationByIdAsync(long userId, long recipientId);

        /// <summary>
        /// 標記單筆通知為已讀
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="recipientId">通知接收者 ID</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult<bool>> MarkAsReadAsync(long userId, long recipientId);

        /// <summary>
        /// 批次標記通知為已讀
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="dto">標記已讀 DTO</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult<int>> MarkBatchAsReadAsync(long userId, MarkAsReadDto dto);

        /// <summary>
        /// 標記所有通知為已讀
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="systemCode">系統代碼（null = 全部系統）</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult<int>> MarkAllAsReadAsync(long userId, string? systemCode = null);

        /// <summary>
        /// 刪除單筆通知
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="recipientId">通知接收者 ID</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult<bool>> DeleteNotificationAsync(long userId, long recipientId);

        /// <summary>
        /// 批次刪除通知
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="dto">刪除通知 DTO</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult<int>> DeleteBatchNotificationsAsync(long userId, DeleteNotificationDto dto);

        /// <summary>
        /// 清空所有通知
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="systemCode">系統代碼（null = 全部系統）</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult<int>> ClearAllNotificationsAsync(long userId, string? systemCode = null);

        /// <summary>
        /// 取得系統模組清單（含未讀數）
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>系統模組清單</returns>
        Task<ServiceResult<List<SystemModuleDto>>> GetSystemModulesAsync(long userId);

        /// <summary>
        /// 發送通知給單一使用者
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="systemCode">系統代碼</param>
        /// <param name="categoryCode">類別代碼</param>
        /// <param name="title">標題</param>
        /// <param name="content">內容</param>
        /// <param name="priorityCode">優先級代碼（預設 MEDIUM）</param>
        /// <param name="relatedObjectType">相關物件類型</param>
        /// <param name="relatedObjectId">相關物件 ID</param>
        /// <returns>通知 ID</returns>
        Task<ServiceResult<long>> SendNotificationAsync(
            long userId,
            string systemCode,
            string categoryCode,
            string title,
            string content,
            string priorityCode = "MEDIUM",
            string? relatedObjectType = null,
            long? relatedObjectId = null);

        /// <summary>
        /// 批次發送通知給多個使用者
        /// </summary>
        /// <param name="userIds">使用者 ID 列表</param>
        /// <param name="systemCode">系統代碼</param>
        /// <param name="categoryCode">類別代碼</param>
        /// <param name="title">標題</param>
        /// <param name="content">內容</param>
        /// <param name="priorityCode">優先級代碼（預設 MEDIUM）</param>
        /// <param name="relatedObjectType">相關物件類型</param>
        /// <param name="relatedObjectId">相關物件 ID</param>
        /// <returns>成功發送的數量</returns>
        Task<ServiceResult<int>> SendBatchNotificationsAsync(
            List<long> userIds,
            string systemCode,
            string categoryCode,
            string title,
            string content,
            string priorityCode = "MEDIUM",
            string? relatedObjectType = null,
            long? relatedObjectId = null);
    }

    /// <summary>
    /// 服務執行結果包裝類別
    /// </summary>
    public class ServiceResult<T>
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 訊息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        // <summary>
        /// 回傳資料
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// 錯誤代碼（選用）
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// 建立成功結果
        /// </summary>
        public static ServiceResult<T> SuccessResult(T data, string message = "操作成功")
        {
            return new ServiceResult<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        /// <summary>
        /// 建立失敗結果
        /// </summary>
        public static ServiceResult<T> FailureResult(string message, string? errorCode = null)
        {
            return new ServiceResult<T>
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode
            };
        }
    }
}


