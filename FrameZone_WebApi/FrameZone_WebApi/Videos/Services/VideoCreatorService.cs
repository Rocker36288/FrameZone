using FrameZone_WebApi.Models;
using FrameZone_WebApi.Videos.Enums;
using FrameZone_WebApi.Videos.Repositories;
using FrameZone_WebApi.Videos.Respositories;
using Microsoft.EntityFrameworkCore;
using static FrameZone_WebApi.Videos.DTOs.VideoCreatorDTO;

namespace FrameZone_WebApi.Videos.Services
{
    public class VideoCreatorService
    {
        private readonly VideoCreatorRepository _videoRepo;
        private readonly IWebHostEnvironment _env;
        public VideoCreatorService(VideoCreatorRepository videoRepo, IWebHostEnvironment env)
        {
            _videoRepo = videoRepo;
            _env = env;
        }

        public async Task<List<VideoDetailDto>> GetVideoDetailsByChannelIdAsync(int channelId, int count)
        {
            var videos = await _videoRepo.GetVideosByChannelIdAsync(channelId, count);

            var result = new List<VideoDetailDto>();

            foreach (var video in videos)
            {
                var viewCount = await _videoRepo.GetViewsCountAsync(video.VideoId);
                var likesCount = await _videoRepo.GetLikesCountAsync(video.VideoId);
                var commentCount = await _videoRepo.GetCommentCountAsync(video.VideoId);

                // 安全轉 enum
                ProcessStatus processStatus = Enum.TryParse<ProcessStatus>(video.ProcessStatus, out var ps) ? ps : ProcessStatus.UPLOADING;
                PrivacyStatus privacyStatus = Enum.TryParse<PrivacyStatus>(video.PrivacyStatus, out var priv) ? priv : PrivacyStatus.DRAFT;

                result.Add(new VideoDetailDto
                {
                    VideoId = video.VideoId,
                    Title = video.Title ?? "",
                    Description = video.Description ?? "",
                    Thumbnail = video.ThumbnailUrl ?? "",
                    VideoUrl = video.VideoUrl ?? "",
                    Duration = video.Duration ?? 0,
                    ViewsCount = viewCount,
                    LikesCount = likesCount,
                    CommentCount = commentCount,
                    PublishDate = video.PublishDate ?? DateTime.MinValue,
                    ProcessStatus = processStatus,
                    PrivacyStatus = privacyStatus
                });
            }

            return result;
        }

        public async Task<VideoDetailDto> GetVideoForEdit(string guid, int id)
        {
            var video = await _videoRepo.GetVideoByGuidForUser(guid, id);
            if (video == null)
            {
                return null; // 沒找到或沒權限，直接回傳 null
            }


            var viewCount = await _videoRepo.GetViewsCountAsync(video.VideoId);
            var likesCount = await _videoRepo.GetLikesCountAsync(video.VideoId);
            var commentCount = await _videoRepo.GetCommentCountAsync(video.VideoId);

            // 安全轉 enum
            ProcessStatus processStatus = Enum.TryParse<ProcessStatus>(video.ProcessStatus, out var ps) ? ps : ProcessStatus.UPLOADING;
            PrivacyStatus privacyStatus = Enum.TryParse<PrivacyStatus>(video.PrivacyStatus, out var priv) ? priv : PrivacyStatus.DRAFT;

            var result = new VideoDetailDto
            {
                VideoId = video.VideoId,
                Title = video.Title ?? "",
                Description = video.Description ?? "",
                Thumbnail = video.ThumbnailUrl ?? "",
                VideoUrl = video.VideoUrl ?? "",
                Duration = video.Duration ?? 0,
                CreatedDate = video.CreatedAt,
                UpdateDate = video.UpdateAt,
                Resolution = video.Resolution,
                CommentCount = commentCount,
                ViewsCount = viewCount,
                LikesCount = likesCount,
                PublishDate = video.PublishDate ?? DateTime.MinValue,
                ProcessStatus = processStatus,
                PrivacyStatus = privacyStatus
            };

            return result;
        }

        public async Task<bool> UpdateVideoAsync(
         int userId,
         string guid,
         UpdateVideoMetadataDto dto)
        {
            // 1️⃣ 拿資料
            var video = await _videoRepo.GetByGuidAsync(guid);
            if (video == null)
                return false;

            // 2️⃣ 權限檢查（Service 做）
            if (video.ChannelId != userId)
                return false;

            // 3️⃣ 組資料（Business Logic）
            video.Title = dto.Title;
            video.Description = dto.Description;
            video.PrivacyStatus = dto.PrivacyStatus;
            video.UpdateAt = DateTime.UtcNow;

            // 4️⃣ 存資料
            await _videoRepo.UpdateAsync(video);

            return true;
        }

        public async Task UpdateThumbnailAsync(string guid, IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("檔案不存在", nameof(file));

            // 固定縮圖路徑
            var filePath = Path.Combine(_env.WebRootPath, "videos", guid, "thumbnail.jpg");

            // 覆蓋既有檔案
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 不需回傳 DTO，也不用更新資料庫
        }
    } 
}
