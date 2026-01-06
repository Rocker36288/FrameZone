using FrameZone_WebApi.DTOs;
using FrameZone_WebApi.DTOs.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace FrameZone_WebApi.Services
{
    /// <summary>
    /// 背景 AI 分析：在新建立的 DI Scope 內執行，避免使用已釋放的 DbContext。
    /// </summary>
    public class BackgroundAIAnalysisService : IBackgroundAIAnalysisService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BackgroundAIAnalysisService> _logger;

        public BackgroundAIAnalysisService(IServiceProvider serviceProvider, ILogger<BackgroundAIAnalysisService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task ProcessAIAnalysisAsync(long photoId, long userId)
        {
            if (photoId <= 0)
            {
                _logger.LogWarning("背景 AI 分析跳過：PhotoId 無效 ({PhotoId})", photoId);
                return;
            }

            if (userId <= 0)
            {
                _logger.LogWarning("背景 AI 分析跳過：UserId 無效 ({UserId})，PhotoId={PhotoId}", userId, photoId);
                return;
            }

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var photoService = scope.ServiceProvider.GetRequiredService<IPhotoService>();

                var request = new PhotoAIAnalysisRequestDto
                {
                    PhotoId = photoId,
                    UserId = userId,
                    ForceReanalysis = false
                };

                _logger.LogInformation("🤖 背景 AI 分析開始，PhotoId={PhotoId}, UserId={UserId}", photoId, userId);
                await photoService.AnalyzePhotoWithAIAsync(request);
                _logger.LogInformation("🤖 背景 AI 分析完成，PhotoId={PhotoId}", photoId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "背景 AI 分析失敗，PhotoId={PhotoId}", photoId);
            }
        }
    }
}
