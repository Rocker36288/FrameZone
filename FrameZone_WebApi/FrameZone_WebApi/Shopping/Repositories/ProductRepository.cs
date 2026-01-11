using FrameZone_WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FrameZone_WebApi.Shopping.Repositories
{
    public class ProductRepository
    {
        private readonly AAContext _context;

        public ProductRepository(AAContext context)
        {
            _context = context;
        }

        // 取得使用者收藏的所有商品 ID
        public List<long> GetFavoriteProductIds(long userId)
        {
            return _context.Favorites
                .Where(f => f.UserId == userId && f.ProductId != null)
                .Select(f => f.ProductId.Value)
                .ToList();
        }

        //所有商品
        public List<Product> GetAllProducts()
        {
            // 篩選已上架且審核狀態包含「通過」二字的商品
            return _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductSpecifications)
                .Include(p => p.ProductSellerCategoryMappins) // 加入分類關聯
                .Include(p => p.User)
                .ThenInclude(u => u.UserProfile)
                .Where(p => p.Status == "上架中" && p.AuditStatus.Contains("通過"))
                .AsNoTracking() // 唯讀查詢優化
                .ToList();
        }

        //單一商品詳情
        public Product GetProductById(long productId)
        {
            return _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductSpecifications)
                .Include(p => p.User)  // 包含賣家資訊
                .ThenInclude(u => u.UserProfile)
                .Include(p => p.ProductSellerCategoryMappins) // 包含賣家分類
                .ThenInclude(m => m.SellerCategory)
                .AsNoTracking() // 詳情也可以用，因為只是顯示
                .FirstOrDefault(p => p.ProductId == productId
                    && p.Status == "上架中"
                    && p.AuditStatus.Contains("通過"));
        }

        // 取得同分類或同關鍵字商品 (類似商品)
        public List<Product> GetSimilarProducts(int categoryId, long excludeId, string keyword = null)
        {
            var query = _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductSpecifications)
                .Include(p => p.ProductSellerCategoryMappins)
                .Include(p => p.User)
                .ThenInclude(u => u.UserProfile)
                .Where(p => p.ProductId != excludeId
                    && p.Status == "上架中"
                    && p.AuditStatus.Contains("通過"));

            if (!string.IsNullOrEmpty(keyword))
            {
                // 如果有關鍵字，優先使用關鍵字搜尋
                query = query.Where(p => p.ProductName.Contains(keyword));
            }
            else
            {
                // 否則使用分類搜尋
                query = query.Where(p => p.ProductSellerCategoryMappins.Any(m => m.SellerCategoryId == categoryId));
            }

            return query
                .AsNoTracking()
                .Take(12)
                .ToList();
        }

        // 取得隨機推薦商品 (猜你喜歡)
        public List<Product> GetRandomProducts(int count)
        {
            // 優化隨機取樣：先取得總數，再隨機跳過
            var total = _context.Products.Count(p => p.Status == "上架中" && p.AuditStatus.Contains("通過"));
            var skip = total > count ? new Random().Next(0, total - count) : 0;

            return _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductSpecifications)
                .Include(p => p.ProductSellerCategoryMappins)
                .Include(p => p.User)
                .ThenInclude(u => u.UserProfile)
                .Where(p => p.Status == "上架中" && p.AuditStatus.Contains("通過"))
                //.OrderBy(p => Guid.NewGuid()) // 隨機排序
                .AsNoTracking()
                .Skip(skip)
                .Take(count)
                .ToList();
        }

        // 批量取得商品 (用於近期瀏覽)
        public List<Product> GetProductsByIds(List<long> productIds)
        {
            if (productIds == null || !productIds.Any())
                return new List<Product>();

            return _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductSpecifications)
                .Include(p => p.ProductSellerCategoryMappins)
                .Include(p => p.User)
                .ThenInclude(u => u.UserProfile)
                .Where(p => productIds.Contains(p.ProductId) 
                         && p.Status == "上架中" 
                         && p.AuditStatus.Contains("通過"))
                .AsNoTracking()
                .ToList();
        }
        // 取得特定使用者的商品 (優化版：不包含 User 關聯，用於單一賣家視圖)
        public List<Product> GetProductsByUserIdOptimized(long userId)
        {
            return _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductSpecifications)
                .Include(p => p.ProductSellerCategoryMappins)
                .Where(p => p.UserId == userId 
                         && p.Status == "上架中" 
                         && p.AuditStatus.Contains("通過"))
                .AsNoTracking()
                .ToList();
        }

        // 取得特定使用者的商品 (預設載入 User 資料，用於通用場景)
        // 取得特定使用者的商品 (進階分頁與投影版)
        public (List<DTOs.ProductListDto> items, int total) GetSellerProductsPagedProjected(long userId, int page, int pageSize, int? categoryId = null, string keyword = null)
        {
            var query = _context.Products.AsQueryable();

            query = query.Where(p => p.UserId == userId 
                         && p.Status == "上架中" 
                         && p.AuditStatus.Contains("通過"));

            if (categoryId.HasValue && categoryId > 0)
            {
                query = query.Where(p => p.ProductSellerCategoryMappins.Any(m => m.SellerCategoryId == categoryId.Value));
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var lowerKeyword = keyword.ToLower();
                query = query.Where(p => p.ProductName.ToLower().Contains(lowerKeyword) || (p.Description != null && p.Description.ToLower().Contains(lowerKeyword)));
            }

            var total = query.Count();

            var items = query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new DTOs.ProductListDto
                {
                    ProductId = p.ProductId,
                    UserId = p.UserId,
                    ProductName = p.ProductName,
                    Description = p.Description,
                    CreatedAt = p.CreatedAt,
                    Price = p.ProductSpecifications.OrderBy(s => s.Price).Select(s => s.Price).FirstOrDefault(),
                    MainImageUrl = p.ProductImages.Where(i => i.IsMainImage).Select(i => i.ImageUrl).FirstOrDefault() 
                                   ?? p.ProductImages.OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).FirstOrDefault(),
                    SellerCategoryIds = p.ProductSellerCategoryMappins.Select(m => m.SellerCategoryId).ToList(),
                    AverageRating = (float)(_context.Reviews.Where(r => r.ReviewType == "Product" && r.OrderDetails.Specification.ProductId == p.ProductId).Average(r => (double?)r.Rating) ?? 0),
                    ReviewCount = _context.Reviews.Count(r => r.ReviewType == "Product" && r.OrderDetails.Specification.ProductId == p.ProductId)
                })
                .ToList();

            // 修正圖片路徑 (補上 BaseUrl)
            string baseUrl = "https://localhost:7213";
            foreach (var item in items)
            {
                if (!string.IsNullOrEmpty(item.MainImageUrl))
                {
                    item.MainImageUrl = baseUrl + item.MainImageUrl;
                }
                else
                {
                    item.MainImageUrl = baseUrl + "/image/shopping/products/default.jpg";
                }
            }

            return (items, total);
        }

        public List<Product> GetProductsByUserId(long userId)
        {
            return _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductSpecifications)
                .Include(p => p.ProductSellerCategoryMappins) // 加入分類關聯
                .Include(p => p.User)
                .ThenInclude(u => u.UserProfile)
                .Where(p => p.UserId == userId 
                         && p.Status == "上架中" 
                         && p.AuditStatus.Contains("通過"))
                .AsNoTracking()
                .ToList();
        }

        // 取得賣場基本資訊
        public StoreBasicInformation GetStoreInfoByUserId(long userId)
        {
            return _context.StoreBasicInformations
                .FirstOrDefault(s => s.UserId == userId);
        }

        // 取得賣家分類
        public List<SellerCategory> GetSellerCategoriesByUserId(long userId)
        {
            return _context.SellerCategories
                .Where(c => c.UserId == userId && c.IsVisible)
                .ToList();
        }

        // 取得特定使用者的商品數量 (輕量查詢，不載入商品資料)
        public int GetProductCountByUserId(long userId)
        {
            return _context.Products
                .Count(p => p.UserId == userId 
                         && p.Status == "上架中" 
                         && p.AuditStatus.Contains("通過"));
        }

        // 取得使用者及其 Profile (輕量查詢)
        public User GetUserWithProfile(long userId)
        {
            return _context.Users
                .Include(u => u.UserProfile)
                .AsNoTracking()
                .FirstOrDefault(u => u.UserId == userId);
        }

        // 取得商品評分資訊 (平均分數與總評價數)
        public (float average, int count) GetProductRatingInfo(long productId)
        {
            var reviews = _context.Reviews
                .Where(r => r.ReviewType == "Product" && r.OrderDetails.Specification.ProductId == productId);

            if (!reviews.Any()) return (0, 0);

            return ((float)reviews.Average(r => r.Rating), reviews.Count());
        }

        // 取得賣家評分資訊
        public (float average, int count) GetSellerRatingInfo(long userId)
        {
            var reviews = _context.Reviews
                .Where(r => r.RevieweeUserId == userId);

            if (!reviews.Any()) return (0, 0);

            return ((float)reviews.Average(r => r.Rating), reviews.Count());
        }

        // 取得商品評價列表
        public List<Review> GetProductReviews(long productId, int take = 5)
        {
            return _context.Reviews
                .Include(r => r.ReviewerUser)
                .ThenInclude(u => u.UserProfile)
                .Include(r => r.ReviewPhotos)
                .Where(r => r.ReviewType == "Product" && r.OrderDetails.Specification.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .Take(take)
                .ToList();
        }

        // 取得賣家所有評價 (包含商品評價與預約評價)
        public List<Review> GetSellerReviews(long userId, int take = 20)
        {
            return _context.Reviews
                .Include(r => r.ReviewerUser)
                .ThenInclude(u => u.UserProfile)
                .Include(r => r.ReviewPhotos)
                .Include(r => r.OrderDetails)
                .ThenInclude(od => od.Specification)
                .ThenInclude(s => s.Product)
                .Where(r => r.RevieweeUserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .Take(take)
                .ToList();
        }
    }
}
