using FrameZone_WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FrameZone_WebApi.Socials.Repositories
{
    public class PostRepository
    {
        private readonly AAContext _context;
        public PostRepository(AAContext context)
        {
            _context = context;
        }

        // ================= 取得多筆貼文 =================
        public async Task<List<Post>> GetPostsAsync()
        {
            try
            {
                return await _context.Posts
                    //依照貼文Id查詢 & 不顯示已刪除的貼文
                    .Where(p =>
                        p.Status != "Deleted" &&
                        p.DeletedAt == null)
                    //依照貼文建立時間排序
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"取得貼文失敗: {ex.Message}");
                return null;
            }
        }

        // ================= 取得貼文 =================
        public async Task<Post?> GetPostByIdAsync(int postId)
        {
            try
            {
                return await _context.Posts
                    //依照貼文Id查詢 & 不顯示已刪除的貼文
                    .Where(p =>
                        p.PostId == postId &&
                        p.Status != "Deleted" &&
                        p.DeletedAt == null)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"取得貼文失敗: {ex.Message}");
                return null;
            }
        }

        // ================= 新增貼文 =================
        public async Task<Post?> AddPostAsync(Post post)
        {
            try
            {
                //設定時間
                post.CreatedAt = DateTime.UtcNow;
                post.UpdatedAt = DateTime.UtcNow;

                //設定貼文狀態
                post.Status = post.Status ?? "Posted";

                //將貼文加入 DbSet
                await _context.Posts.AddAsync(post);

                //儲存到資料庫
                var result = await _context.SaveChangesAsync();

                return result > 0 ? post : null;
            }
            catch (Exception ex)
            {
                // TODO: 記錄例外，例如 ILogger 或 Log 檔
                Console.WriteLine($"新增貼文失敗: {ex.Message}");
                return null;
            }
        }

        // ================= 編輯貼文 =================
        public async Task<Post?> UpdatePostAsync(Post post)
        {
            try
            {
                //設定時間
                post.UpdatedAt = DateTime.UtcNow;

                //更新DB
                _context.Posts.Update(post);

                //儲存到資料庫
                await _context.SaveChangesAsync();
                return post;
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"編輯貼文失敗: {ex.Message}");
                return null;
            }

        }        

        // ================= 軟刪除貼文 =================
        public async Task<bool> DeletePostAsync(Post post)
        {
            try
            {
                //設定時間
                post.UpdatedAt = DateTime.UtcNow;
                post.DeletedAt = DateTime.UtcNow;

                //設定貼文狀態
                post.Status = "Deleted";

                _context.Posts.Update(post);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"刪除貼文失敗: {ex.Message}");
                return false;
            }
        }
    }
}
