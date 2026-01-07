using FrameZone_WebApi.Models;

namespace FrameZone_WebApi.PhotographerBooking.Repositories
{
    public interface IPhotographerRepository
    {
        Task<List<Photographer>> GetAllPhotographersAsync();
        Task<Photographer> GetPhotographerByIdAsync(int id);
        Task<List<Photographer>> SearchPhotographersAsync(string keyword, string location, string studioType);
        Task<bool> CreatePhotographerAsync(Photographer photographer);
        Task<bool> UpdatePhotographerAsync(Photographer photographer);
        Task<bool> DeletePhotographerAsync(int id);
    }
}
//負責攝影師資料的 CRUD 與搜尋。