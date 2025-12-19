using System.Text.RegularExpressions;

namespace FrameZone_WebApi.Videos.Services
{
    public class VideoPlayerService
    {

        // 支援的解析度
        private static readonly string[] AllowedResolutions = { "360p", "480p", "720p" };
        private readonly IWebHostEnvironment _env;
        public VideoPlayerService(IWebHostEnvironment env)
        {
            _env = env;
        }

        #region 路徑與檔案

        /// <summary>
        /// 取得安全的檔案路徑（防止路徑穿越攻擊）
        /// </summary>
        public string GetSecureFilePath(string guid, string fileName)
        {
            var basePath = Path.Combine(_env.WebRootPath, "videos", guid, "hls");
            var fullPath = Path.GetFullPath(Path.Combine(basePath, fileName));

            // 確保路徑在預期的目錄內
            if (!fullPath.StartsWith(Path.GetFullPath(basePath)))
                throw new UnauthorizedAccessException("非法的檔案路徑");

            return fullPath;
        }

        /// <summary>
        /// 檢查檔案是否存在
        /// </summary>
        public bool FileExists(string guid, string fileName)
        {
            var path = GetSecureFilePath(guid, fileName);
            return File.Exists(path);
        }

        #endregion

        #region 驗證

        public bool IsValidGuid(string guid)
        {
            return Guid.TryParse(guid, out _) ||
                   Regex.IsMatch(guid, @"^[a-zA-Z0-9\-_]{20,50}$");
        }

        public bool IsValidTsFileName(string fileName)
        {
            // 只允許字母、數字、底線、連字號和 .ts 副檔名
            return Regex.IsMatch(fileName, @"^[\w\-]+\.ts$") &&
                   !fileName.Contains("..") &&
                   !fileName.Contains("/") &&
                   !fileName.Contains("\\");
        }

        public bool IsValidResolution(string resolution)
        {
            return resolution == "master" || AllowedResolutions.Contains(resolution);
        }

        #endregion

        #region m3u8

        /// <summary>
        /// 修改 m3u8 內容，將 ts 路徑改寫為 API 路徑
        /// </summary>
        public string ModifyM3u8Content(string content, string guid, string resolution)
        {
            // 如果是 master.m3u8，需要改寫子 playlist 路徑
            if (resolution == "master")
            {
                content = Regex.Replace(
                    content,
                    @"(?<=\n)(\d+p\.m3u8)",
                    match => $"/api/VideoPlayer/{guid}/{match.Value.Replace(".m3u8", "")}"
                );
            }

            // 改寫 ts 檔案路徑
            content = Regex.Replace(
                content,
                @"(?<=\n)([^#\s].*?\.ts)",
                match => $"/api/VideoPlayer/{guid}/ts/{match.Groups[1].Value}"
            );

            return content;
        }

        /// <summary>
        /// 讀取並取得 m3u8 內容
        /// </summary>
        public async Task<string> GetM3u8ContentAsync(string guid, string resolution)
        {
            var fileName = resolution == "master" ? "master.m3u8" : $"{resolution}.m3u8";
            var filePath = GetSecureFilePath(guid, fileName);

            if (!File.Exists(filePath))
                throw new FileNotFoundException("M3U8 檔案不存在", filePath);

            var content = await File.ReadAllTextAsync(filePath);
            return ModifyM3u8Content(content, guid, resolution);
        }

        #endregion

        #region 權限

        /// <summary>
        /// 權限驗證（範例，實際可接訂閱/購買/IP限制）
        /// </summary>
        public async Task<bool> UserCanViewVideoAsync(string guid)
        {
            // TODO: 改成實際權限檢查
            return await Task.FromResult(true);
        }

        #endregion
    }
}

