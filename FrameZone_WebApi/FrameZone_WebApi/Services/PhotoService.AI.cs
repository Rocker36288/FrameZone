using FrameZone_WebApi.Constants;
using FrameZone_WebApi.DTOs.AI;
using FrameZone_WebApi.Models;
using Microsoft.Identity.Client.Extensions.Msal;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;

namespace FrameZone_WebApi.Services
{
    /// <summary>
    /// PhotoService 的 AI 功能實作（Partial Class）
    /// 
    /// 📌 重要提醒：這是一個 partial class，需要與主 PhotoService.cs 配合使用
    /// 
    /// <para><b>⚠️ 依賴注入修改說明</b></para>
    /// 
    /// 這個 partial class 需要額外的 AI 服務依賴。請在主 PhotoService.cs 的建構函數中添加：
    /// 
    /// <code>
    /// private readonly IAzureComputerVisionService _azureVisionService;
    /// private readonly IGooglePlacesService _googlePlacesService;
    /// private readonly IClaudeApiService _claudeApiService;
    /// 
    /// public PhotoService(
    ///     // ... 現有參數 ...
    ///     IAzureComputerVisionService azureVisionService,
    ///     IGooglePlacesService googlePlacesService,
    ///     IClaudeApiService claudeApiService)
    /// {
    ///     // ... 現有初始化 ...
    ///     _azureVisionService = azureVisionService;
    ///     _googlePlacesService = googlePlacesService;
    ///     _claudeApiService = claudeApiService;
    /// }
    /// </code>
    /// 
    /// <para><b>職責劃分原則</b></para>
    /// 
    /// 這個 Service 層專注於：
    /// - 協調多個 AI 服務的調用順序
    /// - 實作業務邏輯（例如：何時跳過分析、如何生成標籤建議）
    /// - 錯誤處理和優雅降級（某個服務失敗不影響其他服務）
    /// - DTO 轉換（將底層服務的回應轉換為上層需要的格式）
    /// 
    /// 不負責：
    /// - 直接調用外部 API（由 AI Services 負責）
    /// - 資料庫操作（由 Repository 負責）
    /// - 檔案存取（由 BlobStorageService 負責）
    /// </summary>
    public partial class PhotoService : IPhotoService
    {
        #region AI 服務依賴注入欄位

        // ⚠️ 這些欄位需要在主 PhotoService.cs 的建構函數中注入和初始化
        // 這裡只是宣告，實際的值會在主建構函數中設定
        private readonly IAzureComputerVisionService _azureVisionService = null!;
        private readonly IGooglePlacesService _googlePlacesService = null!;
        private readonly IClaudeApiService _claudeApiService = null!;

        // 注意：_photoRepository, _blobStorageService, _logger 等欄位已在主檔案中定義
        // 這裡不需要重複宣告

        #endregion

        #region 完整 AI 分析

        /// <summary>
        /// 執行照片的完整 AI 分析（三階段流程）
        /// </summary>
        /// <remarks>
        /// 這是整個 AI 分析的核心方法，協調三個不同的 AI 服務來完成完整的照片分析。
        /// 
        /// <para><b>三階段流程</b></para>
        /// 
        /// 階段一：基礎分析（並行執行）
        /// - Azure Computer Vision：識別照片中的物體、場景、顏色等視覺元素
        /// - Google Places：如果照片有 GPS 資料，查詢附近的景點資訊
        /// 
        /// 這兩個服務可以並行執行因為它們互不依賴，這樣可以節省約百分之五十的時間。
        /// 
        /// 階段二：語義整合（依序執行）
        /// - Claude API：整合階段一的所有資訊，進行深度語義分析
        /// - Claude 會理解照片的深層含義，例如判斷這是否為知名景點
        /// - 生成更精確、更有用的標籤建議
        /// 
        /// 階段三：儲存結果
        /// - 將所有分析結果寫入資料庫（PhotoAIClassificationLog 表）
        /// - 生成標籤建議並儲存（PhotoAIClassificationSuggestion 表）
        /// 
        /// <para><b>優雅降級策略</b></para>
        /// 
        /// 如果某個服務失敗，不會導致整個分析失敗。例如：
        /// - Azure Vision 失敗：仍會執行 Google Places 和 Claude
        /// - Google Places 失敗（例如沒有 GPS）：Claude 基於 Azure Vision 的結果進行分析
        /// - Claude 失敗：至少有 Azure Vision 和 Google Places 的原始資料
        /// </remarks>
        public async Task<PhotoAIAnalysisResponseDto> AnalyzePhotoWithAIAsync(PhotoAIAnalysisRequestDto request)
        {
            var stopwatch = Stopwatch.StartNew();
            var analysisStartTime = DateTime.UtcNow;

            _logger.LogInformation("🧠 開始 AI 分析 PhotoId={PhotoId}, UserId={UserId}",
                request.PhotoId, request.UserId);

            try
            {
                // ==================== 步驟 1：驗證和準備 ====================

                var photo = await _photoRepository.GetPhotoByIdAsync(request.PhotoId);
                if (photo == null)
                {
                    _logger.LogWarning("❌ 照片不存在 PhotoId={PhotoId}", request.PhotoId);
                    return CreateErrorResponse(request.PhotoId, "照片不存在");
                }

                // 驗證照片所有權（安全性檢查）
                if (photo.UserId != request.UserId)
                {
                    _logger.LogWarning("❌ 無權限分析此照片 PhotoId={PhotoId}, UserId={UserId}",
                        request.PhotoId, request.UserId);
                    return CreateErrorResponse(request.PhotoId, "無權限分析此照片");
                }

                // 檢查是否已分析過（避免重複分析浪費成本）
                if (!request.ForceReanalysis)
                {
                    var hasAnalysis = await _photoRepository.HasAIAnalysisAsync(request.PhotoId);
                    if (hasAnalysis)
                    {
                        _logger.LogInformation("ℹ️ 照片已有分析記錄，返回現有結果 PhotoId={PhotoId}", request.PhotoId);

                        var existingAnalysis = await GetPhotoAIAnalysisAsync(request.PhotoId);
                        if (existingAnalysis != null)
                        {
                            return existingAnalysis;
                        }
                    }
                }

                // ==================== 步驟 2：階段一 - 基礎分析（並行執行） ====================

                _logger.LogInformation("📊 階段一：執行基礎分析（Azure Vision + Google Places）");

                // 使用 Task.WhenAll 並行執行兩個獨立的分析任務
                // 這樣可以將總時間從 (A + G) 減少到 max(A, G)
                var azureTask = request.EnableObjectDetection
                    ? AnalyzeWithAzureVisionAsync(request.PhotoId, request.UseThumbnail)
                    : Task.FromResult<AzureVisionAnalysisDto>(null!);

                var googleTask = request.EnableTouristSpotDetection && photo.PhotoMetadata.FirstOrDefault()?.Gpslatitude != null && photo.PhotoMetadata.FirstOrDefault()?.Gpslongitude != null
                    ? AnalyzeWithGooglePlacesAsync(request.PhotoId, request.PlaceSearchRadius)
                    : Task.FromResult<TouristSpotIdentificationDto?>(null);

                await Task.WhenAll(azureTask, googleTask);

                var azureResult = await azureTask;
                var googleResult = await googleTask;

                _logger.LogInformation("✅ 階段一完成 - Azure={AzureSuccess}, Google={GoogleSuccess}",
                    azureResult?.Success ?? false,
                    googleResult != null);

                // ==================== 步驟 3：階段二 - Claude 語義分析 ====================

                _logger.LogInformation("🤖 階段二：執行 Claude 語義分析");

                ClaudeAnalysisResultDto? claudeResult = null;

                //// 只有當有基礎資料時才調用 Claude（避免浪費 API 成本）
                //if (azureResult?.Success == true || googleResult != null)
                //{
                //    try
                //    {
                //        claudeResult = await AnalyzeWithClaudeAsync(request.PhotoId);
                //        _logger.LogInformation("✅ Claude 分析完成 Success={Success}", claudeResult.Success);
                //    }
                //    catch (Exception ex)
                //    {
                //        // Claude 失敗不影響整體流程（優雅降級）
                //        _logger.LogWarning(ex, "⚠️ Claude 分析失敗，但繼續處理其他結果");
                //    }
                //}
                //else
                //{
                //    _logger.LogWarning("⚠️ 跳過 Claude 分析（基礎分析無可用資料）");
                //}

                // Claude 可以直接看圖做語義分析；即使基礎分析失敗也應該嘗試，避免完全沒有標籤
                try
                {
                    claudeResult = await AnalyzeWithClaudeAsync(request.PhotoId);
                    _logger.LogInformation("✅ Claude 分析完成 Success={Success}", claudeResult.Success);
                }
                catch (Exception ex)
                {
                    // Claude 失敗不影響整體流程（優雅降級）
                    _logger.LogWarning(ex, "⚠️ Claude 分析失敗，但繼續處理其他結果");
                }

                // ==================== 步驟 4：階段三 - 儲存結果和生成建議 ====================

                _logger.LogInformation("💾 階段三：儲存分析結果並生成標籤建議");

                // 儲存完整的分析記錄到資料庫
                var logId = await SaveAnalysisResultsAsync(
                    request.PhotoId,
                    request.UserId,
                    azureResult,
                    googleResult,
                    claudeResult,
                    stopwatch.ElapsedMilliseconds
                );

                // 生成標籤建議
                var tagSuggestions = await GenerateTagSuggestionsAsync(
                    logId,
                    request.PhotoId,
                    azureResult,
                    googleResult,
                    claudeResult,
                    request.MinConfidenceScore
                );

                stopwatch.Stop();

                // ==================== 步驟 5：組裝回應 ====================

                var response = new PhotoAIAnalysisResponseDto
                {
                    LogId = logId,
                    PhotoId = request.PhotoId,
                    Status = AIConstants.Analysis.Status.Success,
                    AnalyzedAt = DateTimeOffset.UtcNow,
                    AzureVisionResult = MapToAzureSummary(azureResult),
                    GooglePlacesResult = MapToGooglePlacesSummary(googleResult),
                    ClaudeSemanticResult = MapToClaudeSummary(claudeResult),
                    TagSuggestions = tagSuggestions,
                    TotalProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds,
                    QuotaUsed = 1
                };

                _logger.LogInformation("✅ AI 分析完成 PhotoId={PhotoId}, Tags={TagCount}, Time={Time}ms",
                    request.PhotoId, tagSuggestions.Count, stopwatch.ElapsedMilliseconds);

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "💥 AI 分析失敗 PhotoId={PhotoId}", request.PhotoId);

                return new PhotoAIAnalysisResponseDto
                {
                    PhotoId = request.PhotoId,
                    Status = AIConstants.Analysis.Status.Failed,
                    AnalyzedAt = DateTimeOffset.UtcNow,
                    ErrorMessage = ex.Message,
                    Errors = new List<string> { ex.Message },
                    TotalProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds
                };
            }
        }

