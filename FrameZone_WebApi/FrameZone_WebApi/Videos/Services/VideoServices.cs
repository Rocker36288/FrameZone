using FrameZone_WebApi.Models;
using FrameZone_WebApi.Videos.DTOs;
using FrameZone_WebApi.Videos.Enums;
using FrameZone_WebApi.Videos.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FrameZone_WebApi.Videos.Services
{

    public class VideoServices
    {
        private readonly VideoRepository _videoRepo;

        public VideoServices(VideoRepository videoRepo)
        {
            _videoRepo = videoRepo;
        }
        //獲取影片推薦
        public async Task<List<VideoCardDto>> GetVideoRecommendAsync()
        {
            var dto = await _videoRepo.GetRecommendVideosAsync();

            return dto ?? new List<VideoCardDto>();
        }

        //獲取熱門影片
        public async Task<List<VideoCardDto>> GetVideoPopularAsync()
        {
            var dto = await _videoRepo.GetPopularVideosAsync();

            return dto ?? new List<VideoCardDto>();
        }

        // 獲取頻道 Spotlight（頻道卡片 + 最新影片）
        public async Task<ChannelSpotlightDto> GetSpotlightVideosAsync(int channelId)
        {
            var channel = await _videoRepo.GetChannelCardByIdAsync(channelId);
            if (channel == null)
                return null;

            var videos = await _videoRepo.GetChannelLatestVideosAsync(channelId);

            return new ChannelSpotlightDto
            {
                Channel = channel,
                Videos = videos ?? new List<VideoCardDto>()
            };
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


        public async Task<VideoCommentDto> PostVideoComment(VideoCommentRequest req, int userId)
        {
            // 1️⃣ 嘗試取得既有 TargetTypeId
            var targetTypeId = await _videoRepo
                .GetTargetTypeIdBySystemIdAsync((int)TargetTypeEnum.Video);

            if (targetTypeId == 0)
            {
                // 2️⃣ 不存在 → 建立新 TargetType
                var newTargetType = new TargetType
                {
                    SystemId = (int)TargetTypeEnum.Video,
                    TargetType1 = "VideoCommentTarget",
                    CreatedAt = DateTime.UtcNow
                };

                targetTypeId = await _videoRepo.CreateAsync(newTargetType);
            }

            

            // 1️⃣ 取得或建立 CommentTarget
            var commentTarget = await _videoRepo.GetCommentTargetAsync(req.Videoid);

            if (commentTarget == null)
            {
                commentTarget = await _videoRepo.CreateCommentTargetAsync(new CommentTarget
                {
                    VideoId = req.Videoid,
                    TargetTypeId = targetTypeId // 建議用 enum
                });
            }

            // 2️⃣ 建立 Comment
            var comment = new Comment
            {
                CommentContent = req.CommentContent,
                CommentTargetId = commentTarget.CommentTargetId,
                UserId = userId,
                ParentCommentId = req.ParentCommentId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
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

        //==============================頻道追隨================================
        //#確認是否按讚
        public async Task<bool> CheckChannelFollow(int userId, int channelId)
        {
            return await _videoRepo.CheckFollowingAsync(userId, channelId);
        }

        public async Task<bool> ChannelFollowToggle(int userId, int channelId)
        {
            return await _videoRepo.FollowingToggleAsync(userId, channelId);
        }

        /* =====================================================
       * Watch History
       * ===================================================== */
        public async Task WatchVideoUpdateAsync(int userId, int videoId, int lastPosition)
        {
            var watchRecord = await _videoRepo
                .GetByUserAndVideoViewsAsync(userId, videoId);

            if (watchRecord != null)
            {
                watchRecord.LastPosition = lastPosition;
                watchRecord.UpdateAt = DateTime.UtcNow;

                _videoRepo.ViewsUpdate(watchRecord);
            }
            else
            {
                var newRecord = new View
                {
                    UserId = userId,
                    VideoId = videoId,
                    LastPosition = lastPosition,
                    CreatedAt = DateTime.UtcNow,
                    UpdateAt = DateTime.UtcNow
                };

                await _videoRepo.ViewsAddAsync(newRecord);
            }

            await _videoRepo.ViewsSaveChangesAsync();
        }



        /// <summary>
        /// 取得觀看紀錄（含影片資訊 + 已看秒數）
        /// </summary>
        public async Task<List<WatchHistoryDto>> GetWatchHistoryAsync(int userId)
        {
            return await _videoRepo.GetWatchHistoryByUserIdAsync(userId);
        }

        /// <summary>
        /// 搜尋影片
        /// </summary>
        public async Task<List<VideoCardDto>> SearchVideosAsync(
            string? keyword = null,
            string sortBy = "date",
            string sortOrder = "desc",
            int take = 10)
        {
            return await _videoRepo.SearchVideosAsync(keyword, sortBy, sortOrder, take);
        }
    }
}
