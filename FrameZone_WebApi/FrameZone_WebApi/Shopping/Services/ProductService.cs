using FrameZone_WebApi.Shopping.DTOs;
using FrameZone_WebApi.Shopping.Repositories;
using SixLabors.ImageSharp;
using System.Collections.Concurrent;

namespace FrameZone_WebApi.Shopping.Services
{
    public class ProductService
    {
        private readonly ProductRepository _repo;
        private readonly IReviewService _reviewService;

        // 記憶體快取：紀錄商品瀏覽次數 (ProductId -> Count)
        private static readonly ConcurrentDictionary<long, int> _viewCounts = new ConcurrentDictionary<long, int>();


        public ProductService(ProductRepository repo, IReviewService reviewService)
        {
            _repo = repo;
            _reviewService = reviewService;
        }

        public List<ProductListDto> GetAvailableProducts(long? userId = null)
        {
            var products = _repo.GetAllProducts();
            var favoriteIds = userId.HasValue ? _repo.GetFavoriteProductIds(userId.Value) : new List<long>();
            return MapToDtoList(products, favoriteIds);
        }

        // 取得商品詳情
        public ProductDetailDto GetProductDetail(long productId, long? userId = null)
        {
            var product = _repo.GetProductById(productId);
            
            if (product == null)
            {
                return null;
            }

            // 增加瀏覽次數
            _viewCounts.AddOrUpdate(productId, 1, (key, oldValue) => oldValue + 1);

            string baseUrl = "https://localhost:7213";
            
            //收藏
            bool isFavorite = false;
            if (userId.HasValue)
            {
                var favoriteIds = _repo.GetFavoriteProductIds(userId.Value);
                isFavorite = favoriteIds.Contains(productId);
            }

            return new ProductDetailDto
            {
                ProductId = product.ProductId,
                UserId = product.UserId,
                ProductName = product.ProductName,
                Description = product.Description,
                CategoryId = product.ProductSellerCategoryMappins.FirstOrDefault()?.SellerCategoryId ?? 0,
                CategoryName = product.ProductSellerCategoryMappins.FirstOrDefault()?.SellerCategory?.CategoryName ?? "未分類",
                Status = product.Status,
                AuditStatus = product.AuditStatus,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                IsFavorite = isFavorite,

                AverageRating = _reviewService.GetProductRatingSummary(product.ProductId).AverageRating,
                ReviewCount = _reviewService.GetProductRatingSummary(product.ProductId).ReviewCount,
                Reviews = _reviewService.GetProductReviews(product.ProductId),

                // 圖片清單
                Images = product.ProductImages
                    .OrderBy(img => img.DisplayOrder)
                    .Select(img => new ProductImageDto
                    {
                        ProductImageId = img.ProductImageId,
                        ImageUrl = baseUrl + img.ImageUrl,
                        IsMainImage = img.IsMainImage,
                        DisplayOrder = img.DisplayOrder
                    }).ToList(),

                // 規格清單
                Specifications = product.ProductSpecifications
                    .Select(spec => new ProductSpecificationDto
                    {
                        SpecificationId = spec.SpecificationId,
                        //SpecName = spec.SpecName,
                        Price = spec.Price,
                        StockQuantity = spec.StockQuantity,
                        //Sku = spec.Sku
                    }).ToList(),

                // 賣家資訊
                Seller = new SellerDto
                {
                    UserId = product.User.UserId,
                    // 從 UserProfile 取得顯示名稱，若 Profile 為空則改用 User.Account
                    DisplayName = product.User.UserProfile?.DisplayName ?? product.User.Account,

                    // 從 UserProfile 取得頭像
                    Avatar = (product.User.UserProfile != null && !string.IsNullOrEmpty(product.User.UserProfile.Avatar))
                        ? $"{baseUrl}{product.User.UserProfile.Avatar}"
                        : null,
                    
                    Rating = _reviewService.GetSellerRatingSummary(product.UserId).AverageRating,
                    ReviewCount = _reviewService.GetSellerRatingSummary(product.UserId).ReviewCount
                }
            };
        }

        // 取得類似商品
        public List<ProductListDto> GetSimilarProducts(long productId, long? userId = null)
        {
            var currentProduct = _repo.GetProductById(productId);
            // 取得第一個分類 ID
            var categoryId = currentProduct?.ProductSellerCategoryMappins.FirstOrDefault()?.SellerCategoryId;

            if (currentProduct == null || categoryId == null)
            {
                return new List<ProductListDto>();
            }

            // 1. 提取關鍵字
            string keyword = ExtractKeyword(currentProduct.ProductName);

            // 2. 傳入關鍵字進行查詢
            var products = _repo.GetSimilarProducts(categoryId.Value, productId, keyword);
            var favoriteIds = userId.HasValue ? _repo.GetFavoriteProductIds(userId.Value) : new List<long>();
            return MapToDtoList(products, favoriteIds);
        }

