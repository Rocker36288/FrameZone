using FrameZone_WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FrameZone_WebApi.PhotographerBooking.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AAContext _context;
        private readonly ILogger<BookingRepository> _logger;

        public BookingRepository(AAContext context, ILogger<BookingRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Booking> GetBookingByIdAsync(int id)
        {
            return await _context.Bookings
                .Include(b => b.Photographer)
                .Include(b => b.User)
                    .ThenInclude(u => u.UserProfile)
                .Include(b => b.AvailableSlot)
                .FirstOrDefaultAsync(b => b.BookingId == id);
        }

        public async Task<List<Booking>> GetBookingsByUserIdAsync(long userId)
        {
            return await _context.Bookings
                .Where(b => b.UserId == userId)
                .Include(b => b.Photographer)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Booking>> GetBookingsByPhotographerIdAsync(int photographerId)
        {
            return await _context.Bookings
                .Where(b => b.PhotographerId == photographerId)
                .Include(b => b.User)
                    .ThenInclude(u => u.UserProfile)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> CreateBookingAsync(Booking booking)
        {
            try
            {
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking");
                return false;
            }
        }

        public async Task<bool> UpdateBookingStatusAsync(int bookingId, string status)
        {
            try
            {
                var booking = await _context.Bookings.FindAsync(bookingId);
                if (booking == null) return false;

                booking.BookingStatus = status;
                booking.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking status");
                return false;
            }
        }
    }
}
