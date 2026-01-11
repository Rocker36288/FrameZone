using System;
using System.Threading.Tasks;
using FrameZone_WebApi.Models;
using FrameZone_WebApi.Shopping.DTOs;
using FrameZone_WebApi.Shopping.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace FrameZone_WebApi.Shopping.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepo;
        private readonly string _baseUrl = "https://localhost:7213";

        public ReviewService(IReviewRepository reviewRepo)
        {
            _reviewRepo = reviewRepo;
        }

        public RatingSummaryDto GetProductRatingSummary(long productId)
        {
            var (average, count) = _reviewRepo.GetProductRatingInfo(productId);
            // 這裡可以進階實作星星分佈統計
            return new RatingSummaryDto
            {
                AverageRating = average,
                ReviewCount = count
            };
        }

        public RatingSummaryDto GetSellerRatingSummary(long userId)
        {
            var (average, count) = _reviewRepo.GetSellerRatingInfo(userId);
            return new RatingSummaryDto
            {
                AverageRating = average,
                ReviewCount = count
            };
        }

        public List<ReviewDto> GetProductReviews(long productId, int take = 5)
        {
            var reviews = _reviewRepo.GetProductReviews(productId, take);
            return MapToDtoList(reviews);
        }

        public List<ReviewDto> GetSellerReviews(long userId, int take = 20, int skip = 0)
        {
            var reviews = _reviewRepo.GetSellerReviews(userId, take, skip);
            return MapToDtoList(reviews);
        }

        public List<ReviewDto> GetUserSentReviews(long userId, int take = 20, int skip = 0)
        {
            var reviews = _reviewRepo.GetUserSentReviews(userId, take, skip);
            return MapToDtoList(reviews);
        }

        private string MaskAccount(string account)
        {
            if (string.IsNullOrEmpty(account)) return "用戶";
            if (account.Length <= 1) return account;
            if (account.Length == 2) return account[0] + "*";
            // e.g. "myaccount" -> "m*******t"
            return account[0] + new string('*', account.Length - 2) + account[account.Length - 1];
        }

        private List<ReviewDto> MapToDtoList(List<Review> reviews)
        {
            return reviews.Select(r => new ReviewDto
            {
                ReviewId = r.ReviewId,
                ReviewerName = MaskAccount(r.ReviewerUser?.Account),
                RevieweeName = MaskAccount(r.RevieweeUser?.Account),
                ReviewerAvatar = (r.ReviewerUser?.UserProfile != null && !string.IsNullOrEmpty(r.ReviewerUser.UserProfile.Avatar))
                    ? r.ReviewerUser.UserProfile.Avatar
                    : $"https://ui-avatars.com/api/?name={Uri.EscapeDataString((r.ReviewerUser?.UserProfile?.DisplayName ?? r.ReviewerUser?.Account ?? "U").Substring(0, 1).ToUpper())}&background=667eea&color=fff&size=128",
                Rating = r.Rating,
                Content = r.ReviewContent,
                Reply = r.RevieweeReply,
                CreatedAt = r.CreatedAt,
                ImageUrls = r.ReviewPhotos.OrderBy(p => p.DisplayOrder).Select(p => $"{_baseUrl}{p.ImageUrl}").ToList(),
                ReviewType = r.ReviewType,
                TargetName = r.OrderDetails?.Specification?.Product?.ProductName
                             ?? (r.Booking != null ? $"預約評價 (#{r.BookingId})" : "商品評價"),
                TargetImageUrl = (r.OrderDetails?.Specification?.Product?.ProductImages != null && r.OrderDetails.Specification.Product.ProductImages.Any())
                    ? $"{_baseUrl}{(r.OrderDetails.Specification.Product.ProductImages.FirstOrDefault(i => i.IsMainImage) ?? r.OrderDetails.Specification.Product.ProductImages.OrderBy(i => i.DisplayOrder).First()).ImageUrl}"
                    : (r.ReviewType == "Booking" ? $"{_baseUrl}/image/studios/default.jpg" : null)
            }).ToList();
        }


        public async Task CreateReviewsAsync(long userId, List<CreateReviewDto> reviewDtos)
        {
            var reviews = new List<Review>();
            var now = DateTime.UtcNow; // Or local time depending on system config

            foreach (var dto in reviewDtos)
            {
                // 1. 建立 Review Entity
                var review = new Review
                {
                    ReviewerUserId = userId,
                    ReviewType = "Product",
                    Rating = dto.Rating,
                    ReviewContent = dto.Content ?? "",
                    CreatedAt = now,
                    UpdatedAt = now
                };

                // 查詢關聯 ID
                var (odId, sellerId) = _reviewRepo.GetOrderDetailInfo(dto.OrderId, dto.ProductId);
                if (!odId.HasValue)
                {
                    continue; // 找不到訂單明細則跳過，或可選擇報錯
                }

                // 檢查是否已評價過
                if (_reviewRepo.HasUserReviewedOrderDetail(odId.Value))
                {
                    continue; // 已評價過則跳過
                }

                review.OrderDetailsId = odId.Value;
                if (sellerId.HasValue) review.RevieweeUserId = sellerId.Value;

                // 處理圖片
                if (dto.Images != null && dto.Images.Count > 0)
                {
                    foreach (var file in dto.Images)
                    {
                        if (file.Length > 0)
                        {
                            // 儲存檔案
                            var fileName = $"{Guid.NewGuid()}{System.IO.Path.GetExtension(file.FileName)}";
                            var uploadPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "image", "review");

                            if (!System.IO.Directory.Exists(uploadPath))
                            {
                                System.IO.Directory.CreateDirectory(uploadPath);
                            }

                            var filePath = System.IO.Path.Combine(uploadPath, fileName);
                            using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            review.ReviewPhotos.Add(new ReviewPhoto
                            {
                                ImageUrl = $"/image/review/{fileName}",
                                DisplayOrder = 0,
                                CreatedAt = now,
                                UpdatedAt = now
                            });
                        }
                    }
                }

                // 需要將 review 加入集合，但前面提到的 OrderDetailsId 缺失問題
                // 我們可以將「補全資料」的邏輯放在 Repository 的 AddReviewsAsync 中，
                // 或者注入需要的 Repository。比較標準是注入。
                // 但為了不更動太多 Service 建構子，我們使用一個折衷：
                // 假設這個 CreateReviewsAsync 邏輯不完整，還是需要補全
                // 
                // 我們將在 ReviewRepository 實作「智慧新增」，或者
                // 在 Service 裡直接查詢 (但需要 context)。
                // 由於 ReviewRepository 有 Context，我們可以讓 Repository 負責「依據 OrderId/ProductId 找 OrderDetail」。

                // 設定暫時屬性讓 Repository 處理
                // 注意：Review Model 裡的 OrderDetailsId 是 Nullable 嗎？
                // 回頭看 Model: public int? OrderDetailsId { get; set; } -> 是 Nullable

                // 為了讓後端能正確連結，我們將 OrderId/ProductId 暫存於 Review 物件中無法存的欄位嗎？不行。
                // 解決：修改 Repository 的 AddReviewsAsync 接收 CreateReviewDto 嗎？不行，介面是 List<Review>。

                // 最好的方法：查詢。
                // 由於不能在 Service 裡用 _reviewRepo 查 OrderDetails (它沒有這個介面)，
                // 我們應該在 IReviewRepository 加一個輔助方法，或在 Service 注入 IOrderRepository。
                // 鑑於時間與變更最小化，我們假設 DTO 傳入 OrderId 與 ProductId，
                // 並且我們需要將它們傳給層下。
                // 或者，簡單點：我們擴充 Modify ReviewRepository，
                // 讓它有一個 `CreateReviewFromDtoAsync` 方法？

                // 讓我們採用：在 Service 這裡不查，直接存。
                // 但 RevieweeUserId 和 OrderDetailsId 必須有值。
                // 我們可以利用 Review 的 Navigation Property 嗎？
                // review.OrderDetails = ... (但我們只有 ID)

                // 決定：修改 IReviewRepository 增加 `ResolveOrderDetailsAndSeller(long orderId, long productId)`
                // 這會比較乾淨。

                // 暫時：先寫入基本資料，並呼叫 Repository 的 Helper。
                // 由於不能改 Repository too much, let's assume we pass what we have.
                // 實際上，OrderDetail 關聯很重要。

                // 為了避免過度複雜，我們將在 Repository 實作一個特殊的 AddReviewWithLookupAsync
                // 或者我們現在直接修改 IReviewRepository 增加 FindOrderDetailId

                reviews.Add(review);
            }

            // 由於上述查詢困難，這裡先暫停 Service 的修改，
            // 先去 Repository 增加 `GetOrderDetailInfo(orderId, productId)`
            await _reviewRepo.AddReviewsAsync(reviews);
        }

        public Dictionary<long, RatingSummaryDto> GetProductRatingSummaries(IEnumerable<long> productIds)
        {
            var infos = _reviewRepo.GetProductRatingInfos(productIds);
            return infos.ToDictionary(
                x => x.Key,
                x => new RatingSummaryDto { AverageRating = x.Value.average, ReviewCount = x.Value.count }
            );
        }

        public Dictionary<long, RatingSummaryDto> GetSellerRatingSummaries(IEnumerable<long> userIds)
        {
            var infos = _reviewRepo.GetSellerRatingInfos(userIds);
            return infos.ToDictionary(
                x => x.Key,
                x => new RatingSummaryDto { AverageRating = x.Value.average, ReviewCount = x.Value.count }
            );
        }
    }
}

