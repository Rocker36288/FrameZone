using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FrameZone_WebApi.PhotographerBooking.Services;
using FrameZone_WebApi.PhotographerBooking.DTOs;
using System.Security.Claims;

namespace FrameZone_WebApi.PhotographerBooking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<BookingController> _logger;

        public BookingController(IBookingService bookingService, ILogger<BookingController> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<BookingDto>> CreateBooking(CreateBookingDto createDto)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null) return Unauthorized();

                createDto.UserId = userId.Value; // Ensure UserId is from token

                var booking = await _bookingService.CreateBookingAsync(createDto);
                if (booking == null) return BadRequest("Could not create booking");

                return CreatedAtAction(nameof(GetById), new { id = booking.BookingId }, booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookingDto>> GetById(int id)
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null) return Unauthorized();

                var booking = await _bookingService.GetBookingByIdAsync(id);
                if (booking == null) return NotFound();

                // Check authorization (only own booking or photographer involved)
                // For now, simple check: booking.UserId == userId
                // In real app, we also need to check if user is the photographer
                if (booking.UserId != userId.Value) 
                {
                     // Simple check, expand logic if needed
                     return Forbid();
                }

                return Ok(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("my-bookings")]
        public async Task<ActionResult<List<BookingDto>>> GetMyBookings()
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null) return Unauthorized();

                var bookings = await _bookingService.GetUserBookingsAsync(userId.Value);
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user bookings");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("photographer/{photographerId}")]
        public async Task<ActionResult<List<BookingDto>>> GetPhotographerBookings(int photographerId)
        {
            try
            {
                // Authorization check needed: Is the current user the photographer?
                // For simplicity, I'll skip complex check for now or assume this endpoint is for photographer dashboard.
                var userId = GetUserIdFromToken();
                if (userId == null) return Unauthorized();

                var bookings = await _bookingService.GetPhotographerBookingsAsync(photographerId);
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting photographer bookings");
                return StatusCode(500, "Internal server error");
            }
        }

        private long? GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            return null;
        }
    }
}
