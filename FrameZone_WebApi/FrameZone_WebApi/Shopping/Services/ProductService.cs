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

        // 根據 UserId 取得商品列表 (優化版：用於個人賣場，不重複查詢賣家資訊)
        public List<ProductListDto> GetProductsByUserIdOptimized(long userId, long? observerUserId = null)
        {
            var products = _repo.GetProductsByUserIdOptimized(userId);
            var favoriteIds = observerUserId.HasValue ? _repo.GetFavoriteProductIds(observerUserId.Value) : new List<long>();
            
            // 獲取賣家基本資訊
            var user = _repo.GetUserWithProfile(userId);
            var storeInfo = _repo.GetStoreInfoByUserId(userId);
            string baseUrl = "https://localhost:7213";

            // 手動將資訊附加到所有商品上
            foreach (var p in products)
            {
                p.User = user;
            }

            return MapToDtoList(products, favoriteIds);
        }

        // 根據 UserId 取得商品列表
        public List<ProductListDto> GetProductsByUserId(long userId, long? observerUserId = null)
        {
            var products = _repo.GetProductsByUserId(userId);
            var favoriteIds = observerUserId.HasValue ? _repo.GetFavoriteProductIds(observerUserId.Value) : new List<long>();
            return MapToDtoList(products, favoriteIds);
        }

        // 取得賣家公開資料 (優化版：避免載入所有商品)
        public SellerProfileDto GetSellerProfile(long userId)
        {
            var storeInfo = _repo.GetStoreInfoByUserId(userId);
            
            // 只查詢商品數量，不載入完整商品資料
            var productCount = _repo.GetProductCountByUserId(userId);
            
            // 直接查詢使用者資訊，不透過商品
            var user = _repo.GetUserWithProfile(userId);

            string baseUrl = "https://localhost:7213";
            
            // 只查一次評分
            var ratingSummary = _reviewService.GetSellerRatingSummary(userId);

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
                ProductCount = productCount,
                Rating = ratingSummary.AverageRating,
                ReviewCount = ratingSummary.ReviewCount
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
            // 如果產品是 null 或者 數量等於 0，就回傳空列表
            if (products == null || products.Count == 0)
            {
                return new List<ProductListDto>();
            }

            // 1. 批次收集 ID 並轉為 HashSet 提升搜尋速度 (O(1) 複雜度)
            var productIds = products.Select(p => p.ProductId).Distinct().ToList();
            var sellerIds = products.Select(p => p.UserId).Distinct().ToList();
            var favoriteSet = new HashSet<long>(favoriteIds);

            // 2. 批次抓取（這部分你原本就做得很好）
            var productRatings = _reviewService.GetProductRatingSummaries(productIds);
            var sellerRatings = _reviewService.GetSellerRatingSummaries(sellerIds);

            // 3. 預處理圖片與價格 (減少迴圈內的運算)
            return products.Select(product =>
            {
                // 取得主圖片邏輯優化
                var mainImage = product.ProductImages.FirstOrDefault(img => img.IsMainImage)
                              ?? product.ProductImages.OrderBy(img => img.DisplayOrder).FirstOrDefault();

                string fullImageUrl = mainImage != null && !string.IsNullOrEmpty(mainImage.ImageUrl)
                    ? baseUrl + mainImage.ImageUrl
                    : baseUrl + "/image/shopping/products/default.jpg";

                // 價格取第一筆
                var price = product.ProductSpecifications.FirstOrDefault()?.Price ?? 0;

                // 取得統計
                productRatings.TryGetValue(product.ProductId, out var pRating);
                sellerRatings.TryGetValue(product.UserId, out var sRating);

                return new ProductListDto
                {
                    ProductId = product.ProductId,
                    UserId = product.UserId,
                    ProductName = product.ProductName,
                    Description = product.Description,
                    MainImageUrl = fullImageUrl,
                    Price = price,
                    CreatedAt = product.CreatedAt,
                    SellerCategoryIds = product.ProductSellerCategoryMappins.Select(m => m.SellerCategoryId).ToList(),
                    IsFavorite = favoriteSet.Contains(product.ProductId), // HashSet 速度極快
                    AverageRating = pRating?.AverageRating ?? 0,
                    ReviewCount = pRating?.ReviewCount ?? 0,
                    Seller = new SellerDto
                    {
                        UserId = product.User.UserId,
                        DisplayName = product.User.UserProfile?.DisplayName ?? product.User.Account,
                        Avatar = !string.IsNullOrEmpty(product.User.UserProfile?.Avatar)
                            ? $"{baseUrl}{product.User.UserProfile.Avatar}"
                            : null,
                        Rating = sRating?.AverageRating ?? 0,
                        ReviewCount = sRating?.ReviewCount ?? 0
                    }
                };
            }).ToList();
        }
    }
}
