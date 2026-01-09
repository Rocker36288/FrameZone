using FrameZone_WebApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace FrameZone_WebApi.Shopping.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly AAContext _context;

        public ReviewRepository(AAContext context)
        {
            _context = context;
        }

        public (float average, int count) GetProductRatingInfo(long productId)
        {
            var reviews = _context.Reviews
                .Where(r => r.ReviewType == "Product" && r.OrderDetails.Specification.ProductId == productId);

            if (!reviews.Any()) return (0, 0);

            return ((float)reviews.Average(r => r.Rating), reviews.Count());
        }

        public (float average, int count) GetSellerRatingInfo(long userId)
        {
            var reviews = _context.Reviews
                .Where(r => r.RevieweeUserId == userId);

            if (!reviews.Any()) return (0, 0);

            return ((float)reviews.Average(r => r.Rating), reviews.Count());
        }

        public List<Review> GetProductReviews(long productId, int take = 5)
        {
            return _context.Reviews
                .Include(r => r.ReviewerUser).ThenInclude(u => u.UserProfile)
                .Include(r => r.RevieweeUser).ThenInclude(u => u.UserProfile)
                .Include(r => r.ReviewPhotos)
                .Include(r => r.OrderDetails).ThenInclude(od => od.Specification).ThenInclude(s => s.Product).ThenInclude(p => p.ProductImages)
                .Where(r => r.ReviewType == "Product" && r.OrderDetails.Specification.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .Take(take)
                .ToList();
        }

        public List<Review> GetSellerReviews(long userId, int take = 20, int skip = 0)
        {
            return _context.Reviews
                .Include(r => r.ReviewerUser).ThenInclude(u => u.UserProfile)
                .Include(r => r.RevieweeUser).ThenInclude(u => u.UserProfile)
                .Include(r => r.ReviewPhotos)
                .Include(r => r.OrderDetails).ThenInclude(od => od.Specification).ThenInclude(s => s.Product).ThenInclude(p => p.ProductImages)
                .Include(r => r.Booking)
                .Where(r => r.RevieweeUserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToList();
        }

        public List<Review> GetUserSentReviews(long userId, int take = 20, int skip = 0)
        {
            return _context.Reviews
                .Include(r => r.ReviewerUser).ThenInclude(u => u.UserProfile)
                .Include(r => r.RevieweeUser).ThenInclude(u => u.UserProfile)
                .Include(r => r.ReviewPhotos)
                .Include(r => r.OrderDetails).ThenInclude(od => od.Specification).ThenInclude(s => s.Product).ThenInclude(p => p.ProductImages)
                .Include(r => r.Booking)
                .Where(r => r.ReviewerUserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToList();
        }

        public async Task AddReviewsAsync(List<Review> reviews)
        {
            await _context.Reviews.AddRangeAsync(reviews);
            await _context.SaveChangesAsync();
        }

        public (int? orderDetailsId, long? sellerId) GetOrderDetailInfo(long orderId, long productId)
        {
            // 假設 OrderDetails 關聯 Specification，再關聯 Product
            var od = _context.OrderDetails
                .Include(d => d.Specification)
                .ThenInclude(s => s.Product)
                .FirstOrDefault(d => d.OrderId == orderId && d.Specification.ProductId == productId);

            if (od != null)
            {
                // 假設 Product 有 SellerId (或是透過 SellerProductMapping)
                // 這裡假設 Product.SellerId 存在 (根據 UserRequest context 沒有這部分，但通常 SellerId 在 Product 上)
                // 檢查 Product Model? 沒看過 Product model。
                // Correctly identify Seller from Product
                var sellerId = od.Specification.Product.UserId;
                
                return (od.OrderDetailsId, sellerId);
            }
            return (null, null);
        }

        public bool HasUserReviewedOrderDetail(int orderDetailsId)
        {
            return _context.Reviews.Any(r => r.OrderDetailsId == orderDetailsId);
        }
    }
}
