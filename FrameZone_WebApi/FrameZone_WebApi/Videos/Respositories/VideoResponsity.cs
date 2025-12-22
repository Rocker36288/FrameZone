using FrameZone_WebApi.Models;
using FrameZone_WebApi.Videos.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FrameZone_WebApi.Videos.Repositories
{
    public class VideoRespository
    {
        private readonly AAContext _context;

        public VideoRespository(AAContext context)
        {
            _context = context;
        }

        /* =====================================================
         * 🎬 Video Card
         * ===================================================== */

        public async Task<VideoCardDto?> GetVideoCardByGuidAsync(string guid)
        {
            var video = await _context.Videos
                .AsNoTracking()
                .Include(v => v.Channel)
                    .ThenInclude(c => c.UserProfile)
                .FirstOrDefaultAsync(v => v.VideoUrl == guid);

            if (video == null || video.Channel == null)
                return null;

            var viewCount = await _context.Views
                .CountAsync(v => v.VideoId == video.VideoId);

            return new VideoCardDto
            {
                VideoId = video.VideoId,
                Title = video.Title ?? "",
                VideoUri = video.VideoUrl ?? "",
                Thumbnail = video.ThumbnailUrl ?? "",
                Duration = video.Duration ?? 0,
                Views = viewCount,
                PublishDate = video.PublishDate ?? DateTime.MinValue,
                Description = video.Description ?? "",
                ChannelId = video.ChannelId,
                ChannelName = video.Channel.ChannelName ?? "",
                Avatar = video.Channel.UserProfile?.Avatar ?? ""
            };
        }

        /* =====================================================
         * 💬 Comments
         * ===================================================== */

        public async Task<List<VideoCommentDto>> GetVideoCommentsAsync(string guid)
        {// 1️⃣ 取得影片
            var video = await _context.Videos.FirstOrDefaultAsync(v => v.VideoUrl == guid);
            if (video == null) return null!;

            // 2️⃣ 取得 CommentTargets
            var targets = await _context.CommentTargets
                .Where(ct => ct.VideoId == video.VideoId)
                .ToListAsync();
            var targetIds = targets.Select(t => t.CommentTargetId).ToList();

            // 3️⃣ 取得對應留言
            var comments = await _context.Comments
                .Where(c => targetIds.Contains(c.CommentTargetId))
                .Include(c => c.User)
                    .ThenInclude(u => u.Channel)
                .Include(c => c.User)
                    .ThenInclude(u => u.UserProfile)
                    .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            // 4️⃣ 計算喜歡數
            var likes = await _context.CommentLikes
                .Where(cl => comments.Select(c => c.CommentId).Contains(cl.CommentId))
                .GroupBy(cl => cl.CommentId)
                .Select(g => new { CommentId = g.Key, Count = g.Count() })
                .ToListAsync();

            // 5️⃣ 對應留言 DTO
            var commentDtos = comments
                .Where(c => c.ParentCommentId == null)
                .Select(c =>
                {
                    var dto = new VideoCommentDto
                    {
                        Id = c.CommentId,
                        UserName = c.User?.Channel?.ChannelName ?? "Unknown",
                        Avatar = c.User?.UserProfile?.Avatar ?? "",
                        Message = c.CommentContent,
                        CreatedAt = c.CreatedAt,
                        Likes = likes.FirstOrDefault(l => l.CommentId == c.CommentId)?.Count ?? 0,
                        Replies = comments
                            .Where(r => r.ParentCommentId == c.CommentId)
                            .Select(r => new VideoCommentDto
                            {
                                Id = r.CommentId,
                                UserName = r.User?.Channel?.ChannelName ?? "Unknown",
                                Avatar = r.User?.UserProfile?.Avatar ?? "",
                                Message = r.CommentContent,
                                CreatedAt = r.CreatedAt,
                                Likes = likes.FirstOrDefault(l => l.CommentId == r.CommentId)?.Count ?? 0,
                                Replies = new List<VideoCommentDto>()
                            }).ToList()
                    };
                    return dto;
                }).ToList();

            // 6️⃣ 回傳影片 DTO + 留言
            return commentDtos;
        }



        /* =====================================================
        * 🧩 Comment Target / Create
        * ===================================================== */

        public Task<CommentTarget?> GetCommentTargetAsync(int videoId)
            => _context.CommentTargets.FirstOrDefaultAsync(x => x.VideoId == videoId);

        public async Task<CommentTarget> CreateCommentTargetAsync(CommentTarget target)
        {
            _context.CommentTargets.Add(target);
            await _context.SaveChangesAsync();
            return target;
        }

        public async Task<Comment> CreateCommentAsync(Comment comment)
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        /* =====================================================
         * 📺 Channel
         * ===================================================== */

        public async Task<ChannelCardDto?> GetChannelCardByIdAsync(int channelId)
        {
            var channel = await _context.Channels
                .AsNoTracking()
                .Include(c => c.UserProfile)
                .FirstOrDefaultAsync(c => c.ChannelId == channelId);

            if (channel == null)
                return null;

            var followCount = await _context.Followings
                .CountAsync(f => f.ChannelId == channelId);

            return new ChannelCardDto
            {
                Id = channel.ChannelId,
                Name = channel.ChannelName,
                Description = channel.Description,
                Avatar = channel.UserProfile?.Avatar ?? "",
                Follows = followCount
            };
        }

        public async Task<ChannelHomeDto?> GetChannelHomeByIdAsync(int channelId)
        {
            var channel = await _context.Channels
                .AsNoTracking()
                .Include(c => c.UserProfile)
                .FirstOrDefaultAsync(c => c.ChannelId == channelId);

            if (channel == null)
                return null;

            var followCount = await _context.Followings
                .CountAsync(f => f.ChannelId == channelId);

            var videoCount = await _context.Videos
                .CountAsync(v => v.ChannelId == channelId);

            return new ChannelHomeDto
            {
                Id = channel.ChannelId,
                Name = channel.ChannelName,
                Description = channel.Description,
                Avatar = channel.UserProfile?.Avatar ?? "",
                Banner = channel.Banner,
                VideosCount = videoCount,
                CreatedAt = channel.CreatedAt,
                LastUpdateAt = channel.UpdateAt,
                Follows = followCount
            };
        }

        // Channel影片
        public async Task<List<VideoCardDto>> GetChannelVideosAsync(int channelId, int take = 10)
        {
            var videos = await _context.Videos
                .AsNoTracking()
                .Where(v => v.ChannelId == channelId)
                .Include(v => v.Channel)
                    .ThenInclude(c => c.UserProfile)
                .OrderByDescending(v => v.CreatedAt)
                .Take(take)
                .ToListAsync();

            return await MapVideosToDtoAsync(videos);
        }

        // 推薦影片
        public async Task<List<VideoCardDto>> GetRecommendVideosAsync(int take = 10)
        {
            var videos = await _context.Videos
                .AsNoTracking()
                .Include(v => v.Channel)
                    .ThenInclude(c => c.UserProfile)
                .OrderByDescending(v => v.CreatedAt)
                .Take(take)
                .ToListAsync();

            return await MapVideosToDtoAsync(videos);
        }

        // 共用：把影片轉成 DTO
        private async Task<List<VideoCardDto>> MapVideosToDtoAsync(List<Video> videos)
        {
            var videoIds = videos.Select(v => v.VideoId).ToList();

            var viewCounts = await _context.Views
                .Where(v => videoIds.Contains(v.VideoId))
                .GroupBy(v => v.VideoId)
                .Select(g => new { VideoId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.VideoId, x => x.Count);

            return videos.Select(v => new VideoCardDto
            {
                VideoId = v.VideoId,
                Title = v.Title ?? "",
                VideoUri = v.VideoUrl ?? "",
                Thumbnail = v.ThumbnailUrl ?? "",
                Duration = v.Duration ?? 0,
                Views = viewCounts.GetValueOrDefault(v.VideoId),
                PublishDate = v.PublishDate ?? DateTime.MinValue,
                Description = v.Description ?? "",
                ChannelId = v.ChannelId,
                ChannelName = v.Channel.ChannelName ?? "",
                Avatar = v.Channel.UserProfile?.Avatar ?? ""
            }).ToList();
        }
    }
}
