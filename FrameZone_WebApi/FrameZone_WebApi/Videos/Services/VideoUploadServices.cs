using FrameZone_WebApi.Models;
using FrameZone_WebApi.Videos.DTOs;
using FrameZone_WebApi.Videos.Models;
using Humanizer;

namespace FrameZone_WebApi.Videos.Services
{
    public interface IVideoUploadService
    {
        Task<VideoUploadResult> UploadAsync(IFormFile file);
    }

    public class VideoUploadService : IVideoUploadService
    {
        public async Task<VideoUploadResult> UploadAsync(IFormFile file)
        {
            //5.1後端檔案驗證
            ValidateFile(file);

            //5.2建立GUID資料夾
            var videoDir = CreateVideoDirectory();

            // 5.3 儲存檔案
            var filePath = await SaveFileAsync(file, videoDir);

            // ⚠️ 目前只確認 Service 有被呼叫
            await Task.CompletedTask;

            return new VideoUploadResult
            {
                Success = true,
                Message = "檔案上傳成功"
                // 下一步會加：Guid / VideoId / 狀態
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
        private string CreateVideoDirectory()
        {
            var guid = Guid.NewGuid().ToString("N");
            var root = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "videos",
                "video",
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

    }
}
