using FrameZone_WebApi.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrameZone_WebApi.Shopping.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly AAContext _context;

        public CartRepository(AAContext context)
        {
            _context = context;
        }

        public async Task<List<ShoppingCart>> GetUserCartAsync(long userId)
        {
            return await _context.ShoppingCarts
                .Include(c => c.Specification)
                    .ThenInclude(s => s.SpecOptionMappings)
                        .ThenInclude(m => m.PropertyDetails)
                .Include(c => c.Specification)
                    .ThenInclude(s => s.Product)
                        .ThenInclude(p => p.ProductImages)
                .Include(c => c.Specification)
                    .ThenInclude(s => s.Product)
                        .ThenInclude(p => p.User) // 賣家即 Product 的 User
                            .ThenInclude(u => u.UserProfile)
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }

        public async Task<ShoppingCart> GetCartItemAsync(long userId, int specificationId)
        {
            return await _context.ShoppingCarts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.SpecificationId == specificationId);
        }

        public async Task AddToCartAsync(ShoppingCart cartItem)
        {
            cartItem.CreatedAt = DateTime.UtcNow;
            cartItem.UpdatedAt = DateTime.UtcNow;
            _context.ShoppingCarts.Add(cartItem);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCartItemAsync(ShoppingCart cartItem)
        {
            cartItem.UpdatedAt = DateTime.UtcNow;
            _context.ShoppingCarts.Update(cartItem);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveFromCartAsync(long userId, int specificationId)
        {
            var item = await GetCartItemAsync(userId, specificationId);
            if (item != null)
            {
                _context.ShoppingCarts.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ClearUserCartAsync(long userId)
        {
            var items = await _context.ShoppingCarts.Where(c => c.UserId == userId).ToListAsync();
            _context.ShoppingCarts.RemoveRange(items);
            await _context.SaveChangesAsync();
        }
    }
}
