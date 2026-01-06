using FrameZone_WebApi.Shopping.DTOs;
using FrameZone_WebApi.Shopping.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrameZone_WebApi.Shopping.Services
{
    public class FavoriteService
    {
        private readonly IFavoriteRepository _repository;

        public FavoriteService(IFavoriteRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<FavoriteDto>> GetUserFavoritesAsync(long userId)
        {
            var favorites = await _repository.GetUserFavoritesAsync(userId);
            string baseUrl = "https://localhost:7213";

            return favorites.Select(f => new FavoriteDto
            {
                FavoriteId = f.FavoriteId,
                ProductId = f.ProductId ?? 0,
                Name = f.Product?.ProductName ?? "未知商品",
                // 從規格中取得第一個價格
                Price = f.Product?.ProductSpecifications
                            .OrderBy(spec => spec.SpecificationId)
                            .FirstOrDefault()?.Price ?? 0,
                ImageUrl = f.Product?.ProductImages
                            .OrderBy(img => img.DisplayOrder)
                            .Select(img => baseUrl + img.ImageUrl)
                            .FirstOrDefault() ?? $"{baseUrl}/image/shopping/products/default.jpg",
                Date = FormatDate(f.CreatedAt),
                SellerId = f.Product?.UserId ?? 0,
                SellerName = f.Product?.User?.UserProfile?.DisplayName ?? "賣家"
            }).ToList();
        }

        public async Task ToggleFavoriteAsync(long userId, long productId)
        {
            await _repository.ToggleFavoriteAsync(userId, productId);
        }

        private string FormatDate(DateTime date)
        {
            var span = DateTime.Now - date;
            if (span.TotalDays > 30) return date.ToString("yyyy-MM-dd");
            if (span.TotalDays >= 1) return $"{(int)span.TotalDays} 天前";
            if (span.TotalHours >= 1) return $"{(int)span.TotalHours} 小時前";
            return "剛剛";
        }
    }
}
