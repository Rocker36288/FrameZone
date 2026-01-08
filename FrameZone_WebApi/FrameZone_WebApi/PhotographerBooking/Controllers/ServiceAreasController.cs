using FrameZone_WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FrameZone_WebApi.PhotographerBooking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceAreasController : ControllerBase
    {
        private readonly AAContext _context;
        private readonly ILogger<ServiceAreasController> _logger;

        public ServiceAreasController(AAContext context, ILogger<ServiceAreasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("cities")]
        public async Task<ActionResult<List<string>>> GetAvailableCities()
        {
            try
            {
                // 從所有攝影師的 ServiceAreas 中取得不重複的城市
                // 這樣可以確保只回傳真正有攝影師服務的城市
                var cities = await _context.Photographers
                    .SelectMany(p => p.ServiceAreas)
                    .Select(sa => sa.City)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();

                return Ok(cities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service area cities");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