        // 智慧關鍵字提取方法
        private string ExtractKeyword(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName))
                return null;

            // 1. 移除括號及其內容 (包括 【】, [], ())
            var cleanName = System.Text.RegularExpressions.Regex.Replace(productName, @"\【.*?\】|\[.*?\]|\(.*?\)", " ");

            // 2. 移除多餘空白並分割
            var parts = cleanName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
            {
                // 如果清理後沒東西（例如全都是標籤），則回退使用原始名稱的前兩個字
                return productName.Length >= 2 ? productName.Substring(0, 2) : productName;
            }

            // 3. 取最後一個長度 > 1 的詞彙 (假設是商品種類)
            var keyword = parts.LastOrDefault(p => p.Length > 1);

            // 如果找不到大於1個字的詞，退而求其次取最後一個詞
            keyword = keyword ?? parts.Last();

            // 如果提取出的關鍵字太短且沒意義，也可以考慮 fallback (或是就讓它搜尋)
            return keyword;
        }

        // 取得隨機推薦商品 (猜你喜歡)
        public List<ProductListDto> GetRecommendedProducts(long? userId = null)
        {
            var products = _repo.GetRandomProducts(24);
            var favoriteIds = userId.HasValue ? _repo.GetFavoriteProductIds(userId.Value) : new List<long>();
            return MapToDtoList(products, favoriteIds);
        }


        // 取得熱門商品 (混合策略：瀏覽數 + 隨機)
        public List<ProductListDto> GetPopularProducts(long? userId = null)
        {
            // 1. 從記憶體快取取得點擊數最高的商品 ID
            var popularIds = _viewCounts
                .OrderByDescending(x => x.Value)
                .Take(12)
                .Select(x => x.Key)
                .ToList();

            // 2. 取得這些熱門商品的詳細資料
            var popularProducts = _repo.GetProductsByIds(popularIds);

            // 按照點擊數排序 (因為 Repo 回傳順序可能不保證)
            var sortedPopular = popularProducts
                .OrderByDescending(p => _viewCounts.ContainsKey(p.ProductId) ? _viewCounts[p.ProductId] : 0)
                .ToList();

            // 3. 如果不足 12 件，用隨機商品補齊
            if (sortedPopular.Count < 12)
            {
                int needed = 12 - sortedPopular.Count;
                var randomProducts = _repo.GetRandomProducts(needed + 10); // 多抓一點避免重複

                foreach (var p in randomProducts)
                {
                    if (sortedPopular.Count >= 12) break;
                    
                    // 避免重複加入已在熱門列表的商品
                    if (!sortedPopular.Any(existing => existing.ProductId == p.ProductId))
                    {
                        sortedPopular.Add(p);
                    }
                }
            }

            var favoriteIds = userId.HasValue ? _repo.GetFavoriteProductIds(userId.Value) : new List<long>();
            return MapToDtoList(sortedPopular, favoriteIds);
        }

        // 批量取得商品 (用於近期瀏覽)
        public List<ProductListDto> GetProductsByIds(List<long> ids, long? userId = null)
        {
            var products = _repo.GetProductsByIds(ids);
            
            // 按照傳入 ID 的順序排序 (讓最近瀏覽的排前面)
            var sortedProducts = ids
                .Select(id => products.FirstOrDefault(p => p.ProductId == id))
                .Where(p => p != null)
                .ToList();

            var favoriteIds = userId.HasValue ? _repo.GetFavoriteProductIds(userId.Value) : new List<long>();
            return MapToDtoList(sortedProducts, favoriteIds);
        }

        // 根據 UserId 取得商品列表
        public List<ProductListDto> GetProductsByUserId(long userId, long? observerUserId = null)
        {
            var products = _repo.GetProductsByUserId(userId);
            var favoriteIds = observerUserId.HasValue ? _repo.GetFavoriteProductIds(observerUserId.Value) : new List<long>();
            return MapToDtoList(products, favoriteIds);
        }

        // 取得賣家公開資料
        public SellerProfileDto GetSellerProfile(long userId)
        {
            var storeInfo = _repo.GetStoreInfoByUserId(userId);
            var products = _repo.GetProductsByUserId(userId);
            
            // 如果連基本 User 都找不到，可能需要從 UserRepo 拿，但這裡我們先從產品中拿 User 實體（如果有的話）
            // 或是更直接一點，我們假設 userId 是有效的
            var firstProduct = products.FirstOrDefault();
            var user = firstProduct?.User;

            string baseUrl = "https://localhost:7213";

            return new SellerProfileDto
            {
                UserId = userId,
                DisplayName = user?.UserProfile?.DisplayName ?? "未知賣家",
                StoreName = storeInfo?.StoreName ?? user?.UserProfile?.DisplayName ?? "我的賣場",
                Avatar = (user?.UserProfile != null && !string.IsNullOrEmpty(user.UserProfile.Avatar))
                    ? $"{baseUrl}{user.UserProfile.Avatar}"
                    : null,
                CoverImage = (storeInfo != null && !string.IsNullOrEmpty(storeInfo.StoreImageUrl))
                    ? $"{baseUrl}{storeInfo.StoreImageUrl}"
                    : (user?.UserProfile != null && !string.IsNullOrEmpty(user.UserProfile.CoverImage))
                        ? $"{baseUrl}{user.UserProfile.CoverImage}"
                        : $"{baseUrl}/image/sellshop/sellshop4.png",
                Bio = user?.UserProfile?.Bio ?? "這個賣家很懶，什麼都沒留下。",
                StoreDescription = storeInfo?.StoreDescription ?? user?.UserProfile?.Bio ?? "歡迎來到我的賣場！",
                Location = user?.UserProfile?.Location ?? "台灣",
                ProductCount = products.Count,
                Rating = _reviewService.GetSellerRatingSummary(userId).AverageRating,
                ReviewCount = _reviewService.GetSellerRatingSummary(userId).ReviewCount
            };
        }

        // 取得賣家自定義分類
        public List<SellerCategoryDto> GetSellerCategories(long userId)
        {
            var categories = _repo.GetSellerCategoriesByUserId(userId);
            return categories.Select(c => new SellerCategoryDto
            {
                Id = c.SellerCategoryId,
                Name = c.CategoryName
            }).ToList();
        }

        // 取得賣家所有評價 (帶顯示用的 DTO)
        public List<ReviewDto> GetSellerReviews(long userId)
        {
            return _reviewService.GetSellerReviews(userId);
        }

        // 私有方法：統一 DTO 轉換邏輯
        private List<ProductListDto> MapToDtoList(List<Models.Product> products, List<long> favoriteIds)
        {
             string baseUrl = "https://localhost:7213";
             var result = new List<ProductListDto>();

             foreach (var product in products)
             {
                // 取得主圖片（優先 IsMainImage = true，其次用 DisplayOrder）
                 var mainImage = product.ProductImages
                     .Where(img => img.IsMainImage)
                     .FirstOrDefault()
                     ?? product.ProductImages
                         .OrderBy(img => img.DisplayOrder)
                         .FirstOrDefault();

                // 組合完整圖片 URL
                string fullImageUrl;
                 if (mainImage != null && !string.IsNullOrEmpty(mainImage.ImageUrl))
                 {
                     fullImageUrl = baseUrl + mainImage.ImageUrl;
                 }
                 else
                 {
                     fullImageUrl = baseUrl + "/image/shopping/products/default.jpg";
                 }

                // 取得價格
                 var price = product.ProductSpecifications
                     .OrderBy(spec => spec.SpecificationId)
                     .FirstOrDefault()?.Price ?? 0;

                  result.Add(new ProductListDto
                  {
                      ProductId = product.ProductId,
                      UserId = product.UserId,
                      ProductName = product.ProductName,
                      Description = product.Description,
                      MainImageUrl = fullImageUrl,
                      Price = price,
                      CreatedAt = product.CreatedAt,
                      SellerCategoryIds = product.ProductSellerCategoryMappins.Select(m => m.SellerCategoryId).ToList(),
                      IsFavorite = favoriteIds.Contains(product.ProductId),
                      AverageRating = _reviewService.GetProductRatingSummary(product.ProductId).AverageRating,
                      ReviewCount = _reviewService.GetProductRatingSummary(product.ProductId).ReviewCount,
                      Seller = new SellerDto
                      {
                          UserId = product.User.UserId,
                          DisplayName = product.User.UserProfile?.DisplayName ?? product.User.Account,
                          Avatar = (product.User.UserProfile != null && !string.IsNullOrEmpty(product.User.UserProfile.Avatar))
                                ? $"{baseUrl}{product.User.UserProfile.Avatar}"
                                : null,
                          Rating = _reviewService.GetSellerRatingSummary(product.UserId).AverageRating,
                          ReviewCount = _reviewService.GetSellerRatingSummary(product.UserId).ReviewCount
                      }
                  });
              }
              return result;
         }
    }
}
