using FrameZone_WebApi.PhotographerBooking.DTOs;
using FrameZone_WebApi.Models;

namespace FrameZone_WebApi.PhotographerBooking.Services
{
    public interface IPhotographerService
    {
        Task<List<PhotographerDto>> GetAllPhotographersAsync();
        Task<PhotographerDto> GetPhotographerByIdAsync(int id);
        Task<List<PhotographerDto>> SearchPhotographersAsync(PhotographerSearchDto searchDto);
        Task<List<AvailableSlotDto>> GetPhotographerAvailableSlotsAsync(int photographerId, DateTime start, DateTime end);
    }
}
