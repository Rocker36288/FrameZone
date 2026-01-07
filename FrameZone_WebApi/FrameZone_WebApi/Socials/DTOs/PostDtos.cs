using System;
using System.ComponentModel.DataAnnotations;

namespace FrameZone_WebApi.Socials.DTOs
{
    // =========== 貼文相關 ========== 

    /// <summary>
    /// 貼文 DTO，可用於建立或編輯貼文
    /// </summary>
    public class PostDto
    {
        // 貼文文字內容
        [Required(ErrorMessage = "請輸入貼文內容")]
        [MaxLength(500, ErrorMessage = "貼文內容不能超過500個字")]
        public string PostContent { get; set; } = string.Empty;

        // 貼文種類: 社團 / 活動 / 個人
        public string? PostType { get; set; }

        // 貼文種類 Id: 對應社團Id / 活動Id / 個人(null)
        public int? PostTypeId { get; set; }
    }

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
        public bool IsOwner { get; set; }   //就是本人
        public int LikeCount { get; set; }
        public bool IsLiked { get; set; }
    }

    public class UserProfileSummaryDto
    {
        public long UserId { get; set; }
        public string DisplayName { get; set; } = "使用者";
        public string? Avatar { get; set; }
        public int FollowingCount { get; set; }
        public int FollowerCount { get; set; }
    }
}
