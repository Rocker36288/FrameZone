using FrameZone_WebApi.Socials.DTOs;
using FrameZone_WebApi.Socials.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FrameZone_WebApi.Socials.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly CommentService _commentService;

        public CommentsController(CommentService commentService)
        {
            _commentService = commentService;
        }

        // GET: api/Comments/post/123
        [HttpGet("post/{postId}")]
        public async Task<ActionResult<List<CommentReadDto>>> GetByPost(int postId)
        {
            var result = await _commentService.GetPostCommentsAsync(postId);
            return Ok(result);
        }

        // POST: api/Comments
        [HttpPost]
        public async Task<ActionResult<CommentReadDto>> Create(CommentCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CommentContent))
            {
                return BadRequest("留言內容不能為空");
            }

            // 這裡建議從 JWT Token 中取得 UserId，避免前端偽造他人 ID 留言
            //var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //if (userIdClaim == null) return Unauthorized();

            //long userId = long.Parse(userIdClaim);
            long userId = 1;

            var result = await _commentService.CreateCommentAsync(userId, dto);

            return Ok(result);
        }

        [HttpDelete("{commentId}")]
        public async Task<IActionResult> Delete(int commentId)
        {
            try
            {
                // 從 Token 取得 UserId
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null) return Unauthorized();
                long userId = long.Parse(userIdClaim);

                var success = await _commentService.DeleteCommentAsync(userId, commentId);

                if (!success)
                    return NotFound("找不到留言或留言已被刪除");

                return Ok(new { message = "留言已成功刪除" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpPut("{commentId}")]
        public async Task<ActionResult<CommentReadDto>> Update(int commentId, CommentUpdateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CommentContent))
                return BadRequest("留言內容不能為空");

            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null) return Unauthorized();
                long userId = long.Parse(userIdClaim);

                var result = await _commentService.UpdateCommentAsync(userId, commentId, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }
    }
}
