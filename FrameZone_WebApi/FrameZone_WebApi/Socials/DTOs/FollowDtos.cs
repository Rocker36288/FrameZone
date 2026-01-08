using System;

namespace FrameZone_WebApi.Socials.DTOs
{
    public class FollowResponseDto
    {
        public long FollowerId { get; set; }
        public long FollowingId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class FollowUserDto
    {
        public long UserId { get; set; }
        public string DisplayName { get; set; } = "使用者";
        public string? Avatar { get; set; }
    }
}
