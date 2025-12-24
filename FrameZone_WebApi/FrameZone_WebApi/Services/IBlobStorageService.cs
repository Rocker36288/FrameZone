using FrameZone_WebApi.DTOs;

namespace FrameZone_WebApi.Services
{
    /// <summary>
    /// Azure Blob Storage 服務介面
    /// 定義照片和縮圖在 Blob Storage 的核心操作
    /// </summary>
    public interface IBlobStorageService
    {
        #region 上傳操作

        /// <summary>
        /// 上傳照片原圖到 Blob Storage
        /// </summary>
        /// <param name="fileStream">照片檔案串流</param>
        /// <param name="fileName">檔案名稱（含副檔名）</param>
        /// <param name="userId">使用者 ID（用於組織路徑）</param>
        /// <param name="photoId">照片 ID（用於組織路徑）</param>
        /// <param name="uploadDate">上傳日期（用於組織路徑）</param>
        /// <param name="contentType">檔案 MIME 類型（例如：image/jpeg）</param>
        /// <returns>Blob 完整 URL</returns>
        Task<string> UploadPhotoAsync(
            Stream fileStream,
            string fileName,
            long userId,
            long photoId,
            DateTime uploadDate,
            string contentType = "image/jpeg");

        /// <summary>
        /// 上傳縮圖到 Blob Storage
        /// </summary>
        /// <param name="thumbnailStream">縮圖檔案串流</param>
        /// <param name="fileName">檔案名稱（含副檔名）</param>
        /// <param name="userId">使用者 ID</param>
        /// <param name="photoId">照片 ID</param>
        /// <param name="uploadDate">上傳日期</param>
        /// <param name="contentType">檔案 MIME 類型</param>
        /// <returns>Blob 完整 URL</returns>
        Task<string> UploadThumbnailAsync(
            Stream thumbnailStream,
            string fileName,
            long userId,
            long photoId,
            DateTime uploadDate,
            string contentType = "image/jpeg");

        /// <summary>
        /// 通用上傳方法（內部使用）
        /// </summary>
        /// <param name="stream">檔案串流</param>
        /// <param name="blobPath">Blob 相對路徑</param>
        /// <param name="containerName">容器名稱</param>
        /// <param name="contentType">內容類型</param>
        /// <returns>Blob 完整 URL</returns>
        Task<string> UploadAsync(
            Stream stream,
            string blobPath,
            string containerName,
            string contentType = "image/jpeg");

        #endregion

        #region 下載操作

        /// <summary>
        /// 下載照片原圖
        /// </summary>
        /// <param name="blobPath">Blob 相對路徑</param>
        /// <returns>照片檔案串流</returns>
        Task<Stream> DownloadPhotoAsync(string blobPath);

        /// <summary>
        /// 下載縮圖
        /// </summary>
        /// <param name="blobPath">Blob 相對路徑</param>
        /// <returns>縮圖檔案串流</returns>
        Task<Stream> DownloadThumbnailAsync(string blobPath);

        /// <summary>
        /// 通用下載方法
        /// </summary>
        /// <param name="blobPath">Blob 相對路徑</param>
        /// <param name="containerName">容器名稱</param>
        /// <returns>檔案串流</returns>
        Task<Stream> DownloadAsync(string blobPath, string containerName);

        #endregion

        #region 刪除操作

        /// <summary>
        /// 刪除照片原圖
        /// </summary>
        /// <param name="blobPath">Blob 相對路徑</param>
        /// <returns>是否刪除成功</returns>
        Task<bool> DeletePhotoAsync(string blobPath);

        /// <summary>
        /// 刪除縮圖
        /// </summary>
        /// <param name="blobPath">Blob 相對路徑</param>
        /// <returns>是否刪除成功</returns>
        Task<bool> DeleteThumbnailAsync(string blobPath);

        /// <summary>
        /// 通用刪除方法
        /// </summary>
        /// <param name="blobPath">Blob 相對路徑</param>
        /// <param name="containerName">容器名稱</param>
        /// <returns>是否刪除成功</returns>
        Task<bool> DeleteAsync(string blobPath, string containerName);

        #endregion

        #region 檢查操作

        /// <summary>
        /// 檢查 Blob 是否存在
        /// </summary>
        /// <param name="blobPath">Blob 相對路徑</param>
        /// <param name="containerName">容器名稱</param>
        /// <returns>是否存在</returns>
        Task<bool> ExistsAsync(string blobPath, string containerName);

