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

        // 新增評價 (批次)
        Task AddReviewsAsync(List<Review> reviews);

        // 輔助查找：根據 OrderId 與 ProductId 找到 OrderDetailsId 與 SellerId
        (int? orderDetailsId, long? sellerId) GetOrderDetailInfo(long orderId, long productId);

        // 批次取得商品評分統計
        Dictionary<long, (float average, int count)> GetProductRatingInfos(IEnumerable<long> productIds);
        
        // 批次取得賣家評分統計
        Dictionary<long, (float average, int count)> GetSellerRatingInfos(IEnumerable<long> userIds);

        // 檢查是否已評價
        bool HasUserReviewedOrderDetail(int orderDetailsId);
    }
}
