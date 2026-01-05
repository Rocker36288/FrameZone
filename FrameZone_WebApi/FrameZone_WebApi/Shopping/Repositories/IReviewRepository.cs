using FrameZone_WebApi.Models;
using System.Collections.Generic;

namespace FrameZone_WebApi.Shopping.Repositories
{
    public interface IReviewRepository
    {
        // 取得商品評分統計
        (float average, int count) GetProductRatingInfo(long productId);
        
        // 取得賣家評分統計
        (float average, int count) GetSellerRatingInfo(long userId);
        
        // 取得商品評價列表
        List<Review> GetProductReviews(long productId, int take = 5);
        
        // 取得賣家收到的所有評價 (分頁)
        List<Review> GetSellerReviews(long userId, int take = 20, int skip = 0);
        
        // 取得使用者發出的評價
        List<Review> GetUserSentReviews(long userId, int take = 20, int skip = 0);
    }
}
