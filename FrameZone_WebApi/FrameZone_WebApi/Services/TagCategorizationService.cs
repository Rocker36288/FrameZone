using FrameZone_WebApi.Helpers;
using FrameZone_WebApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace FrameZone_WebApi.Services
{
    /// <summary>
    /// 標籤分類服務
    /// 負責判斷標籤應該歸屬於哪個分類
    /// 將業務邏輯從 Repository 層抽離
    /// </summary>
    public class TagCategorizationService : ITagCategorizationService
    {
        private readonly AAContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<TagCategorizationService> _logger;

        private static readonly ConcurrentDictionary<string, int> _categoryCache = new();
        private static readonly SemaphoreSlim _cacheLock = new(1, 1);

        private static readonly Regex YearRegex = new Regex(
            PhotoConstants.REGEX_YEAR,
            RegexOptions.Compiled);

        public TagCategorizationService(
            AAContext context,
            IMemoryCache cache,
            ILogger<TagCategorizationService> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// 根據標籤名稱判斷應屬於哪個分類
        /// </summary>
        /// <param name="tagName">標籤名稱</param>
        /// <param name="tagType">標籤類型</param>
        /// <returns>分類 ID</returns>
        public async Task<int> DetermineCategoryIdAsync(string tagName, string tagType)
        {
            await EnsureCategoryCacheLoadedAsync();

            if (YearRegex.IsMatch(tagName))
            {
                return _categoryCache.GetValueOrDefault(PhotoConstants.CATEGORY_TIME, -1);
            }

            if (IsCameraBrand(tagName))
            {
                return _categoryCache.GetValueOrDefault(PhotoConstants.CATEGORY_CAMERA, -1);
            }

            return _categoryCache.GetValueOrDefault(PhotoConstants.CATEGORY_GENERAL, -1);
        }

        private async Task EnsureCategoryCacheLoadedAsync()
        {
            if (_categoryCache.Count > 0) return;

            await _cacheLock.WaitAsync();
            try
            {
                if (_categoryCache.Count > 0) return;

                var categories = await _context.PhotoCategories
                    .AsNoTracking()
                    .Where(c => c.IsActive &&
                        (c.CategoryCode == PhotoConstants.CATEGORY_TIME ||
                         c.CategoryCode == PhotoConstants.CATEGORY_CAMERA ||
                         c.CategoryCode == PhotoConstants.CATEGORY_GENERAL))
                    .Select(c => new { c.CategoryCode, c.CategoryId })
                    .ToListAsync();

                foreach (var cat in categories)
                {
                    _categoryCache[cat.CategoryCode] = cat.CategoryId;
                }

                _logger.LogInformation("分類快取已載入，共 {Count} 筆", categories.Count);

            }
            finally
            {
                _cacheLock.Release();
            }

            
        }

        /// <summary>
        /// 判斷是否為相機品牌
        /// 優先檢查資料庫，如果資料庫沒有設定則使用預設清單
        /// </summary>
        private bool IsCameraBrand(string tagName)
        {
            // TODO: 未來應該從資料庫的 PhotoCategory 中動態查詢相機品牌
            // 目前使用硬編碼清單作為備用方案
            return PhotoConstants.CAMERA_BRANDS.Any(brand =>
                tagName.Contains(brand, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 根據分類代碼查詢分類
        /// </summary>
        private async Task<PhotoCategory> GetCategoryByCodeAsync(string categoryCode)
        {
            return await _context.PhotoCategories
                .AsNoTracking()
                .Where(c => c.CategoryCode == categoryCode && c.IsActive == true)
                .FirstOrDefaultAsync();
        }
    }

    /// <summary>
    /// 標籤分類服務介面
    /// </summary>
    public interface ITagCategorizationService
    {
        /// <summary>
        /// 根據標籤名稱判斷應屬於哪個分類
        /// </summary>
        Task<int> DetermineCategoryIdAsync(string tagName, string tagType);
    }
}