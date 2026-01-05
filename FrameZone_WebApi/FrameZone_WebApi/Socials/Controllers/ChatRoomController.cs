using FrameZone_WebApi.Socials.DTOs;
using FrameZone_WebApi.Socials.Services;
using Microsoft.AspNetCore.Mvc;

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
        [HttpPost("private/social")]
        public ActionResult<ChatRoomDto> CreateSocialPrivateRoom(CreatePrivateRoomDto dto)
        {
            long userId = 1; // TODO: 從 JWT 拿到使用者 ID
            return _roomService.GetOrCreateSocialPrivateRoom(userId, dto.TargetUserId);
        }

        // =======================
        // 商城私聊
        // =======================
        [HttpPost("private/shopping")]
        public ActionResult<ChatRoomDto> CreateShoppingPrivateRoom(CreatePrivateRoomDto dto)
        {
            long userId = 1; // TODO: 從 JWT 拿到使用者 ID
            return _roomService.GetOrCreateShoppingPrivateRoom(userId, dto.TargetUserId);
        }

        // =======================
        // 多人聊天室
        // =======================
        [HttpPost("group")]
        public ActionResult<ChatRoomDto> CreateGroupRoom(CreateGroupRoomDto dto)
        {
            long userId = 1; // TODO: 從 JWT 拿到使用者 ID
            return _roomService.CreateGroupRoom(userId, dto);
        }

        // =======================
        // 取得使用者聊天室清單（私聊 + 群聊）
        // =======================
        [HttpGet("rooms")]
        public ActionResult<List<ChatRoomDto>> GetMyRooms()
        {
            long userId = 1; // TODO: 從 JWT 拿到使用者 ID
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
    }

}
