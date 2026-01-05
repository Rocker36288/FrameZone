using FrameZone_WebApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FrameZone_WebApi.Shopping.Repositories
{
    public interface IFavoriteRepository
    {
        Task<List<Favorite>> GetUserFavoritesAsync(long userId);
        Task ToggleFavoriteAsync(long userId, long productId);
    }
}