        #endregion

        #region 階段一：基礎分析服務

        /// <summary>
        /// 使用 Azure Computer Vision 分析照片
        /// </summary>
        /// <remarks>
        /// 這個方法負責準備照片資料並調用 Azure Vision Service。
        /// 
        /// <para><b>智慧照片版本選擇</b></para>
        /// 
        /// Azure Vision API 有 4MB 的檔案大小限制。為了在保證分析品質的同時節省成本，
        /// 這個方法實作了一個三層智慧選擇策略：
        /// 
        /// 1. 使用者明確要求使用縮圖 → 直接使用縮圖
        /// 2. 使用者要求使用原圖 → 檢查原圖大小：
        ///    - 原圖 ≤ 4MB → 使用原圖（最佳品質）
        ///    - 原圖 > 4MB → 自動切換縮圖（優雅降級）
        /// 3. 縮圖不存在 → 視情況拋出錯誤或降級使用原圖
        /// 
        /// 這種設計讓系統能夠在保證功能可用性的同時，盡可能提供最佳的分析品質。
        /// 
        /// <para><b>為什麼記錄使用的照片大小？</b></para>
        /// 
        /// 記錄原圖大小和實際用於分析的照片大小有幾個重要用途：
        /// 1. 成本追蹤：了解有多少照片因為太大而使用了縮圖
        /// 2. 品質評估：原圖分析和縮圖分析的準確度可能有差異
        /// 3. 系統優化：如果大部分照片都超過 4MB，可能需要調整縮圖策略
        /// </remarks>
        public async Task<AzureVisionAnalysisDto> AnalyzeWithAzureVisionAsync(long photoId, bool useThumbnail)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("📸 開始 Azure Vision 分析 PhotoId={PhotoId}, UseThumbnail={UseThumbnail}",
                    photoId, useThumbnail);

                // ==================== 步驟 0：取得照片實體 ====================
                // 🆕 優先從資料庫讀取，如果沒有再從 Blob Storage 讀取

                var photo = await _photoRepository.GetPhotoByIdAsync(photoId);
                if (photo == null)
                {
                    throw new InvalidOperationException($"照片不存在 PhotoId={photoId}");
                }

                // ==================== 🆕 取得 Blob Storage 實際儲存路徑 ====================
                // 這裡從 PhotoStorage 讀取正確的 StoragePath
                var storages = await _photoRepository.GetAllStoragesByPhotoIdAsync(photoId);
                var primaryStoragePath = storages.FirstOrDefault(s => s.IsPrimary)?.StoragePath;
                var thumbnailStoragePath = storages.FirstOrDefault(s => !s.IsPrimary)?.StoragePath;
                if (string.IsNullOrWhiteSpace(primaryStoragePath))
                {
                    _logger.LogWarning("⚠️ 找不到原圖 StoragePath（PhotoStorage），PhotoId={PhotoId}，將回退使用 photoId 作為 blobPath", photoId);
                }

                if (string.IsNullOrWhiteSpace(thumbnailStoragePath))
                {
                    _logger.LogWarning("⚠️ 找不到縮圖 StoragePath（PhotoStorage），PhotoId={PhotoId}，將回退使用 photoId 作為 blobPath", photoId);
                }

                // ==================== 步驟 1：智慧照片版本選擇 ====================
                // 這是關鍵邏輯，確保我們發送給 Azure 的照片不會超過 4MB 限制

                byte[] imageData;
                bool actuallyUsedThumbnail = false;
                double originalSizeMB = 0;
                double analyzedSizeMB = 0;

