using System.ComponentModel;
using FrameZone_WebApi.Models;
using FrameZone_WebApi.Videos.DTOs;
using Humanizer;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static System.Runtime.InteropServices.JavaScript.JSType;
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
        public async Task<VideoCardDto> GetVideoCard(string guid)
        {
            var video = await _context.Videos
             .Include(v => v.Channel)
             .ThenInclude(c => c.UserProfile)
             .FirstOrDefaultAsync(v => v.VideoUrl == guid);

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

        //獲取留言資料by 留言id
        public async Task<List<VideoCommentDto>> GetVideoWithComments(string guid)
        {
            // 1️⃣ 取得影片
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

        //先查詢CommentTarget是否存在，不存在則建立，已存在就傳
        public async Task<CommentTarget?> GetVideoCommentTarget(int videoId)
        {
            return await _context.CommentTargets
                .FirstOrDefaultAsync(x => x.VideoId == videoId);
        }

        public async Task<CommentTarget> CreateVideoCommentTarget(CommentTarget target)
        {
            _context.CommentTargets.Add(target);
            await _context.SaveChangesAsync();
            return target;
        }

        public async Task<Comment> CreateComment(Comment comment)
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }
    }
}
