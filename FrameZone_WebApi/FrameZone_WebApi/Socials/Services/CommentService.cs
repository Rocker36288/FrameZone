using FrameZone_WebApi.Models;
using FrameZone_WebApi.Socials.DTOs;
using FrameZone_WebApi.Socials.Repositories;

namespace FrameZone_WebApi.Socials.Services
{
    /// <summary>
    /// Comment Service
    /// 負責處理留言相關的「商業邏輯」
    /// 
    /// 包含：
    /// - 建立留言
    /// - 取得貼文留言（樹狀結構）
    /// - 更新留言（權限檢查）
    /// - 刪除留言（軟刪除 + 權限檢查）
    /// 
    /// 不直接處理 HTTP / Request / Response
    /// </summary>
    public class CommentService
    {
        private readonly CommentRepository _repository;

        /// <summary>
        /// 透過 DI 注入 CommentRepository
        /// </summary>
        public CommentService(CommentRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// 新增一筆留言
        /// </summary>
        /// <param name="userId">目前登入的使用者 ID</param>
        /// <param name="dto">建立留言所需資料</param>
        /// <returns>建立完成後的留言資料（Read DTO）</returns>
        public async Task<CommentReadDto> CreateAsync(long userId, CommentCreateDto dto)
        {
            // 取得（或建立）該貼文對應的 CommentTargetId
            int targetId = await _repository.GetOrCreateTargetIdAsync(dto.PostId);

            // 建立 Comment 實體
            var comment = new Comment
            {
                UserId = userId,
                CommentTargetId = targetId,
                ParentCommentId = dto.ParentCommentId,
                CommentContent = dto.CommentContent
            };

            // 寫入資料庫
            var created = await _repository.CreateAsync(comment);

            // 轉換成回傳用的 DTO
            return MapToReadDto(created, userId);
        }

        /// <summary>
        /// 取得指定貼文底下的所有留言（樹狀結構）
        /// </summary>
        /// <param name="postId">貼文 ID</param>
        /// <returns>樹狀留言清單</returns>
        public async Task<List<CommentReadDto>> GetByPostIdAsync(int postId, long? currentUserId)
        {
            // 1. 取得資料庫中的所有留言（扁平結構）
            var comments = await _repository.GetByPostIdAsync(postId);

            // 2. 先轉換成扁平的 DTO 清單
            var flatDtos = comments
                .Select(c => MapToReadDto(c, currentUserId))
                .ToList();

            // 3. 建立樹狀結構（只從頂層留言開始）
            return flatDtos
                .Where(c => c.ParentCommentId == null)
                .Select(c => BuildReplyTree(c, flatDtos))
                .OrderByDescending(c => c.CreatedAt)
                .ToList();
        }

        /// <summary>
        /// 更新留言內容
        /// 僅允許留言作者本人操作
        /// </summary>
        public async Task<CommentReadDto> UpdateAsync(
            long userId,
            int commentId,
            CommentUpdateDto dto)
        {
            // 1. 取得留言
            var comment = await _repository.GetByIdAsync(commentId);

            // 2. 基本狀態檢查
            if (comment == null || comment.DeletedAt != null)
                throw new KeyNotFoundException("找不到留言");

            // 3. 權限檢查（只能修改自己的留言）
            if (comment.UserId != userId)
                throw new UnauthorizedAccessException("沒有權限修改此留言");

            // 4. 更新內容
            comment.CommentContent = dto.CommentContent;

            await _repository.UpdateAsync(comment);

            // 5. 回傳更新後結果
            return MapToReadDto(comment, userId);
        }

        /// <summary>
        /// 刪除留言（軟刪除）
        /// 僅允許留言作者本人操作
        /// </summary>
        public async Task DeleteAsync(long userId, int commentId)
        {
            // 1. 取得留言
            var comment = await _repository.GetByIdAsync(commentId);

            // 2. 基本狀態檢查
            if (comment == null || comment.DeletedAt != null)
                throw new KeyNotFoundException("找不到留言");

            // 3. 權限檢查
            if (comment.UserId != userId)
                throw new UnauthorizedAccessException("沒有權限刪除此留言");

            // 4. 執行軟刪除
            await _repository.SoftDeleteAsync(comment);
        }

        // ================= Private Helpers =================

        /// <summary>
        /// 將 Comment Entity 轉換成 CommentReadDto
        /// 若留言已被刪除，會隱藏使用者資訊與內容
        /// 判斷是不是本人，如果是本人則IsOwner=true
        /// </summary>
        private CommentReadDto MapToReadDto(Comment comment, long? currentUserId)
        {
            bool isDeleted = comment.DeletedAt != null;

            return new CommentReadDto
            {
                CommentId = comment.CommentId,
                UserId = comment.UserId,
                DisplayName = isDeleted
                    ? null
                    : comment.User?.UserProfile?.DisplayName ?? "新使用者",
                Avatar = isDeleted
                    ? null
                    : comment.User?.UserProfile?.Avatar,
                CommentTargetId = comment.CommentTargetId,
                ParentCommentId = comment.ParentCommentId,
                CommentContent = isDeleted
                    ? "此留言已刪除"
                    : comment.CommentContent,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,

                // 判斷是不是本人
                IsOwner = currentUserId.HasValue && comment.UserId == currentUserId.Value
            };
        }

        /// <summary>
        /// 遞迴建立留言的回覆樹狀結構
        /// </summary>
        private CommentReadDto BuildReplyTree(
            CommentReadDto parent,
            List<CommentReadDto> all)
        {
            parent.Replies = all
                .Where(c => c.ParentCommentId == parent.CommentId)
                .Select(c => BuildReplyTree(c, all))
                .OrderBy(c => c.CreatedAt)
                .ToList();

            return parent;
        }
    }
}
