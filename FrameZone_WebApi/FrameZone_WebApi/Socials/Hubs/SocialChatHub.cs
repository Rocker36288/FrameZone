using System.Security.Claims;
using FrameZone_WebApi.Socials.DTOs;
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
        private readonly SocialChatConnectionManager _connectionManager;

        public SocialChatHub(ChatRoomRepository roomRepo, MessageService messageService, SocialChatConnectionManager connectionManager)
        {
            _roomRepo = roomRepo;
            _messageService = messageService;
            _connectionManager = connectionManager;
        }

        public static string GetGroupName(int roomId) => $"room:{roomId}";

        public override async Task OnConnectedAsync()
        {
            long userId;
            try
            {
                userId = GetUserId();
            }
            catch (UnauthorizedAccessException)
            {
                throw new HubException("Unauthorized");
            }

            var httpContext = Context.GetHttpContext();
            var roomIdRaw = httpContext?.Request.Query["roomId"].FirstOrDefault();

            if (!int.TryParse(roomIdRaw, out var roomId))
                throw new HubException("roomId is required");

            if (!_roomRepo.IsUserInRoom(roomId, userId))
                throw new HubException("User not in room");

            await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(roomId));
            _connectionManager.Add(userId, Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var roomIdRaw = Context.GetHttpContext()?.Request.Query["roomId"].FirstOrDefault();
            if (int.TryParse(roomIdRaw, out var roomId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupName(roomId));
            }
            try
            {
                var userId = GetUserId();
                _connectionManager.Remove(userId, Context.ConnectionId);
            }
            catch (UnauthorizedAccessException)
            {
            }
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// ?? Hub ?湔??摮??荔??舫嚗?蝡舐????REST嚗?
        /// </summary>
        public async Task SendMessage(int roomId, string messageContent)
        {
            long userId;
            try
            {
                userId = GetUserId();
            }
            catch (UnauthorizedAccessException)
            {
                throw new HubException("Unauthorized");
            }

            var content = messageContent?.Trim();
            if (string.IsNullOrWhiteSpace(content))
                throw new HubException("MessageContent is required");

            if (!_roomRepo.IsUserInRoom(roomId, userId))
                throw new HubException("User not in room");

            var saved = _messageService.SendTextMessage(roomId, userId, content, userId);
            var messageForOthers = CloneWithOwner(saved, false);

            await Clients.Caller.SendAsync(ReceiveMessageEvent, saved);
            await Clients.OthersInGroup(GetGroupName(roomId)).SendAsync(ReceiveMessageEvent, messageForOthers);
        }

        private static MessageDto CloneWithOwner(MessageDto source, bool isOwner)
        {
            return new MessageDto
            {
                MessageId = source.MessageId,
                SenderUserId = source.SenderUserId,
                MessageContent = source.MessageContent,
                MessageType = source.MessageType,
                StickerId = source.StickerId,
                MediaUrl = source.MediaUrl,
                ThumbnailUrl = source.ThumbnailUrl,
                ProductId = source.ProductId,
                OrderId = source.OrderId,
                LinkTitle = source.LinkTitle,
                LinkDescription = source.LinkDescription,
                CreatedAt = source.CreatedAt,
                IsOwner = isOwner
            };
        }

        private long GetUserId()
        {
            var idClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (idClaim == null)
                throw new UnauthorizedAccessException("尚未登入");

            return long.Parse(idClaim);
        }
    }
}

