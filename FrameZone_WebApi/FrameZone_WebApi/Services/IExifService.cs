using FrameZone_WebApi.DTOs;

namespace FrameZone_WebApi.Services
{
    public interface IExifService
    {
        /// <summary>
        /// EXIF 服務介面
        /// </summary>
        PhotoMetadataDtos ExtractMetadata(Stream imageStream, string fileName);

        /// <summary>
        /// 根據 EXIF 資訊自動生成分類標籤
        /// </summary>
        List<string> GenerateAutoTags(PhotoMetadataDtos metadata);

        /// <summary>
        /// 計算檔案 Hash
        /// </summary>
        string CalculateFileHash(Stream fileStream);
    }
}
