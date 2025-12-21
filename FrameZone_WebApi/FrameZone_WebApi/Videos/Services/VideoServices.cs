using FrameZone_WebApi.Models;
using FrameZone_WebApi.Videos.DTOs;
using FrameZone_WebApi.Videos.Enums;
using FrameZone_WebApi.Videos.Repositories;
using Microsoft.AspNetCore.Mvc;
using static FrameZone_WebApi.Videos.DTOs.ChannelCardDto;
namespace FrameZone_WebApi.Videos.Services
{

    public class VideoServices
    {
        private readonly VideoCardResponsity _videoRepo;

        public VideoServices(VideoCardResponsity videoRepo)
        {
            _videoRepo = videoRepo;
        }

        public async Task<VideoCardDto?> GetVideoCardAsync(string guid)
        {
            var dto = await _videoRepo.GetVideoCard(guid);

            if (dto == null)
            {
                return null;
            }

            return dto;
        }

        //=======================獲取影片推薦服務
        public async Task<List<VideoCardDto>> GetVideoRecommendAsync()
        {
            var dto = await _videoRepo.RecommendVideos();

            return dto ?? new List<VideoCardDto>();
        }



        public async Task<List<VideoCommentDto?>> GetVideoCommentsByGuid(string guid)
        {
            var dto = await _videoRepo.GetVideoWithComments(guid);

            if (dto == null)
            {
                return null;
            }

            return dto;
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

        public async Task<ChannelCardDto?> GetChannelbyid(int id)
        {
            var dto = await _videoRepo.getChannelCardbyId(id);

            if (dto == null)
            {
                return null;
            }

            return dto;
        }

        public async Task<VideoCommentDto> PostVideoComment(VideoCommentRequest req)
        {
            // 1️⃣ 取得或建立 CommentTarget
            var commentTarget = await _videoRepo.GetVideoCommentTarget(req.Videoid);

            if (commentTarget == null)
            {
                commentTarget = await _videoRepo.CreateVideoCommentTarget(new CommentTarget
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

            var createdComment = await _videoRepo.CreateComment(comment);

            // 3️⃣ 回傳 DTO
            return await _videoRepo.GetVideoCommentByCommentid(createdComment.CommentId);
        }


    }
}
