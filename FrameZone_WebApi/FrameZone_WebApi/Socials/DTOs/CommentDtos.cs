namespace FrameZone_WebApi.Socials.DTOs
{
    public class CommentCreateDto
    {
        public int PostId { get; set; } // 前端傳入貼文 ID
        public int? ParentCommentId { get; set; } // 如果是回覆某則留言則傳入 ID，否則為 null
        public string CommentContent { get; set; } // 留言內容
    }

    public class CommentReadDto
    {
        public int CommentId { get; set; }
        public long UserId { get; set; }
        public string DisplayName { get; set; }
        public string Avatar { get; set; }
        public int CommentTargetId { get; set; }
        public int? ParentCommentId { get; set; }
        public string CommentContent { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int LikeCount { get; set; }

        // 用於存放下一層的回覆，形成無限遞迴結構
        public List<CommentReadDto> Replies { get; set; } = new List<CommentReadDto>();
    }
    public class CommentUpdateDto
    {
        public string CommentContent { get; set; }
    }
}
