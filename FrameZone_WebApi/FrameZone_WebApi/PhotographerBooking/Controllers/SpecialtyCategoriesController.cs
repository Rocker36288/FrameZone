using FrameZone_WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FrameZone_WebApi.PhotographerBooking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpecialtyCategoriesController : ControllerBase
    {
        private readonly AAContext _context;
        private readonly ILogger<SpecialtyCategoriesController> _logger;

        public SpecialtyCategoriesController(AAContext context, ILogger<SpecialtyCategoriesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<CategoryWithTagsDto>>> GetCategoriesWithTags()
        {
            try
            {
                var categories = await _context.SpecialtyCategories
                    .Where(sc => sc.IsActive)
                    .Include(sc => sc.SpecialtyTags.Where(st => st.IsActive))
                    .OrderBy(sc => sc.DisplayOrder)
                    .Select(sc => new CategoryWithTagsDto
                    {
                        CategoryId = sc.CategoryId,
                        CategoryName = sc.CategoryName,
                        Tags = sc.SpecialtyTags
                            .OrderBy(st => st.DisplayOrder)
                            .Select(st => st.SpecialtyName)
                            .ToList()
                    })
                    .ToListAsync();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting specialty categories with tags");
                return StatusCode(500, "Internal server error");
            }
        }
    }

    public class CategoryWithTagsDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public List<string> Tags { get; set; }
    }
}
