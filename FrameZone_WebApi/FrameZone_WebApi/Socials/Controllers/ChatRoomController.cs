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
            long userId;
            try
            {
                userId = GetUserId();
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            if (userId == dto.TargetUserId)
                return BadRequest("銝?芸楛蝘??芸楛");
            return _roomService.GetOrCreateSocialPrivateRoom(userId, dto.TargetUserId);
        }

        [Authorize]
        [HttpPost("private/shopping")]
        public ActionResult<ChatRoomDto> CreateShoppingPrivateRoom(CreatePrivateRoomDto dto)
        {
            long userId;
            try
            {
                userId = GetUserId();
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            if (userId == dto.TargetUserId)
                return BadRequest("銝?芸楛蝘??芸楛");
            return _roomService.GetOrCreateShoppingPrivateRoom(userId, dto.TargetUserId);
        }

        [Authorize]
        [HttpPost("group")]
        public ActionResult<ChatRoomDto> CreateGroupRoom(CreateGroupRoomDto dto)
        {
            long userId;
            try
            {
                userId = GetUserId();
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            return _roomService.CreateGroupRoom(userId, dto);
        }

        [Authorize]
        [HttpGet("rooms")]
        public ActionResult<List<ChatRoomDto>> GetMyRooms()
        {
            long userId;
            try
            {
                userId = GetUserId();
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            return _roomService.GetUserRooms(userId);
        }

        [Authorize]
        [HttpGet("recent/social")]
        public ActionResult<List<RecentChatDto>> GetRecentSocialChats()
        {
            long userId;
            try
            {
                userId = GetUserId();
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            return _roomService.GetRecentSocialChats(userId);
        }

        [Authorize]
        [HttpGet("unread/social")]
        public ActionResult<List<UnreadCountDto>> GetUnreadCounts()
        {
            long userId;
            try
            {
                userId = GetUserId();
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            return _roomService.GetUnreadCounts(userId);
        }

        [HttpGet("{roomId}/messages")]
        public ActionResult<List<MessageDto>> GetMessages(int roomId)
        {
            long? currentUserId = null;
            try
            {
                currentUserId = GetUserId();
            }
            catch (UnauthorizedAccessException)
            {
            }
            return _messageService.GetMessages(roomId, currentUserId);
        }

        [Authorize]
        [HttpPost("{roomId}/read")]
        public IActionResult MarkRoomRead(int roomId)
        {
            long userId;
            try
            {
                userId = GetUserId();
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            if (!_roomService.IsUserInRoom(roomId, userId))
                return Forbid();

            var affected = _messageService.MarkRoomRead(userId, roomId);
            return Ok(new { readCount = affected });
        }

        [Authorize]
        [HttpPost("{roomId}/messages")]
        public async Task<ActionResult<MessageDto>> SendMessage(int roomId, SendMessageDto dto)
        {
            long userId;
            try
            {
                userId = GetUserId();
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            var content = dto?.MessageContent?.Trim();
            if (string.IsNullOrWhiteSpace(content))
                return BadRequest("閮?批捆銝?箇征");
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
            long userId;
            try
            {
                userId = GetUserId();
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }

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

        private long GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                throw new UnauthorizedAccessException("尚未登入");

            return long.Parse(userIdClaim);
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

