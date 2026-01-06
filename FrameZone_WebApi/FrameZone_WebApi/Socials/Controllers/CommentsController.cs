using FrameZone_WebApi.Socials.DTOs;
using FrameZone_WebApi.Socials.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FrameZone_WebApi.Socials.Controllers
{
    /// <summary>
    /// Comments Controller
    /// 
    /// 負責處理「留言」相關的 HTTP Request / Response
    /// 僅處理：
    /// - 路由
    /// - 參數驗證
    /// - HTTP 狀態碼回傳
    /// 
    /// 所有商業邏輯（權限、刪除規則等）皆交由 Service 層處理
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly CommentService _commentService;

        /// <summary>
        /// 透過 DI 注入 CommentService
        /// </summary>
        public CommentsController(CommentService commentService)
        {
            _commentService = commentService;
        }

        /// <summary>
        /// 取得指定貼文底下的所有留言（樹狀結構）
        /// </summary>
        /// <param name="postId">貼文 ID</param>
        /// <returns>該貼文底下的留言清單</returns>
        /// GET: api/comments/post/{postId}
        [HttpGet("post/{postId:int}")]
        public async Task<ActionResult<List<CommentReadDto>>> GetByPost(int postId)
        {
            // 預設：未登入
            long? currentUserId = User.Identity?.IsAuthenticated == true
    ? long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)
    : null;


            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier); // ⭐ 正確
                if (userIdClaim != null)
                {
                    currentUserId = long.Parse(userIdClaim.Value);
                }
            }
            
            var result = await _commentService.GetByPostIdAsync(postId, currentUserId);
            return Ok(result);
        }

        /// <summary>
        /// 新增一筆留言
        /// 使用者必須登入
        /// </summary>
        /// <param name="dto">建立留言所需資料</param>
        /// <returns>建立完成後的留言資料</returns>
        /// POST: api/comments
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<CommentReadDto>> Create(
            [FromBody] CommentCreateDto dto)
        {
            // 基本參數檢查（避免空留言）
            if (string.IsNullOrWhiteSpace(dto.CommentContent))
                return BadRequest("留言內容不能為空");

            // 從 JWT Token 中取得目前登入使用者 ID
            long userId = GetUserId();

            var result = await _commentService.CreateAsync(userId, dto);
            return Ok(result);
        }

        /// <summary>
        /// 更新指定留言內容
        /// 僅允許留言作者本人操作
        /// </summary>
        /// <param name="commentId">留言 ID</param>
        /// <param name="dto">更新內容</param>
        /// PUT: api/comments/{commentId}
        [Authorize]
        [HttpPut("{commentId:int}")]
        public async Task<ActionResult<CommentReadDto>> Update(
            int commentId,
            [FromBody] CommentUpdateDto dto)
        {
            // 基本參數檢查
            if (string.IsNullOrWhiteSpace(dto.CommentContent))
                return BadRequest("留言內容不能為空");

            try
            {
                long userId = GetUserId();

                var result = await _commentService.UpdateAsync(userId, commentId, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                // 找不到留言或留言已被刪除
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                // 嘗試修改非本人留言
                return Forbid(ex.Message);
            }
        }

        /// <summary>
        /// 刪除指定留言（軟刪除）
        /// 僅允許留言作者本人操作
        /// </summary>
        /// <param name="commentId">留言 ID</param>
        /// DELETE: api/comments/{commentId}
        [Authorize]
        [HttpDelete("{commentId:int}")]
        public async Task<IActionResult> Delete(int commentId)
        {
            try
            {
                long userId = GetUserId();

                await _commentService.DeleteAsync(userId, commentId);
                return Ok(new { message = "留言已成功刪除" });
            }
            catch (KeyNotFoundException ex)
            {
                // 找不到留言或留言已被刪除
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                // 嘗試刪除非本人留言
                return Forbid(ex.Message);
            }
        }

        // ================= Private Helpers =================

        /// <summary>
        /// 從 JWT Token 中取得目前登入使用者的 UserId
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">
        /// 當使用者尚未登入或 Token 無效時拋出
        /// </exception>
        private long GetUserId()
        {
            var userIdClaim = User
                .FindFirst(ClaimTypes.NameIdentifier)
                ?.Value;

            if (userIdClaim == null)
                throw new UnauthorizedAccessException("尚未登入");

            return long.Parse(userIdClaim);
        }
    }
}
