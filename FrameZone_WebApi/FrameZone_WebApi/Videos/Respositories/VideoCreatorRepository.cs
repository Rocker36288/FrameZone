using FrameZone_WebApi.Models;
using FrameZone_WebApi.Videos.DTOs;
using FrameZone_WebApi.Videos.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Channels;
using static FrameZone_WebApi.Videos.DTOs.VideoCreatorDTO;
using static FrameZone_WebApi.Videos.Respositories.VideoCreatorRepository;

namespace FrameZone_WebApi.Videos.Respositories
{
    public class VideoCreatorRepository
    {
        private readonly AAContext _context;

        public VideoCreatorRepository(AAContext context)
        {
            _context = context;
        }
        /* =====================================================
         * 🎬 Video Detail Card
         * ===================================================== */

        public async Task<List<Video>> GetVideosByChannelIdAsync(
     int channelId,
     int page
 )
        {
            const int pageSize = 5;

            return await _context.Videos
                .AsNoTracking()
                .Include(v => v.Channel)
                    .ThenInclude(c => c.UserProfile)
                .Where(v => v.ChannelId == channelId)
                .OrderByDescending(v => v.PublishDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // 🔧 修正：這個方法應該回傳影片總數，不是總頁數
        public async Task<int> GetTotalVideosByChannel(int channelId)
        {
            return await _context.Videos
                .Where(v => v.ChannelId == channelId)
                .CountAsync();  // 直接回傳總數
        }


        public async Task<int> GetViewsCountAsync(int videoId)
        {
            return await _context.Views.CountAsync(v => v.VideoId == videoId);
        }

        public async Task<int> GetLikesCountAsync(int videoId)
        {
            return await _context.Likes.CountAsync(l => l.VideoId == videoId);
        }

        public async Task<int> GetCommentCountAsync(int videoId)
        {
            return await _context.CommentTargets
    .Where(ct => ct.VideoId == videoId)
    .CountAsync();

        }

        /* =====================================================
         * 🎬 Video Edit Data with Ownership Check
         * ===================================================== */
        public async Task<Video> GetVideoByGuidForUser(string guid, int userId)
        {
            var video = await _context.Videos
                .AsNoTracking()
                .Where(v => v.VideoUrl == guid && v.ChannelId == userId)
                .FirstOrDefaultAsync();

            return video; // 找不到就回傳 null
        }

        //影片資運編輯

        public async Task<Video?> GetByGuidAsync(string guid)
        {
            return await _context.Videos
                .FirstOrDefaultAsync(v => v.VideoUrl == guid);
        }

        public async Task UpdateAsync(Video video)
        {
            _context.Videos.Update(video);
            await _context.SaveChangesAsync();
        }
        /* =====================================================
        * 🎬 影片資訊解析區
        * ===================================================== */

        public async Task<int> GetTotalVideosAsync(int channelId)
        {
            return await _context.Videos
                .AsNoTracking()
                .CountAsync(v =>
                    v.ChannelId == channelId &&
                    !v.IsDeleted &&
                    v.PrivacyStatus == "PUBLIC");
        }

        public async Task<long> GetTotalViewsAsync(
            int channelId, DateTime from, DateTime to)
        {
            return await _context.Views
                .AsNoTracking()
                .Where(v =>
                    v.Video.ChannelId == channelId &&
                    v.CreatedAt >= from &&
                    v.CreatedAt <= to)
                .LongCountAsync();
        }

        public async Task<int> GetSubscribersCountAsync(int channelId)
        {
            return await _context.Followings
                .AsNoTracking()
                .CountAsync(f => f.ChannelId == channelId);
        }

        public async Task<int> GetLikesCountAsync(
            int channelId, DateTime from, DateTime to)
        {
            return await _context.Likes
                .AsNoTracking()
                .Where(l =>
                    l.Video.ChannelId == channelId &&
                    l.CreatedAt >= from &&
                    l.CreatedAt <= to)
                .CountAsync();
        }

        public async Task<int> GetCommentsCountAsync(
            int channelId, DateTime from, DateTime to)
        {
            return await _context.Comments
                .AsNoTracking()
                .Join(_context.CommentTargets,
                    c => c.CommentTargetId,
                    t => t.CommentTargetId,
                    (c, t) => new { c, t })
                .Where(x =>
                    x.t.VideoId != null &&
                    x.c.CreatedAt >= from &&
                    x.c.CreatedAt <= to &&
                    _context.Videos.Any(v =>
                        v.VideoId == x.t.VideoId &&
                        v.ChannelId == channelId))
                .CountAsync();
        }

        public async Task<List<DailyViewStatDto>> GetDailyViewsAsync(
            int channelId, DateTime from, DateTime to)
        {
            return await _context.Views
                .AsNoTracking()
                .Where(v =>
                    v.Video.ChannelId == channelId &&
                    v.CreatedAt >= from &&
                    v.CreatedAt <= to)
                .GroupBy(v => v.CreatedAt.Date)
                .Select(g => new DailyViewStatDto
                {
                    Date = g.Key,
                    Views = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();
        }

        public async Task<List<RecentVideoStatDto>> GetRecentVideosAsync(
            int channelId, int take)
        {
            return await _context.Videos
                .AsNoTracking()
                .Where(v =>
                    v.ChannelId == channelId &&
                    !v.IsDeleted && v.PublishDate != null)
                .OrderByDescending(v => v.PublishDate)
                .Take(take)
                .Select(v => new RecentVideoStatDto
                {
                    VideoId = v.VideoId,
                    Title = v.Title,
                    ThumbnailUrl = v.VideoUrl,
                    PublishDate = (DateTime)v.PublishDate,
                    Views = _context.Views.Count(x => x.VideoId == v.VideoId),
                    Likes = _context.Likes.Count(x => x.VideoId == v.VideoId),
                    Comments = _context.CommentTargets
                        .Where(t => t.VideoId == v.VideoId)
                        .Join(_context.Comments,
                            t => t.CommentTargetId,
                            c => c.CommentTargetId,
                            (t, c) => c)
                        .Count()
                })
                .ToListAsync();
        }

        public async Task<String> GetVideoAIResult(string guid)
        {
            return await _context.Videos.Where(v=> v.VideoUrl == guid).Select(v=> v.AiAuditResult).FirstOrDefaultAsync();
        }
    }
}
