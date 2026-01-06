using FrameZone_WebApi.Shopping.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FrameZone_WebApi.Shopping.Services
{
    public interface IReviewService
    {
        // 取得商品評分概要
        RatingSummaryDto GetProductRatingSummary(long productId);
        
        // 取得賣家評分概要
        RatingSummaryDto GetSellerRatingSummary(long userId);
        
        // 取得商品評價列表
        List<ReviewDto> GetProductReviews(long productId, int take = 5);
        
        // 取得賣家收到的評價列表
        List<ReviewDto> GetSellerReviews(long userId, int take = 20, int skip = 0);
        
        // 取得使用者發出的評價列表
        List<ReviewDto> GetUserSentReviews(long userId, int take = 20, int skip = 0);

        // 建立評價 (批次)
        Task CreateReviewsAsync(long userId, List<CreateReviewDto> reviewDtos);
    }
}
