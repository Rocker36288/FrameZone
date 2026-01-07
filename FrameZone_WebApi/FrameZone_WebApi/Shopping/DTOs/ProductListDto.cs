namespace FrameZone_WebApi.Shopping.DTOs
{
    public class ProductListDto
    {
        public long ProductId { get; set; }

        public long UserId { get; set; }

        public string ProductName { get; set; }

        public string Description { get; set; }

        // ProductImage
        public string MainImageUrl { get; set; }

        // ProductSpecification
        public decimal Price { get; set; }

        // Seller information
        public SellerDto Seller { get; set; }

        // Created date for display
        public DateTime CreatedAt { get; set; }

        public List<int> SellerCategoryIds { get; set; } = new List<int>();
        
        // 收藏狀態
        public bool IsFavorite { get; set; }
        public float AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }
}
