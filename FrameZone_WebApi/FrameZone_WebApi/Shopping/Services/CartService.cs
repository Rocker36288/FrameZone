using FrameZone_WebApi.Models;
using FrameZone_WebApi.Shopping.DTOs;
using FrameZone_WebApi.Shopping.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrameZone_WebApi.Shopping.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepo;
        private readonly string _baseUrl = "https://localhost:7213";

        public CartService(ICartRepository cartRepo)
        {
            _cartRepo = cartRepo;
        }

        public async Task<List<CartResponseDto>> GetUserCartAsync(long userId)
        {
            var cartItems = await _cartRepo.GetUserCartAsync(userId);
            return cartItems.Select(c => 
            {
                var productImg = c.Specification.Product.ProductImages?.FirstOrDefault(i => i.IsMainImage)?.ImageUrl ?? 
                               c.Specification.Product.ProductImages?.FirstOrDefault()?.ImageUrl;
                var sellerAvatar = c.Specification.Product.User?.UserProfile?.Avatar;

                return new CartResponseDto
                {
                    ShoppingCartId = c.ShoppingCartId,
                    SpecificationId = c.SpecificationId,
                    Quantity = c.Quantity,
                    ProductId = c.Specification.ProductId,
                    ProductName = c.Specification.Product.ProductName,
                    ProductImage = !string.IsNullOrEmpty(productImg) ? _baseUrl + productImg : "",
                    Price = c.Specification.Price,
                    SpecificationName = string.Join(" / ", c.Specification.SpecOptionMappings.Select(m => m.PropertyDetails.OptionValue)),
                    StockQuantity = c.Specification.StockQuantity,
                    SellerId = c.Specification.Product.UserId,
                    SellerName = c.Specification.Product.User?.UserProfile?.DisplayName ?? 
                                 c.Specification.Product.User?.Account ?? "官方賣場",
                    SellerAvatar = !string.IsNullOrEmpty(sellerAvatar) ? _baseUrl + sellerAvatar : null
                };
            }).ToList();
        }

        public async Task<bool> AddToCartAsync(long userId, CartDto cartDto)
        {
            var existing = await _cartRepo.GetCartItemAsync(userId, cartDto.SpecificationId);
            if (existing != null)
            {
                existing.Quantity += cartDto.Quantity;
                await _cartRepo.UpdateCartItemAsync(existing);
            }
            else
            {
                var newItem = new ShoppingCart
                {
                    UserId = userId,
                    SpecificationId = cartDto.SpecificationId,
                    Quantity = cartDto.Quantity
                };
                await _cartRepo.AddToCartAsync(newItem);
            }
            return true;
        }

        public async Task<bool> UpdateCartItemAsync(long userId, CartDto cartDto)
        {
            var existing = await _cartRepo.GetCartItemAsync(userId, cartDto.SpecificationId);
            if (existing != null)
            {
                existing.Quantity = cartDto.Quantity;
                await _cartRepo.UpdateCartItemAsync(existing);
                return true;
            }
            return false;
        }

        public async Task<bool> RemoveFromCartAsync(long userId, int specificationId)
        {
            await _cartRepo.RemoveFromCartAsync(userId, specificationId);
            return true;
        }

        public async Task<bool> ClearCartAsync(long userId)
        {
            await _cartRepo.ClearUserCartAsync(userId);
            return true;
        }

        public async Task<bool> SyncCartAsync(long userId, List<CartDto> cartItems)
        {
            // 基礎實作：先清空再加入
            // 進階實作：比對現有項目進行更新
            await _cartRepo.ClearUserCartAsync(userId);
            foreach (var item in cartItems)
            {
                await _cartRepo.AddToCartAsync(new ShoppingCart
                {
                    UserId = userId,
                    SpecificationId = item.SpecificationId,
                    Quantity = item.Quantity
                });
            }
            return true;
        }
    }
}
