using System.Diagnostics;
using FrameZone_WebApi.Models;
using FrameZone_WebApi.Videos.DTOs;
using FrameZone_WebApi.Videos.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;

namespace FrameZone_WebApi.Videos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VideoUploadController : ControllerBase
    {
        private readonly IVideoUploadService _videoUploadService;
        private readonly IWebHostEnvironment _env;
        public VideoUploadController(IVideoUploadService videoUploadService, IWebHostEnvironment env)
        {
            _videoUploadService = videoUploadService;
            _env = env; // 注入成功
        }

        //VideoUpload/calltest
        [HttpGet("calltest")]
        public IActionResult G(int id)
        {
            Console.WriteLine("FF");
            return StatusCode(500, "測試成功");
        }

        //videoupload/upload
        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] IFormFile file)
        {
            try
            {
                Console.WriteLine("上傳啟動");

                if (file == null || file.Length == 0)
                    return BadRequest("未選擇檔案");

                var result = await _videoUploadService.UploadAsync(file);

                if (!result.Success)
                    return BadRequest(result.Message);

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"上傳失敗: {ex}");
                return StatusCode(500, "伺服器錯誤，請稍後再試");
            }
        }

        //==========================縮圖區塊=========================
        //========================== 後端 =========================
        [HttpPost("thumbnails-preview")]
        public async Task<IActionResult> GetThumbnails([FromBody] VideoGuidDto dto)
        {
            var videoPath = Path.Combine(_env.WebRootPath, "videos", dto.VideoGuid, "source.mp4");

            if (!System.IO.File.Exists(videoPath))
                return NotFound(new { message = "影片檔案不存在" });

            try
            {
                var thumbs = await _videoUploadService.GenerateThumbnailsInMemoryAsync(videoPath);

                // 將 base64 轉換為完整的 data URI
                var dataUris = thumbs.Select(base64 => $"data:image/jpeg;base64,{base64}").ToList();

                return Ok(dataUris);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "生成縮圖失敗", error = ex.Message });
            }
        }

        public class VideoGuidDto
        {
            public string VideoGuid { get; set; }
        }

        [HttpPost("save-thumbnail")]
        public async Task<IActionResult> SaveThumbnail([FromBody] ThumbnailDto dto)
        {
            try
            {
                // 移除 data URI 前綴
                var base64Data = dto.ThumbnailBase64.Contains(",")
                    ? dto.ThumbnailBase64.Split(',')[1]
                    : dto.ThumbnailBase64;

                var bytes = Convert.FromBase64String(base64Data);
                var dir = Path.Combine(_env.WebRootPath, "videos", dto.VideoGuid);

                Directory.CreateDirectory(dir);

                var path = Path.Combine(dir, "thumbnail.jpg");
                await System.IO.File.WriteAllBytesAsync(path, bytes);

                return Ok(new { message = "縮圖儲存成功", path = $"/videos/{dto.VideoGuid}/thumbnail.jpg" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "儲存縮圖失敗", error = ex.Message });
            }
        }

        public class ThumbnailDto
        {
            public string VideoGuid { get; set; }
            public string ThumbnailBase64 { get; set; }
        }

        //====================================================


        //==========================影片轉碼呼叫===================

        [HttpGet("{videoGuid}/status")]
        public async Task<IActionResult> GetVideoStatus(string videoGuid)
        {
            var video = await _videoUploadService.GetVideoByGuidAsync(videoGuid);
            if (video == null) return NotFound();

            return Ok(new
            {
                video.VideoUrl,
                video.ProcessStatus,       // Uploaded / PendingReview / Approved / Rejected / Transcoding / Ready
                video.AiAuditResult, // 如果被拒絕可給使用者原因
                TranscodeProgress = 0
            });
        }


        //===========================草稿或發布建立====================

        [HttpPost("publish")]
        public async Task<IActionResult> Publish([FromBody] VideoPublishRequest req)
        {
            if (req == null || string.IsNullOrEmpty(req.VideoGuid))
                return BadRequest("Invalid request");

            try
            {
                var result = await _videoUploadService.PublishedVideo(req);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}
