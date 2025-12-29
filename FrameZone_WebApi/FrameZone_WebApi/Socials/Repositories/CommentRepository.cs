using FrameZone_WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FrameZone_WebApi.Socials.Repositories
{
    public class CommentRepository
    {
        private readonly AAContext _context;

        public CommentRepository(AAContext context)
        {
            _context = context;
        }
                
        // 根據 PostId 取得該貼文的所有留言
        public async Task<List<Comment>> GetCommentsByPostIdAsync(int postId)
        {
            return await _context.Comments
                .Include(c => c.User) // 關聯使用者資訊
                .ThenInclude(u => u.UserProfile) // 關鍵：跨過 User 抓取 Profile
                .Include(c => c.CommentLikes) // 關聯點讚資訊
                .Where(c => c.CommentTarget.PostId == postId && c.DeletedAt == null)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        // 取得或建立 CommentTargetId
        public async Task<int> GetOrCreateTargetIdByPostIdAsync(int postId)
        {
            // 查找是否已經有對應此 PostId 的 Target
            var target = await _context.CommentTargets
                .FirstOrDefaultAsync(t => t.PostId == postId);

            if (target == null)
            {
                // 如果沒有，則建立一個新的
                target = new CommentTarget
                {
                    PostId = postId,
                    TargetTypeId = 3 // 資料庫中對應「貼文」的正確 ID
                }; 
                _context.CommentTargets.Add(target);
                await _context.SaveChangesAsync();
            }

            return target.CommentTargetId;
        }

        // 新增留言
        public async Task<Comment> CreateAsync(Comment comment)
        {
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // 為了回傳時有使用者資訊，載入 Profile
            await _context.Entry(comment).Reference(c => c.User).LoadAsync();
            if (comment.User != null)
            {
                await _context.Entry(comment.User).Reference(u => u.UserProfile).LoadAsync();
            }

            return comment;
        }
        public async Task<Comment> GetByIdAsync(int commentId)
        {
            return await _context.Comments.FirstOrDefaultAsync(c => c.CommentId == commentId);
        }

        public async Task UpdateAsync(Comment comment)
        {
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();
        }
    }
}
