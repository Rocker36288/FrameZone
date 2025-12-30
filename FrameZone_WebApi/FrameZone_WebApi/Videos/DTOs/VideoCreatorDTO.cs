using FrameZone_WebApi.Videos.Enums;
using System.ComponentModel.DataAnnotations;

namespace FrameZone_WebApi.Videos.DTOs
{
    public class VideoCreatorDTO
    {
        //創作者影片資訊
        public class VideoDetailDto
        {
            [Key]
            public int VideoId { get; set; }

            // ─── 核心內容 ─────────────────
            [Required, MaxLength(255)]
            public string Title { get; set; } = string.Empty;

            public string Description { get; set; } = string.Empty;

            [MaxLength(500)]
            public string Thumbnail { get; set; } = string.Empty;

            // ─── 時間相關 ─────────────────
            public int Duration { get; set; }

            public DateTime PublishDate { get; set; } = DateTime.UtcNow;
            public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
            public DateTime UpdateDate { get; set; } = DateTime.UtcNow;
            

            // ─── 成效數據 ─────────────────
            public int ViewsCount { get; set; } = 0;
            public int LikesCount { get; set; } = 0;
            public int CommentCount { get; set; } = 0;

            // ─── 資源 / 技術 ──────────────
            [MaxLength(500)]
            public string VideoUrl { get; set; } = string.Empty;

            public string Resolution { get; set; } = "";

            [Required]
            public ProcessStatus ProcessStatus { get; set; } = ProcessStatus.UPLOADING;

            [Required]
            public PrivacyStatus PrivacyStatus { get; set; } = PrivacyStatus.DRAFT;
        }

        //首頁
        public class VideoCreatorOverviewDto
        {
            public int TotalVideos { get; set; }
            public int TotalViews { get; set; }
            public int TotalLikes { get; set; }
            public int TotalFollowers { get; set; }
            public int WeeklyViews { get; set; }
        }

        public class VideoCreatorTodoDto
        {
            public int UnfinishedDescriptionCount { get; set; } // 尚未填寫描述影片數
            public int UnansweredCommentsCount { get; set; }   // 未回覆留言數
            public int DraftVideosCount { get; set; }          // 草稿影片數
            public int DaysSinceLastUpload { get; set; }       // 超過多少天未上傳
        }
    }
}
