using Microsoft.AspNetCore.Http;
using FrameZone_WebApi.DTOs;

namespace FrameZone_WebApi.Services
{
    /// <summary>
    /// 照片服務介面
    /// </summary>
    public interface IPhotoService
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
        Task<TagHierarchyResponseDTO> GetTagHierarchyAsync(long userId);

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
    }
}