                if (useThumbnail)
                {
                    // 使用者要求使用縮圖
                    // 🆕 優先使用資料庫中的縮圖資料
                    if (photo.ThumbnailData != null && photo.ThumbnailData.Length > 0)
                    {
                        _logger.LogInformation("📦 使用資料庫中的縮圖資料 PhotoId={PhotoId}, Size={Size}KB",
                            photoId, photo.ThumbnailData.Length / 1024);
                        imageData = photo.ThumbnailData;
                        actuallyUsedThumbnail = true;
                    }
                    else
                    {
                        // 資料庫沒有縮圖，嘗試從 Blob Storage 下載
                        try
                        {
                            _logger.LogInformation("☁️ 資料庫無縮圖，嘗試從 Blob Storage 下載 PhotoId={PhotoId}", photoId);
                            var thumbnailStream = await _blobStorageService.DownloadThumbnailAsync(thumbnailStoragePath ?? photoId.ToString());
                            if (thumbnailStream == null || thumbnailStream.Length == 0)
                            {
                                throw new InvalidOperationException("縮圖串流為空");
                            }

                            imageData = await StreamToByteArrayAsync(thumbnailStream);
                            actuallyUsedThumbnail = true;
                        }
                        catch (Exception ex)
                        {
                            // Blob Storage 也沒有縮圖，降級使用原圖
                            _logger.LogWarning("⚠️ 縮圖不存在（資料庫和 Blob Storage 都沒有），改用原圖 PhotoId={PhotoId}, Error={Error}",
                                photoId, ex.Message);

                            // 嘗試使用資料庫中的原圖
                            if (photo.PhotoData != null && photo.PhotoData.Length > 0)
                            {
                                _logger.LogInformation("📦 使用資料庫中的原圖資料 PhotoId={PhotoId}, Size={Size}KB",
                                    photoId, photo.PhotoData.Length / 1024);
                                imageData = photo.PhotoData;
                                actuallyUsedThumbnail = false;
                            }
                            else
                            {
                                // 最後嘗試從 Blob Storage 下載原圖
                                var originalStream = await _blobStorageService.DownloadPhotoAsync(photoId.ToString());
                                if (originalStream == null || originalStream.Length == 0)
                                {
                                    throw new InvalidOperationException("照片資料不存在（資料庫和 Blob Storage 都沒有）");
                                }

                                imageData = await StreamToByteArrayAsync(originalStream);
                                actuallyUsedThumbnail = false;
                            }
                        }
                    }
                }
                else
                {
                    // 使用者要求使用原圖，但我們需要檢查大小
                    // 🆕 優先使用資料庫中的原圖資料
                    if (photo.PhotoData != null && photo.PhotoData.Length > 0)
                    {
                        _logger.LogInformation("📦 使用資料庫中的原圖資料 PhotoId={PhotoId}, Size={Size}KB",
                            photoId, photo.PhotoData.Length / 1024);
                        imageData = photo.PhotoData;
                        actuallyUsedThumbnail = false;
                    }
                    else
                    {
                        // 資料庫沒有原圖，從 Blob Storage 下載
                        _logger.LogInformation("☁️ 資料庫無原圖，從 Blob Storage 下載 PhotoId={PhotoId}", photoId);
                        var originalStream = await _blobStorageService.DownloadPhotoAsync(primaryStoragePath ?? photoId.ToString());
                        if (originalStream == null || originalStream.Length == 0)
                        {
                            throw new InvalidOperationException("照片資料不存在（資料庫和 Blob Storage 都沒有）");
                        }

                        imageData = await StreamToByteArrayAsync(originalStream);
                        actuallyUsedThumbnail = false;
                    }
                }

                originalSizeMB = photo.PhotoData?.Length / 1024.0 / 1024.0 ?? 0;
                analyzedSizeMB = imageData.Length / (1024.0 * 1024.0);

                _logger.LogInformation("📏 照片版本選擇 - 原圖={Original:F2}MB, 分析用={Analyzed:F2}MB, 使用縮圖={UsedThumbnail}",
                    originalSizeMB > 0 ? originalSizeMB : analyzedSizeMB,
                    analyzedSizeMB,
                    actuallyUsedThumbnail);

                // ==================== 步驟 2：調用 Azure Vision Service ====================
                // Service 層的職責：協調，不直接調用 API
                // 實際的 API 調用由 AzureComputerVisionService 負責

                var features = new List<string> { "Objects", "Tags", "Color", "Adult", "Description", "Categories" };
                var result = await _azureVisionService.AnalyzeImageAsync(imageData, features);

                stopwatch.Stop();

                // ==================== 步驟 3：補充檔案大小資訊 ====================
                // 這些資訊對於成本分析和系統優化很重要

                result.UsedThumbnail = actuallyUsedThumbnail;
                result.OriginalImageSizeMB = originalSizeMB > 0 ? originalSizeMB : analyzedSizeMB;
                result.AnalyzedImageSizeMB = analyzedSizeMB;
                result.ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds;

                _logger.LogInformation("✅ Azure Vision 分析完成 PhotoId={PhotoId}, Success={Success}, Objects={Objects}, Tags={Tags}, Time={Time}ms",
                    photoId,
                    result.Success,
                    result.Objects?.Count ?? 0,
                    result.Tags?.Count ?? 0,
                    stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "💥 Azure Vision 分析失敗 PhotoId={PhotoId}", photoId);

