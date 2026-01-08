using FrameZone_WebApi.Models;
using FrameZone_WebApi.Shopping.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FrameZone_WebApi.Shopping.Services
{
    /// <summary>
    /// FAQ 服務實作
    /// </summary>
    public class FaqService : IFaqService
    {
        private readonly AAContext _context;
        private readonly ILogger<FaqService> _logger;

        public FaqService(AAContext context, ILogger<FaqService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 取得指定系統的所有 FAQ
        /// </summary>
        public async Task<List<FaqDto>> GetAllFaqsAsync(int systemId)
        {
            try
            {
                var faqs = await _context.FrequentlyAskedQuestions
                    .Where(f => f.SystemId == systemId)
                    .OrderBy(f => f.Category)
                    .ThenBy(f => f.CreatedAt)
                    .Select(f => new FaqDto
                    {
                        FaqId = f.FaqId,
                        SystemId = f.SystemId,
                        Category = f.Category,
                        Question = f.Question,
                        Answer = f.Answer,
                        CreatedAt = f.CreatedAt,
                        UpdatedAt = f.UpdatedAt
                    })
                    .ToListAsync();

                return faqs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得 FAQ 列表時發生錯誤，SystemId: {SystemId}", systemId);
                throw;
            }
        }

        /// <summary>
        /// 取得指定系統與分類的 FAQ
        /// </summary>
        public async Task<List<FaqDto>> GetFaqsByCategoryAsync(int systemId, string category)
        {
            try
            {
                var faqs = await _context.FrequentlyAskedQuestions
                    .Where(f => f.SystemId == systemId && f.Category == category)
                    .OrderBy(f => f.CreatedAt)
                    .Select(f => new FaqDto
                    {
                        FaqId = f.FaqId,
                        SystemId = f.SystemId,
                        Category = f.Category,
                        Question = f.Question,
                        Answer = f.Answer,
                        CreatedAt = f.CreatedAt,
                        UpdatedAt = f.UpdatedAt
                    })
                    .ToListAsync();

                return faqs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得 FAQ 列表時發生錯誤，SystemId: {SystemId}, Category: {Category}", systemId, category);
                throw;
            }
        }
    }
}
