using FrameZone_WebApi.Models;
using FrameZone_WebApi.Videos.DTOs;
using FrameZone_WebApi.Videos.Enums;
using FrameZone_WebApi.Videos.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace FrameZone_WebApi.Videos.Repositories
{

    public class VideoUploadRepository
    {
        private readonly AaContextFactoryHelper _contextFactory;
        private readonly AAContext _context;
        public VideoUploadRepository(AAContext context, AaContextFactoryHelper contextFactory)
        {
            _context = context;
            _contextFactory = contextFactory;
        }

        //建立影片草稿資料表
        public async Task<Video> VideoDraftCreateAsync(Video video)
        {
            _context.Videos.Add(video);
            await _context.SaveChangesAsync();

            return video; // 回傳已生成 Id 的 Video
        }

        //獲取影片資料by guid
        public async Task<Video> GetVideoAsyncByGuid(string guid)
        {
            var data = await _context.Videos
                .FirstOrDefaultAsync(v => v.VideoUrl == guid);

            if (data == null) return null!; // 或 throw new Exception("留言不存在");

            return data; // 回傳已生成 Video
        }

        //更新影片處理資料狀態
        public async Task VideoUpdateStatusAsync(int videoId, string status)
        {
            await using var context = await _contextFactory.CreateAsync();
            var video = await context.Videos.FindAsync(videoId);
            if (video != null)
            {
                video.ProcessStatus = status;
                await context.SaveChangesAsync();
            }
        }

        //儲存發布資料
        public async Task<VideoPublishResult> VideoPublishedAsync(Video video)
        {
            if (video == null)
                throw new ArgumentNullException(nameof(video));

            if (video.ProcessStatus != ProcessStatus.READY.ToString())
                throw new InvalidOperationException("Video is not ready to be published");

            video.ProcessStatus = ProcessStatus.PUBLISHED.ToString();
            video.PublishDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return new VideoPublishResult
            {
                VideoGuid = video.VideoUrl,
                Title = video.Title,
                ProcessStatus = video.ProcessStatus,
                PublishDate = video.PublishDate.Value
            };
        }
    }
}
