using FrameZone_WebApi.DTOs;
using FrameZone_WebApi.Models;

namespace FrameZone_WebApi.Repositories
{
    public interface IPhotoRepository
    {
        #region Photo 表操作

        /// <summary>
        /// 新增照片
        /// </summary>
        /// <param name="photo">照片物件</param>
        /// <returns>新增後的照片</returns>
        Task<Photo> AddPhotoAsync(Photo photo);

        /// <summary>
        /// 根據 PhotoId 查詢照片
        /// </summary>
        /// <param name="photoId">照片ID</param>
        /// <returns>照片物件，不存在則返回 null</returns>
        Task<Photo> GetPhotoByIdAsync(long photoId);


        /// <summary>
        /// 根據 Hash 查詢照片
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="hash">檔案 Hash</param>
        /// <returns>照片物件，不存在則返回 null</returns>
        Task<Photo> GetPhotoByHashAsync(long userId, string hash);

        /// <summary>
        /// 更新照片資訊
        /// </summary>
        /// <param name="photo">照片物件</param>
        /// <returns>是否更新成功</returns>
        Task<bool> UpdatePhotoAsync(Photo photo);

        /// <summary>
        /// 軟刪除照片
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <returns>是否刪除成功</returns>
        Task<bool> SoftDeletePhotoAsync(long photoId);

        /// <summary>
        /// 查詢使用者的所有照片
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="pageIndex">頁碼</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <returns>照片列表</returns>
        Task<List<Photo>> GetPhotosByUserIdAsync(long userId, int pageIndex = 1, int pageSize = 20);

        Task<int> GetUserPhotoCountAsync(long userId);

        Task<long?> GetPhotoOwnerUserIdAsync(long photoId);

        #endregion

        #region PhotoMetadata 表操作

        /// <summary>
        /// 新增照片元數據
        /// </summary>
        /// <param name="photoMetadata">元數據物件</param>
        /// <returns>新增後的元數據</returns>
        Task<PhotoMetadatum> AddPhotoMetadataAsync(PhotoMetadatum photoMetadata);

        /// <summary>
        /// 根據 PhotoId 查詢元數據
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <returns>元數據物件，不存在則返回 null</returns>
        Task<PhotoMetadatum> GetPhotoMetadataByPhotoIdAsync(long photoId);

        #endregion

        #region PhotoTag 表操作

        /// <summary>
        /// 根據標籤名稱和類型查詢標籤
        /// </summary>
        /// <param name="tagName">標籤名稱</param>
        /// <param name="tagType">標籤類型</param>
        /// <returns>標籤物件，不存在則返回</returns>
        Task<PhotoTag> GetTagByNameAsync(string tagName, string tagType);

        /// <summary>
        /// 建立新標籤
        /// </summary>
        /// <param name="tag">標籤物件</param>
        /// <returns>新增後的標籤</returns>
        Task<PhotoTag> CreateTagAsync(PhotoTag tag);

        /// <summary>
        /// 取得或建立標籤
        /// </summary>
        /// <param name="tagName">標籤名稱</param>
        /// <param name="tagType">標籤類型</param>
        /// <param name="categoryId">分類 ID (必填)</param>
        /// <param name="parentTagId">父標籤 ID (可選)</param>
        /// <param name="userId">使用者 ID (可選)</param>
        /// <returns>標籤物件</returns>
        Task<PhotoTag> GetOrCreateTagAsync(string tagName, string tagType, int categoryId, int? parentTagId = null, long? userId = null);

        #endregion

        #region PhotoPhotoTag 表操作

        /// <summary>
        /// 新增照片與標籤的關聯
        /// </summary>
        /// <param name="photoPhotoTag">照片標籤關聯物件</param>
        /// <returns>新增後的關聯物件</returns>
        Task<PhotoPhotoTag> AddPhotoTagAsync(PhotoPhotoTag photoPhotoTag);

        /// <summary>
        /// 批次新增照片標籤
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <param name="tagIds">標籤 ID 列表</param>
        /// <param name="sourceId">來源 ID</param>
        /// <param name="confidence">信心度</param>
        /// <returns></returns>
        Task<int> AddPhotoTagsBatchAsync(long photoId, List<int> tagIds, int sourceId, decimal? confidence = null);

        /// <summary>
        /// 查詢照片的所有標籤
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <returns>標籤列表</returns>
        Task<List<PhotoTag>> GetPhotoTagsByPhotoIdAsync(long photoId);

