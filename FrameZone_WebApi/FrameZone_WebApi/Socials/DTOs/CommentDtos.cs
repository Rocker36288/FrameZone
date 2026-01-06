using System.ComponentModel.DataAnnotations;

namespace FrameZone_WebApi.Socials.DTOs
{
    public class CommentCreateDto
    {
        [Required]
        public int PostId { get; set; }

        public int? ParentCommentId { get; set; }

        [Required]
        [MaxLength(500)]
        public string CommentContent { get; set; } = null!;
    }

    public class CommentUpdateDto
    {
        [Required]
        [MaxLength(500)]
        public string CommentContent { get; set; } = null!;
    }

    public class CommentReadDto
    {
        public int CommentId { get; set; }
        public long UserId { get; set; }

        public string? DisplayName { get; set; }
        public string? Avatar { get; set; }

        public int CommentTargetId { get; set; }
        public int? ParentCommentId { get; set; }

        public string CommentContent { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public int LikeCount { get; set; }

        public bool IsOwner { get; set; }   //就是本人

        public List<CommentReadDto> Replies { get; set; } = new();
    }
}
