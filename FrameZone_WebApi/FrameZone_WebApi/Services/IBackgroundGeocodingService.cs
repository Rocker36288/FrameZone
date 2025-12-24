namespace FrameZone_WebApi.Services
{
    /// <summary>
    /// 背景地理編碼服務介面
    /// </summary>
    public interface IBackgroundGeocodingService
    {
        /// <summary>
        /// 背景處理照片的地理編碼
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <param name="userId">使用者 ID</param>
        /// <param name="latitude">GPS 緯度</param>
        /// <param name="longitude">GPS 經度</param>
        /// <returns></returns>
        Task ProcessGeocodingAsync(long photoId, long userId, decimal latitude, decimal longitude);
    }
}