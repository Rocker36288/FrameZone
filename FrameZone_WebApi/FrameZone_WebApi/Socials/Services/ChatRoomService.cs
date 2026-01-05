using FrameZone_WebApi.Socials.Constants;
using FrameZone_WebApi.Socials.DTOs;
using FrameZone_WebApi.Socials.Repositories;

namespace FrameZone_WebApi.Socials.Services
{
    public class ChatRoomService
    {
        private readonly ChatRoomRepository _roomRepo;

        public ChatRoomService(ChatRoomRepository roomRepo)
        {
            _roomRepo = roomRepo;
        }

        /// <summary>
        /// 共用：取得或建立私聊聊天室（依 RoomCategory）
        /// </summary>
        private ChatRoomDto GetOrCreatePrivateRoom(
            long userId,
            long targetUserId,
            string roomCategory)
        {
            if (userId == targetUserId)
                throw new InvalidOperationException("不能自己私訊自己");

            // 嘗試查詢是否已有對應分類的私聊聊天室
            var roomId = _roomRepo.GetPrivateRoomId(userId, targetUserId, roomCategory);

            if (roomId == 0)
            {
                // 不存在 → 建立新聊天室
                var room = _roomRepo.CreatePrivateRoom(roomCategory);

                // 將雙方加入聊天室
                _roomRepo.AddMembers(room.RoomId, new[] { userId, targetUserId });

                roomId = room.RoomId;
            }

            return new ChatRoomDto
            {
                RoomId = roomId,
                RoomType = "Private",
                RoomCategory = roomCategory
            };
        }

        /// <summary>
        /// 社交私聊
        /// </summary>
        public ChatRoomDto GetOrCreateSocialPrivateRoom(long userId, long targetUserId)
        {
            return GetOrCreatePrivateRoom(
                userId,
                targetUserId,
                RoomCategoryConst.Social
            );
        }

        /// <summary>
        /// 商城私聊
        /// </summary>
        public ChatRoomDto GetOrCreateShoppingPrivateRoom(long userId, long targetUserId)
        {
            return GetOrCreatePrivateRoom(
                userId,
                targetUserId,
                RoomCategoryConst.Shopping
            );
        }

        /// <summary>
        /// 建立多人聊天室
        /// </summary>
        public ChatRoomDto CreateGroupRoom(
            long creatorUserId,
            CreateGroupRoomDto dto)
        {
            var room = _roomRepo.CreateGroupRoom(
                dto.RoomName,
                RoomCategoryConst.Social // 群聊目前歸類為 Social
            );

            var memberIds = dto.MemberUserIds.ToList();
            memberIds.Add(creatorUserId);

            _roomRepo.AddMembers(room.RoomId, memberIds);

            return new ChatRoomDto
            {
                RoomId = room.RoomId,
                RoomName = room.RoomName,
                RoomType = room.RoomType,
                RoomCategory = room.RoomCategory
            };
        }

        /// <summary>
        /// 取得使用者的聊天室清單
        /// </summary>
        public List<ChatRoomDto> GetUserRooms(long userId)
        {
            return _roomRepo.GetUserRooms(userId)
                .Select(r => new ChatRoomDto
                {
                    RoomId = r.RoomId,
                    RoomType = r.RoomType,
                    RoomCategory = r.RoomCategory,
                    RoomName = r.RoomName
                })
                .ToList();
        }
    }

}
