using FrameZone_WebApi.Models;
using FrameZone_WebApi.Videos.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace FrameZone_WebApi.Videos.Repositories
{

    public class VideoUploadRepository
    {
        private readonly AAContext _context;
        public VideoUploadRepository(AAContext context)
        {
            _context = context;
        }

        //建立影片草稿資料表
        public async Task<Video> VideoDraftCreateAsync(Video video)
        {
            _context.Videos.Add(video);
            await _context.SaveChangesAsync();

            return video; // 回傳已生成 Id 的 Video
        }

        //獲取影片資料by guid
        public async Task<Video> GetVideoAsyncByGuid(Guid guid)
        {
            var data = await _context.Videos
                .FirstOrDefaultAsync(v => v.VideoUrl == guid.ToString());

            if (data == null) return null!; // 或 throw new Exception("留言不存在");

            return data; // 回傳已生成 Video
        }

    }
}
