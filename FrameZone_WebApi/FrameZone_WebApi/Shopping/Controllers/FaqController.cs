using FrameZone_WebApi.Shopping.Services;
using Microsoft.AspNetCore.Mvc;

namespace FrameZone_WebApi.Shopping.Controllers
{
    /// <summary>
    /// 常見問題 API Controller
    /// </summary>
    [ApiController]
    [Route("api/shopping/faq")]
    public class FaqController : ControllerBase
    {
        private readonly IFaqService _faqService;
        private readonly ILogger<FaqController> _logger;
        private const int ShoppingSystemId = 4; // 購物系統的 SystemId

        public FaqController(
            IFaqService faqService,
            ILogger<FaqController> logger)
        {
            _faqService = faqService;
            _logger = logger;
        }

        /// <summary>
        /// 取得購物系統的所有 FAQ
        /// </summary>
        /// <returns>FAQ 列表</returns>
        /// <response code="200">成功取得 FAQ 列表</response>
        /// <response code="500">伺服器錯誤</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllFaqs()
        {
            try
            {
                var faqs = await _faqService.GetAllFaqsAsync(ShoppingSystemId);

                return Ok(new
                {
                    success = true,
                    data = faqs,
                    message = "成功取得 FAQ 列表"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得 FAQ 列表時發生錯誤");
                return StatusCode(500, new
                {
                    success = false,
                    message = "伺服器錯誤，請稍後再試"
                });
            }
        }

        /// <summary>
        /// 依分類取得購物系統的 FAQ
        /// </summary>
        /// <param name="category">分類名稱</param>
        /// <returns>FAQ 列表</returns>
        /// <response code="200">成功取得 FAQ 列表</response>
        /// <response code="400">分類參數無效</response>
        /// <response code="500">伺服器錯誤</response>
        [HttpGet("{category}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFaqsByCategory(string category)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(category))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "分類參數不可為空"
                    });
                }

                var faqs = await _faqService.GetFaqsByCategoryAsync(ShoppingSystemId, category);

                return Ok(new
                {
                    success = true,
                    data = faqs,
                    message = $"成功取得分類 '{category}' 的 FAQ 列表"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得 FAQ 列表時發生錯誤，Category: {Category}", category);
                return StatusCode(500, new
                {
                    success = false,
                    message = "伺服器錯誤，請稍後再試"
                });
            }
        }
    }
}
