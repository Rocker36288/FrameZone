using FrameZone_WebApi.Models;
using FrameZone_WebApi.Shopping.DTOs;
using FrameZone_WebApi.Shopping.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace FrameZone_WebApi.Shopping.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepo;
        private readonly string _baseUrl = "https://localhost:7213";

        public ReviewService(IReviewRepository reviewRepo)
        {
            _reviewRepo = reviewRepo;
        }

        public RatingSummaryDto GetProductRatingSummary(long productId)
        {
            var (average, count) = _reviewRepo.GetProductRatingInfo(productId);
            // 這裡可以進階實作星星分佈統計
            return new RatingSummaryDto
            {
                AverageRating = average,
                ReviewCount = count
            };
        }

        public RatingSummaryDto GetSellerRatingSummary(long userId)
        {
            var (average, count) = _reviewRepo.GetSellerRatingInfo(userId);
            return new RatingSummaryDto
            {
                AverageRating = average,
                ReviewCount = count
            };
        }

        public List<ReviewDto> GetProductReviews(long productId, int take = 5)
        {
            var reviews = _reviewRepo.GetProductReviews(productId, take);
            return MapToDtoList(reviews);
        }

        public List<ReviewDto> GetSellerReviews(long userId, int take = 20, int skip = 0)
        {
            var reviews = _reviewRepo.GetSellerReviews(userId, take, skip);
            return MapToDtoList(reviews);
        }

        public List<ReviewDto> GetUserSentReviews(long userId, int take = 20, int skip = 0)
        {
            var reviews = _reviewRepo.GetUserSentReviews(userId, take, skip);
            return MapToDtoList(reviews);
        }

        private string MaskAccount(string account)
        {
            if (string.IsNullOrEmpty(account)) return "用戶";
            if (account.Length <= 1) return account;
            if (account.Length == 2) return account[0] + "*";
            // e.g. "myaccount" -> "m*******t"
            return account[0] + new string('*', account.Length - 2) + account[account.Length - 1];
        }

        private List<ReviewDto> MapToDtoList(List<Review> reviews)
        {
            return reviews.Select(r => new ReviewDto
            {
                ReviewId = r.ReviewId,
                ReviewerName = MaskAccount(r.ReviewerUser?.Account),
                RevieweeName = MaskAccount(r.RevieweeUser?.Account),
                ReviewerAvatar = (r.ReviewerUser?.UserProfile != null && !string.IsNullOrEmpty(r.ReviewerUser.UserProfile.Avatar))
                    ? $"{_baseUrl}{r.ReviewerUser.UserProfile.Avatar}"
                    : $"{_baseUrl}/image/users/default-avatar.jpg",
                Rating = r.Rating,
                Content = r.ReviewContent,
                Reply = r.RevieweeReply,
                CreatedAt = r.CreatedAt,
                ImageUrls = r.ReviewPhotos.OrderBy(p => p.DisplayOrder).Select(p => $"{_baseUrl}{p.ImageUrl}").ToList(),
                ReviewType = r.ReviewType,
                TargetName = r.OrderDetails?.Specification?.Product?.ProductName 
                             ?? (r.Booking != null ? $"預約評價 (#{r.BookingId})" : "商品評價"),
                TargetImageUrl = (r.OrderDetails?.Specification?.Product?.ProductImages != null && r.OrderDetails.Specification.Product.ProductImages.Any())
                    ? $"{_baseUrl}{ (r.OrderDetails.Specification.Product.ProductImages.FirstOrDefault(i => i.IsMainImage) ?? r.OrderDetails.Specification.Product.ProductImages.OrderBy(i => i.DisplayOrder).First()).ImageUrl }"
                    : (r.ReviewType == "Booking" ? $"{_baseUrl}/image/studios/default.jpg" : null)
            }).ToList();
        }
    }
}
