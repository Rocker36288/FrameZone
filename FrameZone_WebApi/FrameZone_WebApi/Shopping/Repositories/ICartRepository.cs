using FrameZone_WebApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FrameZone_WebApi.Shopping.Repositories
{
    public interface ICartRepository
    {
        Task<List<ShoppingCart>> GetUserCartAsync(long userId);
        Task<ShoppingCart> GetCartItemAsync(long userId, int specificationId);
        Task AddToCartAsync(ShoppingCart cartItem);
        Task UpdateCartItemAsync(ShoppingCart cartItem);
        Task RemoveFromCartAsync(long userId, int specificationId);
        Task ClearUserCartAsync(long userId);
    }
}
