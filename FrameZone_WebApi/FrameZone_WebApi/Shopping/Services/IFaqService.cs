using FrameZone_WebApi.Shopping.DTOs;

namespace FrameZone_WebApi.Shopping.Services
{
    /// <summary>
    /// FAQ 服務介面
    /// </summary>
    public interface IFaqService
    {
        /// <summary>
        /// 取得指定系統的所有 FAQ
        /// </summary>
        /// <param name="systemId">系統 ID</param>
        /// <returns>FAQ 列表</returns>
        Task<List<FaqDto>> GetAllFaqsAsync(int systemId);

        /// <summary>
        /// 取得指定系統與分類的 FAQ
        /// </summary>
        /// <param name="systemId">系統 ID</param>
        /// <param name="category">分類</param>
        /// <returns>FAQ 列表</returns>
        Task<List<FaqDto>> GetFaqsByCategoryAsync(int systemId, string category);
    }
}
