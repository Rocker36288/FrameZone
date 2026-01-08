using System;

namespace FrameZone_WebApi.Socials.DTOs
{
    public class PostReadDto
    {
        public int PostId { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; } = "新使用者";
        public string? Avatar { get; set; }
        public string PostContent { get; set; } = string.Empty;
        public string? PostType { get; set; }
        public int? PostTypeId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsOwner { get; set; }
        public int LikeCount { get; set; }
        public bool IsLiked { get; set; }
        public int ShareCount { get; set; }
        public bool IsShared { get; set; }
        public bool IsSharedPost { get; set; }
        public int CommentCount { get; set; }
        public PostReadDto? SharedPost { get; set; }
    }
}
