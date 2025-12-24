using FrameZone_WebApi.DTOs;
using FrameZone_WebApi.Helpers;
using FrameZone_WebApi.Models;
using FrameZone_WebApi.Repositories;

namespace FrameZone_WebApi.Services
{
    /// <summary>
    /// 背景地理編碼服務
    /// 處理耗時的 GPS 反向地理編碼操作
    /// </summary>
    public class BackgroundGeocodingService : IBackgroundGeocodingService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BackgroundGeocodingService> _logger;

        public BackgroundGeocodingService(
            IServiceProvider serviceProvider,
            ILogger<BackgroundGeocodingService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// 背景處理照片的地理編碼
        /// </summary>
        public async Task ProcessGeocodingAsync(
            long photoId,
            long userId,
            decimal latitude,
            decimal longitude)
        {
            try
            {
                _logger.LogInformation(
                    "🌍 開始背景地理編碼，PhotoId: {PhotoId}, GPS: ({Latitude}, {Longitude})",
                    photoId, latitude, longitude);

                // 建立獨立的 Scope，避免 DbContext 生命週期問題
                using var scope = _serviceProvider.CreateScope();

                var geocodingService = scope.ServiceProvider.GetRequiredService<IGeocodingService>();
                var photoRepository = scope.ServiceProvider.GetRequiredService<IPhotoRepository>();
                var tagCategorizationService = scope.ServiceProvider.GetRequiredService<ITagCategorizationService>();

                // 1️⃣ 反向地理編碼
                var geocodingResult = await geocodingService.ReverseGeocodeAsync(
                    latitude,
                    longitude,
                    "zh-TW"
                );

                if (!geocodingResult.Success || geocodingResult.AddressInfo == null)
                {
                    _logger.LogWarning(
                        "⚠️ 背景地理編碼失敗，PhotoId: {PhotoId}, 錯誤: {Error}",
                        photoId, geocodingResult.ErrorMessage);
                    return;
                }

                var addressInfo = geocodingResult.AddressInfo;

                _logger.LogInformation(
                    "✅ 地理編碼成功，PhotoId: {PhotoId}, 地點: {Country} - {City} - {District}",
                    photoId, addressInfo.Country, addressInfo.City, addressInfo.District);

                // 2️⃣ 寫入 PhotoLocation
                var location = new PhotoLocation
                {
                    PhotoId = photoId,
                    SourceId = PhotoConstants.SOURCE_ID_GEOCODING,
                    Latitude = latitude,
                    Longitude = longitude,
                    Country = addressInfo.Country,
                    City = addressInfo.City,
                    District = addressInfo.District,
                    PlaceName = addressInfo.PlaceName,
                    Address = addressInfo.Address,
                    SetBy = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await photoRepository.AddPhotoLocationAsync(location);

                _logger.LogInformation("✅ PhotoLocation 寫入成功，PhotoId: {PhotoId}", photoId);

                // 3️⃣ 生成地點標籤
                var locationTags = new List<string>();

                if (!string.IsNullOrWhiteSpace(addressInfo.Country))
                    locationTags.Add(addressInfo.Country.Trim());

                if (!string.IsNullOrWhiteSpace(addressInfo.City))
                    locationTags.Add(addressInfo.City.Trim());

                if (!string.IsNullOrWhiteSpace(addressInfo.District))
                    locationTags.Add(addressInfo.District.Trim());

                if (!string.IsNullOrWhiteSpace(addressInfo.PlaceName))
                    locationTags.Add(addressInfo.PlaceName.Trim());

                // 4️⃣ 建立並關聯標籤
                if (locationTags.Any())
                {
                    var tagIds = new List<int>();

                    foreach (var tagName in locationTags)
                    {
                        // 判斷標籤應屬於哪個分類
                        int categoryId = await tagCategorizationService.DetermineCategoryIdAsync(
                            tagName,
                            PhotoConstants.TAG_TYPE_SYSTEM
                        );

                        // 取得或建立標籤
                        var tag = await photoRepository.GetOrCreateTagAsync(
                            tagName: tagName,
                            tagType: PhotoConstants.TAG_TYPE_SYSTEM,
                            categoryId: categoryId,
                            parentTagId: null,
                            userId: userId
                        );

                        tagIds.Add(tag.TagId);
                    }

                    // 批次新增標籤關聯
                    await photoRepository.AddPhotoTagsBatchAsync(
                        photoId,
                        tagIds,
                        PhotoConstants.SOURCE_ID_GEOCODING,
                        confidence: null
                    );

                    _logger.LogInformation(
                        "✅ 地點標籤生成完成，PhotoId: {PhotoId}, 數量: {Count}",
                        photoId, tagIds.Count);
                }

                _logger.LogInformation("🎉 背景地理編碼完成，PhotoId: {PhotoId}", photoId);
            }
            catch (Exception ex)
            {
                // 背景任務失敗不應該影響主流程
                _logger.LogError(ex,
                    "❌ 背景地理編碼失敗，PhotoId: {PhotoId}, GPS: ({Latitude}, {Longitude})",
                    photoId, latitude, longitude);
            }
        }
    }
}