using FrameZone_WebApi.Videos.DTOs;
using FrameZone_WebApi.Videos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static FrameZone_WebApi.Videos.DTOs.VideoCreatorDTO;

namespace FrameZone_WebApi.Videos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VideoCreatorController : ControllerBase
    {
        private readonly VideoCreatorService _videoCreatorService;

        public VideoCreatorController(VideoCreatorService videoCreatorService)
        {
            _videoCreatorService = videoCreatorService;
        }

        //api/VideoCreator/RecentUpload
        [HttpGet("RecentUpload")]
        [Authorize]
        public async Task<ActionResult<List<VideoDetailDto>>> GetVideo([FromQuery] int count = 5)
        {
            int channelId = GetUserId();
            var videos = await _videoCreatorService.GetVideoDetailsByChannelIdAsync(channelId, count);

            return Ok(videos);
        }

        //============創作者影片詳細編輯 =====================
        [HttpGet("edit/{guid}")]
        [Authorize]
        public async Task<IActionResult> GetVideoForEdit(string guid)
        {
            var userId = GetUserId(); // 從登入資訊取得使用者 ID
            var video = await _videoCreatorService.GetVideoForEdit(guid, userId);

            if (video == null)
            {
                //_logger.LogWarning("User {UserId} tried to edit video {Guid} but not found or not owned.", userId, guid);
                return NotFound();
            }

            return Ok(video);
        }

        //=============獲取userid=======================================
        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null
                || !int.TryParse(userIdClaim.Value, out var userId)
                || userId <= 0)
            {
                throw new UnauthorizedAccessException("Invalid user.");
            }

            return userId;
        }
    }
}