        /// <summary>
        /// 檢查照片原圖是否存在
        /// </summary>
        /// <param name="blobPath">Blob 相對路徑</param>
        /// <returns>是否存在</returns>
        Task<bool> PhotoExistsAsync(string blobPath);

        /// <summary>
        /// 檢查縮圖是否存在
        /// </summary>
        /// <param name="blobPath">Blob 相對路徑</param>
        /// <returns>是否存在</returns>
        Task<bool> ThumbnailExistsAsync(string blobPath);

        #endregion

        #region URL 生成

        /// <summary>
        /// 生成照片的存取 URL（可能是 CDN URL 或 Blob URL）
        /// </summary>
        /// <param name="blobPath">Blob 相對路徑</param>
        /// <param name="useSasToken">是否使用 SAS Token（安全存取）</param>
        /// <returns>完整的存取 URL</returns>
        Task<string> GetPhotoUrlAsync(string blobPath, bool useSasToken = true);

        /// <summary>
        /// 生成縮圖的存取 URL
        /// </summary>
        /// <param name="blobPath">Blob 相對路徑</param>
        /// <param name="useSasToken">是否使用 SAS Token</param>
        /// <returns>完整的存取 URL</returns>
        Task<string> GetThumbnailUrlAsync(string blobPath, bool useSasToken = true);

        /// <summary>
        /// 生成 SAS Token URL（具有時效性的安全 URL）
        /// </summary>
        /// <param name="blobPath">Blob 相對路徑</param>
        /// <param name="containerName">容器名稱</param>
        /// <param name="expiryMinutes">有效期限（分鐘）</param>
        /// <returns>帶有 SAS Token 的完整 URL</returns>
        Task<string> GenerateSasUrlAsync(
            string blobPath,
            string containerName,
            int? expiryMinutes = null);

        #endregion

        #region 路徑生成

        /// <summary>
        /// 生成照片原圖的 Blob 路徑
        /// 格式：{userId}/{year}/{month}/{photoId}_original.{extension}
        /// 範例：1001/2024/12/550001_original.jpg
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="photoId">照片 ID</param>
        /// <param name="uploadDate">上傳日期</param>
        /// <param name="fileExtension">副檔名（不含點，例如：jpg）</param>
        /// <returns>Blob 相對路徑</returns>
        string GeneratePhotoBlobPath(
            long userId,
            long photoId,
            DateTime uploadDate,
            string fileExtension);

        /// <summary>
        /// 生成縮圖的 Blob 路徑
        /// 格式：{userId}/{year}/{month}/{photoId}_thumbnail.{extension}
        /// 範例：1001/2024/12/550001_thumbnail.jpg
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="photoId">照片 ID</param>
        /// <param name="uploadDate">上傳日期</param>
        /// <param name="fileExtension">副檔名（不含點）</param>
        /// <returns>Blob 相對路徑</returns>
        string GenerateThumbnailBlobPath(
            long userId,
            long photoId,
            DateTime uploadDate,
            string fileExtension);

        #endregion

        #region 容器管理

        /// <summary>
        /// 確保容器存在（如果不存在則建立）
        /// 應用程式啟動時呼叫
        /// </summary>
        /// <returns>是否成功</returns>
        Task<bool> EnsureContainersExistAsync();

        /// <summary>
        /// 取得容器的使用統計資訊（選用功能）
        /// </summary>
        /// <param name="containerName">容器名稱</param>
        /// <returns>容器統計資訊</returns>
        Task<BlobContainerStatsDto> GetContainerStatsAsync(string containerName);

        #endregion

        #region 批次操作（選用）

        /// <summary>
        /// 批次上傳照片
        /// </summary>
        /// <param name="uploadRequests">上傳請求列表</param>
        /// <returns>上傳結果列表</returns>
        Task<List<BlobUploadResultDto>> UploadBatchAsync(
            List<BlobUploadRequestDto> uploadRequests);

        /// <summary>
        /// 批次刪除照片
        /// </summary>
        /// <param name="blobPaths">Blob 路徑列表</param>
        /// <param name="containerName">容器名稱</param>
        /// <returns>刪除結果</returns>
        Task<BatchDeleteResultDto> DeleteBatchAsync(
            List<string> blobPaths,
            string containerName);

        #endregion
    }
}