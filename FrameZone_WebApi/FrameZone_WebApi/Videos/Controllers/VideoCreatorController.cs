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

        //===================編輯影片===================
        [HttpPatch("edit/{guid}/update")]
        [Authorize]
        public async Task<IActionResult> UpdateVideo(
    string guid,
    [FromBody] UpdateVideoMetadataDto dto)
        {
            var userId = GetUserId();

            var result = await _videoCreatorService.UpdateVideoAsync(userId, guid, dto);

            if (!result)
                return Forbid();

            return NoContent(); // 204
        }

        [HttpPost("edit/{guid}/thumbnail")]
        [Authorize]
        public async Task<IActionResult> UploadThumbnail(string guid, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("檔案不存在");

            try
            {
                await _videoCreatorService.UpdateThumbnailAsync(guid, file);
                return NoContent(); // 204，僅表示成功
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// 取得創作者頻道數據分析
        /// </summary>
        /// <param name="period">分析區間（7days / 30days / 90days）</param>
        [HttpGet("analytics")]  // 完整路徑：api/VideoCreator/analytics
        public async Task<IActionResult> GetCreatorAnalytics([FromQuery] string period = "7days")
        {
            if (!IsValidPeriod(period))
                return BadRequest("Invalid period. Allowed values: 7days, 30days, 90days");

            var result = await _videoCreatorService
                .GetAnalyticsAsync(GetUserId(), period);

            return Ok(result);
        }

        private static bool IsValidPeriod(string period)
        {
            return period is "7days" or "30days" or "90days";
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
