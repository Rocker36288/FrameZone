using FrameZone_WebApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrameZone_WebApi.Shopping.Repositories
{
    public class FavoriteRepository : IFavoriteRepository
    {
        private readonly AAContext _context;

        public FavoriteRepository(AAContext context)
        {
            _context = context;
        }

        public async Task<List<Favorite>> GetUserFavoritesAsync(long userId)
        {
            return await _context.Favorites
                .Include(f => f.Product)
                    .ThenInclude(p => p.ProductImages)
                .Include(f => f.Product)
                    .ThenInclude(p => p.ProductSpecifications)
                .Where(f => f.UserId == userId && f.ProductId != null)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }
        public async Task ToggleFavoriteAsync(long userId, long productId)
        {
            var existing = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);

            if (existing != null)
            {
                _context.Favorites.Remove(existing);
            }
            else
            {
                var favorite = new Favorite
                {
                    UserId = userId,
                    ProductId = productId,
                    CreatedAt = DateTime.Now
                };
                _context.Favorites.Add(favorite);
            }

            await _context.SaveChangesAsync();
        }
    }
}
