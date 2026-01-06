using FrameZone_WebApi.DTOs.AI;

namespace FrameZone_WebApi.Services
{
    /// <summary>
    /// Azure Computer Vision API 服務介面
    /// </summary>
    public interface IAzureComputerVisionService
    {
        /// <summary>
        /// 分析照片內容
        /// </summary>
        /// <param name="imageData">照片的二進位資料</param>
        /// <param name="features">要分析的特徵列表（例如：Objects, Tags, Description）</param>
        /// <returns>Azure Vision 分析結果</returns>
        Task<AzureVisionAnalysisDto> AnalyzeImageAsync(byte[] imageData, List<string> features);

        /// <summary>
        /// 從 URL 分析照片
        /// </summary>
        /// <param name="imageUrl">照片的公開 URL</param>
        /// <param name="features">要分析的特徵列表</param>
        /// <returns>Azure Vision 分析結果</returns>
        Task<AzureVisionAnalysisDto> AnalyzeImageFromUrlAsync(string imageUrl, List<string> features);

        /// <summary>
        /// 驗證照片是否符合 Azure 的要求
        /// </summary>
        /// <param name="imageData">照片的二進位資料</param>
        /// <returns>
        /// 返回三元組：
        /// - IsValid: 照片是否符合直接使用的條件
        /// - ErrorMessage: 不符合時的說明訊息
        /// - ShouldUseThumbnail: 是否建議使用縮圖替代
        /// </returns>
        Task<(bool IsValid, string ErrorMessage, bool ShouldUseThumbnail)> ValidateImageAsync(byte[] imageData);

        /// <summary>
        /// 測試 Azure API 連線是否正常
        /// </summary>
        /// <returns>是否連線成功</returns>
        Task<bool> TestConnectionAsync();
    }
}