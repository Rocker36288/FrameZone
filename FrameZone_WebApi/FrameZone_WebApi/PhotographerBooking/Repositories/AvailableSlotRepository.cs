using FrameZone_WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FrameZone_WebApi.PhotographerBooking.Repositories
{
    public class AvailableSlotRepository : IAvailableSlotRepository
    {
        private readonly AAContext _context;
        private readonly ILogger<AvailableSlotRepository> _logger;

        public AvailableSlotRepository(AAContext context, ILogger<AvailableSlotRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<AvailableSlot>> GetAvailableSlotsAsync(int photographerId, DateTime start, DateTime end)
        {
            return await _context.AvailableSlots
                .Where(s => s.PhotographerId == photographerId && s.StartDateTime >= start && s.EndDateTime <= end)
                .OrderBy(s => s.StartDateTime)
                .ToListAsync();
        }

        public async Task<bool> AddAvailableSlotAsync(AvailableSlot slot)
        {
            try
            {
                _context.AvailableSlots.Add(slot);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding available slot");
                return false;
            }
        }

        public async Task<bool> DeleteAvailableSlotAsync(int slotId)
        {
            try
            {
                var slot = await _context.AvailableSlots.FindAsync(slotId);
                if (slot == null) return false;

                _context.AvailableSlots.Remove(slot);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting available slot");
                return false;
            }
        }

        public async Task<AvailableSlot> GetSlotByIdAsync(int slotId)
        {
            return await _context.AvailableSlots.FindAsync(slotId);
        }

        public async Task<bool> IsSlotAvailableAsync(int slotId)
        {
            // Assuming logic: check if there are any active bookings for this slot
            // However, the Booking model has AvailableSlotId FK.
            // If Booking table has a record with this SlotId and status is not Cancelled, it's not available.
            
            var existingBooking = await _context.Bookings
                .AnyAsync(b => b.AvailableSlotId == slotId && b.BookingStatus != "已取消");
            
            return !existingBooking;
        }
    }
}
