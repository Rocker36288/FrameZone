using FrameZone_WebApi.DTOs;
using System.Threading.Tasks;

namespace FrameZone_WebApi.Services
{
    public interface IGeocodingService
    {
        /// <summary>
        /// 反向地理編碼 - 將 GPS 座標轉換為地址資訊
        /// </summary>
        /// <param name="latitude">緯度</param>
        /// <param name="longitude">經度</param>
        /// <param name="language">語言</param>
        /// <returns>地址資訊</returns>
        Task<ReverseGeocodeResponseDTO> ReverseGeocodeAsync(decimal latitude, decimal longitude, string language = "zh-TW");

        /// <summary>
        /// 反向地理編碼 - 使用 DTO 請求
        /// </summary>
        /// <param name="request">反向地理編碼請求</param>
        /// <returns>地址資訊</returns>
        Task<ReverseGeocodeResponseDTO> ReverseGeocodeAsync(ReverseGeocodeRequestDTO request);
    }
}
