namespace FrameZone_WebApi.Shopping.DTOs
{
    public class ProductDetailDto
    {
        public long ProductId { get; set; }
        public long UserId { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Status { get; set; }
        public string AuditStatus { get; set; }
        public List<ProductImageDto> Images { get; set; }
        public List<ProductSpecificationDto> Specifications { get; set; }
        public SellerDto Seller { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsFavorite { get; set; }
        public float AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public List<ReviewDto> Reviews { get; set; } = new List<ReviewDto>();
    }

    public class ProductImageDto
    {
        public int ProductImageId { get; set; }
        public string ImageUrl { get; set; }
        public bool IsMainImage { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class ProductSpecificationDto
    {
        public long SpecificationId { get; set; }
        //public string SpecName { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        //public string Sku { get; set; }
    }

    public class SellerDto
    {
        public long UserId { get; set; }
        public string DisplayName { get; set; }
        public string Avatar { get; set; }
        public float Rating { get; set; }
        public int ReviewCount { get; set; }
    }

    public class SellerProfileDto
    {
        public long UserId { get; set; }
        public string DisplayName { get; set; }
        public string StoreName { get; set; }
        public string Avatar { get; set; }
        public string CoverImage { get; set; }
        public string Bio { get; set; }
        public string StoreDescription { get; set; }
        public string Location { get; set; }
        public int ProductCount { get; set; }
        public float Rating { get; set; }
        public int ReviewCount { get; set; }
    }

    public class SellerCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
