using System.Security.Claims;
using FrameZone_WebApi.Socials.Repositories;
using FrameZone_WebApi.Socials.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FrameZone_WebApi.Socials.Hubs
{
    [Authorize]
    public class SocialChatHub : Hub
    {
        public const string ReceiveMessageEvent = "ReceiveMessage";

        private readonly ChatRoomRepository _roomRepo;
        private readonly MessageService _messageService;

        public SocialChatHub(ChatRoomRepository roomRepo, MessageService messageService)
        {
            _roomRepo = roomRepo;
            _messageService = messageService;
        }

        public static string GetGroupName(int roomId) => $"room:{roomId}";

        public override async Task OnConnectedAsync()
        {
            if (!TryGetUserId(out var userId))
                throw new HubException("Unauthorized");

            var httpContext = Context.GetHttpContext();
            var roomIdRaw = httpContext?.Request.Query["roomId"].FirstOrDefault();

            if (!int.TryParse(roomIdRaw, out var roomId))
                throw new HubException("roomId is required");

            if (!_roomRepo.IsUserInRoom(roomId, userId))
                throw new HubException("User not in room");

            await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(roomId));
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var roomIdRaw = Context.GetHttpContext()?.Request.Query["roomId"].FirstOrDefault();
            if (int.TryParse(roomIdRaw, out var roomId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupName(roomId));
            }
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// 透過 Hub 直接送文字訊息（可選，前端目前仍用 REST）。
        /// </summary>
        public async Task SendMessage(int roomId, string messageContent)
        {
            if (!TryGetUserId(out var userId))
                throw new HubException("Unauthorized");

            var content = messageContent?.Trim();
            if (string.IsNullOrWhiteSpace(content))
                throw new HubException("MessageContent is required");

            if (!_roomRepo.IsUserInRoom(roomId, userId))
                throw new HubException("User not in room");

            var saved = _messageService.SendTextMessage(roomId, userId, content);
            await Clients.Group(GetGroupName(roomId)).SendAsync(ReceiveMessageEvent, saved);
        }

        private bool TryGetUserId(out long userId)
        {
            userId = 0;
            var idClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return !string.IsNullOrEmpty(idClaim) && long.TryParse(idClaim, out userId);
        }
    }
}
