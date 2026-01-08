using FrameZone_WebApi.Socials.DTOs;
using FrameZone_WebApi.Socials.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FrameZone_WebApi.Socials.Controllers
{
    [ApiController]
    [Route("api/posts")]
    public class PostSharesController : ControllerBase
    {
        private readonly PostShareService _postShareService;

        public PostSharesController(PostShareService postShareService)
        {
            _postShareService = postShareService;
        }

        // POST: api/posts/1/share
        [Authorize]
        [HttpPost("{postId}/share")]
        public async Task<IActionResult> SharePost(int postId, [FromBody] SharePostDto dto)
        {
            try
            {
                long userId = GetUserId();
                var created = await _postShareService.CreateSharePostAsync(userId, postId, dto?.PostContent);
                if (created == null)
                {
                    return BadRequest(new { message = "已分享過" });
                }
                return Ok(created);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GET: api/posts/shared?limit=20
        [Authorize]
        [HttpGet("shared")]
        public async Task<IActionResult> GetSharedPosts([FromQuery] int limit = 20)
        {
            long userId = GetUserId();
            limit = Math.Clamp(limit, 1, 50);
            var posts = await _postShareService.GetSharedPostsAsync(userId, limit);
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
