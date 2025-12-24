using System.Text.RegularExpressions;
using FrameZone_WebApi.Videos.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace FrameZone_WebApi.Videos.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VideoPlayerController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly IMemoryCache _cache;
        private readonly ILogger<VideoPlayerController> _logger;
        private readonly VideoPlayerService _videoPlayerService;

        public VideoPlayerController(
            IWebHostEnvironment env,
            IMemoryCache cache,
            ILogger<VideoPlayerController> logger,VideoPlayerService videoPlayerService)
        {
            _env = env;
            _cache = cache;
            _logger = logger;
            _videoPlayerService = videoPlayerService;
        }

        /// <summary>
        /// 取得 master.m3u8 或指定解析度 m3u8
        /// </summary>
        /// <param name="guid">影片 GUID</param>
        /// <param name="resolution">可選：360p、480p、720p、1080p，預設 master</param>
        [HttpGet("{guid}/{resolution?}")]
        public async Task<IActionResult> GetM3u8(string guid, string resolution = "master")
        {
            if (!_videoPlayerService.IsValidGuid(guid))
                return BadRequest("無效 GUID");

            if (!await _videoPlayerService.UserCanViewVideoAsync(guid))
                return Forbid();

            if (!_videoPlayerService.IsValidResolution(resolution))
                return BadRequest("不支援的解析度");

            try
            {
                var content = await _videoPlayerService.GetM3u8ContentAsync(guid, resolution);
                return Content(content, "application/vnd.apple.mpegurl");
            }
            catch (FileNotFoundException)
            {
                return NotFound("影片檔案不存在");
            }
        }

        /// <summary>
        /// 回傳 ts 片段，走控制器進行權限驗證
        /// </summary>
        [HttpGet("{guid}/ts/{fileName}")]
        [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> GetTsSegment(string guid, string fileName)
        {
            try
            {
                // 驗證 GUID 格式
                if (!IsValidGuid(guid))
                {
                    return BadRequest("無效的影片 ID");
                }

                // 驗證權限
                if (!await UserCanViewVideoAsync(guid))
                {
                    return Forbid();
                }

                // 驗證檔名安全性
                if (!IsValidTsFileName(fileName))
                {
                    _logger.LogWarning("非法的 TS 檔名: {FileName}", fileName);
                    return BadRequest("非法檔名");
                }

                var filePath = GetSecureFilePath(guid, fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogWarning("TS 檔案不存在: {FilePath}", filePath);
                    return NotFound("影片片段不存在");
                }

                // 支援 Range Request (斷點續傳)
                var fileInfo = new FileInfo(filePath);
                var enableRangeProcessing = true;

                Response.Headers["Accept-Ranges"] = "bytes";
                Response.Headers["Access-Control-Allow-Origin"] = "*";

                return PhysicalFile(filePath, "video/mp2t", enableRangeProcessing: enableRangeProcessing);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得 TS 片段時發生錯誤: {Guid}, {FileName}", guid, fileName);
                return StatusCode(500, "伺服器錯誤");
            }
        }

        /// <summary>
        /// 檢查影片是否存在
        /// </summary>
        [HttpHead("{guid}")]
        public async Task<IActionResult> CheckVideoExists(string guid)
        {
            if (!IsValidGuid(guid))
                return BadRequest();

            if (!await UserCanViewVideoAsync(guid))
                return Forbid();

            var masterPath = GetSecureFilePath(guid, "master.m3u8");
            return System.IO.File.Exists(masterPath) ? Ok() : NotFound();
        }

        #region 私有方法
       

        /// <summary>
        /// 取得安全的檔案路徑（防止路徑穿越攻擊）
        /// </summary>
        private string GetSecureFilePath(string guid, string fileName)
        {
            var basePath = Path.Combine(_env.WebRootPath, "videos", guid, "hls");
            var fullPath = Path.GetFullPath(Path.Combine(basePath, fileName));

            // 確保路徑在預期的目錄內
            if (!fullPath.StartsWith(Path.GetFullPath(basePath)))
            {
                throw new UnauthorizedAccessException("非法的檔案路徑");
            }

            return fullPath;
        }

        /// <summary>
        /// 驗證 GUID 格式
        /// </summary>
        private bool IsValidGuid(string guid)
        {
            return Guid.TryParse(guid, out _) ||
                   Regex.IsMatch(guid, @"^[a-zA-Z0-9\-_]{20,50}$");
        }

        /// <summary>
        /// 驗證 TS 檔名安全性
        /// </summary>
        private bool IsValidTsFileName(string fileName)
        {
            // 只允許字母、數字、底線、連字號和 .ts 副檔名
            return Regex.IsMatch(fileName, @"^[\w\-]+\.ts$") &&
                   !fileName.Contains("..") &&
                   !fileName.Contains("/") &&
                   !fileName.Contains("\\");
        }

        /// <summary>
        /// 權限驗證（改為非同步方法）
        /// </summary>
        private async Task<bool> UserCanViewVideoAsync(string guid)
        {
            // TODO: 實作你的權限邏輯
            // 範例：

            // 1. 檢查用戶是否登入
            // if (!User.Identity?.IsAuthenticated ?? false)
            //     return false;

            // 2. 檢查用戶是否有訂閱
            // var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // var hasSubscription = await _subscriptionService.HasActiveSubscription(userId);
            // if (!hasSubscription)
            //     return false;

            // 3. 檢查用戶是否購買此影片
            // var hasPurchased = await _videoService.UserHasPurchased(userId, guid);
            // return hasPurchased;

            // 4. 檢查 IP 限制
            // var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            // return await _videoService.IsIpAllowed(guid, clientIp);

            return await Task.FromResult(true);
        }

        #endregion
    }
}
