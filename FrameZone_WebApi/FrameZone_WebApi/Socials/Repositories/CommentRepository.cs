using FrameZone_WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FrameZone_WebApi.Socials.Repositories
{
    /// <summary>
    /// Comment Repository
    /// 專責處理 Comment 相關的資料存取（Data Access Layer）
    /// </summary>
    public class CommentRepository
    {
        private readonly AAContext _context;

        public CommentRepository(AAContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 依貼文 ID 取得該貼文底下所有「未刪除」的留言
        /// 會一併載入：
        /// - User（留言者）
        /// - UserProfile（顯示名稱、頭像）
        /// - CommentLikes（留言按讚資訊）
        /// </summary>
        public async Task<List<Comment>> GetByPostIdAsync(int postId)
        {
            return await _context.Comments
                // 載入留言者基本資料
                .Include(c => c.User)
                    // 再載入使用者的個人資料（顯示名稱、頭像）
                    .ThenInclude(u => u.UserProfile)
                // 載入留言的按讚資料
                .Include(c => c.CommentLikes)
                // 條件：
                // 1. 留言屬於該貼文
                // 2. 排除已被軟刪除的留言
                .Where(c =>
                    c.CommentTarget.PostId == postId &&
                    c.DeletedAt == null)
                // 最新留言排在最前面
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// 依留言 ID 取得單一留言
        /// 主要用於：
        /// - 更新留言
        /// - 刪除留言
        /// 會一併載入使用者與個人資料，方便後續組 DTO
        /// </summary>
        public async Task<Comment?> GetByIdAsync(int commentId)
        {
            return await _context.Comments
                .Include(c => c.User)
                    .ThenInclude(u => u.UserProfile)
                .FirstOrDefaultAsync(c => c.CommentId == commentId);
        }

        /// <summary>
        /// 依 PostId 取得對應的 CommentTargetId
        /// 若該貼文尚未建立 CommentTarget，則自動建立一筆
        /// 
        /// TargetTypeId = 3 代表「貼文」
        /// </summary>
        public async Task<int> GetOrCreateTargetIdAsync(int postId)
        {
            // 嘗試查詢是否已存在對應該貼文的 CommentTarget
            var target = await _context.CommentTargets
                .FirstOrDefaultAsync(t => t.PostId == postId);

            // 若已存在，直接回傳其 ID
            if (target != null)
                return target.CommentTargetId;

            // 若不存在，建立新的 CommentTarget
            target = new CommentTarget
            {
                PostId = postId,
                TargetTypeId = 3 // 3 = 貼文（對應資料庫定義）
            };

            _context.CommentTargets.Add(target);
            await _context.SaveChangesAsync();

            return target.CommentTargetId;
        }

        /// <summary>
        /// 新增一筆留言
        /// Repository 負責設定建立時間並寫入資料庫
        /// </summary>
        public async Task<Comment> CreateAsync(Comment comment)
        {
            // 統一使用 UTC 時間，避免時區問題
            comment.CreatedAt = DateTime.UtcNow;

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return comment;
        }

        /// <summary>
        /// 更新留言內容
        /// 主要由 Service 層呼叫（已完成權限與狀態檢查）
        /// </summary>
        public async Task UpdateAsync(Comment comment)
        {
            // 設定最後更新時間
            comment.UpdatedAt = DateTime.UtcNow;

            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 軟刪除留言
        /// 不會實際刪除資料，只標記 DeletedAt
        /// </summary>
        public async Task SoftDeleteAsync(Comment comment)
        {
            // 同時更新更新時間與刪除時間
            comment.UpdatedAt = DateTime.UtcNow;
            comment.DeletedAt = DateTime.UtcNow;

            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();
        }
    }
}
