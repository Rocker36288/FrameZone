using FrameZone_WebApi.Socials.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FrameZone_WebApi.Socials.Controllers
{
    [ApiController]
    [Route("api/posts")]
    public class PostLikesController : ControllerBase
    {
        private readonly PostLikeService _postLikeService;

        public PostLikesController(PostLikeService postLikeService)
        {
            _postLikeService = postLikeService;
        }

        // GET: api/posts/liked?limit=20
        [Authorize]
        [HttpGet("liked")]
        public async Task<IActionResult> GetLikedPosts([FromQuery] int limit = 20)
        {
            long userId = GetUserId();
            limit = Math.Clamp(limit, 1, 50);
            var posts = await _postLikeService.GetLikedPostsAsync(userId, limit);
            return Ok(posts);
        }

        // POST: api/posts/1/like
        [Authorize]
        [HttpPost("{postId}/like")]
        public async Task<IActionResult> LikePost(int postId)
        {
            try
            {
                long userId = GetUserId();
                var created = await _postLikeService.AddLikeAsync(userId, postId);
                if (!created)
                {
                    return BadRequest(new { message = "已經按讚過" });
                }
                return Ok(new { message = "已按讚" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // DELETE: api/posts/1/like
        [Authorize]
        [HttpDelete("{postId}/like")]
        public async Task<IActionResult> UnlikePost(int postId)
        {
            try
            {
                long userId = GetUserId();
                var removed = await _postLikeService.RemoveLikeAsync(userId, postId);
                if (!removed)
                {
                    return NotFound(new { message = "尚未按讚" });
                }
                return Ok(new { message = "已取消按讚" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        private long GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                throw new UnauthorizedAccessException("尚未登入");

            return long.Parse(userIdClaim);
        }
    }
}
