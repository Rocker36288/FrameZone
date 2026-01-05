namespace FrameZone_WebApi.Socials.DTOs
{
    public class CreatePrivateRoomDto
    {
        public long TargetUserId { get; set; }
    }

    public class CreateGroupRoomDto
    {
        // 群組名稱
        public string RoomName { get; set; }

        // 群組成員（不包含自己）
        public List<long> MemberUserIds { get; set; } = new();
    }

    public class ChatRoomDto
    {
        public int RoomId { get; set; }
        public string RoomType { get; set; }
        public string RoomCategory { get; set; }
        public string RoomName { get; set; }
    }

}
