using System.Diagnostics;
using FrameZone_WebApi.Videos.DTOs;
using FrameZone_WebApi.Videos.Services;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xabe.FFmpeg;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static FrameZone_WebApi.Videos.DTOs.ChannelCardDto;

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

        //api/videos/comment/{id}
        //[HttpGet("comment/{id}")]
        //public async Task<ActionResult<VideoCardDto>> GetComment(int id)
        //{
        //    var dto = await _videoServices.GetVideoCommentByCommentidAsync(id);

        //    if (dto == null)
        //    {
        //        Console.WriteLine("API呼叫成功，但失敗!");
        //        return NotFound();
        //    }

        //    return Ok(dto);
        //}

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

        //===============================留言發布========================================

        [HttpPost("comment/publish")]
        public async Task<IActionResult> CommentPublish([FromBody] VideoCommentRequest req)
        {
            if (req == null)
                return BadRequest("Request body is required");

            if (req.Videoid <= 0)
                return BadRequest("Invalid video id");

            if (req.UserId <= 0)
                return BadRequest("Invalid user id");

            if (string.IsNullOrWhiteSpace(req.CommentContent))
                return BadRequest("Comment content is required");

            try
            {
                var result = await _videoServices.PostVideoComment(req);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //

        //api/videos/channel/{id}
        [HttpGet("channel/{id}")]
        public async Task<ActionResult<VideoCardDto>> Getchannel(int id)
        {
            var dto = await _videoServices.GetChannelbyid(id);

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
    }
}

