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

        public class UpdateVideoMetadataDto
        {
            public string Title { get; set; } = string.Empty;
            public string? Description { get; set; }
            public string PrivacyStatus { get; set; } = "PRIVATE";
        }

        //分析相關DTO
        public class CreatorAnalyticsDto
        {
            /// <summary>總觀看次數</summary>
            public long TotalViews { get; set; }

            /// <summary>訂閱總數</summary>
            public int TotalSubscribers { get; set; }

            /// <summary>影片總數</summary>
            public int TotalVideos { get; set; }

            /// <summary>平均互動率 (%)</summary>
            public double AvgEngagement { get; set; }

            /// <summary>觀看成長率 (%)</summary>
            public double ViewsGrowth { get; set; }

            /// <summary>訂閱成長率 (%)</summary>
            public double SubscribersGrowth { get; set; }

            /// <summary>趨勢圖資料</summary>
            public List<CreatorChartDto> PerformanceChart { get; set; } = new();

            /// <summary>最新影片表現</summary>
            public List<RecentVideoDto> RecentVideos { get; set; } = new();
        }

        public class CreatorChartDto
        {
            /// <summary>日期（MM/dd）</summary>
            public string Date { get; set; } = string.Empty;

            /// <summary>當日觀看數</summary>
            public int Views { get; set; }

            /// <summary>當日互動率（預留）</summary>
            public double Engagement { get; set; }
        }

        public class RecentVideoDto
        {
            public int VideoId { get; set; }

            public string Title { get; set; } = string.Empty;

            public string ThumbnailUrl { get; set; } = string.Empty;

            public DateTime PublishDate { get; set; }

            public long Views { get; set; }

            public int Likes { get; set; }

            public int Comments { get; set; }
        }

        public class DailyViewStatDto
        {
            public DateTime Date { get; set; }

            public int Views { get; set; }
        }

        public class RecentVideoStatDto
        {
            public int VideoId { get; set; }

            public string Title { get; set; } = string.Empty;

            public string ThumbnailUrl { get; set; } = string.Empty;

            public DateTime PublishDate { get; set; }

            public long Views { get; set; }

            public int Likes { get; set; }

            public int Comments { get; set; }
        }

    }
}