        /// <summary>
        /// 移除照片的標籤
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <param name="tagId">標籤 ID</param>
        /// <returns>是否移除成功</returns>
        Task<bool> RemovePhotoTagAsync(long photoId, int tagId);

        #endregion

        #region PhotoLocation 表操作

        /// <summary>
        /// 新增照片地點資訊
        /// </summary>
        /// <param name="location">地點物件</param>
        /// <returns>新增後的地點物件</returns>
        Task<PhotoLocation> AddPhotoLocationAsync(PhotoLocation location);

        /// <summary>
        /// 根據 PhotoId 查詢地點資訊
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <returns>地點物件，不存在則返回</returns>
        Task<PhotoLocation> GetPhotoLocationByPhotoIdAsync(long photoId);

        #endregion

        #region ClassificationSource 表操作

        /// <summary>
        /// 根據來源代碼查詢分類來源
        /// </summary>
        /// <param name="sourceCode">來源代碼</param>
        /// <returns>分類來源物件</returns>
        Task<PhotoClassificationSource> GetClassificationSourceByCodeAsync(string sourceCode);

        #endregion

        #region PhotoCategory 表操作

        /// <summary>
        /// 根據分類代碼查詢分類
        /// </summary>
        /// <param name="categoryCode">分類代碼</param>
        /// <returns>分類物件</returns>
        Task<PhotoCategory> GetCategoryByCodeAsync(string categoryCode);

        /// <summary>
        /// 根據分類 ID 查詢分類
        /// </summary>
        /// <param name="categoryId">分類 ID</param>
        /// <returns>分類物件</returns>
        Task<PhotoCategory> GetCategoryByIdAsync(int categoryId);

        /// <summary>
        /// 根據分類類型查詢所有分類
        /// </summary>
        /// <param name="categoryTypeCode">分類類型代碼</param>
        /// <returns>分類列表</returns>
        Task<List<PhotoCategory>> GetCategoriesByTypeAsync(string categoryTypeCode);

        /// <summary>
        /// 取得或建立分類
        /// </summary>
        /// <param name="categoryName">分類名稱</param>
        /// <param name="categoryCode">分類代碼</param>
        /// <param name="categoryTypeId">分類類型 ID</param>
        /// <param name="parentCategoryId">父分類 ID</param>
        /// <param name="userId">使用者 ID（自定義分類時使用）</param>
        /// <returns>分類物件</returns>
        Task<PhotoCategory> GetOrCreateCategoryAsync(
            string categoryName,
            string categoryCode,
            int categoryTypeId,
            int? parentCategoryId = null,
            long? userId = null);

        /// <summary>
        /// 取得分類樹（包含照片數量統計）
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="parentCategoryId">父分類 ID（null 表示根節點）</param>
        /// <returns>分類樹節點列表</returns>
        Task<List<CategoryTreeNodeDTO>> GetCategoryTreeWithCountsAsync(long userId, int? parentCategoryId = null);

        #endregion

        #region PhotoPhotoCategory 表操作

        /// <summary>
        /// 批次新增照片與分類的關聯
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <param name="categoryIds">分類 ID 列表</param>
        /// <param name="sourceId">來源 ID</param>
        /// <param name="confidence">信心度</param>
        /// <returns>新增的記錄數量</returns>
        Task<int> AddPhotoCategoriesAsync(
            long photoId,
            List<int> categoryIds,
            int sourceId,
            decimal? confidence = null);

        /// <summary>
        /// 查詢照片的所有分類
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <returns>分類列表</returns>
        Task<List<PhotoCategory>> GetPhotoCategoriesByPhotoIdAsync(long photoId);

        #endregion

        #region 進階查詢

        /// <summary>
        /// 查詢照片完整資訊
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <returns>照片詳細資訊 DTO</returns>
        Task<PhotoDetailDTO> GetPhotoDetailAsync(long photoId);

        /// <summary>
        /// 根據標籤查詢照片
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="tagId">標籤 ID</param>
        /// <param name="pageIndex">頁碼</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <returns>照片列表</returns>
        Task<List<Photo>> GetPhotosByTagIdAsync(long userId, int tagId, int pageIndex = 1, int pageSize = 20);

        /// <summary>
        /// 根據日期範圍查詢照片
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="startDate">開始日期</param>
        /// <param name="endDate">結束日期</param>
        /// <param name="pageIndex">頁碼</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <returns>照片列表</returns>
        Task<List<Photo>> GetPhotosByDateRangeAsync(long userId, DateTime? startDate, DateTime? endDate, int pageIndex = 1, int pageSize = 20);

