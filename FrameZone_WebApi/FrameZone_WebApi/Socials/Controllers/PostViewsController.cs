using FrameZone_WebApi.Socials.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FrameZone_WebApi.Socials.Controllers
{
    [ApiController]
    [Route("api/posts")]
    public class PostViewsController : ControllerBase
    {
        private readonly PostViewService _postViewService;

        public PostViewsController(PostViewService postViewService)
        {
            _postViewService = postViewService;
        }

        // POST: api/posts/1/view
        [Authorize]
        [HttpPost("{postId}/view")]
        public async Task<IActionResult> RecordView(int postId)
        {
            try
            {
                long userId = GetUserId();
                var saved = await _postViewService.RecordPostViewAsync(userId, postId);
                if (!saved)
                {
                    return BadRequest(new { message = "記錄失敗" });
                }
                return Ok(new { message = "已記錄" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GET: api/posts/recent-views?limit=20
        [Authorize]
        [HttpGet("recent-views")]
        public async Task<IActionResult> GetRecentViews([FromQuery] int limit = 20)
        {
            long userId = GetUserId();
            limit = Math.Clamp(limit, 1, 50);
            var posts = await _postViewService.GetRecentViewedPostsAsync(userId, limit);
            return Ok(posts);
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
