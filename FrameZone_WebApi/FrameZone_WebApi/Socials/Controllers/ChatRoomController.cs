using FrameZone_WebApi.Socials.DTOs;
using FrameZone_WebApi.Socials.Hubs;
using FrameZone_WebApi.Socials.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace FrameZone_WebApi.Socials.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatRoomController : ControllerBase
    {
        private readonly ChatRoomService _roomService;
        private readonly MessageService _messageService;
        private readonly IHubContext<SocialChatHub> _chatHubContext;
        private readonly SocialChatConnectionManager _connectionManager;

        public ChatRoomController(
            ChatRoomService roomService,
            MessageService messageService,
            IHubContext<SocialChatHub> chatHubContext,
            SocialChatConnectionManager connectionManager)
        {
            _roomService = roomService;
            _messageService = messageService;
            _chatHubContext = chatHubContext;
            _connectionManager = connectionManager;
        }

        [Authorize]
        [HttpPost("private/social")]
        public ActionResult<ChatRoomDto> CreateSocialPrivateRoom(CreatePrivateRoomDto dto)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();
            if (userId == dto.TargetUserId)
                return BadRequest("不能自己私訊自己");
            return _roomService.GetOrCreateSocialPrivateRoom(userId, dto.TargetUserId);
        }

        [Authorize]
        [HttpPost("private/shopping")]
        public ActionResult<ChatRoomDto> CreateShoppingPrivateRoom(CreatePrivateRoomDto dto)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();
            if (userId == dto.TargetUserId)
                return BadRequest("不能自己私訊自己");
            return _roomService.GetOrCreateShoppingPrivateRoom(userId, dto.TargetUserId);
        }

        [Authorize]
        [HttpPost("group")]
        public ActionResult<ChatRoomDto> CreateGroupRoom(CreateGroupRoomDto dto)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();
            return _roomService.CreateGroupRoom(userId, dto);
        }

        [Authorize]
        [HttpGet("rooms")]
        public ActionResult<List<ChatRoomDto>> GetMyRooms()
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();
            return _roomService.GetUserRooms(userId);
        }

        [HttpGet("{roomId}/messages")]
        public ActionResult<List<MessageDto>> GetMessages(int roomId)
        {
            long? currentUserId = TryGetUserId(out var userId) ? userId : (long?)null;
            return _messageService.GetMessages(roomId, currentUserId);
        }

        [Authorize]
        [HttpPost("{roomId}/messages")]
        public async Task<ActionResult<MessageDto>> SendMessage(int roomId, SendMessageDto dto)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();
            var content = dto?.MessageContent?.Trim();
            if (string.IsNullOrWhiteSpace(content))
                return BadRequest("訊息內容不可為空");
            var message = _messageService.SendTextMessage(roomId, userId, content, userId);
            var messageForOthers = CloneWithOwner(message, false);

            var senderConnections = _connectionManager.GetConnections(userId);
            await _chatHubContext.Clients.Clients(senderConnections)
                .SendAsync(SocialChatHub.ReceiveMessageEvent, message);
            await _chatHubContext.Clients.GroupExcept(SocialChatHub.GetGroupName(roomId), senderConnections)
                .SendAsync(SocialChatHub.ReceiveMessageEvent, messageForOthers);
            return message;
        }

        [Authorize]
        [HttpPost("{roomId}/messages/shop")]
        public async Task<ActionResult<MessageDto>> SendShopMessage(int roomId, SendShopMessageDto dto)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            try
            {
                var message = _messageService.SendShopMessage(roomId, userId, dto, userId);
                var messageForOthers = CloneWithOwner(message, false);

                var senderConnections = _connectionManager.GetConnections(userId);
                await _chatHubContext.Clients.Clients(senderConnections)
                    .SendAsync(SocialChatHub.ReceiveMessageEvent, message);
                await _chatHubContext.Clients.GroupExcept(SocialChatHub.GetGroupName(roomId), senderConnections)
                    .SendAsync(SocialChatHub.ReceiveMessageEvent, messageForOthers);
                return message;
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private bool TryGetUserId(out long userId)
        {
            userId = 0;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return !string.IsNullOrEmpty(userIdClaim) && long.TryParse(userIdClaim, out userId);
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
    }

}
