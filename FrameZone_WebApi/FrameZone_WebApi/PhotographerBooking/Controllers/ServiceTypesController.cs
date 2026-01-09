using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FrameZone_WebApi.Models;
using FrameZone_WebApi.PhotographerBooking.DTOs;

namespace FrameZone_WebApi.PhotographerBooking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceTypesController : ControllerBase
    {
        private readonly AAContext _context;
        private readonly ILogger<ServiceTypesController> _logger;

        public ServiceTypesController(AAContext context, ILogger<ServiceTypesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<ServiceTypeDto>>> GetServiceTypes()
        {
            try
            {
                var serviceTypes = await _context.ServiceTypes
                    .Where(st => st.IsActive)
                    .OrderBy(st => st.DisplayOrder)
                    .Select(st => new ServiceTypeDto
                    {
                        ServiceTypeId = st.ServiceTypeId,
                        ServiceName = st.ServiceName,
                        IconUrl = st.IconUrl
                    })
                    .ToListAsync();

                return Ok(serviceTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service types");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
