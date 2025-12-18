using Microsoft.AspNetCore.Http;
using FrameZone_WebApi.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        Task<PhotoMetadataDtos> TestExifAsync(IFormFile file);

        /// <summary>
        /// 取得照片資訊
        /// </summary>
        Task<PhotoDetailDTO> GetPhotoByIdAsync(long photoId, long userId);

        /// <summary>
        /// 刪除照片
        /// </summary>
        Task<bool> DeletePhotoAsync(long photoId, long userId);
    }
}
