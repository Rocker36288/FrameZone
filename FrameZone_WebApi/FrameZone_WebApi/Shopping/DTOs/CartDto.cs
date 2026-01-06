using System;

namespace FrameZone_WebApi.Shopping.DTOs
{
    public class CartDto
    {
        public int SpecificationId { get; set; }
        public int Quantity { get; set; }
    }

    public class CartResponseDto
    {
        public int ShoppingCartId { get; set; }
        public int SpecificationId { get; set; }
        public int Quantity { get; set; }
        
        // 商品資訊 (供前端顯示)
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public decimal Price { get; set; }
        public string SpecificationName { get; set; }
        public int StockQuantity { get; set; }
        
        // 賣家資訊
        public long SellerId { get; set; }
        public string SellerName { get; set; }
        public string SellerAvatar { get; set; }
    }
}
