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

//封裝業務邏輯，如搜尋過濾、詳情組裝。