using FrameZone_WebApi.PhotographerBooking.DTOs;
using FrameZone_WebApi.Models;

namespace FrameZone_WebApi.PhotographerBooking.Services
{
    public interface IBookingService
    {
        Task<BookingDto> CreateBookingAsync(CreateBookingDto createDto);
        Task<BookingDto> GetBookingByIdAsync(int id);
        Task<List<BookingDto>> GetUserBookingsAsync(long userId);
        Task<List<BookingDto>> GetPhotographerBookingsAsync(int photographerId);
    }
}
