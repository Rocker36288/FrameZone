using System;

namespace FrameZone_WebApi.Shopping.DTOs
{
    public class FavoriteDto
    {
        public int FavoriteId { get; set; }
        public long ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public string Date { get; set; } // 格式化後的日期，例如 "3 天前"
        public long SellerId { get; set; }
        public string SellerName { get; set; }
    }
}
