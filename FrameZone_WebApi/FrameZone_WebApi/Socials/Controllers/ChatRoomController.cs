using FrameZone_WebApi.Socials.DTOs;
using FrameZone_WebApi.Socials.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FrameZone_WebApi.Socials.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatRoomController : ControllerBase
    {
        private readonly ChatRoomService _roomService;
        private readonly MessageService _messageService;

        public ChatRoomController(
            ChatRoomService roomService,
            MessageService messageService)
        {
            _roomService = roomService;
            _messageService = messageService;
        }

        // =======================
        // 社交私聊
        // =======================
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

        // =======================
        // 商城私聊
        // =======================
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

        // =======================
        // 多人聊天室
        // =======================
        [Authorize]
        [HttpPost("group")]
        public ActionResult<ChatRoomDto> CreateGroupRoom(CreateGroupRoomDto dto)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();
            return _roomService.CreateGroupRoom(userId, dto);
        }

        // =======================
        // 取得使用者聊天室清單（私聊 + 群聊）
        // =======================
        [Authorize]
        [HttpGet("rooms")]
        public ActionResult<List<ChatRoomDto>> GetMyRooms()
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();
            return _roomService.GetUserRooms(userId);
        }

        // =======================
        // 取得聊天室訊息
        // =======================
        [HttpGet("{roomId}/messages")]
        public ActionResult<List<MessageDto>> GetMessages(int roomId)
        {
            return _messageService.GetMessages(roomId);
        }

        // =======================
        // 發送聊天室文字訊息
        // =======================
        [Authorize]
        [HttpPost("{roomId}/messages")]
        public ActionResult<MessageDto> SendMessage(int roomId, SendMessageDto dto)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();
            var content = dto?.MessageContent?.Trim();
            if (string.IsNullOrWhiteSpace(content))
                return BadRequest("訊息內容不可為空");
            return _messageService.SendTextMessage(roomId, userId, content);
        }

        private bool TryGetUserId(out long userId)
        {
            userId = 0;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return !string.IsNullOrEmpty(userIdClaim) && long.TryParse(userIdClaim, out userId);
        }
    }

}
