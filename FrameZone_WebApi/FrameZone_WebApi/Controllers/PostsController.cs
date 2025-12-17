using FrameZone_WebApi.DTOs;
using FrameZone_WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace FrameZone_WebApi.Controllers
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
        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] PostDto dto)
        {
            if (!ModelState.IsValid)
            { 
                return BadRequest(ModelState);
            }

            long userId = 1;

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
        [HttpPut("{postId}")]
        public async Task<IActionResult> EditPost(int postId, [FromBody] PostDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var post = await _postService.EditPostAsync(postId, dto);

            if (post == null)
                return NotFound(new { message = "貼文不存在" });

            return Ok(post);
        }

        // DELETE: api/posts/1
        [HttpDelete("{postId}")]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var success = await _postService.DeletePostAsync(postId);

            if (!success)
            { 
                return NotFound(new { message = "貼文不存在或已刪除" });            
            }

            return NoContent();
        }
    }
}

