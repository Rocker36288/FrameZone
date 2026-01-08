using FrameZone_WebApi.Models;

namespace FrameZone_WebApi.PhotographerBooking.Repositories
{
    public interface IAvailableSlotRepository
    {
        Task<List<AvailableSlot>> GetAvailableSlotsAsync(int photographerId, DateTime start, DateTime end);
        Task<bool> AddAvailableSlotAsync(AvailableSlot slot);
        Task<bool> DeleteAvailableSlotAsync(int slotId);
        Task<AvailableSlot> GetSlotByIdAsync(int slotId);
        Task<bool> IsSlotAvailableAsync(int slotId);
    }
}
//管理攝影師的可預約時段。