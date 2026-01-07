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
                    .Include(p => p.User)
                        .ThenInclude(u => u.UserProfile)
                    .Include(p => p.PostLikes)
                    .Include(p => p.CommentTargets)
                        .ThenInclude(ct => ct.Comments)
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

        // ================= 取得指定使用者貼文 =================
        public async Task<List<Post>> GetPostsByUserIdAsync(long userId)
        {
            try
            {
                return await _context.Posts
                    .Include(p => p.User)
                        .ThenInclude(u => u.UserProfile)
                    .Include(p => p.PostLikes)
                    .Include(p => p.CommentTargets)
                        .ThenInclude(ct => ct.Comments)
                    .Where(p =>
                        p.UserId == userId &&
                        p.Status != "Deleted" &&
                        p.DeletedAt == null)
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
                    .Include(p => p.User)
                        .ThenInclude(u => u.UserProfile)
                    .Include(p => p.PostLikes)
                    .Include(p => p.CommentTargets)
                        .ThenInclude(ct => ct.Comments)
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

        // ================= 取得使用者基本資料 =================
        public async Task<User?> GetUserByIdAsync(long userId)
        {
            try
            {
                return await _context.Users
                    .Include(u => u.UserProfile)
                    .Where(u =>
                        u.UserId == userId &&
                        !u.IsDeleted &&
                        u.DeletedAt == null)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"取得使用者失敗: {ex.Message}");
                return null;
            }
        }

        // ================= 取得粉絲數 =================
        public async Task<int> GetFollowerCountAsync(long userId)
        {
            try
            {
                return await _context.Follows
                    .Where(f => f.FollowingId == userId && f.DeleteAt == null)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"取得粉絲數失敗: {ex.Message}");
                return 0;
            }
        }

        // ================= 取得追蹤中數量 =================
        public async Task<int> GetFollowingCountAsync(long userId)
        {
            try
            {
                return await _context.Follows
                    .Where(f => f.FollowerId == userId && f.DeleteAt == null)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"取得追蹤中數量失敗: {ex.Message}");
                return 0;
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

        public async Task<PostLike?> GetPostLikeAsync(long userId, int postId)
        {
            return await _context.PostLikes
                .FirstOrDefaultAsync(l => l.UserId == userId && l.PostId == postId);
        }

        public async Task<PostLike?> AddPostLikeAsync(PostLike like)
        {
            await _context.PostLikes.AddAsync(like);
            var result = await _context.SaveChangesAsync();
            return result > 0 ? like : null;
        }

        public async Task<bool> RemovePostLikeAsync(PostLike like)
        {
            _context.PostLikes.Remove(like);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<Post>> GetLikedPostsAsync(long userId, int limit)
        {
            var liked = await _context.PostLikes
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.CreatedAt)
                .Take(limit)
                .Select(l => new { l.PostId, l.CreatedAt })
                .ToListAsync();

            if (liked.Count == 0)
            {
                return new List<Post>();
            }

            var postIds = liked.Select(x => x.PostId).ToList();
            var posts = await _context.Posts
                .Include(p => p.User)
                    .ThenInclude(u => u.UserProfile)
                .Include(p => p.PostLikes)
                .Include(p => p.CommentTargets)
                    .ThenInclude(ct => ct.Comments)
                .Where(p =>
                    postIds.Contains(p.PostId) &&
                    p.Status != "Deleted" &&
                    p.DeletedAt == null)
                .ToListAsync();

            var orderMap = liked
                .Select((item, index) => new { item.PostId, index })
                .ToDictionary(x => x.PostId, x => x.index);

            return posts
                .OrderBy(p => orderMap.ContainsKey(p.PostId) ? orderMap[p.PostId] : int.MaxValue)
                .ToList();
        }

        public async Task<PostView?> GetPostViewAsync(long userId, int postId)
        {
            return await _context.PostViews
                .Where(pv => pv.UserId == userId && pv.PostId == postId)
                .OrderByDescending(pv => pv.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<PostView?> AddPostViewAsync(PostView view)
        {
            view.CreatedAt = DateTime.UtcNow;
            await _context.PostViews.AddAsync(view);
            var result = await _context.SaveChangesAsync();
            return result > 0 ? view : null;
        }

        public async Task<PostView?> UpdatePostViewAsync(PostView view)
        {
            view.CreatedAt = DateTime.UtcNow;
            _context.PostViews.Update(view);
            var result = await _context.SaveChangesAsync();
            return result > 0 ? view : null;
        }

        public async Task<List<Post>> GetRecentViewedPostsAsync(long userId, int limit)
        {
            var recent = await _context.PostViews
                .Where(pv => pv.UserId == userId)
                .GroupBy(pv => pv.PostId)
                .Select(g => new { PostId = g.Key, LastViewedAt = g.Max(x => x.CreatedAt) })
                .OrderByDescending(x => x.LastViewedAt)
                .Take(limit)
                .ToListAsync();

            if (recent.Count == 0)
            {
                return new List<Post>();
            }

            var postIds = recent.Select(x => x.PostId).ToList();
            var posts = await _context.Posts
                .Include(p => p.User)
                    .ThenInclude(u => u.UserProfile)
                .Include(p => p.PostLikes)
                .Include(p => p.CommentTargets)
                    .ThenInclude(ct => ct.Comments)
                .Where(p =>
                    postIds.Contains(p.PostId) &&
                    p.Status != "Deleted" &&
                    p.DeletedAt == null)
                .ToListAsync();

            return posts
                .OrderByDescending(p => p.UpdatedAt)
                .ToList();
        }
    }
}
