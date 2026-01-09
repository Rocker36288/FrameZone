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
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using Xabe.FFmpeg;
using static FrameZone_WebApi.Videos.Services.VideoUploadService;

namespace FrameZone_WebApi.Videos.Services
{
    public interface IVideoUploadService
    {
        Task<VideoUploadResult> UploadAsync(IFormFile file,int userId);
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
        private readonly IConfiguration _configuration;
        public VideoUploadService(VideoUploadRepository repository, HttpClient httpClient, IWebHostEnvironment env, VideoTranscodeServices transcodeService, AaContextFactoryHelper AaContextFactoryHelper, IConfiguration configuration)
        {
            _repository = repository;
            _httpClient = httpClient;
            _videoTranscodeServices = transcodeService;
            _aaContextFactoryHelper = AaContextFactoryHelper;
            _configuration = configuration;
        }
        public async Task<VideoUploadResult> UploadAsync(IFormFile file, int userId)
        {
            ValidateFile(file);

            var guid = Guid.NewGuid().ToString("N");  // 無連字號格式
            var videoDir = CreateVideoDirectory(guid);

            var filePath = await SaveFileAsync(file, videoDir);

            var mediaInfo = await FFmpeg.GetMediaInfo(filePath);
            var duration = mediaInfo.Duration.TotalSeconds;
            var width = mediaInfo.VideoStreams.FirstOrDefault()?.Width ?? 0;
            var height = mediaInfo.VideoStreams.FirstOrDefault()?.Height ?? 0;
            var fileSize = new FileInfo(filePath).Length;

            var video = new Video
            {
                Title = file.FileName,
                ChannelId = userId,
                VideoUrl = guid,
                PrivacyStatus = "DRAFT",
                ProcessStatus = "UPLOADED",
                IsDeleted = false,
                IsFeatured = false,
                Duration = (int)Math.Round(mediaInfo.Duration.TotalSeconds),
                FileSize = fileSize,
                CreatedAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow,
                Resolution = $"{width}x{height}"
            };
            var createdVideo = await _repository.VideoDraftCreateAsync(video);

            // ✅ 傳字串格式的 guid
            var reviewResult = await ReviewVideoAsync_Simulated(filePath, guid);
            createdVideo.ProcessStatus = reviewResult.Passed ? "REVIEWED_APPROVED" : "REVIEWED_REJECTED";

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

        //確認上傳至wwwroot/video/...
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
        // 修改後的方法：返回實際檔案路徑列表
        public async Task<List<string>> GenerateThumbnailsForModerationAsync(string videoPath)
        {
            var thumbnailPaths = new List<string>();
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            try
            {
                Directory.CreateDirectory(tempDir);
                var mediaInfo = await FFmpeg.GetMediaInfo(videoPath);
                var duration = mediaInfo.Duration.TotalSeconds;

                // 根據影片長度決定取樣數量
                int sampleCount = duration < 60 ? 1 : 1;
                var timestamps = GenerateTimestamps(duration, sampleCount);

                for (int i = 0; i < timestamps.Length; i++)
                {
                    var ts = timestamps[i];
                    var outputPath = Path.Combine(tempDir, $"thumbnail_{i}.jpg");
                    var timeStr = TimeSpan.FromSeconds(ts).ToString(@"hh\:mm\:ss\.fff");

                    try
                    {
                        // 用 FFmpeg 提取幀
                        await FFmpeg.Conversions.New()
                            .AddParameter($"-ss {timeStr}")
                            .AddParameter($"-i \"{videoPath}\"")
                            .AddParameter("-vf scale=640:-1") // 縮小到 640px 寬
                            .AddParameter("-frames:v 1")
                            .AddParameter("-q:v 5") // 中等品質
                            .AddParameter($"\"{outputPath}\"")
                            .Start();

                        if (File.Exists(outputPath))
                        {
                            // 可選：進一步壓縮 (使用 ImageSharp)
                            var compressedPath = Path.Combine(tempDir, $"thumbnail_{i}_compressed.jpg");
                            await CompressImageWithImageSharpAsync(outputPath, compressedPath);

                            thumbnailPaths.Add(compressedPath);

                            var fileSize = new FileInfo(compressedPath).Length;
                            Console.WriteLine($"✓ 縮圖 {i}: {fileSize / 1024}KB (時間點: {timeStr})");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"✗ 縮圖 {i} 生成失敗: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"生成縮圖過程發生錯誤: {ex.Message}");
                // 清理已生成的檔案
                CleanupThumbnails(thumbnailPaths);
                throw;
            }

            return thumbnailPaths;
        }

        // 輔助方法：壓縮圖片並儲存到新檔案
        private async Task CompressImageWithImageSharpAsync(string inputPath, string outputPath)
        {
            using var image = await Image.LoadAsync(inputPath);
            var encoder = new JpegEncoder { Quality = 75 };
            await image.SaveAsJpegAsync(outputPath, encoder);
        }

        // 輔助方法：清理縮圖檔案
        private void CleanupThumbnails(List<string> thumbnailPaths)
        {
            foreach (var path in thumbnailPaths)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);

                        // 如果目錄為空，也刪除目錄
                        var directory = Path.GetDirectoryName(path);
                        if (Directory.Exists(directory) && !Directory.EnumerateFileSystemEntries(directory).Any())
                        {
                            Directory.Delete(directory);
                        }
                    }
                }
                catch { /* 忽略清理錯誤 */ }
            }
        }

