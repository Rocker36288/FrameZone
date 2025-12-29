using FrameZone_WebApi.Models;
using FrameZone_WebApi.Videos.Enums;
using FrameZone_WebApi.Videos.Repositories;
using FrameZone_WebApi.Videos.Respositories;
using static FrameZone_WebApi.Videos.DTOs.VideoCreatorDTO;

namespace FrameZone_WebApi.Videos.Services
{
    public class VideoCreatorService
    {
        private readonly VideoCreatorRepository _videoRepo;
        public VideoCreatorService(VideoCreatorRepository videoRepo)
        {
            _videoRepo = videoRepo;
        }

        public async Task<List<VideoDetailDto>> GetVideoDetailsByChannelIdAsync(int channelId, int count)
        {
            var videos = await _videoRepo.GetVideosByChannelIdAsync(channelId, count);

            var result = new List<VideoDetailDto>();

            foreach (var video in videos)
            {
                var viewCount = await _videoRepo.GetViewsCountAsync(video.VideoId);
                var likesCount = await _videoRepo.GetLikesCountAsync(video.VideoId);

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


            var viewCount = await _videoRepo.GetViewsCountAsync(video.VideoId);
            var likesCount = await _videoRepo.GetLikesCountAsync(video.VideoId);

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
                ViewsCount = viewCount,
                LikesCount = likesCount,
                PublishDate = video.PublishDate ?? DateTime.MinValue,
                ProcessStatus = processStatus,
                PrivacyStatus = privacyStatus
            };

            return result;
        }
    } 
}
