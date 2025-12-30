namespace FrameZone_WebApi.Videos.DTOs
{
    //用於傳輸資料給前端的影片卡片資料
    public class VideoCardDto
    {
        // ── 影片識別 ─────────────────────

        // ⚠️ 內部識別（可留、但前端不該用來導頁）
        public int VideoId { get; set; }

        // ✅ 對外識別（GUID 檔名 / 播放位置）
        public string VideoUri { get; set; } = "";

        // ── 影片資訊 ─────────────────────
        public string Title { get; set; } = "";
        public string Thumbnail { get; set; } = "";
        public int Duration { get; set; }
        public int Views { get; set; }
        public int Likes { get; set; }
        public DateTime PublishDate { get; set; }
        public string? Description { get; set; }

        // ── 頻道資訊 ─────────────────────
        public long ChannelId { get; set; }
        public string ChannelName { get; set; } = "";
        public string Avatar { get; set; } = "";
    }

    // 留言卡片
    public class VideoCommentDto
    {
        public int Id { get; set; } = 0;
        public string UserName { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int Likes { get; set; } = 0;

        // 可選回覆
        public List<VideoCommentDto>? Replies { get; set; } = new List<VideoCommentDto>();

        // 預設建構子
        public VideoCommentDto() { }

        // 可以透過傳入部分資料初始化
        public VideoCommentDto(VideoCommentDto data)
        {
            if (data != null)
            {
                Id = data.Id;
                UserName = data.UserName;
                Avatar = data.Avatar;
                Message = data.Message;
                CreatedAt = data.CreatedAt;
                Likes = data.Likes;
                Replies = data.Replies ?? new List<VideoCommentDto>();
            }
        }
    }

    public class VideoCommentRequest
    {
        public long UserId { get; set; }
        public int Videoid { get; set; }
        public int TargetTypeId { get; set; }
        public string CommentContent { get; set; } = string.Empty; public int? ParentCommentId { get; set; }
    }


    public class ChannelCardDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Follows { get; set; }
    }

    // 頻道首頁完整資料
    public class ChannelHomeDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public string Banner { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public int Follows { get; set; }
        public int VideosCount { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdateAt { get; set; }
    }

    //影片likes DTO
    public class VideoLikesRequset
    {
        public bool IsLikes { get; set; }
        public int VideoId { get; set; }
    }

    public class VideoLikesDto
    {
       public bool IsLikes { get; set; }
    }

    //頻道 DTO
    public class ChannelFollowRequset
    {
        public bool IsFollow { get; set; }
        public int ChannelId { get; set; }
    }
    public class ChannelFollowDto
    {
        public bool IsFollow { get; set; }
    }
}
