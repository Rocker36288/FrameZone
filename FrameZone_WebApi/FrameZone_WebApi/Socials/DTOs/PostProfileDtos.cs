namespace FrameZone_WebApi.Socials.DTOs
{
    public class UserProfileSummaryDto
    {
        public long UserId { get; set; }
        public string DisplayName { get; set; } = "使用者";
        public string? Avatar { get; set; }
        public int FollowingCount { get; set; }
        public int FollowerCount { get; set; }
    }
}
