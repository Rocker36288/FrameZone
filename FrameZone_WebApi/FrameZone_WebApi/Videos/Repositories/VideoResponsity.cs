using FrameZone_WebApi.Models;
using FrameZone_WebApi.Videos.DTOs;
using Humanizer;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace FrameZone_WebApi.Videos.Repositories
{
    public class VideoCardResponsity
    {
        private readonly AAContext _context;
        public VideoCardResponsity(AAContext context)
        {
            _context = context;
        }

        ////取得所有影片
        //public async Task<List<Video>> GetAllVideosAsync()
        //{
        //    return await _context.Videos.ToListAsync();
        //}

        ////根據id取得影片資料
        //public async Task<Video> GetVideosWithIdAsync(int id)
        //{
        //    return await _context.Videos.FindAsync(id);
        //}

        ////取得影片對應的channel
        //public async Task<Channel> GetVideoCardChannelAsync(int id)
        //{
        //    var video = await _context.Videos
        //        .Include(v => v.Channel)
        //        .FirstOrDefaultAsync(v => v.VideoId == id);
        //    return video?.Channel;
        //}

        ////取得影片對應的userAvatar
        //public async Task<UserProfile> GetVideoCardAvatar(int id)
        //{
        //    var video = await _context.Videos
        //        .FindAsync(id);
        //    return video.UserProfile;
        //}

        public async Task<VideoCardDto> GetVideoCard(int id)
        {
            var video = await _context.Videos
             .Include(v => v.Channel)
             .ThenInclude(c => c.UserProfile)
             .FirstOrDefaultAsync(v => v.VideoId == id);

            if (video == null || video.Channel == null) 
            {
                Console.WriteLine("沒找到資料");
                return null; // 找不到影片或頻道，直接回 null
            }

            var viewCount = await _context.Views
                .CountAsync(v => v.VideoId == video.VideoId);

            var dto = new VideoCardDto
            {
                VideoId = video.VideoId,
                Title = video.Title ?? "",
                VideoUri = video.VideoUrl ?? "",
                Thumbnail = video.ThumbnailUrl ?? "",
                Duration = video.Duration ?? 0,
                Views = viewCount,
                PublishDate = video.PublishDate ?? DateTime.MinValue,
                Description = video.Description ?? "",
                ChannelName = video.Channel.ChannelName ?? "",
                Avatar = video.Channel.UserProfile?.Avatar ?? ""
            };

            return dto;
        }
    }
}
