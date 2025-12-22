using FrameZone_WebApi.DTOs;

namespace FrameZone_WebApi.Services
{
    public interface IExifService
    {
        /// <summary>
        /// EXIF 服務介面
        /// </summary>
        PhotoMetadataDTO ExtractMetadata(Stream imageStream, string fileName);

        /// <summary>
        /// 根據 EXIF 資訊和地址資訊自動生成分類標籤
        /// </summary>
        /// <param name="metadata">EXIF 元數據</param>
        /// <param name="addressInfo">反向地理編碼取得的地址資訊</param>
        /// <returns>自動生成的標籤列表</returns>
        List<string> GenerateAutoTags(PhotoMetadataDTO metadata, AddressInfoDTO addressInfo = null);

        /// <summary>
        /// 計算檔案 Hash
        /// </summary>
        string CalculateFileHash(Stream fileStream);
    }
}