                // 返回失敗結果而非拋出例外（優雅降級）
                return new AzureVisionAnalysisDto
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds
                };
            }
        }

        /// <summary>
        /// 使用 Google Places 識別景點
        /// </summary>
        /// <remarks>
        /// 這個方法負責從照片的 GPS 資料查詢附近的景點。
        /// 
        /// <para><b>工作流程</b></para>
        /// 
        /// 1. 從資料庫取得照片的 GPS 座標（EXIF 資料）
        /// 2. 如果有 GPS 資料，調用 Google Places Service 查詢附近景點
        /// 3. Service 層會自動判斷是否為知名景點
        /// 
        /// <para><b>GPS 資料的重要性</b></para>
        /// 
        /// GPS 是景點識別的關鍵證據。沒有 GPS 的照片無法使用這個功能，
        /// 只能依靠 Azure Vision 和 Claude 的視覺分析來推測可能的地點。
        /// 
        /// 這也是為什麼鼓勵使用者保留照片 EXIF 資料的原因之一。
        /// </remarks>
        public async Task<TouristSpotIdentificationDto?> AnalyzeWithGooglePlacesAsync(long photoId, int searchRadius)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("🗺️ 開始 Google Places 分析 PhotoId={PhotoId}, Radius={Radius}m",
                    photoId, searchRadius);

                // ==================== 步驟 1：取得照片的 GPS 資料 ====================

                var photo = await _photoRepository.GetPhotoByIdAsync(photoId);
                if (photo == null)
                {
                    throw new InvalidOperationException("照片不存在");
                }

                var metadata = photo.PhotoMetadata.FirstOrDefault();
                if (metadata?.Gpslatitude == null || metadata?.Gpslongitude == null)
                {
                    _logger.LogInformation("ℹ️ 照片沒有 GPS 資料，跳過 Google Places 分析 PhotoId={PhotoId}", photoId);
                    return null;
                }

                // ==================== 步驟 2：調用 Google Places Service ====================

                var result = await _googlePlacesService.IdentifyTouristSpotAsync(
                    (double)metadata.Gpslatitude,
                    (double)metadata.Gpslongitude,
                    searchRadius
                );

                stopwatch.Stop();

                if (result.IsTouristSpot)
                {
                    _logger.LogInformation("✅ 識別為景點 PhotoId={PhotoId}, SpotName={SpotName}, Confidence={Confidence:F2}, Time={Time}ms",
                        photoId, result.SpotName, result.Confidence, stopwatch.ElapsedMilliseconds);
                }
                else
                {
                    _logger.LogInformation("ℹ️ 不是知名景點 PhotoId={PhotoId}, Time={Time}ms",
                        photoId, stopwatch.ElapsedMilliseconds);
                }

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "💥 Google Places 分析失敗 PhotoId={PhotoId}", photoId);

                // 返回 null 而非拋出例外（優雅降級）
                return null;
            }
        }

        #endregion

        #region 階段二：Claude 語義分析

        /// <summary>
        /// 使用 Claude API 進行語義分析
        /// </summary>
        /// <remarks>
        /// Claude 是整個分析流程的「大腦」，負責整合所有資訊並生成有意義的結果。
        /// 
        /// <para><b>Claude 的角色定位</b></para>
        /// 
        /// Azure Vision 告訴我們「照片裡有什麼」（物體、顏色、場景）
        /// Google Places 告訴我們「照片在哪裡拍的」（GPS 附近的景點）
        /// Claude 告訴我們「照片的意義是什麼」（這張照片的故事、應該加什麼標籤）
        /// 
        /// <para><b>為什麼要整合所有資訊？</b></para>
        /// 
        /// 舉例說明 Claude 的價值：
        /// - Azure Vision 說：「建築物、天空、紅色」
        /// - Google Places 說：「淺草寺，評分 4.5，距離 10 公尺」
        /// - Claude 整合後說：「這是在東京淺草寺拍攝的傳統建築照片」
        ///   建議標籤：日本、東京、淺草寺、寺廟、建築、文化
        /// 
        /// <para><b>成本控制策略</b></para>
        /// 
        /// Claude API 按 token 計費，因此我們：
        /// 1. 使用照片縮圖而非原圖（降低 token 消耗）
        /// 2. 只在有基礎資料時才調用（避免無效請求）
        /// 3. 設定合理的 max_tokens 限制
        /// 4. 使用較低的 temperature（減少不必要的創意輸出）
        /// </remarks>
        public async Task<ClaudeAnalysisResultDto> AnalyzeWithClaudeAsync(long photoId)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("🤖 開始 Claude 語義分析 PhotoId={PhotoId}", photoId);

                // ==================== 步驟 1：取得照片基本資訊 ====================

                var photo = await _photoRepository.GetPhotoByIdAsync(photoId);
                if (photo == null)
                {
                    throw new InvalidOperationException("照片不存在");
                }

                // ==================== 🆕 取得縮圖 StoragePath（避免用 photoId 當 blob key） ====================
                var storages = await _photoRepository.GetAllStoragesByPhotoIdAsync(photoId);
                var thumbnailStoragePath = storages.FirstOrDefault(s => !s.IsPrimary)?.StoragePath;
                
                if (string.IsNullOrWhiteSpace(thumbnailStoragePath))
                {
                    _logger.LogWarning("⚠️ 找不到縮圖 StoragePath（PhotoStorage），PhotoId={PhotoId}，將回退使用 photoId 作為 blobPath", photoId);
                }

                // ==================== 步驟 2：準備照片縮圖（Claude 的「眼睛」） ====================

                var thumbnailStream = await _blobStorageService.DownloadThumbnailAsync(thumbnailStoragePath ?? photoId.ToString());
                string? thumbnailBase64 = null;

                if (thumbnailStream != null && thumbnailStream.Length > 0)
                {
                    // 將縮圖轉換為 Base64 編碼，讓 Claude 能夠「看到」照片
                    var thumbnailBytes = await StreamToByteArrayAsync(thumbnailStream);
                    thumbnailBase64 = Convert.ToBase64String(thumbnailBytes);
                }

                // ==================== 步驟 3：收集 Azure Vision 的分析結果 ====================

                var azureLog = await _photoRepository.GetLatestAILogByModelAsync(photoId, AIConstants.Analysis.Provider.Azure);
                AzureVisionAnalysisDto? azureResult = null;

                if (!string.IsNullOrEmpty(azureLog?.AzureVisionResponse))
                {
                    try
                    {
                        // 從資料庫的 JSON 字串反序列化回 DTO
                        azureResult = JsonSerializer.Deserialize<AzureVisionAnalysisDto>(azureLog.AzureVisionResponse);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "⚠️ 無法解析 Azure Vision 結果");
                    }
                }

                // ==================== 步驟 4：收集 Google Places 的分析結果 ====================

                var googleLog = await _photoRepository.GetLatestAILogByModelAsync(photoId, AIConstants.Analysis.Provider.Google);
                List<PlaceResult> googlePlaces = new();

                if (!string.IsNullOrEmpty(googleLog?.GooglePlacesResponse))
                {
                    try
                    {
                        // 從資料庫的 JSON 字串反序列化回 DTO
                        var googleResponse = JsonSerializer.Deserialize<GooglePlacesResponseDto>(googleLog.GooglePlacesResponse);
                        if (googleResponse?.Results != null)
                        {
                            googlePlaces = googleResponse.Results;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "⚠️ 無法解析 Google Places 結果");
                    }
                }

                // ==================== 步驟 5：組裝 Claude 分析請求 ====================
                // 這裡組裝所有可用的資訊，讓 Claude 有足夠的上下文進行分析

                var request = new PhotoAnalysisContextDto
                {
                    PhotoId = photoId,
                    ThumbnailBase64 = thumbnailBase64,
                    ThumbnailMediaType = "image/jpeg",

                    // EXIF 資料（拍攝時間、地點、相機等）
                    Exif = new ExifContextDto
                    {
                        DateTaken = photo.PhotoMetadata.FirstOrDefault()?.DateTaken,
                        Latitude = photo.PhotoMetadata.FirstOrDefault()?.Gpslatitude,
                        Longitude = photo.PhotoMetadata.FirstOrDefault()?.Gpslongitude,
                        CameraModel = photo.PhotoMetadata.FirstOrDefault()?.CameraModel != null && photo.PhotoMetadata.FirstOrDefault()?.CameraMake != null
                            ? $"{photo.PhotoMetadata.FirstOrDefault()?.CameraMake} {photo.PhotoMetadata.FirstOrDefault()?.CameraModel}"
                            : null
                    },

                    // 直接傳遞完整的 Azure Vision 分析結果
                    // ClaudeApiService 會自行決定要使用哪些欄位
                    AzureVision = ConvertToAzureVisionContext(azureResult),

                    // 直接傳遞 Google Places 的原始資料
                    GooglePlaces = ConvertToGooglePlacesContext(googlePlaces),

                    // 分析選項
                    Options = new AnalysisOptionsDto
                    {
                        IncludeHistoricalContext = false,  // 不需要歷史背景（節省 token）
                        Temperature = 0.2,  // 低溫度 = 更一致的輸出
                        MaxTokens = 2048
                    }
                };

                // ==================== 步驟 6：調用 Claude API Service ====================
                // Service 層的職責：協調，不直接調用 API

                var result = await _claudeApiService.AnalyzeSinglePhotoAsync(request);

                stopwatch.Stop();
                result.ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds;

                if (result.Success)
                {
                    _logger.LogInformation("✅ Claude 分析成功 PhotoId={PhotoId}, IsTouristSpot={IsTouristSpot}, Tags={Tags}, Time={Time}ms",
                        photoId,
                        result.SemanticOutput?.IsTouristSpot ?? false,
                        result.SemanticOutput?.SuggestedTags.Count ?? 0,
                        stopwatch.ElapsedMilliseconds);
                }
                else
                {
                    _logger.LogWarning("⚠️ Claude 分析失敗 PhotoId={PhotoId}, Error={Error}, Time={Time}ms",
                        photoId, result.ErrorMessage, stopwatch.ElapsedMilliseconds);
                }

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "💥 Claude 分析發生例外 PhotoId={PhotoId}", photoId);

                // 返回失敗結果
                return new ClaudeAnalysisResultDto
                {
                    PhotoId = photoId,
                    Success = false,
                    ErrorMessage = ex.Message,
                    ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds
                };
            }
        }

        #endregion

        #region 階段三：儲存結果

        /// <summary>
        /// 儲存完整的 AI 分析結果到資料庫
        /// </summary>
        /// <remarks>
        /// 這個方法將所有分析結果寫入 PhotoAIClassificationLog 表。
        /// 
        /// <para><b>為什麼要儲存完整的原始資料？</b></para>
        /// 
        /// 1. 除錯和追溯：當出現問題時，可以查看當時的完整分析結果
        /// 2. 資料分析：可以統計哪些類型的照片分析效果較好
        /// 3. API 升級：未來 API 升級時，可以重新解析舊資料
        /// 4. 成本追蹤：記錄每次 API 調用的成本和效能
        /// 
        /// 原始 JSON 資料會以字串形式儲存在 NVARCHAR(MAX) 欄位中，
        /// 這樣既保留了彈性，又不會影響查詢效能（不會直接查詢 JSON 內容）。
        /// </remarks>
        private async Task<long> SaveAnalysisResultsAsync(
            long photoId,
            long userId,
            AzureVisionAnalysisDto? azureResult,
            TouristSpotIdentificationDto? googleResult,
            ClaudeAnalysisResultDto? claudeResult,
            long processingTimeMs)
        {
            try
            {
                _logger.LogInformation("💾 儲存分析結果 PhotoId={PhotoId}", photoId);

                var log = new PhotoAiclassificationLog
                {
                    PhotoId = photoId,
                    UserId = userId,
                    Aimodel = "MultiProvider",
                    AnalyzedAt = DateTime.UtcNow,
                    ProcessingTimeMs = (int)processingTimeMs,
                    Status = (azureResult?.Success == true || googleResult != null || claudeResult?.Success == true)
        ? AIConstants.Analysis.Status.Success
        : AIConstants.Analysis.Status.Failed,
                    QuotaUsed = 1,

                    // 各服務原始回應（序列化後存入 DB）
                    AzureVisionResponse = azureResult != null ? JsonSerializer.Serialize(azureResult) : null,
                    GooglePlacesResponse = googleResult != null ? JsonSerializer.Serialize(googleResult) : null,
                    ClaudeResponse = claudeResult != null ? JsonSerializer.Serialize(claudeResult) : null,

                    // 錯誤資訊（如果有）
                    ErrorMessage = claudeResult?.ErrorMessage ?? azureResult?.ErrorMessage
                };


                var savedLog = await _photoRepository.AddAILogAsync(log);

                _logger.LogInformation("✅ 分析結果已儲存 LogId={LogId}, PhotoId={PhotoId}",
                    savedLog.LogId, photoId);

                return savedLog.LogId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 儲存分析結果失敗 PhotoId={PhotoId}", photoId);
                throw;
            }
        }

        #endregion

        #region 標籤建議生成

        /// <summary>
        /// 從 AI 分析結果生成標籤建議
        /// </summary>
        /// <remarks>
        /// 這是一個關鍵的業務邏輯方法，負責從三個不同的 AI 服務結果中
        /// 提取、整合、去重並排序標籤建議。
        /// 
        /// <para><b>標籤來源優先級</b></para>
        /// 
        /// 優先級從低到高：
        /// 1. Azure Vision：最基礎的視覺標籤（例如：建築、天空、戶外）
        /// 2. Google Places：地點相關標籤（例如：淺草寺、東京）
        /// 3. Claude：最精確的語義標籤（例如：日本傳統建築、文化遺產）
        /// 
        /// 當同一個標籤出現在多個來源時，使用優先級最高的那個版本。
        /// 
        /// <para><b>為什麼 Claude 優先級最高？</b></para>
        /// 
        /// Claude 看到了所有資訊（照片、Azure 結果、Google 結果）後做出的判斷，
        /// 因此它的標籤通常最準確、最有用。例如：
        /// - Azure 可能說「紅色物體」
        /// - Google 說「淺草寺」
        /// - Claude 整合後說「淺草寺雷門」（更具體）
        /// 
        /// <para><b>信心分數過濾</b></para>
        /// 
        /// 只有信心分數高於閾值的標籤才會被建議給使用者。
        /// 這避免了低品質標籤的干擾，提高使用者體驗。
        /// </remarks>
        private async Task<List<AITagSuggestionDto>> GenerateTagSuggestionsAsync(
            long logId,
            long photoId,
            AzureVisionAnalysisDto? azureResult,
            TouristSpotIdentificationDto? googleResult,
            ClaudeAnalysisResultDto? claudeResult,
            double minConfidenceScore)
        {
            try
            {
                _logger.LogInformation("💡 生成 AI 標籤建議 PhotoId={PhotoId}, MinConfidence={MinConfidence}",
                    photoId, minConfidenceScore);

                var suggestions = new List<PhotoAiclassificationSuggestion>();

                // ==================== 1. 從 Azure Vision 生成建議（最低優先級）====================

                if (azureResult?.Success == true)
                {
                    // 從物件識別生成標籤
                    foreach (var obj in azureResult.Objects.Where(o => o.Confidence >= minConfidenceScore))
                    {
                        suggestions.Add(new PhotoAiclassificationSuggestion
                        {
                            LogId = logId,
                            CategoryId = DetermineCategoryId(obj.Name, AIConstants.Analysis.Provider.Azure),
                            PhotoId = photoId,
                            TagName = obj.Name,
                            ConfidenceScore = (decimal)obj.Confidence,
                            Source = AIConstants.Analysis.Provider.Azure,
                            IsAdopted = false,
                            CreatedAt = DateTime.UtcNow
                        });
                    }

                    // 從標籤生成建議
                    foreach (var tag in azureResult.Tags.Where(t => t.Confidence >= minConfidenceScore))
                    {
                        // 檢查是否已經有相同的標籤（避免重複）
                        if (!suggestions.Any(s => s.TagName == tag.Name))
                        {
                            suggestions.Add(new PhotoAiclassificationSuggestion
                            {
                                LogId = logId,
                                CategoryId = DetermineCategoryId(tag.Name, AIConstants.Analysis.Provider.Azure),
                                PhotoId = photoId,
                                TagName = tag.Name,
                                ConfidenceScore = (decimal)tag.Confidence,
                                Source = AIConstants.Analysis.Provider.Azure,
                                IsAdopted = false,
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }
                }

                // ==================== 2. 從 Google Places 生成建議（中等優先級）====================

                if (googleResult?.IsTouristSpot == true)
                {
                    // 景點名稱
                    if (!string.IsNullOrEmpty(googleResult.SpotName))
                    {
                        var existingSuggestion = suggestions.FirstOrDefault(s => s.TagName == googleResult.SpotName);

                        if (existingSuggestion != null)
                        {
                            // 標籤已存在，用 Google 的版本覆蓋（Google 優先級高於 Azure）
                            existingSuggestion.Source = AIConstants.Analysis.Provider.Google;
                            existingSuggestion.ConfidenceScore = (decimal)googleResult.Confidence;
                        }
                        else
                        {
                            // 新增 Google 的標籤
                            suggestions.Add(new PhotoAiclassificationSuggestion
                            {
                                LogId = logId,
                                CategoryId = DetermineCategoryId(googleResult.SpotName, AIConstants.Analysis.Provider.Google, isSpotName: true),
                                PhotoId = photoId,
                                TagName = googleResult.SpotName,
                                ConfidenceScore = (decimal)googleResult.Confidence,
                                Source = AIConstants.Analysis.Provider.Google,
                                IsAdopted = false,
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }

                    // 從景點類型生成標籤
                    foreach (var type in googleResult.SpotTypes)
                    {
                        // 將 Google Places 的類型轉換為更友善的標籤名稱
                        // 例如：tourist_attraction → 旅遊景點
                        var friendlyTagName = ConvertPlaceTypeToFriendlyName(type);

                        var existingSuggestion = suggestions.FirstOrDefault(s => s.TagName == friendlyTagName);

                        if (existingSuggestion != null)
                        {
                            existingSuggestion.Source = AIConstants.Analysis.Provider.Google;
                            existingSuggestion.ConfidenceScore = (decimal)googleResult.Confidence;
                        }
                        else
                        {
                            suggestions.Add(new PhotoAiclassificationSuggestion
                            {
                                LogId = logId,
                                CategoryId = DetermineCategoryId(friendlyTagName, AIConstants.Analysis.Provider.Google),
                                PhotoId = photoId,
                                TagName = friendlyTagName,
                                ConfidenceScore = (decimal)googleResult.Confidence,
                                Source = AIConstants.Analysis.Provider.Google,
                                IsAdopted = false,
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }
                }

                // ==================== 3. 從 Claude 生成建議（最高優先級）====================

                if (claudeResult?.Success == true && claudeResult.SemanticOutput != null)
                {
                    var semantic = claudeResult.SemanticOutput;

                    // Claude 的標籤建議通常更精確且具有層次性
                    foreach (var tag in semantic.SuggestedTags)
                    {
                        var existingSuggestion = suggestions.FirstOrDefault(s => s.TagName == tag);

                        if (existingSuggestion != null)
                        {
                            // 標籤已存在，用 Claude 的版本覆蓋（Claude 優先級最高）
                            existingSuggestion.Source = AIConstants.Analysis.Provider.Claude;
                            existingSuggestion.ConfidenceScore = (decimal)semantic.Confidence;
                        }
                        else
                        {
                            // 新增 Claude 的標籤
                            suggestions.Add(new PhotoAiclassificationSuggestion
                            {
                                LogId = logId,
                                CategoryId = DetermineCategoryId(tag, AIConstants.Analysis.Provider.Claude),
                                PhotoId = photoId,
                                TagName = tag,
                                ConfidenceScore = (decimal)semantic.Confidence,
                                Source = AIConstants.Analysis.Provider.Claude,
                                IsAdopted = false,
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }

                    // 如果 Claude 確認是景點，添加景點名稱
                    if (semantic.IsTouristSpot && !string.IsNullOrEmpty(semantic.SpotName))
                    {
                        var existingSpot = suggestions.FirstOrDefault(s => s.TagName == semantic.SpotName);
                        if (existingSpot != null)
                        {
                            existingSpot.Source = AIConstants.Analysis.Provider.Claude;
                            existingSpot.ConfidenceScore = (decimal)semantic.Confidence;
                        }
                        else
                        {
                            suggestions.Add(new PhotoAiclassificationSuggestion
                            {
                                LogId = logId,
                                CategoryId = DetermineCategoryId(semantic.SpotName, AIConstants.Analysis.Provider.Claude, isSpotName: true),
                                PhotoId = photoId,
                                TagName = semantic.SpotName,
                                ConfidenceScore = (decimal)semantic.Confidence,
                                Source = AIConstants.Analysis.Provider.Claude,
                                IsAdopted = false,
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }
                }

                // ==================== 4. 批次儲存建議到資料庫 ====================

                var persistMin = (decimal)AIConstants.Analysis.ConfidenceThreshold.High; // 0.95

                var suggestionsToSave = suggestions
                    .Where(s => s.ConfidenceScore >= persistMin)
                    .ToList();

                _logger.LogInformation(
                    "🧹 入庫前信心過濾：原始 {OrigCount} 筆 -> 95%+ {FilteredCount} 筆 (Threshold={Threshold})",
                    suggestions.Count, suggestionsToSave.Count, persistMin);


                if (suggestionsToSave.Any())
                {
                    var savedSuggestions = await _photoRepository.AddAISuggestionsAsync(suggestionsToSave);

                    _logger.LogInformation("💡 生成 {Count} 個 AI 標籤建議", savedSuggestions.Count);

                    return savedSuggestions.Select(s => new AITagSuggestionDto
                    {
                        SuggestionId = s.SuggestionId,
                        LogId = s.LogId,
                        CategoryId = s.CategoryId,
                        CategoryName = s.CategoryName ?? "",
                        CategoryType = s.CategoryType ?? "",
                        TagId = s.TagId,
                        TagName = s.TagName ?? "",
                        Confidence = (double)s.ConfidenceScore,
                        IsAdopted = s.IsAdopted,
                        Source = s.Source ?? "",
                        CreatedAt = s.CreatedAt
                    }).ToList();
                }

                _logger.LogInformation("🧹 無 95% 以上的建議，略過入庫 PhotoId={PhotoId}", photoId);
                return new List<AITagSuggestionDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 生成 AI 標籤建議失敗 PhotoId={PhotoId}", photoId);
                return new List<AITagSuggestionDto>();
            }
        }

        /// <summary>
        /// 將 Google Places 的類型代碼轉換為友善的中文標籤名稱
        /// </summary>
        private string ConvertPlaceTypeToFriendlyName(string placeType)
        {
            // 這裡可以建立一個完整的對照表
            // 目前先提供一些常見的轉換
            return placeType switch
            {
                "tourist_attraction" => "旅遊景點",
                "museum" => "博物館",
                "park" => "公園",
                "restaurant" => "餐廳",
                "cafe" => "咖啡廳",
                "shopping_mall" => "購物中心",
                "store" => "商店",
                "point_of_interest" => "興趣點",
                "establishment" => "場所",
                _ => placeType  // 找不到對應就保留原始名稱
            };
        }

        /// <summary>
        /// 根據標籤來源和內容判斷應分配的 CategoryId
        /// </summary>
        /// <remarks>
        /// 這是暫時的寫死分類邏輯，依據以下規則：
        /// 
        /// <para><b>分類規則</b></para>
        /// 
        /// 1. **來自 Google Places 的標籤** → CategoryId 4 (LOCATION)
        ///    - 景點名稱、地點類型標籤
        /// 
        /// 2. **場景相關標籤** → CategoryId 5 (SCENE)
        ///    - 包含場景關鍵字（建築、天空、戶外等）
        ///    - 參考 AIConstants.Tags.CategoryId.SceneKeywords
        /// 
        /// 3. **其他 AI 標籤** → CategoryId 10 (AI)
        ///    - 無法明確分類的標籤
        /// 
        /// <para><b>未來改進方向</b></para>
        /// 
        /// - 使用 NLP 技術進行語義分析
        /// - 建立機器學習模型自動判斷分類
        /// - 支援多分類（一個標籤可屬於多個 Category）
        /// </remarks>
        /// <param name="tagName">標籤名稱</param>
        /// <param name="source">標籤來源 (Azure/Google/Claude)</param>
        /// <param name="isSpotName">是否為景點名稱</param>
        /// <returns>對應的 CategoryId</returns>
        private int DetermineCategoryId(string tagName, string source, bool isSpotName = false)
        {
            // 規則 1：來自 Google Places 的標籤 → LOCATION
            if (source == AIConstants.Analysis.Provider.Google)
            {
                return AIConstants.Tags.CategoryId.Location;
            }

            // 規則 2：Claude 確認的景點名稱 → LOCATION
            if (isSpotName && source == AIConstants.Analysis.Provider.Claude)
            {
                return AIConstants.Tags.CategoryId.Location;
            }

            // 規則 3：包含場景關鍵字的標籤 → SCENE
            // 使用 LINQ Any() 來檢查標籤名稱是否包含任何場景關鍵字（不區分大小寫）
            if (AIConstants.Tags.CategoryId.SceneKeywords.Any(keyword =>
                tagName.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            {
                return AIConstants.Tags.CategoryId.Scene;
            }

            // 規則 4：其他標籤 → AI
            return AIConstants.Tags.CategoryId.AI;
        }

        #endregion

        #region 私有輔助方法 - DTO 轉換

        /// <summary>
        /// 建立錯誤回應
        /// </summary>
        private PhotoAIAnalysisResponseDto CreateErrorResponse(long photoId, string errorMessage)
        {
            return new PhotoAIAnalysisResponseDto
            {
                PhotoId = photoId,
                Status = AIConstants.Analysis.Status.Failed,
                AnalyzedAt = DateTime.UtcNow,
                ErrorMessage = errorMessage,
                Errors = new List<string> { errorMessage }
            };
        }

        /// <summary>
        /// 將 Azure Vision 結果轉換為摘要 DTO
        /// </summary>
        /// <remarks>
        /// 摘要 DTO 只包含最重要的資訊，減少資料傳輸量。
        /// 完整的原始資料已經儲存在資料庫中，需要時可以查詢。
        /// </remarks>
        private AzureVisionSummaryDto? MapToAzureSummary(AzureVisionAnalysisDto? azureResult)
        {
            if (azureResult == null)
                return null;

            return new AzureVisionSummaryDto
            {
                Success = azureResult.Success,
                ObjectCount = azureResult.Objects.Count,
                TagCount = azureResult.Tags.Count,
                TopObjects = azureResult.Objects
                    .OrderByDescending(o => o.Confidence)
                    .Take(5)
                    .Select(o => o.Name)
                    .ToList(),
                TopTags = azureResult.Tags
                    .OrderByDescending(t => t.Confidence)
                    .Take(10)
                    .Select(t => t.Name)
                    .ToList(),
                Description = azureResult.Description?.Captions.FirstOrDefault()?.Text,
                HasAdultContent = azureResult.Adult?.IsAdultContent ?? false,
                ProcessingTimeMs = azureResult.ProcessingTimeMs,
                ErrorMessage = azureResult.ErrorMessage
            };
        }

        /// <summary>
        /// 將 Google Places 結果轉換為摘要 DTO
        /// </summary>
        private GooglePlacesSummaryDto? MapToGooglePlacesSummary(TouristSpotIdentificationDto? googleResult)
        {
            if (googleResult == null)
                return null;

            return new GooglePlacesSummaryDto
            {
                Success = true,
                PlaceCount = 1,
                NearestPlaceName = googleResult.SpotName,
                NearestPlaceDistance = googleResult.DistanceMeters,
                NearbyPlaces = new List<string> { googleResult.SpotName ?? "" }
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList(),
                ProcessingTimeMs = 0  // Google Places Service 沒有追蹤處理時間
            };
        }

        /// <summary>
        /// 將 Claude 結果轉換為摘要 DTO
        /// </summary>
        private ClaudeSemanticSummaryDto? MapToClaudeSummary(ClaudeAnalysisResultDto? claudeResult)
        {
            if (claudeResult == null)
                return null;

            var semantic = claudeResult.SemanticOutput;

            return new ClaudeSemanticSummaryDto
            {
                Success = claudeResult.Success,
                IsTouristSpot = semantic?.IsTouristSpot ?? false,
                SpotName = semantic?.SpotName,
                Confidence = semantic?.Confidence ?? 0,
                Description = semantic?.Description,
                InputTokens = claudeResult.TokenUsage?.InputTokens ?? 0,
                OutputTokens = claudeResult.TokenUsage?.OutputTokens ?? 0,
                ProcessingTimeMs = claudeResult.ProcessingTimeMs,
                ErrorMessage = claudeResult.ErrorMessage
            };
        }

        /// <summary>
        /// 將 AzureVisionAnalysisDto 轉換為 AzureVisionContextDto（用於 Claude 分析）
        /// </summary>
        /// <remarks>
        /// 這個方法從完整的 Azure Vision 結果中提取 Claude 需要的資訊。
        /// 
        /// <para><b>為什麼需要這個轉換？</b></para>
        /// 
        /// AzureVisionAnalysisDto 包含了大量詳細資訊（邊界框、類別詳情、色彩分析等），
        /// 但 Claude 不需要所有這些資訊。AzureVisionContextDto 只包含 Claude 真正需要的資訊：
        /// - 辨識出的物體名稱列表
        /// - 標籤列表
        /// - 主要顏色
        /// - 照片描述
        /// - 內容安全性標記
        /// 
        /// 這樣做有幾個好處：
        /// 1. 減少發送給 Claude 的資料量（降低 token 成本）
        /// 2. 讓 prompt 更清晰，避免資訊過載
        /// 3. 保持 Claude 服務的獨立性（不需要知道 Azure 的完整資料結構）
        /// 
        /// <para><b>為什麼還要保留完整的原始資料？</b></para>
        /// 
        /// 完整的 AzureVisionAnalysisDto 仍然會以 JSON 格式儲存在資料庫中。
        /// 這樣做有幾個好處：
        /// 1. 保留完整的原始資料，方便日後除錯和分析
        /// 2. 未來 API 升級時，可以重新解析舊資料
        /// 3. 可以進行資料探勘和統計分析
        /// </remarks>
        private AzureVisionContextDto? ConvertToAzureVisionContext(AzureVisionAnalysisDto? azureResult)
        {
            if (azureResult == null) return null;

            return new AzureVisionContextDto
            {
                Objects = azureResult.Objects.Select(o => o.Name).ToList(),
                Tags = azureResult.Tags.Select(t => t.Name).ToList(),
                DominantColors = azureResult.Color?.DominantColors ?? new List<string>(),
                Caption = azureResult.Description?.Captions.FirstOrDefault()?.Text,
                IsAdultContent = azureResult.Adult?.IsAdultContent ?? false,
                IsRacyContent = azureResult.Adult?.IsRacyContent ?? false
            };
        }

        private GooglePlacesContextDto? ConvertToGooglePlacesContext(List<PlaceResult> googlePlaces)
        {
            if (googlePlaces == null || !googlePlaces.Any()) return null;

            // 從資料庫讀取時，googlePlaces 實際上來自 TouristSpotIdentificationDto
            // 這裡簡化處理，只返回 null 或最基本的資訊
            // 實際應用中，應該重構這部分邏輯
            return null;
        }

        /// <summary>
        /// 將 Stream 轉換為 byte[]
        /// </summary>
        /// <remarks>
        /// 這是一個輔助方法，用於將 BlobStorageService 返回的 Stream 轉換為 byte[]。
        /// Azure Vision API 和其他服務通常需要 byte[] 格式的圖片資料。
        /// </remarks>
        private async Task<byte[]> StreamToByteArrayAsync(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }

        #endregion

        #region 查詢現有分析結果

        /// <summary>
        /// 取得照片的 AI 分析結果
        /// </summary>
        public async Task<PhotoAIAnalysisResponseDto?> GetPhotoAIAnalysisAsync(long photoId)
        {
            try
            {
                var log = await _photoRepository.GetLatestAILogAsync(photoId);
                if (log == null)
                {
                    return null;
                }

                // 反序列化各個服務的結果
                AzureVisionAnalysisDto? azureResult = null;
                TouristSpotIdentificationDto? googleResult = null;
                ClaudeAnalysisResultDto? claudeResult = null;

                if (!string.IsNullOrEmpty(log.AzureVisionResponse))
                {
                    azureResult = JsonSerializer.Deserialize<AzureVisionAnalysisDto>(log.AzureVisionResponse);
                }

                if (!string.IsNullOrEmpty(log.GooglePlacesResponse))
                {
                    googleResult = JsonSerializer.Deserialize<TouristSpotIdentificationDto>(log.GooglePlacesResponse);
                }

                if (!string.IsNullOrEmpty(log.ClaudeResponse))
                {
                    claudeResult = JsonSerializer.Deserialize<ClaudeAnalysisResultDto>(log.ClaudeResponse);
                }

                // 取得標籤建議
                var suggestions = await _photoRepository.GetAllAISuggestionsAsync(photoId);

                return new PhotoAIAnalysisResponseDto
                {
                    LogId = log.LogId,
                    PhotoId = photoId,
                    Status = log.Status ?? AIConstants.Analysis.Status.Success,
                    AnalyzedAt = log.AnalyzedAt,
                    AzureVisionResult = MapToAzureSummary(azureResult),
                    GooglePlacesResult = MapToGooglePlacesSummary(googleResult),
                    ClaudeSemanticResult = MapToClaudeSummary(claudeResult),
                    TagSuggestions = suggestions.Select(s => new AITagSuggestionDto
                    {
                        SuggestionId = s.SuggestionId,
                        LogId = s.LogId,
                        CategoryId = s.CategoryId,
                        CategoryName = s.CategoryName ?? "",
                        CategoryType = s.CategoryType ?? "",
                        TagId = s.TagId,
                        TagName = s.TagName ?? "",
                        Confidence = (double)s.ConfidenceScore,
                        IsAdopted = s.IsAdopted,
                        Source = s.Source ?? "",
                        CreatedAt = s.CreatedAt
                    }).ToList(),
                    TotalProcessingTimeMs = log.ProcessingTimeMs ?? 0,
                    QuotaUsed = 1
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 取得 AI 分析結果失敗 PhotoId={PhotoId}", photoId);
                return null;
            }
        }

        #region IPhotoService - 介面補齊（查詢/套用/批次/統計）

        public async Task<PhotoAIAnalysisStatusDto> GetPhotoAIStatusAsync(long photoId)
        {
            var hasAnalysis = await _photoRepository.HasAIAnalysisAsync(photoId);
            if (!hasAnalysis)
            {
                return new PhotoAIAnalysisStatusDto
                {
                    HasAnalysis = false,
                    SuggestionCount = 0,
                    AdoptedCount = 0,
                    PendingCount = 0,
                    CanReanalyze = true
                };
            }

            var stats = await _photoRepository.GetPhotoAIAnalysisStatsAsync(photoId);

            return new PhotoAIAnalysisStatusDto
            {
                HasAnalysis = true,
                SuggestionCount = stats.TotalSuggestions,
                AdoptedCount = stats.AdoptedCount,
                PendingCount = stats.PendingCount,
                CanReanalyze = true
            };
        }

        public async Task<List<AITagSuggestionDto>> GetPendingAISuggestionsAsync(long photoId, double? minConfidence = null)
        {
            var suggestions = await _photoRepository.GetPendingSuggestionsAsync(
                photoId,
                minConfidence.HasValue ? (decimal?)minConfidence.Value : null);

            return suggestions.Select(s => new AITagSuggestionDto
            {
                SuggestionId = s.SuggestionId,
                LogId = s.LogId,
                CategoryId = s.CategoryId,
                CategoryName = s.CategoryName ?? "",
                CategoryType = s.CategoryType ?? "",
                TagId = s.TagId,
                TagName = s.TagName ?? "",
                Confidence = (double)s.ConfidenceScore,
                IsAdopted = s.IsAdopted,
                Source = s.Source ?? "",
                CreatedAt = s.CreatedAt
            }).ToList();
        }

        public async Task<ApplyAITagsResponseDto> ApplyAITagsAsync(ApplyAITagsRequestDto request)
        {
            long photoId = TryGetLong(request, "PhotoId") ?? 0;
            long? userId = TryGetLong(request, "UserId") ?? TryGetLong(request, "AppliedBy");
            var suggestionIds = TryGetLongList(request, "SuggestionIds", "SelectedSuggestionIds") ?? new List<long>();

            const int aiSourceId = 3;

            int applied = 0, skipped = 0, failed = 0;
            var errors = new List<string>();

            foreach (var suggestionId in suggestionIds.Distinct())
            {
                try
                {
                    var suggestion = await _photoRepository.GetAISuggestionByIdAsync(suggestionId);
                    if (suggestion == null)
                    {
                        skipped++;
                        errors.Add($"SuggestionId={suggestionId} 不存在");
                        continue;
                    }

                    if (photoId != 0 && suggestion.PhotoId != photoId)
                    {
                        failed++;
                        errors.Add($"SuggestionId={suggestionId} 不屬於 PhotoId={photoId}");
                        continue;
                    }

                    if (suggestion.IsAdopted)
                    {
                        skipped++;
                        continue;
                    }

                    bool changed = false;

                    if (suggestion.TagId.HasValue)
                    {
                        var exists = await _photoRepository.HasPhotoTagAsync(suggestion.PhotoId, suggestion.TagId.Value);
                        if (exists)
                        {
                            skipped++;
                        }
                        else
                        {
                            changed = await _photoRepository.AddPhotoTagAsync(
                                suggestion.PhotoId,
                                suggestion.TagId.Value,
                                aiSourceId,
                                suggestion.ConfidenceScore,
                                userId);
                        }
                    }
                    else
                    {
                        var exists = await _photoRepository.HasPhotoCategoryAsync(suggestion.PhotoId, suggestion.CategoryId);
                        if (exists)
                        {
                            skipped++;
                        }
                        else
                        {
                            changed = await _photoRepository.AddPhotoCategoryAsync(
                                suggestion.PhotoId,
                                suggestion.CategoryId,
                                aiSourceId,
                                suggestion.ConfidenceScore,
                                userId);
                        }
                    }

                    if (changed)
                    {
                        applied++;
                        await _photoRepository.AdoptSuggestionAsync(suggestionId);
                    }
                }
                catch (Exception ex)
                {
                    failed++;
                    errors.Add($"SuggestionId={suggestionId} 失敗：{ex.Message}");
                }
            }

            var response = new ApplyAITagsResponseDto();
            TrySetProperty(response, "AppliedCount", applied);
            TrySetProperty(response, "SkippedCount", skipped);
            TrySetProperty(response, "FailedCount", failed);
            TrySetProperty(response, "Errors", errors);

            return response;
        }

        public async Task<BatchPhotoAIAnalysisResponseDto> BatchAnalyzePhotosAsync(BatchPhotoAIAnalysisRequestDto request)
        {
            var photoIds = TryGetLongList(request, "PhotoIds", "PhotoIdList") ?? new List<long>();
            bool forceReanalysis = TryGetBool(request, "ForceReanalysis") ?? false;
            long userId = TryGetLong(request, "UserId") ?? 0;
            int maxParallel = (int)(TryGetLong(request, "MaxParallelism") ?? 3);

            var results = new List<object>();
            int total = photoIds.Count, processed = 0, skipped = 0, success = 0, failed = 0;

            using var semaphore = new SemaphoreSlim(Math.Max(1, maxParallel));

            var tasks = photoIds.Select(async pid =>
            {
                await semaphore.WaitAsync();
                try
                {
                    if (!forceReanalysis && await _photoRepository.HasAIAnalysisAsync(pid))
                    {
                        Interlocked.Increment(ref skipped);
                        return;
                    }

                    var singleReq = new PhotoAIAnalysisRequestDto();
                    TrySetProperty(singleReq, "PhotoId", pid);
                    TrySetProperty(singleReq, "UserId", userId);
                    TrySetProperty(singleReq, "ForceReanalysis", forceReanalysis);

                    var r = await AnalyzePhotoWithAIAsync(singleReq);
                    lock (results) results.Add(r);

                    Interlocked.Increment(ref success);
                }
                catch
                {
                    Interlocked.Increment(ref failed);
                }
                finally
                {
                    Interlocked.Increment(ref processed);
                    semaphore.Release();
                }
            }).ToList();

            await Task.WhenAll(tasks);

            var resp = new BatchPhotoAIAnalysisResponseDto();
            TrySetProperty(resp, "TotalCount", total);
            TrySetProperty(resp, "ProcessedCount", processed);
            TrySetProperty(resp, "SkippedCount", skipped);
            TrySetProperty(resp, "SuccessCount", success);
            TrySetProperty(resp, "FailedCount", failed);
            TrySetProperty(resp, "Results", results);

            return resp;
        }

        public Task<UserAIAnalysisStatsDto> GetUserAIStatsAsync(long userId)
            => _photoRepository.GetUserAIAnalysisStatsAsync(userId);

        private static long? TryGetLong(object obj, params string[] names)
        {
            foreach (var n in names)
            {
                var p = obj.GetType().GetProperty(n);
                if (p == null) continue;
                var v = p.GetValue(obj);
                if (v == null) continue;
                if (v is long l) return l;
                if (v is int i) return i;
                if (long.TryParse(v.ToString(), out var parsed)) return parsed;
            }
            return null;
        }

        private static bool? TryGetBool(object obj, params string[] names)
        {
            foreach (var n in names)
            {
                var p = obj.GetType().GetProperty(n);
                if (p == null) continue;
                var v = p.GetValue(obj);
                if (v == null) continue;
                if (v is bool b) return b;
                if (bool.TryParse(v.ToString(), out var parsed)) return parsed;
            }
            return null;
        }

        private static List<long>? TryGetLongList(object obj, params string[] names)
        {
            foreach (var n in names)
            {
                var p = obj.GetType().GetProperty(n);
                if (p == null) continue;
                var v = p.GetValue(obj);
                if (v == null) continue;

                if (v is IEnumerable<long> longs) return longs.ToList();
                if (v is IEnumerable<int> ints) return ints.Select(x => (long)x).ToList();

                if (v is string s)
                {
                    var parts = s.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    var list = new List<long>();
                    foreach (var part in parts)
                        if (long.TryParse(part, out var parsed))
                            list.Add(parsed);
                    return list;
                }
            }
            return null;
        }

        private static bool TrySetProperty(object obj, string name, object? value)
        {
            var p = obj.GetType().GetProperty(name);
            if (p == null || !p.CanWrite) return false;

            try
            {
                if (value != null && !p.PropertyType.IsAssignableFrom(value.GetType()))
                {
                    if (p.PropertyType == typeof(int) && value is long l) value = (int)l;
                    else if (p.PropertyType == typeof(long) && value is int i) value = (long)i;
                }

                p.SetValue(obj, value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #endregion
    }
}