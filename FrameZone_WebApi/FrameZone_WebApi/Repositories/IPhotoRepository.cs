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
        /// <param name="userId">使用者 ID</param>
        /// <returns>標籤物件</returns>
        Task<PhotoTag> GetOrCreateTagAsync(string tagName, string tagType, long? userId = null);

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

    }
}
