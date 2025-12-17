using FrameZone_WebApi.Videos.DTOs;
using FrameZone_WebApi.Videos.Services;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
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


        //api/videos/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<VideoCardDto>> GetVideo(int id)
        {
            var dto = await _videoServices.GetVideoCardAsync(id);

            if(dto == null)
            {
                Console.WriteLine("API呼叫成功，但失敗!");
                return NotFound();
            }

            return Ok(dto);
        }

        //api/videos/comment/{id}
        [HttpGet("comment/{id}")]
        public async Task<ActionResult<VideoCardDto>> GetComment(int id)
        {
            var dto = await _videoServices.GetVideoCommentByCommentidAsync(id);

            if (dto == null)
            {
                Console.WriteLine("API呼叫成功，但失敗!");
                return NotFound();
            }

            return Ok(dto);
        }

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

