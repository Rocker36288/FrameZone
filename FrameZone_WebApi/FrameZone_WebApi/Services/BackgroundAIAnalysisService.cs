using FrameZone_WebApi.DTOs;
using FrameZone_WebApi.DTOs.AI;
using FrameZone_WebApi.Helpers;
using FrameZone_WebApi.Services.Interfaces;
using Microsoft.CodeAnalysis.Diagnostics;
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

            PhotoAIAnalysisResponseDto? analysisResult = null;
            bool success = false;
            string errorMessage = string.Empty;

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var photoService = scope.ServiceProvider.GetRequiredService<IPhotoService>();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                var request = new PhotoAIAnalysisRequestDto
                {
                    PhotoId = photoId,
                    UserId = userId,
                    ForceReanalysis = false
                };

                _logger.LogInformation("🤖 背景 AI 分析開始，PhotoId={PhotoId}, UserId={UserId}", photoId, userId);

                // 執行 AI 分析（直接返回 PhotoAIAnalysisResponseDto）
                analysisResult = await photoService.AnalyzePhotoWithAIAsync(request);

                // 根據 Status 判斷成功或失敗
                success = analysisResult.Status == AIAnalysisConstants.Status.Success;

                if (success)
                {
                    _logger.LogInformation("🤖 背景 AI 分析完成，PhotoId={PhotoId}, 建議數={SuggestionCount}",
                        photoId, analysisResult.TagSuggestions?.Count ?? 0);

                    // ⭐ 發送成功通知
                    await SendSuccessNotificationAsync(notificationService, userId, photoId, analysisResult);
                }
                else
                {
                    errorMessage = analysisResult.ErrorMessage ?? "AI 分析失敗";
                    _logger.LogWarning("🤖 背景 AI 分析失敗，PhotoId={PhotoId}, 狀態={Status}, 錯誤={Error}",
                        photoId, analysisResult.Status, errorMessage);

                    // ⭐ 發送失敗通知
                    await SendFailureNotificationAsync(notificationService, userId, photoId, errorMessage);
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                _logger.LogError(ex, "背景 AI 分析發生例外，PhotoId={PhotoId}", photoId);

                // ⭐ 發送失敗通知（例外情況）
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                    await SendFailureNotificationAsync(notificationService, userId, photoId, "系統錯誤，請稍後再試");
                }
                catch (Exception notifyEx)
                {
                    _logger.LogError(notifyEx, "發送失敗通知時發生錯誤，PhotoId={PhotoId}", photoId);
                }
            }
        }

        /// <summary>
        /// 發送 AI 分析成功通知
        /// </summary>
        private async Task SendSuccessNotificationAsync(
            INotificationService notificationService,
            long userId,
            long photoId,
            PhotoAIAnalysisResponseDto analysisResult)
        {
            try
            {
                var suggestionCount = analysisResult.TagSuggestions?.Count ?? 0;
                var title = "AI 分析完成 ✨";
                var content = suggestionCount > 0
                    ? $"照片已完成 AI 分析，找到 {suggestionCount} 個標籤建議！點擊查看並套用標籤。"
                    : "照片已完成 AI 分析，但未找到標籤建議。";

                var result = await notificationService.SendNotificationAsync(
                    userId: userId,
                    systemCode: NotificationConstant.SystemCodes.PHOTO,
                    categoryCode: NotificationConstant.CategoryCodes.AI_ANALYSIS_COMPLETED,
                    title: title,
                    content: content,
                    priorityCode: NotificationConstant.PriorityCodes.MEDIUM,
                    relatedObjectType: "Photo",
                    relatedObjectId: photoId
                );

                if (result.Success)
                {
                    _logger.LogInformation("✅ AI 分析成功通知已發送，UserId={UserId}, PhotoId={PhotoId}, NotificationId={NotificationId}",
                        userId, photoId, result.Data);
                }
                else
                {
                    _logger.LogWarning("⚠️ 發送 AI 分析成功通知失敗：{Message}", result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發送 AI 分析成功通知時發生錯誤，PhotoId={PhotoId}", photoId);
            }
        }

        /// <summary>
        /// 發送 AI 分析失敗通知
        /// </summary>
        private async Task SendFailureNotificationAsync(
            INotificationService notificationService,
            long userId,
            long photoId,
            string errorMessage)
        {
            try
            {
                var title = "AI 分析失敗 ⚠️";
                var content = $"照片 AI 分析失敗：{errorMessage}。您可以稍後重試或手動標記照片。";

                var result = await notificationService.SendNotificationAsync(
                    userId: userId,
                    systemCode: NotificationConstant.SystemCodes.PHOTO,
                    categoryCode: NotificationConstant.CategoryCodes.AI_ANALYSIS_FAILED,
                    title: title,
                    content: content,
                    priorityCode: NotificationConstant.PriorityCodes.LOW,
                    relatedObjectType: "Photo",
                    relatedObjectId: photoId
                );

                if (result.Success)
                {
                    _logger.LogInformation("✅ AI 分析失敗通知已發送，UserId={UserId}, PhotoId={PhotoId}", userId, photoId);
                }
                else
                {
                    _logger.LogWarning("⚠️ 發送 AI 分析失敗通知失敗：{Message}", result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發送 AI 分析失敗通知時發生錯誤，PhotoId={PhotoId}", photoId);
            }
        }
    }
}