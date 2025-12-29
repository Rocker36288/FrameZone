using FrameZone_WebApi.Models;
using FrameZone_WebApi.Socials.DTOs;
using FrameZone_WebApi.Socials.Repositories;

namespace FrameZone_WebApi.Socials.Services
{
    public class CommentService
    {
        private readonly CommentRepository _repository;

        public CommentService(CommentRepository repository)
        {
            _repository = repository;
        }

        //新增留言
        public async Task<CommentReadDto> CreateCommentAsync(long userId, CommentCreateDto dto)
        {
            // 1. 取得對應的 CommentTargetId
            int targetId = await _repository.GetOrCreateTargetIdByPostIdAsync(dto.PostId);

            // 2. 建立 Comment 實體
            var comment = new Comment
            {
                UserId = userId,
                CommentTargetId = targetId,
                ParentCommentId = dto.ParentCommentId,
                CommentContent = dto.CommentContent,
                CreatedAt = DateTime.Now
            };

            // 3. 儲存
            var result = await _repository.CreateAsync(comment);

            // 4. 回傳扁平化 DTO
            return new CommentReadDto
            {
                CommentId = result.CommentId,
                UserId = result.UserId,
                DisplayName = result.User?.UserProfile?.DisplayName ?? "新使用者",
                Avatar = result.User?.UserProfile?.Avatar,
                CommentTargetId = result.CommentTargetId,
                ParentCommentId = result.ParentCommentId,
                CommentContent = result.CommentContent,
                CreatedAt = result.CreatedAt
            };
        }

        //取得留言
        public async Task<List<CommentReadDto>> GetPostCommentsAsync(int postId)
        {
            // 1. 取得所有留言
            var allComments = await _repository.GetCommentsByPostIdAsync(postId);

            // 2. 轉換成扁平的 DTO 清單
            var allDtos = allComments.Select(c => new CommentReadDto
            {
                CommentId = c.CommentId,
                UserId = c.UserId,
                DisplayName = c.DeletedAt == null ? (c.User?.UserProfile?.DisplayName) : null,
                Avatar = c.DeletedAt == null ? (c.User?.UserProfile?.Avatar) : null,
                ParentCommentId = c.ParentCommentId,
                // 關鍵：如果已刪除，隱藏內容
                CommentContent = c.DeletedAt == null ? c.CommentContent : "此留言已刪除",
                CreatedAt = c.CreatedAt,
            }).ToList();

            // 3. 呼叫遞迴函數，找出所有「頂層留言」(ParentCommentId 為 null)
            var tree = allDtos
                .Where(c => c.ParentCommentId == null)
                .Select(c => MapReplies(c, allDtos)) // 為每個頂層留言找子留言
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            return tree;
        }

        // 遞迴助手函數
        private CommentReadDto MapReplies(CommentReadDto parent, List<CommentReadDto> allDtos)
        {
            // 在所有資料中，找到「父 ID 等於目前 ID」的資料
            parent.Replies = allDtos
                .Where(c => c.ParentCommentId == parent.CommentId)
                .Select(c => MapReplies(c, allDtos)) // 關鍵：繼續往下找子留言的子留言
                .OrderBy(c => c.CreatedAt)
                .ToList();

            return parent;
        }

        public async Task<bool> DeleteCommentAsync(long userId, int commentId)
        {
            var comment = await _repository.GetByIdAsync(commentId);

            if (comment == null || comment.DeletedAt != null)
                return false;

            // 權限檢查：只能刪除自己的留言
            if (comment.UserId != userId)
                throw new UnauthorizedAccessException("你沒有權限刪除此留言");

            // 執行軟刪除
            comment.DeletedAt = DateTime.Now;
            await _repository.UpdateAsync(comment);

            return true;
        }

        public async Task<CommentReadDto> UpdateCommentAsync(long userId, int commentId, CommentUpdateDto dto)
        {
            var comment = await _repository.GetByIdAsync(commentId);

            // 1. 基本檢查
            if (comment == null || comment.DeletedAt != null)
                throw new KeyNotFoundException("找不到留言或留言已被刪除");

            // 2. 權限檢查：只能修改自己的留言
            if (comment.UserId != userId)
                throw new UnauthorizedAccessException("你沒有權限修改此留言");

            // 3. 更新內容與時間
            comment.CommentContent = dto.CommentContent;
            comment.UpdatedAt = DateTime.Now;

            await _repository.UpdateAsync(comment);

            // 4. 回傳更新後的結果（扁平化 DTO）
            // 這裡建議重新讀取 UserProfile 資訊以確保回傳完整
            return new CommentReadDto
            {
                CommentId = comment.CommentId,
                UserId = comment.UserId,
                DisplayName = comment.User?.UserProfile?.DisplayName ?? "新使用者",
                Avatar = comment.User?.UserProfile?.Avatar,
                CommentTargetId = comment.CommentTargetId,
                ParentCommentId = comment.ParentCommentId,
                CommentContent = comment.CommentContent,
                UpdatedAt = comment.UpdatedAt,
            };
        }
    }
}
