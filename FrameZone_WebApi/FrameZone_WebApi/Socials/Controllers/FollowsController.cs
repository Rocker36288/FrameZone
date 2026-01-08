using FrameZone_WebApi.Socials.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FrameZone_WebApi.Socials.Controllers
{
    [ApiController]
    [Route("api/follows")]
    public class FollowsController : ControllerBase
    {
        private readonly FollowService _followService;

        public FollowsController(FollowService followService)
        {
            _followService = followService;
        }

        // POST: api/follows/1
        [Authorize]
        [HttpPost("{userId}")]
        public async Task<IActionResult> AddFollow(long userId)
        {
            try
            {
                var followerId = GetUserId();
                var result = await _followService.AddFollowAsync(followerId, userId);
                if (result == null)
                {
                    return BadRequest(new { message = "新增追蹤失敗" });
                }

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/follows/1
        [Authorize]
        [HttpDelete("{userId}")]
        public async Task<IActionResult> RemoveFollow(long userId)
        {
            var followerId = GetUserId();
            var success = await _followService.RemoveFollowAsync(followerId, userId);
            if (!success)
            {
                return NotFound(new { message = "未追蹤此使用者" });
            }

            return Ok(new { message = "已取消追蹤" });
        }

        // GET: api/follows/1/status
        [Authorize]
        [HttpGet("{userId}/status")]
        public async Task<IActionResult> GetFollowStatus(long userId)
        {
            var followerId = GetUserId();
            var isFollowing = await _followService.IsFollowingAsync(followerId, userId);
            return Ok(new { isFollowing });
        }

        // GET: api/follows/1/following
        [HttpGet("{userId}/following")]
        public async Task<IActionResult> GetFollowing(long userId)
        {
            var users = await _followService.GetFollowingAsync(userId);
            return Ok(users);
        }

        // GET: api/follows/1/followers
        [HttpGet("{userId}/followers")]
        public async Task<IActionResult> GetFollowers(long userId)
        {
            var users = await _followService.GetFollowersAsync(userId);
            return Ok(users);
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
