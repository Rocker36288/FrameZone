using FrameZone_WebApi.Models;
using FrameZone_WebApi.Videos.DTOs;
using FrameZone_WebApi.Videos.Enums;
using Microsoft.EntityFrameworkCore;
using static FrameZone_WebApi.Videos.DTOs.VideoCreatorDTO;

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

        public async Task<List<Video>> GetVideosByChannelIdAsync(int channelId, int count)
        {
            return await _context.Videos
                .AsNoTracking()
                .Include(v => v.Channel)
                    .ThenInclude(c => c.UserProfile)
                .Where(v => v.ChannelId == channelId)
                .OrderByDescending(v => v.PublishDate)
                .Take(count)
                .ToListAsync();
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


    }
}
