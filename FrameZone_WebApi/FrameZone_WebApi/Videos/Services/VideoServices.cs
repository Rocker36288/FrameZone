using FrameZone_WebApi.Models;
using FrameZone_WebApi.Videos.DTOs;
using FrameZone_WebApi.Videos.Enums;
using FrameZone_WebApi.Videos.Repositories;
using Microsoft.AspNetCore.Mvc;
namespace FrameZone_WebApi.Videos.Services
{

    public class VideoServices
    {
        private readonly VideoRespository _videoRepo;

        public VideoServices(VideoRespository videoRepo)
        {
            _videoRepo = videoRepo;
        }


        //public async Task<VideoCommentDto?> GetVideoCommentByCommentidAsync(int videoid)
        //{
        //    var dto = await _videoRepo.GetVideoCommentByCommentid(videoid);

        //    if (dto == null)
        //    {
        //        return null;
        //    }

        //    return dto;
        //}
        public async Task<List<VideoCardDto>> GetVideoRecommendAsync()
        {
            var dto = await _videoRepo.GetRecommendVideosAsync();

            return dto ?? new List<VideoCardDto>();
        }
        //用guid獲取影片資訊
        public async Task<VideoCardDto?> GetVideoCardAsync(string guid)
        {
            var dto = await _videoRepo.GetVideoCardByGuidAsync(guid);

            if (dto == null)
            {
                return null;
            }

            return dto;
        }

        public async Task<ChannelCardDto?> GetChannelbyid(int id)
        {
            var dto = await _videoRepo.GetChannelCardByIdAsync(id);

            if (dto == null)
            {
                return null;
            }

            return dto;
        }

        //=======================獲取頻道首頁服務=======================
        public async Task<ChannelHomeDto> GetChannelHome(int id)
        {
            var dto = await _videoRepo.GetChannelHomeByIdAsync(id);

            if (dto == null)
            {
                return null;
            }

            return dto;
        }

        //#獲取首頁上船影片

        public async Task<List<VideoCardDto>> GetChannelVideosAsync(int channelId)
        {
            var dto = await _videoRepo.GetChannelVideosAsync(channelId);

            return dto ?? new List<VideoCardDto>();
        }




        //=======================獲取影片留言相關=======================
        public async Task<List<VideoCommentDto?>> GetVideoCommentsByGuid(string guid)
        {
            var dto = await _videoRepo.GetVideoCommentsAsync(guid);

            if (dto == null)
            {
                return null;
            }

            return dto;
        }


        public async Task<Comment> PostVideoComment(VideoCommentRequest req)
        {
            // 1️⃣ 取得或建立 CommentTarget
            var commentTarget = await _videoRepo.GetCommentTargetAsync(req.Videoid);

            if (commentTarget == null)
            {
                commentTarget = await _videoRepo.CreateCommentTargetAsync(new CommentTarget
                {
                    VideoId = req.Videoid,
                    TargetTypeId = (int)TargetTypeEnum.Video // 建議用 enum
                });
            }

            // 2️⃣ 建立 Comment
            var comment = new Comment
            {
                CommentContent = req.CommentContent,
                CommentTargetId = commentTarget.CommentTargetId,
                UserId = req.UserId,
                ParentCommentId = req.ParentCommentId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };

            var createdComment = await _videoRepo.CreateCommentAsync(comment);

            // 3️⃣ 回傳 DTO
            return createdComment;
        }

        //==============================影片按讚================================
        //#確認是否按讚
        public async Task<VideoLikesDto> CheckVideoLike(int userId, string guid)
        {
            return await _videoRepo.CheckLikesAsync(userId, guid);
        }

        public async Task<VideoLikesDto> VideosLikeToggle(int userId, string guid)
        {
            return await _videoRepo.VideosLikeToggleAsync(userId, guid);
        }
    }
}
