using FrameZone_WebApi.Shopping.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FrameZone_WebApi.Shopping.Services
{
    public interface ICartService
    {
        Task<List<CartResponseDto>> GetUserCartAsync(long userId);
        Task<bool> AddToCartAsync(long userId, CartDto cartDto);
        Task<bool> UpdateCartItemAsync(long userId, CartDto cartDto);
        Task<bool> RemoveFromCartAsync(long userId, int specificationId);
        Task<bool> ClearCartAsync(long userId);
        Task<bool> SyncCartAsync(long userId, List<CartDto> cartItems);
    }
}
