using FrameZone_WebApi.Models;

namespace FrameZone_WebApi.PhotographerBooking.Repositories
{
    public interface IBookingRepository
    {
        Task<Booking> GetBookingByIdAsync(int id);
        Task<List<Booking>> GetBookingsByUserIdAsync(long userId);
        Task<List<Booking>> GetBookingsByPhotographerIdAsync(int photographerId);
        Task<bool> CreateBookingAsync(Booking booking);
        Task<bool> UpdateBookingStatusAsync(int bookingId, string status);
    }
}
//處理預約單的建立與查詢