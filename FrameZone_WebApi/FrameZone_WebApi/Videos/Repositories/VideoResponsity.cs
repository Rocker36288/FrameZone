using FrameZone_WebApi.Models;
using FrameZone_WebApi.Videos.DTOs;
using Humanizer;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel;
using static FrameZone_WebApi.Videos.DTOs.ChannelCardDto;

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

        //獲取videocard資料(根據id單個)
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

        //獲取留言資料by 留言id
        public async Task<VideoCommentDto> GetVideoCommentByCommentid(int id)
        {
            var data = await _context.Comments
                .Include(v => v.User)
                .ThenInclude(u => u.UserProfile)
                .FirstOrDefaultAsync(u => u.CommentId == id);

            if (data == null) return null!; // 或 throw new Exception("留言不存在");


            var likeCount = await _context.CommentLikes
               .CountAsync(c => c.CommentId == id);

            var dto = new VideoCommentDto
            {
                Id = data.CommentId,
                UserName = data.User?.Channel?.ChannelName ?? "Unknown",
                Avatar = data.User?.UserProfile?.Avatar ?? "",
                Message = data.CommentContent,
                CreatedAt = data.CreatedAt,
                Likes = likeCount,
                Replies = new List<VideoCommentDto>(),
            };

            // 檢查是否有子留言（回覆）
            var replies = await _context.Comments
                .Where(c => c.ParentCommentId == id) // 假設你有 ParentCommentId
                .ToListAsync();

            foreach (var reply in replies)
            {
                var replyLikeCount = await _context.CommentLikes
                    .CountAsync(c => c.CommentId == reply.CommentId);

                dto.Replies.Add(new VideoCommentDto
                {
                    Id = reply.CommentId,
                    UserName = reply.User?.Channel?.ChannelName ?? "Unknown",
                    Avatar = reply.User?.UserProfile?.Avatar ?? "",
                    Message = reply.CommentContent,
                    CreatedAt = reply.CreatedAt,
                    Likes = replyLikeCount
                });
            }



            return dto;
        }


        public async Task<ChannelCardDto> getChannelCardbyId(int id)
        {
            var data = await _context.Channels
                .Include(c => c.UserProfile)
                .FirstOrDefaultAsync(c => c.ChannelId == id);

            if (data == null) return null!; // 或 throw new Exception("留言不存在");


            var followCount = await _context.Followings
               .CountAsync(c => c.ChannelId == id);

            var dto = new ChannelCardDto
            {
                Id = data.ChannelId,
                 Name = data.ChannelName,
                Description = data.Description,
                Avatar = data.UserProfile.Avatar,
                Follows = followCount,
            };

            return dto;
        }

        public async Task<ChannelHomeDto> getChannelHomebyId(int id)
        {
            var data = await _context.Channels
                .Include(c => c.UserProfile)
                .FirstOrDefaultAsync(c => c.ChannelId == id);

            if (data == null) return null!; // 或 throw new Exception("留言不存在");


            var followCount = await _context.Followings
               .CountAsync(c => c.ChannelId == id);

            var dto = new ChannelHomeDto
            {
                Id = data.ChannelId,
                Name = data.ChannelName,
                Description = data.Description,
                Avatar = data.UserProfile.Avatar,
                Banner = data.Banner,
                VideosCount = followCount,
                LastUpdateAt = new DateTime(),
                CreatedAt = data.CreatedAt,
                Follows = followCount,
            };

            return dto;
        }
    }
}
