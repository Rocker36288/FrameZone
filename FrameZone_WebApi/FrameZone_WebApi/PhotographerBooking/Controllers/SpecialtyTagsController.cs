using FrameZone_WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FrameZone_WebApi.PhotographerBooking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpecialtyTagsController : ControllerBase
    {
        private readonly AAContext _context;
        private readonly ILogger<SpecialtyTagsController> _logger;

        public SpecialtyTagsController(AAContext context, ILogger<SpecialtyTagsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("popular")]
        public async Task<ActionResult<List<string>>> GetPopularTags([FromQuery] int count = 10)
        {
            try
            {
                // 取得最熱門的標籤(根據使用該標籤的攝影師數量)
                var popularTags = await _context.SpecialtyTags
                    .Where(st => st.IsActive)
                    .Include(st => st.PhotographerSpecialties)
                    .OrderByDescending(st => st.PhotographerSpecialties.Count)
                    .ThenBy(st => st.DisplayOrder)
                    .Take(count)
                    .Select(st => st.SpecialtyName)
                    .ToListAsync();

                return Ok(popularTags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting popular specialty tags");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<string>>> GetAllTags()
        {
            try
            {
                var tags = await _context.SpecialtyTags
                    .Where(st => st.IsActive)
                    .OrderBy(st => st.DisplayOrder)
                    .Select(st => st.SpecialtyName)
                    .ToListAsync();

                return Ok(tags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting specialty tags");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
