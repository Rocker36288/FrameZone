using System.Diagnostics;
using FrameZone_WebApi.Models;
using FrameZone_WebApi.Videos.DTOs;
using FrameZone_WebApi.Videos.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        //==========================縮圖區塊==========================


        //==========================影片轉碼呼叫===================
        //========================== 後端 =========================

        [HttpGet("{videoGuid}/status")]
        public async Task<IActionResult> GetVideoStatus(Guid videoGuid)
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

        //[HttpPost("{videoGuid}/transcode")]
        //public async Task<IActionResult> TranscodeVideo(string videoGuid)
        //{
        //    var fileName = "source.mp4"; // 假設原始影片固定名稱
        //    var result = await _videoUploadService.TranscodeAsync(videoGuid, fileName);

        //    if (!result.Success)
        //        return BadRequest(result);

        //    return Ok(result);
        //}

        //test解碼測試
        [HttpGet("test")]
        public IActionResult TestVideo(int id)
        {
            try
            {
                // 1️⃣ 輸入影片路徑檢查
                var inputPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "videos", "video", "test.mp4");
                if (!System.IO.File.Exists(inputPath))
                {
                    return BadRequest(new { error = $"找不到輸入檔案: {inputPath}" });
                }

                // 2️⃣ 輸出資料夾 - 清理舊檔案
                var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "videos", "video", "test");
                if (Directory.Exists(outputDir))
                {
                    Directory.Delete(outputDir, true); // 刪除舊檔案避免衝突
                }
                Directory.CreateDirectory(outputDir);

                // 3️⃣ FFmpeg 路徑檢查
                var ffmpegPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "FFmpeg", "ffmpeg.exe");
                if (!System.IO.File.Exists(ffmpegPath))
                {
                    return BadRequest(new { error = $"找不到 FFmpeg: {ffmpegPath}" });
                }

                // 4️⃣ 多畫質 HLS 轉檔參數
                var arguments = $"-i \"{inputPath}\" " +
                    // 360p 串流
                    "-vf scale=640:360 -c:v libx264 -preset fast -crf 23 -c:a aac -b:a 96k -ar 44100 " +
                    $"-hls_time 6 -hls_playlist_type vod -hls_segment_filename \"{Path.Combine(outputDir, "360p_%03d.ts")}\" " +
                    $"\"{Path.Combine(outputDir, "360p.m3u8")}\" " +

                    // 480p 串流
                    "-vf scale=854:480 -c:v libx264 -preset fast -crf 22 -c:a aac -b:a 128k -ar 44100 " +
                    $"-hls_time 6 -hls_playlist_type vod -hls_segment_filename \"{Path.Combine(outputDir, "480p_%03d.ts")}\" " +
                    $"\"{Path.Combine(outputDir, "480p.m3u8")}\" " +

                    // 720p 串流
                    "-vf scale=1280:720 -c:v libx264 -preset fast -crf 21 -c:a aac -b:a 192k -ar 44100 " +
                    $"-hls_time 6 -hls_playlist_type vod -hls_segment_filename \"{Path.Combine(outputDir, "720p_%03d.ts")}\" " +
                    $"\"{Path.Combine(outputDir, "720p.m3u8")}\"";

                Console.WriteLine("開始轉檔...");
                Console.WriteLine($"FFmpeg 路徑: {ffmpegPath}");
                Console.WriteLine($"輸入檔案: {inputPath}");
                Console.WriteLine($"輸出資料夾: {outputDir}");

                // 5️⃣ 執行 FFmpeg
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = ffmpegPath,
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();

                // 非同步讀取輸出避免死鎖
                var stdErr = process.StandardError.ReadToEnd();
                var stdOut = process.StandardOutput.ReadToEnd();

                process.WaitForExit();

                Console.WriteLine("=== FFmpeg 標準錯誤輸出 ===");
                Console.WriteLine(stdErr);

                if (process.ExitCode != 0)
                {
                    return BadRequest(new
                    {
                        error = "FFmpeg 轉檔失敗",
                        exitCode = process.ExitCode,
                        details = stdErr
                    });
                }

                // 6️⃣ 生成主 Master Playlist
                var masterM3u8Path = Path.Combine(outputDir, "master.m3u8");
                var masterContent = "#EXTM3U\n" +
                    "#EXT-X-VERSION:3\n" +
                    "#EXT-X-STREAM-INF:BANDWIDTH=800000,RESOLUTION=640x360\n" +
                    "360p.m3u8\n" +
                    "#EXT-X-STREAM-INF:BANDWIDTH=1400000,RESOLUTION=854x480\n" +
                    "480p.m3u8\n" +
                    "#EXT-X-STREAM-INF:BANDWIDTH=2800000,RESOLUTION=1280x720\n" +
                    "720p.m3u8\n";

                System.IO.File.WriteAllText(masterM3u8Path, masterContent);

                // 7️⃣ 驗證輸出檔案
                var requiredFiles = new[] { "360p.m3u8", "480p.m3u8", "720p.m3u8", "master.m3u8" };
                var missingFiles = requiredFiles.Where(f => !System.IO.File.Exists(Path.Combine(outputDir, f))).ToList();

                if (missingFiles.Any())
                {
                    return BadRequest(new
                    {
                        error = "部分輸出檔案未生成",
                        missingFiles = missingFiles
                    });
                }

                Console.WriteLine("轉檔完成!");

                // 8️⃣ 回傳 Master Playlist URL
                var m3u8Url = "/videos/video/test/master.m3u8";
                return Ok(new
                {
                    m3u8 = m3u8Url,
                    qualities = new[] { "360p", "480p", "720p" },
                    message = "多畫質轉檔成功"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"錯誤: {ex.Message}");
                return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

    }
}
