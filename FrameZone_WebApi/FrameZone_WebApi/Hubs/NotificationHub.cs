using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace FrameZone_WebApi.Hubs
{
    /// <summary>
    /// 通知 SignalR Hub - 即時推送通知給前端
    /// </summary>
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 連接時自動加入以使用者 ID 為名稱的群組
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            if (userId.HasValue)
            {
                var groupName = GetUserGroupName(userId.Value);
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                _logger.LogInformation("🔔 SignalR 連接成功，UserId={UserId}, ConnectionId={ConnectionId}", userId.Value, Context.ConnectionId);
            }
            else
            {
                _logger.LogWarning("⚠️ SignalR 連接失敗：無法取得 UserId，ConnectionId={ConnectionId}", Context.ConnectionId);
            }

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// 斷線時從群組移除
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();
            if (userId.HasValue)
            {
                var groupName = GetUserGroupName(userId.Value);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
                _logger.LogInformation("🔕 SignalR 斷線，UserId={UserId}, ConnectionId={ConnectionId}", userId.Value, Context.ConnectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// 從 JWT Token 取得使用者 ID
        /// </summary>
        private long? GetUserId()
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                return null;
            }
            return userId;
        }

        /// <summary>
        /// 取得使用者群組名稱
        /// </summary>
        private static string GetUserGroupName(long userId)
        {
            return $"user_{userId}";
        }

        /// <summary>
        /// 推送新通知給特定使用者（供 Service 層呼叫）
        /// 注意：此方法不是由前端直接呼叫，而是透過 IHubContext 從 Service 層呼叫
        /// </summary>
        public static async Task SendNotificationToUserAsync(
            IHubContext<NotificationHub> hubContext,
            long userId,
            object notification)
        {
            var groupName = GetUserGroupName(userId);
            await hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", notification);
        }

        /// <summary>
        /// 推送未讀數更新給特定使用者（供 Service 層呼叫）
        /// </summary>
        public static async Task SendUnreadCountUpdateAsync(
            IHubContext<NotificationHub> hubContext,
            long userId,
            int unreadCount)
        {
            var groupName = GetUserGroupName(userId);
            await hubContext.Clients.Group(groupName).SendAsync("UnreadCountUpdated", unreadCount);
        }
    }
}