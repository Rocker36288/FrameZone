using FrameZone_WebApi.Models;
using FrameZone_WebApi.Videos.DTOs;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.X509;
using System.Threading.Channels;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            //檢索影片是否存在
            var video = await _context.Videos
                .AsNoTracking()
                .Include(v => v.Channel)
                    .ThenInclude(c => c.UserProfile)
                .FirstOrDefaultAsync(v => v.VideoUrl == guid);

            if (video == null || video.Channel == null)
                return null;

            //計算觀看次數數量
            var viewCount = await _context.Views
                .CountAsync(v => v.VideoId == video.VideoId);

            // 4️⃣ 計算喜歡數
            var likesCount = await _context.Likes
                .CountAsync(l => l.VideoId == video.VideoId);


            return new VideoCardDto
            {
                VideoId = video.VideoId,
                Title = video.Title ?? "",
                VideoUri = video.VideoUrl ?? "",
                Thumbnail = video.ThumbnailUrl ?? "",
                Duration = video.Duration ?? 0,
                Views = viewCount,
                Likes = likesCount,
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

        /// <summary>
        /// 1️⃣ 檢測是否存在指定 SystemId
        /// </summary>
        public async Task<int> GetTargetTypeIdBySystemIdAsync(int systemId)
        {
            return await _context.TargetTypes
                .AsNoTracking()
                .Where(t => t.SystemId == systemId)
                .Select(t => t.TargetTypeId)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// 2️⃣ 建立 TargetType
        /// </summary>
        public async Task<int> CreateAsync(TargetType targetType)
        {
            _context.TargetTypes.Add(targetType);
            await _context.SaveChangesAsync();
            return targetType.TargetTypeId;
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
                .Where(v=> v.IsDeleted == false)
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

        /* =====================================================
        * Likes
        * ===================================================== */
        public async Task<VideoLikesDto> CheckLikesAsync(int userid, string guid)
        {
            var video = await _context.Videos
             .AsNoTracking()
             .FirstOrDefaultAsync(v => v.VideoUrl == guid);

            if (video == null)
            {
                return new VideoLikesDto
                {
                    IsLikes = false
                };
            }

            var data = await _context.Likes
                .AsNoTracking()
                .AnyAsync(l => l.UserId == userid && l.VideoId == video.VideoId);

            return new VideoLikesDto
            {
                IsLikes = data
            };
        }

        //#LikeToggle
        public async Task<VideoLikesDto> VideosLikeToggleAsync(int userId, string guid)
        {
            var video = await _context.Videos
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.VideoUrl == guid);

            if (video == null)
                throw new KeyNotFoundException("Video not found.");

            var like = await _context.Likes
                .FirstOrDefaultAsync(l => l.UserId == userId && l.VideoId == video.VideoId);

            if (like == null)
            {
                // 不追蹤實體，EF 只生成 INSERT
                var newLike = new Like
                {
                    UserId = userId,
                    VideoId = video.VideoId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Likes.Attach(newLike); // 告訴 EF 這個實體存在
                _context.Entry(newLike).State = EntityState.Added; // 明確指定新增

                await _context.SaveChangesAsync();
                return new VideoLikesDto { IsLikes = true };
            }
            else
            {
                _context.Likes.Remove(like);
                await _context.SaveChangesAsync();
                return new VideoLikesDto { IsLikes = false };
            }
        }

        /* =====================================================
        * 搜尋
        * ===================================================== */
        // 推薦影片
        public async Task<List<VideoCardDto>> VideoSearchAsync(
         string? keyword = null,
         int? channelId = null,
         int? categoryId = null,
         string sortBy = "date",
         string sortOrder = "desc",
         int take = 10)
        {
            // 1️ 基礎 Query
            IQueryable<Video> query = _context.Videos
                .AsNoTracking()
                .Include(v => v.Channel)
                    .ThenInclude(c => c.UserProfile)
                .Where(v => v.ProcessStatus == "published");

            // 2️ 搜尋條件
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(v =>
                    v.Title.Contains(keyword) ||
                    v.Description.Contains(keyword));
            }

            if (channelId.HasValue)
            {
                query = query.Where(v => v.ChannelId == channelId.Value);
            }


            // 3️ 排序條件
            query = (sortBy.ToLower(), sortOrder.ToLower()) switch
            {
                //("views", "asc") => query.OrderBy(v => v.ViewCount),
                //("views", "desc") => query.OrderByDescending(v => v.ViewCount),

                //("likes", "asc") => query.OrderBy(v => v.LikeCount),
                //("likes", "desc") => query.OrderByDescending(v => v.LikeCount),

                ("date", "asc") => query.OrderBy(v => v.CreatedAt),
                _ => query.OrderByDescending(v => v.CreatedAt) // default
            };

            // 4️ 限制筆數
            var videos = await query
                .Take(take)
                .ToListAsync();

            // 5️⃣ 映射 DTO
            return await MapVideosToDtoAsync(videos);
        }
    }
}
