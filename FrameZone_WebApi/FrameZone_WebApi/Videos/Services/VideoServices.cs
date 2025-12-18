using FrameZone_WebApi.Models;
using FrameZone_WebApi.Videos.DTOs;
using FrameZone_WebApi.Videos.Repositories;
namespace FrameZone_WebApi.Videos.Services
{

    public class VideoServices
    {
        private readonly VideoCardResponsity _videoRepo;

        public VideoServices(VideoCardResponsity videoRepo)
        {
            _videoRepo = videoRepo;
        }

        public async Task<VideoCardDto?> GetVideoCardAsync(int videoid)
        {
            var dto = await _videoRepo.GetVideoCard(videoid);

            if (dto == null)
            {
                return null;
            }

            return dto;
        }

       

        public async Task<VideoCommentDto?> GetVideoCommentByCommentidAsync(int videoid)
        {
            var dto = await _videoRepo.GetVideoCommentByCommentid(videoid);

            if (dto == null)
            {
                return null;
            }

            return dto;
        }

        public async Task<ChannelCardDto?> GetChannelbyid(int id)
        {
            var dto = await _videoRepo.getChannelCardbyId(id);

            if (dto == null)
            {
                return null;
            }

            return dto;
        }

       
    }
}
