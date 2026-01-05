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
    }
}
