using System.Diagnostics;
using System.Net.Http;
using FrameZone_WebApi.Videos.DTOs;
using FrameZone_WebApi.Videos.Repositories;
using NuGet.Protocol.Core.Types;

namespace FrameZone_WebApi.Videos.Services
{
    public class VideoTranscodeServices
    {
        private readonly IWebHostEnvironment _env;
        public VideoTranscodeServices(IWebHostEnvironment env)
        {
            _env = env;
        }
        public async Task<TranscodeResult> TranscodeAsync(string videoGuid, string fileName)
        {
            try
            {
                // 1️⃣ 輸入影片路徑檢查
                var inputPath = Path.Combine(_env.WebRootPath, "videos", videoGuid, fileName);
                if (!File.Exists(inputPath))
                    return TranscodeResult.Fail($"找不到輸入檔案: {inputPath}");

                // 2️⃣ 輸出資料夾 - 清理舊檔案
                var outputDir = Path.Combine(_env.WebRootPath, "videos", videoGuid, "hls");
                if (Directory.Exists(outputDir))
                    Directory.Delete(outputDir, true);

                Directory.CreateDirectory(outputDir);

                // 3️⃣ FFmpeg 路徑檢查
                var ffmpegPath = Path.Combine(_env.WebRootPath, "FFmpeg", "ffmpeg.exe");
                if (!File.Exists(ffmpegPath))
                    return TranscodeResult.Fail($"找不到 FFmpeg: {ffmpegPath}");

                // 4️⃣ 建立 FFmpeg 參數 (多畫質 HLS)
                var arguments = BuildFFmpegArguments(inputPath, outputDir);

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
                var stdErrTask = process.StandardError.ReadToEndAsync();
                var stdOutTask = process.StandardOutput.ReadToEndAsync();

                await Task.WhenAll(stdErrTask, stdOutTask);

                process.WaitForExit();

                if (process.ExitCode != 0)
                    return TranscodeResult.Fail("FFmpeg 轉檔失敗", process.ExitCode, stdErrTask.Result);

                // 6️⃣ 生成 Master Playlist
                var masterM3u8Path = Path.Combine(outputDir, "master.m3u8");
                var masterContent = "#EXTM3U\n" +
                                    "#EXT-X-VERSION:3\n" +
                                    "#EXT-X-STREAM-INF:BANDWIDTH=800000,RESOLUTION=640x360\n" +
                                    "360p.m3u8\n" +
                                    "#EXT-X-STREAM-INF:BANDWIDTH=1400000,RESOLUTION=854x480\n" +
                                    "480p.m3u8\n" +
                                    "#EXT-X-STREAM-INF:BANDWIDTH=2800000,RESOLUTION=1280x720\n" +
                                    "720p.m3u8\n";
                await File.WriteAllTextAsync(masterM3u8Path, masterContent);

                // 7️⃣ 驗證輸出檔案
                var requiredFiles = new[] { "360p.m3u8", "480p.m3u8", "720p.m3u8", "master.m3u8" };
                var missingFiles = requiredFiles.Where(f => !File.Exists(Path.Combine(outputDir, f))).ToList();
                if (missingFiles.Any())
                    return TranscodeResult.Fail("部分輸出檔案未生成", missingFiles: missingFiles);

                // 8️⃣ 回傳成功結果
                return TranscodeResult.SuccessResult($"/videos/{videoGuid}/hls/master.m3u8", new[] { "360p", "480p", "720p" });
            }
            catch (Exception ex)
            {
                return TranscodeResult.Fail(ex.Message, stackTrace: ex.StackTrace);
            }
        }

        private string BuildFFmpegArguments(string inputPath, string outputDir)
        {
            return $"-i \"{inputPath}\" " +
                   // 360p
                   "-vf scale=640:360 -c:v libx264 -preset fast -crf 23 -c:a aac -b:a 96k -ar 44100 " +
                   $"-hls_time 6 -hls_playlist_type vod -hls_segment_filename \"{Path.Combine(outputDir, "360p_%03d.ts")}\" " +
                   $"\"{Path.Combine(outputDir, "360p.m3u8")}\" " +
                   // 480p
                   "-vf scale=854:480 -c:v libx264 -preset fast -crf 22 -c:a aac -b:a 128k -ar 44100 " +
                   $"-hls_time 6 -hls_playlist_type vod -hls_segment_filename \"{Path.Combine(outputDir, "480p_%03d.ts")}\" " +
                   $"\"{Path.Combine(outputDir, "480p.m3u8")}\" " +
                   // 720p
                   "-vf scale=1280:720 -c:v libx264 -preset fast -crf 21 -c:a aac -b:a 192k -ar 44100 " +
                   $"-hls_time 6 -hls_playlist_type vod -hls_segment_filename \"{Path.Combine(outputDir, "720p_%03d.ts")}\" " +
                   $"\"{Path.Combine(outputDir, "720p.m3u8")}\"";
        }
    
}
}