        private double[] GenerateTimestamps(double duration, int count)
        {
            var timestamps = new double[count];
            for (int i = 0; i < count; i++)
            {
                timestamps[i] = duration * (i + 1) / (count + 1);
            }
            return timestamps;
        }


        //實際傳送API審核
        // 改用 Sightengine API
        // Service
        public async Task<ReviewResult> ReviewVideoAsync(string videoPath, string videoGuid)
        {
            List<string> thumbnailPaths = null;

            try
            {
                thumbnailPaths = await GenerateThumbnailsForModerationAsync(videoPath);

                if (thumbnailPaths == null || thumbnailPaths.Count == 0)
                {
                    return new ReviewResult
                    {
                        Passed = false,
                        Reason = "Failed to generate thumbnails",
                        RawJson = ""
                    };
                }

                var apiUser = _configuration["Sightengine:ApiUser"];
                var apiSecret = _configuration["Sightengine:ApiSecret"];

                if (string.IsNullOrWhiteSpace(apiUser) || string.IsNullOrWhiteSpace(apiSecret))
                    throw new InvalidOperationException("Sightengine API credentials not found.");

                using var client = new HttpClient
                {
                    Timeout = TimeSpan.FromMinutes(2)
                };

                bool overallPassed = true;
                var allReasons = new List<string>();
                var allResponses = new List<string>();

                foreach (var thumbnailPath in thumbnailPaths)
                {
                    try
                    {
                        using var content = new MultipartFormDataContent();

                        var imageBytes = await File.ReadAllBytesAsync(thumbnailPath);
                        var imageContent = new ByteArrayContent(imageBytes);
                        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

                        content.Add(imageContent, "media", Path.GetFileName(thumbnailPath));
                        content.Add(new StringContent("nudity-2.1"), "models");
                        content.Add(new StringContent(apiUser), "api_user");
                        content.Add(new StringContent(apiSecret), "api_secret");

                        var response = await client.PostAsync("https://api.sightengine.com/1.0/check.json", content);
                        response.EnsureSuccessStatusCode();

                        var responseJson = await response.Content.ReadAsStringAsync();
                        allResponses.Add(responseJson);
                        Console.WriteLine($"Sightengine Response: {responseJson}");

                        var (passed, reasons) = ParseSightengineResponse(responseJson);

                        if (!passed)
                        {
                            overallPassed = false;
                            allReasons.AddRange(reasons);
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        Console.WriteLine($"API 請求失敗 ({Path.GetFileName(thumbnailPath)}): {ex.Message}");
                        overallPassed = false;
                        allReasons.Add($"API request failed: {ex.Message}");
                    }
                }

                var reviewResult = new ReviewResult
                {
                    Passed = overallPassed,
                    Reason = allReasons.Count > 0
                        ? string.Join(", ", allReasons.Distinct())
                        : "Content approved",
                    RawJson = string.Join("\n---\n", allResponses)
                };

                await _repository.SaveAuditResultAsync(videoGuid, reviewResult);  // ✅ 傳字串

                return reviewResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"審核過程發生錯誤: {ex.Message}");
                return new ReviewResult
                {
                    Passed = false,
                    Reason = $"Review process failed: {ex.Message}",
                    RawJson = ""
                };
            }
            finally
            {
                if (thumbnailPaths != null)
                {
                    CleanupThumbnails(thumbnailPaths);
                }
            }
        }

        // 解析 Sightengine 回應
        private (bool passed, List<string> reasons) ParseSightengineResponse(string responseJson)
        {
            var reasons = new List<string>();
            bool passed = true;

            try
            {
                using var doc = JsonDocument.Parse(responseJson);
                var root = doc.RootElement;

                // 檢查狀態
                if (root.GetProperty("status").GetString() != "success")
                {
                    passed = false;
                    reasons.Add("API returned non-success status");
                    return (passed, reasons);
                }

                // 解析 nudity 結果 (根據你的需求設定閾值)
                if (root.TryGetProperty("nudity", out var nudity))
                {
                    var threshold = 0.5; // 可調整閾值

                    CheckCategory(nudity, "sexual_activity", threshold, reasons, ref passed);
                    CheckCategory(nudity, "sexual_display", threshold, reasons, ref passed);
                    CheckCategory(nudity, "erotica", threshold, reasons, ref passed);
                    CheckCategory(nudity, "very_suggestive", threshold, reasons, ref passed);
                    CheckCategory(nudity, "suggestive", 0.7, reasons, ref passed); // 較高閾值
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"解析回應失敗: {ex.Message}");
                passed = false;
                reasons.Add("Failed to parse Sightengine response");
            }

            return (passed, reasons);
        }

        // 檢查單一分類
        private void CheckCategory(JsonElement parent, string categoryName, double threshold,
            List<string> reasons, ref bool passed)
        {
            if (parent.TryGetProperty(categoryName, out var categoryElement))
            {
                var value = categoryElement.GetDouble();
                if (value > threshold)
                {
                    passed = false;
                    reasons.Add($"{categoryName}: {value:F3}");
                }
            }
        }

        public async Task<ReviewResult> ReviewVideoAsync_Simulated(string videoPath, string videoGuid)
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
            video.PrivacyStatus = req.PrivacyStatus;

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
