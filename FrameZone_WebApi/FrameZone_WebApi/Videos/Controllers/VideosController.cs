using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using FrameZone_WebApi.Videos.DTOs;
using FrameZone_WebApi.Videos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xabe.FFmpeg;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FrameZone_WebApi.Videos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VideosController : ControllerBase
    {
        private readonly VideoServices _videoServices;

        public VideosController(VideoServices videoServices)
        {
            _videoServices = videoServices;
        }

        //獲取影片資訊
        //api/videos/{id}
        [HttpGet("{guid}")]
        public async Task<ActionResult<VideoCardDto>> GetVideo(string guid)
        {
            var dto = await _videoServices.GetVideoCardAsync(guid);

            if(dto == null)
            {
                Console.WriteLine("API呼叫成功，但失敗!");
                return NotFound();
            }

            return Ok(dto);
        }

        //獲取縮圖
        [HttpGet("video-thumbnail/{guid}")]
        public IActionResult GetVideoThumbnail(string guid)
        {
            // 取得實際檔案路徑
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "videos", guid, "thumbnail.jpg");

            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var mimeType = "image/jpeg";
            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, mimeType);
        }

        //呼叫獲取影片底下的留言
        //api/videos/{guid}/comment
        [HttpGet("{guid}/comment")]
        public async Task<ActionResult<List<VideoCommentDto>>> GetVideoComments(string guid)
        {
            var dto = await _videoServices.GetVideoCommentsByGuid(guid);

            if (dto == null)
            {
                Console.WriteLine("API呼叫成功，但失敗!");
                return NotFound();
            }

            return Ok(dto);
        }
        //===============================獲取頻道資訊========================================

        //api/videos/channel/{id}
        [HttpGet("channel/{id}")]
        public async Task<ActionResult<ChannelCardDto>> Getchannel(int id)
        {
            var dto = await _videoServices.GetChannelbyid(id);

            if (dto == null)
            {
                Console.WriteLine("API呼叫成功，但失敗!");
                return NotFound();
            }

            return Ok(dto);
        }

        //===============================獲取頻道首頁資訊========================================
        [HttpGet("channel/home/{id}")]
        public async Task<ActionResult<ChannelHomeDto>> GetChannelHome(int id)
        {
            var dto = await _videoServices.GetChannelHome(id);

            if (dto == null)
            {
                Console.WriteLine("API呼叫成功，但失敗!");
                return NotFound();
            }

            return Ok(dto);
        }
        
        [HttpGet("channel/{id}/videos")]
        public async Task<ActionResult<ChannelHomeDto>> GetChannelVideos(int id)
        {
            var dto = await _videoServices.GetChannelVideosAsync(id);

            if (dto == null)
            {
                Console.WriteLine("API呼叫成功，但失敗!");
                return NotFound();
            }

            return Ok(dto);
        }

       


        //==============================獲取影片列表===========================

        [HttpGet("Recommend")]
        public async Task<ActionResult<List<VideoCardDto>>> GetVideoRecommend()

        {
            var dto = await _videoServices.GetVideoRecommendAsync();

            if (dto == null)
            {
                return NotFound();
            }

            return Ok(dto);
        }

        //===============================留言發布========================================

        [HttpPost("comment/publish")]
        [Authorize]
        public async Task<IActionResult> CommentPublish([FromBody] VideoCommentRequest req)
        {
            if (req == null)
                return BadRequest("Request body is required");

            if (req.Videoid <= 0)
                return BadRequest("Invalid video id");

            if (string.IsNullOrWhiteSpace(req.CommentContent))
                return BadRequest("Comment content is required");

            var userId = GetUserId();

            try
            {
                var result = await _videoServices.PostVideoComment(req, userId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        //==============================Likes相關===========================
        [Authorize]
        [HttpGet("{guid}/likecheck")]
        public async Task<ActionResult<VideoLikesDto>> GetVideoLikes(string guid)
        {

            var userId = GetUserId();

            var dto = await _videoServices.CheckVideoLike(userId, guid);
            return Ok(dto);
        }

        [HttpPost("{guid}/liketoggle")]
        [Authorize]
        public async Task<ActionResult<VideoLikesDto>> ToggleVideoLikes(string guid)
        {

            var userId = GetUserId();

            var dto = await _videoServices.VideosLikeToggle(userId, guid);
            return Ok(dto);
        }

        // ============================== Channel Follow ==============================

        [Authorize]
        [HttpGet("channels/{channelId}/followcheck")]
        public async Task<ActionResult<bool>> CheckChannelFollow(int channelId)
        {
            var userId = GetUserId();
            var isFollowing = await _videoServices.CheckChannelFollow(userId, channelId);
            return Ok(isFollowing);
        }

        [Authorize]
        [HttpPost("channels/{channelId}/followtoggle")]
        public async Task<ActionResult<bool>> ToggleChannelFollow(int channelId)
        {
            var userId = GetUserId();
            var isFollowing = await _videoServices.ChannelFollowToggle(userId, channelId);
            return Ok(isFollowing);
        }

        // ============================== 更新觀看 ==============================

        public class UpdateWatchHistoryDto
        {
            public int VideoId { get; set; }
            public int LastPosition { get; set; }
        }

       
        [HttpPost("views/update")]
        [Authorize]
        public async Task<IActionResult> UpdateWatchHistory([FromBody] UpdateWatchHistoryDto dto)
        {
            var userId = GetUserId();

            await _videoServices
                .WatchVideoUpdateAsync(userId, dto.VideoId, dto.LastPosition);

            return Ok(true);
        }

        /// <summary>
        /// 取得使用者觀看紀錄
        /// </summary>
        [HttpGet("views/history")]
        [Authorize]
        public async Task<ActionResult<List<WatchHistoryDto>>> GetWatchHistory()
        {
            // 假設 User.Identity.Name 或 Claims 取得 UserId
            var userId = GetUserId();
            var history = await _videoServices.GetWatchHistoryAsync(userId);
            return Ok(history);
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

        //搜尋
        [HttpGet("search")]
        public async Task<IActionResult> Search(
     [FromQuery] string? keyword,
     [FromQuery] string sortBy = "date",
     [FromQuery] string sortOrder = "desc",
     [FromQuery] int take = 10)
        {
            var result = await _videoServices.SearchVideosAsync(
                keyword, sortBy, sortOrder, take);

            return Ok(result);
        }
    }
}

