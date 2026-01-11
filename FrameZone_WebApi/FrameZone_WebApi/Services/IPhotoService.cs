using Microsoft.AspNetCore.Http;
using FrameZone_WebApi.DTOs;

namespace FrameZone_WebApi.Services
{
    /// <summary>
    /// 照片服務介面
    /// </summary>
    public partial interface IPhotoService
    {
        /// <summary>
        /// 上傳單張照片
        /// </summary>
        Task<PhotoUploadResponseDTO> UploadPhotoAsync(IFormFile file, long userId);

        /// <summary>
        /// 批次上傳照片
        /// </summary>
        Task<BatchUploadResponseDTO> UploadPhotoBatchAsync(List<IFormFile> files, long userId);

        /// <summary>
        /// 測試 EXIF 解析
        /// </summary>
        Task<PhotoMetadataDTO> TestExifAsync(IFormFile file);

        /// <summary>
        /// 取得照片資訊
        /// </summary>
        Task<PhotoDetailDTO> GetPhotoByIdAsync(long photoId, long userId);

        /// <summary>
        /// 刪除照片
        /// </summary>
        Task<bool> DeletePhotoAsync(long photoId, long userId);

        /// <summary>
        /// 查詢照片 (支援多條件篩選、分頁、排序)
        /// </summary>
        /// <param name="request">查詢請求 DTO</param>
        /// <param name="userId">使用者 ID</param>
        /// <returns>查詢結果 DTO</returns>
        Task<PhotoQueryResponseDTO> QueryPhotosAsync(PhotoQueryRequestDTO request, long userId);

        /// <summary>
        /// 生成縮圖
        /// </summary>
        /// <param name="imageData"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        Task<byte[]> GenerateThumbnailAsync(byte[] imageData, int width, int height);

        /// <summary>
        /// 取得標籤階層（用於 Sidebar）
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>標籤階層回應</returns>
        Task<TagHierarchyResponseDTO> GetTagHierarchyAsync(long userId, string? aiSource = null);

        /// <summary>
        /// 根據標籤篩選照片（使用現有的 QueryPhotosAsync，這是一個便捷方法）
        /// </summary>
        /// <param name="request">查詢請求（包含 TagIds）</param>
        /// <param name="userId">使用者 ID</param>
        /// <returns>查詢結果</returns>
        Task<PhotoQueryResponseDTO> GetPhotosByTagsAsync(PhotoQueryRequestDTO request, long userId);

        /// <summary>
        /// 建立自訂標籤
        /// </summary>
        /// <param name="request">建立自訂標籤請求</param>
        /// <param name="userId">使用者 ID</param>
        /// <returns>建立結果</returns>
        Task<CreateCustomTagResponseDTO> CreateCustomTagAsync(CreateCustomTagRequestDTO request, long userId);

        /// <summary>
        /// 批次添加標籤到多張照片
        /// 支援同時添加現有標籤和建立新標籤
        /// </summary>
        /// <param name="request">批次添加標籤請求（包含照片 ID 列表、現有標籤 ID、新標籤資訊）</param>
        /// <param name="userId">使用者 ID</param>
        /// <returns>批次添加結果（包含成功/失敗統計、建立的新標籤列表）</returns>
        Task<BatchAddTagsResponseDTO> BatchAddTagsToPhotosAsync(BatchAddTagsRequestDTO request, long userId);

        /// <summary>
        /// 從照片移除標籤
        /// 只能移除手動添加的標籤（SOURCE_MANUAL）
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <param name="tagId">標籤 ID</param>
        /// <param name="userId">使用者 ID（用於權限驗證）</param>
        /// <returns>移除結果</returns>
        Task<RemoveTagResponseDTO> RemoveTagFromPhotoAsync(long photoId, int tagId, long userId);

        /// <summary>
        /// 搜尋標籤
        /// 支援關鍵字模糊搜尋、標籤類型篩選、分類篩選
        /// </summary>
        /// <param name="request">搜尋標籤請求（包含關鍵字、類型篩選、分類篩選、數量限制）</param>
        /// <param name="userId">使用者 ID（用於統計該使用者的照片數量）</param>
        /// <returns>搜尋結果（包含標籤列表、總數）</returns>
        Task<SearchTagsResponseDTO> SearchTagsAsync(SearchTagsRequestDTO request, long userId);

        /// <summary>
        /// 取得照片的所有標籤詳細資訊
        /// 返回標籤並按來源分類（EXIF、MANUAL、AI、GEOCODING）
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <param name="userId">使用者 ID（用於權限驗證）</param>
        /// <returns>照片標籤詳細資訊（包含按來源分類的標籤列表）</returns>
        Task<PhotoTagsDetailDTO> GetPhotoTagsAsync(long photoId, long userId);

        /// <summary>
        /// 取得可用分類列表
        /// 返回系統分類和用戶自定義分類，用於標籤建立時選擇分類
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>可用分類列表（區分系統分類和用戶自定義分類）</returns>
        Task<AvailableCategoriesResponseDTO> GetAvailableCategoriesAsync(long userId);
    }
}