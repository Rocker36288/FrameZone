using Microsoft.AspNetCore.Mvc;
using FrameZone_WebApi.PhotographerBooking.Services;
using FrameZone_WebApi.PhotographerBooking.DTOs;

namespace FrameZone_WebApi.PhotographerBooking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PhotographerController : ControllerBase
    {
        private readonly IPhotographerService _photographerService;
        private readonly ILogger<PhotographerController> _logger;

        public PhotographerController(IPhotographerService photographerService, ILogger<PhotographerController> logger)
        {
            _photographerService = photographerService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<PhotographerDto>>> GetAll([FromQuery] PhotographerSearchDto searchDto)
        {
            try
            {
                if (!string.IsNullOrEmpty(searchDto.Keyword) || 
                    !string.IsNullOrEmpty(searchDto.Location) || 
                    !string.IsNullOrEmpty(searchDto.StudioType) || 
                    searchDto.StartDate.HasValue || 
                    searchDto.EndDate.HasValue)
                {
                    var results = await _photographerService.SearchPhotographersAsync(searchDto);
                    return Ok(results);
                }

                var photographers = await _photographerService.GetAllPhotographersAsync();
                return Ok(photographers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting photographers");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PhotographerDto>> GetById(int id)
        {
            try
            {
                var photographer = await _photographerService.GetPhotographerByIdAsync(id);
                if (photographer == null) return NotFound();
                return Ok(photographer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting photographer details");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}/slots")]
        public async Task<ActionResult<List<AvailableSlotDto>>> GetSlots(int id, [FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            try
            {
                var slots = await _photographerService.GetPhotographerAvailableSlotsAsync(id, start, end);
                return Ok(slots);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting photographer slots");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}


//提供 API 供前端取得列表與詳情。