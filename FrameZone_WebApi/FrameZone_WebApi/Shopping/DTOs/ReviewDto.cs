using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace FrameZone_WebApi.Shopping.DTOs
{
    /// <summary>
    /// 共用的評價數據傳輸物件 (支援購物訂單與工作室預約)
    /// </summary>
    public class ReviewDto
    {
        public int ReviewId { get; set; }
        public string ReviewerName { get; set; }
        public string RevieweeName { get; set; }
        public string ReviewerAvatar { get; set; }
        public byte Rating { get; set; }
        public string Content { get; set; }
        public string Reply { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
        
        // 擴充屬性：標識來源 (e.g., "Product", "Booking")
        public string ReviewType { get; set; }
        
        // 擴充屬性：關聯的目標名稱 (e.g., 商品名稱 或 工作室名稱)
        public string TargetName { get; set; }
        public string TargetImageUrl { get; set; }
    }

    /// <summary>
    /// 評分概要資訊
    /// </summary>
    public class RatingSummaryDto
    {
        public float AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public List<RatingDistributionDto> Distribution { get; set; } = new List<RatingDistributionDto>();
    }

    public class RatingDistributionDto
    {
        public int Star { get; set; }
        public int Count { get; set; }
        public float Percentage { get; set; }
    }

    /// <summary>
    /// 建立評價的傳輸物件
    /// </summary>
    public class CreateReviewDto
    {
        public long OrderId { get; set; }
        public long ProductId { get; set; }
        public byte Rating { get; set; }
        public string Content { get; set; }
        public List<IFormFile> Images { get; set; }
    }
}