        /// <summary>
        /// 查詢照片（支援多條件篩選、分頁、排序）
        /// 這是最核心的查詢方法，支援所有篩選條件
        /// </summary>
        /// <param name="request">查詢請求 DTO</param>
        /// <param name="userId">使用者 ID</param>
        /// <returns>照片列表和總筆數</returns>
        Task<(List<Photo> Photos, int TotalCount)> QueryPhotosAsync(PhotoQueryRequestDTO request, long userId);


        #endregion

        #region Transaction 相關

        /// <summary>
        /// 完整上傳照片
        /// </summary>
        /// <param name="photo">照片物件</param>
        /// <param name="metadata">元數據物件</param>
        /// <param name="tagNames">標籤名稱列表</param>
        /// <param name="sourceCode">分類來源代碼</param>
        /// <param name="location">地點物件</param>
        /// <returns>新增後的照片</returns>
        Task<Photo> UploadPhotoWithDetailsAsync(
            Photo photo,
            PhotoMetadatum metadata,
            List<string> tagNames,
            string sourceCode,
            PhotoLocation location = null);

        #endregion


        Task<bool> ExistsPhotoByHashAsync(long userId, string hash);

        #region 縮圖優化相關

        /// <summary>
        /// 只取得縮圖資料
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <returns>縮圖資料 DTO</returns>
        Task<ThumbnailDataDTO> GetThumbnailDataAsync(long photoId);

        /// <summary>
        /// 只取得照片原圖資料
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <returns>原圖資料 DTO</returns>
        Task<PhotoDataDTO> GetPhotoDataAsync(long photoId);

        /// <summary>
        /// 更新縮圖資料
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <param name="thumbnailData">縮圖資料</param>
        /// <returns>是否更新成功</returns>
        Task<bool> UpdateThumbnailAsync(long photoId, byte[] thumbnailData);

        #endregion

        #region PhotoStorage 表操作

        /// <summary>
        /// 新增照片儲存記錄
        /// 記錄照片在 Blob Storage 的儲存位置
        /// </summary>
        /// <param name="storage">照片儲存物件</param>
        /// <returns>新增後的儲存記錄</returns>
        Task<PhotoStorage> AddPhotoStorageAsync(PhotoStorage storage);

        /// <summary>
        /// 根據 PhotoId 查詢主要儲存位置
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <returns>主要儲存記錄（IsPrimary = true），不存在則返回 null</returns>
        Task<PhotoStorage> GetPrimaryStorageByPhotoIdAsync(long photoId);

        /// <summary>
        /// 根據 PhotoId 查詢所有儲存位置
        /// 支援多個儲存位置（例如：主要儲存 + 備份）
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <returns>儲存記錄列表</returns>
        Task<List<PhotoStorage>> GetAllStoragesByPhotoIdAsync(long photoId);

        /// <summary>
        /// 更新照片儲存記錄
        /// 例如：變更存取層級、更新 AccessURL
        /// </summary>
        /// <param name="storage">照片儲存物件</param>
        /// <returns>是否更新成功</returns>
        Task<bool> UpdatePhotoStorageAsync(PhotoStorage storage);

        /// <summary>
        /// 刪除照片儲存記錄
        /// 實體刪除，用於清理儲存記錄
        /// </summary>
        /// <param name="storageId">儲存記錄 ID</param>
        /// <returns>是否刪除成功</returns>
        Task<bool> DeletePhotoStorageAsync(long storageId);

        /// <summary>
        /// 根據 StorageId 查詢儲存記錄
        /// </summary>
        /// <param name="storageId">儲存記錄 ID</param>
        /// <returns>儲存記錄，不存在則返回 null</returns>
        Task<PhotoStorage> GetStorageByIdAsync(long storageId);

        /// <summary>
        /// 檢查照片是否有儲存記錄
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <returns>是否存在儲存記錄</returns>
        Task<bool> HasStorageRecordAsync(long photoId);

        #endregion

        /// <summary>
        /// 根據標籤 ID 查詢標籤
        /// </summary>
        Task<PhotoTag> GetTagByIdAsync(int tagId);

        Task<List<CategoryWithTagsDTO>> GetTagHierarchyAsync(long userId);
        Task<Dictionary<int, int>> GetTagPhotoCountsAsync(long userId);
        Task<PhotoTag> CreateCustomTagAsync(string tagName, int categoryId, int? parentTagId, long userId);
    }
}