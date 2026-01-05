using FrameZone_WebApi.Socials.DTOs;
using FrameZone_WebApi.Socials.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FrameZone_WebApi.Socials.Controllers
{
    [ApiController]
    [Route("api/posts")]
    public class PostsController : ControllerBase
    {
        private readonly PostService _postService;

        public PostsController(PostService postService)
        {
            _postService = postService;
        }

        // GET: api/posts
        [HttpGet]
        public async Task<IActionResult> GetPosts()
        {
            var post = await _postService.GetPostsAsync();

            if (post == null)
            {
                return NotFound(new { message = "貼文不存在" });
            }
            return Ok(post);
        }

        // GET: api/posts/1
        [HttpGet("{postId}")]
        public async Task<IActionResult> GetPostById(int postId)
        {
            var post = await _postService.GetPostByIdAsync(postId);

            if (post == null)
            {
                return NotFound(new { message = "貼文不存在" });
            }
            return Ok(post);
        }

        // POST: api/posts
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] PostDto dto)
        {
            if (!ModelState.IsValid)
            { 
                return BadRequest(ModelState);
            }

            long userId = GetUserId();

            var post = await _postService.CreatePostAsync(dto, userId);

            if (post == null)
            {
                return BadRequest(new { message = "新增貼文失敗" });
            }

            return CreatedAtAction(
                    nameof(GetPostById),
                    new { postId = post.PostId },
                    post
                    );
        }

        // PUT: api/posts/1
        [Authorize]
        [HttpPut("{postId}")]
        public async Task<IActionResult> EditPost(int postId, [FromBody] PostDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                long userId = GetUserId();
                var post = await _postService.EditPostAsync(userId, postId, dto);
                return Ok(post);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/posts/1
        [Authorize]
        [HttpDelete("{postId}")]
        public async Task<IActionResult> DeletePost(int postId)
        {
            try
            {
                long userId = GetUserId();
                await _postService.DeletePostAsync(userId, postId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
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

