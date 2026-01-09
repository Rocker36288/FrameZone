using FrameZone_WebApi.Models;
using FrameZone_WebApi.Videos.DTOs;
using FrameZone_WebApi.Videos.Enums;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.X509;
using System.Threading.Channels;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FrameZone_WebApi.Videos.Repositories
{
    public class VideoRepository
    {
        private readonly AAContext _context;

        public VideoRepository(AAContext context)
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

        public async Task<VideoCommentDto> CreateCommentAsync(Comment comment)
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var c = await _context.Comments
      .Include(x => x.User)
          .ThenInclude(u => u.Channel)
      .Include(x => x.User)
          .ThenInclude(u => u.UserProfile)
      .SingleAsync(x => x.CommentId == comment.CommentId);

            var returnComment = new VideoCommentDto
            {
                Id = c.CommentId,
                UserName = c.User?.Channel?.ChannelName ?? "Unknown",
                Avatar = c.User?.UserProfile?.Avatar ?? "",
                Message = c.CommentContent,
                CreatedAt = c.CreatedAt,
                Likes = 0,
                Replies = new List<VideoCommentDto>()
            };

            return returnComment;
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
                .Where(v => v.IsDeleted == false)
                .Include(v => v.Channel)
                    .ThenInclude(c => c.UserProfile)
                .OrderByDescending(v => v.CreatedAt)
                .Take(take)
                .ToListAsync();

            return await MapVideosToDtoAsync(videos);
        }

        // 熱門影片
        public async Task<List<VideoCardDto>> GetPopularVideosAsync(int take = 5)
        {
            // 先從 View 表 group 出 Top VideoId
            var topVideoIds = await _context.Views
                .AsNoTracking()
                .GroupBy(v => v.VideoId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(take)
                .ToListAsync();

            // 再回查 Video 主資料
            var videos = await _context.Videos
                .AsNoTracking()
                .Where(v => !v.IsDeleted && topVideoIds.Contains(v.VideoId))
                .Include(v => v.Channel)
                    .ThenInclude(c => c.UserProfile)
                .ToListAsync();

            // 保持排序一致（很重要）

            return await MapVideosToDtoAsync(videos);
        }

        // 指定頻道最新影片
        public async Task<List<VideoCardDto>> GetChannelLatestVideosAsync(
            int channelId,
            int take = 5)
        {
            var videos = await _context.Videos
                .AsNoTracking()
                .Where(v =>
                    !v.IsDeleted &&
                    v.ChannelId == channelId &&
                    v.ProcessStatus == ProcessStatus.PUBLISHED.ToString() // 建議加
                )
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
      .Where(v => v.VideoUrl == guid)
      .Select(v => new { v.VideoId })
      .FirstOrDefaultAsync();

            if (video == null)
                throw new KeyNotFoundException("Video not found.");

            var like = await _context.Likes
                .FirstOrDefaultAsync(l => l.UserId == userId && l.VideoId == video.VideoId);

            if (like == null)
            {
                _context.Likes.Add(new Like
                {
                    UserId = userId,
                    VideoId = video.VideoId,
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                return new VideoLikesDto { IsLikes = true };
            }

            _context.Likes.Remove(like);
            await _context.SaveChangesAsync();
            return new VideoLikesDto { IsLikes = false };
        }

        /* =====================================================
        * Comment Likes
        * ===================================================== */

        /* =====================================================
        * Channel Folow
        * ===================================================== */
        public async Task<bool> CheckFollowingAsync(int userId, int channelId)
        {
            return await _context.Followings
                .AsNoTracking()
                .AnyAsync(f => f.UserId == userId && f.ChannelId == channelId);
        }

        public async Task<bool> FollowingToggleAsync(int userId, int channelId)
        {
            var follow = await _context.Followings
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ChannelId == channelId);

            if (follow == null)
            {
                _context.Followings.Add(new Following
                {
                    UserId = userId,
                    ChannelId = channelId,
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                return true; // 已 Follow
            }

            _context.Followings.Remove(follow);
            await _context.SaveChangesAsync();
            return false; // 已 Unfollow
        }

        /* =====================================================
        * Watch History
        * ===================================================== */
        public async Task<View?> GetByUserAndVideoViewsAsync(int userId, int videoId)
        {
            return await _context.Views
                .FirstOrDefaultAsync(v => v.UserId == userId && v.VideoId == videoId);
        }

        public async Task ViewsAddAsync(View view)
        {
            await _context.Views.AddAsync(view);
        }

        public void ViewsUpdate(View view)
        {
            _context.Views.Update(view);
        }

        public async Task ViewsSaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<List<WatchHistoryDto>> GetWatchHistoryByUserIdAsync(int userId)
        {
            var views = await _context.Views
                .Include(v => v.Video)
                    .ThenInclude(video => video.Channel)
                        .ThenInclude(c => c.UserProfile)
                .Where(v => v.UserId == userId)
                .OrderBy(v => v.UpdateAt)
                .ToListAsync();

            var videos = views.Select(v => v.Video!).ToList();

            var videoDtos = await MapVideosToDtoAsync(videos);

            var result = views.Select(v => new WatchHistoryDto
            {
                Video = videoDtos.First(dto => dto.VideoId == v.VideoId),
                LastPosition = v.LastPosition,
                LastWatchedAt = v.UpdateAt
            }).ToList();

            return result;
        }



        /* =====================================================
         * 搜尋影片 + DTO 映射
         * ===================================================== */

        public async Task<List<VideoCardDto>> SearchVideosAsync(
     string? keyword = null,
     string sortBy = "date",
     string sortOrder = "desc",
     int take = 10)
        {
            var query = _context.Videos
                .AsNoTracking()
                .Include(v => v.Channel)
                    .ThenInclude(c => c.UserProfile)
                .AsQueryable();

            // 關鍵字搜尋
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(v =>
                    (v.Title != null && v.Title.Contains(keyword)) ||
                    (v.Description != null && v.Description.Contains(keyword))
                );
            }

            // 投影並計算統計
            var projectedQuery = query.Select(v => new
            {
                Video = v,
                LikeCount = v.Likes.Count,      // 使用導航屬性
                ViewCount = v.Views.Count       // 使用導航屬性
            });

            // 排序
            projectedQuery = (sortBy.ToLower(), sortOrder.ToLower()) switch
            {
                ("likes", "asc") => projectedQuery.OrderBy(x => x.LikeCount),
                ("likes", "desc") => projectedQuery.OrderByDescending(x => x.LikeCount),
                ("views", "asc") => projectedQuery.OrderBy(x => x.ViewCount),
                ("views", "desc") => projectedQuery.OrderByDescending(x => x.ViewCount),
                ("date", "asc") => projectedQuery.OrderBy(x => x.Video.CreatedAt),
                _ => projectedQuery.OrderByDescending(x => x.Video.CreatedAt)
            };

            var videoStats = await projectedQuery.Take(take).ToListAsync();

            return videoStats.Select(x => new VideoCardDto
            {
                VideoId = x.Video.VideoId,
                Title = x.Video.Title ?? "",
                VideoUri = x.Video.VideoUrl ?? "",
                Thumbnail = x.Video.ThumbnailUrl ?? "",
                Duration = x.Video.Duration ?? 0,
                Views = x.ViewCount,
                Likes = x.LikeCount,
                PublishDate = x.Video.PublishDate ?? DateTime.MinValue,
                Description = x.Video.Description ?? "",
                ChannelId = x.Video.ChannelId,
                ChannelName = x.Video.Channel?.ChannelName ?? "",
                Avatar = x.Video.Channel?.UserProfile?.Avatar ?? ""
            }).ToList();
        }

    }

 }
