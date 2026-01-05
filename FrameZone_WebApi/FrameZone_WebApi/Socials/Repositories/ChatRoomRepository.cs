using FrameZone_WebApi.Models;

namespace FrameZone_WebApi.Socials.Repositories
{
    public class ChatRoomRepository
    {
        private readonly AAContext _context;
        public ChatRoomRepository(AAContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 取得私聊聊天室 RoomId
        /// 如果使用者已經有共同的私聊房間，回傳 RoomId
        /// 若不存在則回傳 0
        /// </summary>
        public int GetPrivateRoomId(long userId, long targetUserId, string roomCategory)
        {
            return _context.ChatMembers
                .Where(cm =>
                    (cm.UserId == userId || cm.UserId == targetUserId) &&
                    cm.Room.RoomType == "Private" &&
                    cm.Room.RoomCategory == roomCategory &&
                    cm.LeaveAt == null)
                .GroupBy(cm => cm.RoomId)
                .Where(g => g.Count() == 2)
                .Select(g => g.Key)
                .FirstOrDefault();
        }

        /// <summary>
        /// 建立新的私聊聊天室（不含成員）
        /// </summary>
        public ChatRoom CreatePrivateRoom(string roomCategory)
        {
            var room = new ChatRoom
            {
                RoomType = "Private",
                RoomCategory = roomCategory,
                CreatedAt = DateTime.UtcNow
            };

            _context.ChatRooms.Add(room);
            _context.SaveChanges();
            return room;
        }

        /// <summary>
        /// 建立多人聊天室（Group）
        /// </summary>
        public ChatRoom CreateGroupRoom(string roomName, string roomCategory)
        {
            var room = new ChatRoom
            {
                RoomType = "Group",
                RoomCategory = roomCategory,
                RoomName = roomName,
                CreatedAt = DateTime.UtcNow
            };

            _context.ChatRooms.Add(room);
            _context.SaveChanges();

            return room;
        }

        /// <summary>
        /// 將多位使用者加入聊天室
        /// （私聊 / 群聊共用）
        /// </summary>
        public void AddMembers(int roomId, IEnumerable<long> userIds)
        {
            foreach (var userId in userIds.Distinct())
            {
                _context.ChatMembers.Add(new ChatMember
                {
                    RoomId = roomId,
                    UserId = userId,
                    JoinAt = DateTime.UtcNow
                });
            }

            _context.SaveChanges();
        }

        /// <summary>
        /// 取得使用者參與的所有聊天室
        /// （之後聊天室清單會用到）
        /// </summary>
        public List<ChatRoom> GetUserRooms(long userId)
        {
            return _context.ChatMembers
                .Where(cm => cm.UserId == userId && cm.LeaveAt == null)
                .Select(cm => cm.Room)
                .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
                .ToList();
        }
    }
}
