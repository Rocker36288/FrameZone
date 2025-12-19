using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using FrameZone_WebApi.Models;
using FrameZone_WebApi.Videos.DTOs;
using FrameZone_WebApi.Videos.Helpers;
using FrameZone_WebApi.Videos.Repositories;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NuGet.Protocol.Core.Types;
using Xabe.FFmpeg;

namespace FrameZone_WebApi.Videos.Services
{
    public interface IVideoUploadService
    {
        Task<VideoUploadResult> UploadAsync(IFormFile file);
        // 新增：生成 Base64 縮圖
        Task<List<string>> GenerateThumbnailsInMemoryAsync(string videoPath);

        Task<Video> GetVideoByGuidAsync(string guid);
        Task<VideoPublishResult> PublishedVideo([FromBody] VideoPublishRequest req);
    }

    public class VideoUploadService : IVideoUploadService
    {
        
        private readonly VideoUploadRepository _repository;
        private readonly HttpClient _httpClient;
        private readonly VideoTranscodeServices _videoTranscodeServices;
        private readonly AaContextFactoryHelper _aaContextFactoryHelper;
        public VideoUploadService(VideoUploadRepository repository, HttpClient httpClient, IWebHostEnvironment env, VideoTranscodeServices transcodeService, AaContextFactoryHelper AaContextFactoryHelper)
        {
            _repository = repository;
            _httpClient = httpClient;
            _videoTranscodeServices = transcodeService;
            _aaContextFactoryHelper = AaContextFactoryHelper;
        }
        public async Task<VideoUploadResult> UploadAsync(IFormFile file)
        {
            // 5.1 後端檔案驗證
            ValidateFile(file);

            // 5.2 建立 GUID 資料夾
            var guid = Guid.NewGuid().ToString("N");
            var videoDir = CreateVideoDirectory(guid);

            // 5.3 儲存檔案
            var filePath = await SaveFileAsync(file, videoDir);

            // 6️ 生成草稿資料表
            var video = new Video
            {
                Title = file.FileName,
                ChannelId = 1,
                VideoUrl = guid,
                PrivacyStatus = "DRAFT",
                ProcessStatus = "UPLOADED",
                IsDeleted = false,
                IsFeatured = false,
            };
            var createdVideo = await _repository.VideoDraftCreateAsync(video);

            // 7️ 呼叫審核流程
            var reviewResult = await ReviewVideoAsync_Simulated(filePath, Guid.Parse(guid));
            createdVideo.ProcessStatus = reviewResult.Passed ? "REVIEWED_APPROVED" : "REVIEWED_REJECTED";
            //await _repository.VideoUpdateStatusAsync(createdVideo.VideoId, createdVideo.ProcessStatus);

            // 8️ 如果審核通過，立即啟動轉碼（非同步背景 Task）
            if (reviewResult.Passed)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        Console.WriteLine($"開始轉碼 VideoGuid={guid}");
                        var transcodeResult = await _videoTranscodeServices.TranscodeAsync(guid, "source.mp4");

                        await using var context = await _aaContextFactoryHelper.CreateAsync();
                        var video = await context.Videos.FindAsync(createdVideo.VideoId);

                        if (video != null)
                        {
                            video.ProcessStatus = transcodeResult.Success ? "READY" : "FAILED_TRANSCODE";
                            await context.SaveChangesAsync();
                        }

                        Console.WriteLine(transcodeResult.Success
                            ? $"轉碼完成 VideoGuid={guid}"
                            : $"轉碼失敗 VideoGuid={guid}，原因: {transcodeResult.Error}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"轉碼異常 VideoGuid={guid}: {ex.Message}");
                    }
                });
            }

            // 回傳 Upload + 審核結果給前端
            return new VideoUploadResult
            {
                Success = true,
                Message = "檔案上傳成功並審核完成",
                VideoId = createdVideo.VideoId,
                Guid = createdVideo.VideoUrl,
                Status = createdVideo.ProcessStatus,
                ReviewPassed = reviewResult.Passed,
                ReviewReason = reviewResult.Reason
            };
        }

        //檔案驗證
        private void ValidateFile(IFormFile file)
        {
            if (file == null)
                throw new ArgumentException("未接收到檔案");

            if (file.Length == 0)
                throw new ArgumentException("檔案為空");

            if (!file.ContentType.StartsWith("video/"))
                throw new ArgumentException("不支援的檔案格式");

            const long maxSize = 500L * 1024 * 1024; // 500MB
            if (file.Length > maxSize)
                throw new ArgumentException("檔案大小超過限制");
        }

        //建立資料夾
        private string CreateVideoDirectory(string guid)
        {

            var root = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "videos",
                guid
            );

            Directory.CreateDirectory(root);

            return root;
        }

        //確認上傳至wwwroot/videos/video/...
        private async Task<string> SaveFileAsync(IFormFile file, string directoryPath)
        {
            var filePath = Path.Combine(directoryPath, "source.mp4");

            try
            {
                await using var stream = new FileStream(
                    filePath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None
                );

                await file.CopyToAsync(stream);
            }
            catch (Exception ex)
            {
                throw new IOException("檔案儲存失敗", ex);
            }

            if (!File.Exists(filePath))
                throw new IOException("檔案未成功寫入磁碟");

            return filePath;
        }

        //生成縮圖
        public async Task<List<string>> GenerateThumbnailsInMemoryAsync(string videoPath)
        {
            var base64Images = new List<string>();
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            try
            {
                Directory.CreateDirectory(tempDir);

                var mediaInfo = await FFmpeg.GetMediaInfo(videoPath);
                var duration = mediaInfo.Duration.TotalSeconds;
                var timestamps = new[] { duration * 0.1, duration * 0.3, duration * 0.5, duration * 0.7 };

                for (int i = 0; i < timestamps.Length; i++)
                {
                    var ts = timestamps[i];
                    var outputPath = Path.Combine(tempDir, $"thumbnail_{i}.jpg");
                    var timeStr = TimeSpan.FromSeconds(ts).ToString(@"hh\:mm\:ss");

                    // 使用正確的 FFmpeg 參數格式
                    await FFmpeg.Conversions.New()
                        .AddParameter($"-ss {timeStr}")
                        .AddParameter($"-i \"{videoPath}\"")
                        .AddParameter("-frames:v 1")
                        .AddParameter("-q:v 2") // 設置較高的圖片品質
                        .AddParameter($"\"{outputPath}\"")
                        .Start();

                    // 讀取生成的圖片並轉換為 base64
                    if (File.Exists(outputPath))
                    {
                        var imageBytes = await File.ReadAllBytesAsync(outputPath);
                        var base64 = Convert.ToBase64String(imageBytes);
                        base64Images.Add(base64);
                    }
                }
            }
            finally
            {
                // 確保清理臨時目錄
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }

            return base64Images;
        }

        // 審核結果 DTO，可依 API 規格調整
        public class ReviewResult
        {
            public bool Passed { get; set; }
            public string Reason { get; set; }
            public string RawJson { get; set; }
        }

        //實際傳送API審核
        public async Task<ReviewResult> ReviewVideoAsync(string videoPath, Guid videoGuid)
        {
            // 1. 生成縮圖 base64
            var thumbnails = await GenerateThumbnailsInMemoryAsync(videoPath);

            // 2. 準備 OpenAI Moderation API 請求 payload
            var inputList = new List<object>
                {
                    new { type = "text", text = $"Video ID: {videoGuid}" } // 可以送標題或其他文字
                };

            // 加入每張縮圖
            for (int i = 0; i < thumbnails.Count; i++)
            {
                inputList.Add(new
                {
                    type = "image_url",
                    image_url = new { url = $"data:image/jpeg;base64,{thumbnails[i]}" }
                });
            }

            var requestBody = new
            {
                model = "omni-moderation-latest",
                input = inputList
            };

            // 3. 建立 HttpClient 並使用環境變數讀取 API Key
            using var client = new HttpClient();
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("OpenAI API key not found in environment variables.");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            // 4. 呼叫 OpenAI Moderation API
            var response = await client.PostAsJsonAsync("https://api.openai.com/v1/moderations", requestBody);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();


            // 5. 解析結果
            bool passed = true;
            string reason = "";

            try
            {
                using var doc = JsonDocument.Parse(responseJson);
                var results = doc.RootElement.GetProperty("results");
                foreach (var r in results.EnumerateArray())
                {
                    bool flagged = r.GetProperty("flagged").GetBoolean();
                    if (flagged)
                    {
                        passed = false;
                        // 可以從 categories 拿違規類型
                        reason += r.GetProperty("categories").ToString() + "; ";
                    }
                }
            }
            catch
            {
                // 若解析失敗，直接當成不通過
                passed = false;
                reason = "Failed to parse OpenAI response.";
            }


            // 5. 回傳 DTO
            return new ReviewResult
            {
                Passed = passed,
                Reason = reason,
                RawJson = responseJson
            };
        }

        public async Task<ReviewResult> ReviewVideoAsync_Simulated(string videoPath, Guid videoGuid)
        {
            Console.WriteLine("審核模擬");
            // 1. 生成縮圖 base64 (保留，確保流程測試)
            var thumbnails = await GenerateThumbnailsInMemoryAsync(videoPath);

            // 2. 模擬審核結果
            var random = new Random();
            bool passed = true; //random.Next(0, 2) == 0; // 隨機通過或不通過
            string reason = passed ? "Simulated: approved" : "Simulated: flagged for testing";

            // 3. 模擬原始 JSON
            string rawJson = @"{
        ""results"": [
            { ""flagged"": " + (!passed).ToString().ToLower() + @",
              ""categories"": { ""violence"": false, ""sexual"": false } }
        ]
    }";

            // 4. 回傳 DTO
            return new ReviewResult
            {
                Passed = passed,
                Reason = reason,
                RawJson = rawJson
            };
        }

        //用guid拿資料
        public async Task<Video> GetVideoByGuidAsync(string guid)
        {
            var video = await _repository.GetVideoAsyncByGuid(guid);

            if (video == null)
            {
                return null;
            }

            return video;
        }

        //影片發布服務
        public async Task<VideoPublishResult> PublishedVideo([FromBody] VideoPublishRequest req)
        {
            var video = await _repository.GetVideoAsyncByGuid(req.VideoGuid);

            if (video == null)
                throw new KeyNotFoundException($"Video with guid {req.VideoGuid} not found");

            video.Title = req.Title;
            video.Description = req.Description;

            var result = await _repository.VideoPublishedAsync(video);

            return new VideoPublishResult
            {
                VideoGuid = result.VideoGuid,
                Title = result.Title,
                ProcessStatus = result.ProcessStatus,
                PublishDate = result.PublishDate
            };
        }


    }
}
