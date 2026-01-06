using FrameZone_WebApi.DTOs.AI;

namespace FrameZone_WebApi.Services
{
    /// <summary>
    /// IPhotoService 的 AI 功能擴充
    /// 定義照片智能分析的業務邏輯介面
    /// </summary>
    /// <remarks>
    /// 這個 partial interface 擴展了 IPhotoService，專門處理 AI 相關功能。
    /// 
    /// <para><b>設計理念：</b></para>
    /// 
    /// <b>三階段分析流程：</b>
    /// - 階段一：基礎分析（Azure Vision + Google Places）→ 自動執行
    /// - 階段二：語義整合（Claude 分析）→ 自動執行
    /// - 階段三：使用者確認（套用標籤）→ 手動觸發
    /// 
    /// <b>職責分離：</b>
    /// - Service 層：協調多個 AI 服務、業務邏輯、錯誤處理
    /// - AI Service 層（Azure/Google/Claude）：專注於 API 調用
    /// - Repository 層：資料庫操作
    /// 
    /// <b>使用場景：</b>
    /// 1. 照片上傳時自動分析（背景執行）
    /// 2. 使用者手動觸發分析（重新分析）
    /// 3. 批次分析大量照片（非同步處理）
    /// 4. 查看和採用 AI 建議（使用者互動）
    /// </remarks>
    public partial interface IPhotoService
    {
        #region 完整 AI 分析

        /// <summary>
        /// 執行照片的完整 AI 分析（三階段流程）
        /// </summary>
        /// <param name="request">AI 分析請求</param>
        /// <returns>完整的 AI 分析結果</returns>
        /// <remarks>
        /// <para><b>完整流程：</b></para>
        /// 
        /// <b>階段一：基礎分析（並行執行）</b>
        /// 1. Azure Computer Vision 分析：
        ///    - 識別照片中的物體（例如：建築物、樹木、人物）
        ///    - 生成標籤（例如：outdoor, sky, landmark）
        ///    - 提供照片描述（例如：a view of a building）
        ///    - 偵測成人內容
        /// 
        /// 2. Google Places 查詢（如果有 GPS 座標）：
        ///    - 反向地理編碼（國家、城市、地區）
        ///    - 附近景點搜尋（500 公尺範圍內）
        ///    - 景點類型判斷（tourist_attraction, museum 等）
        /// 
        /// <b>階段二：語義整合</b>
        /// 3. Claude 智能分析：
        ///    - 整合階段一的所有資訊
        ///    - 理解照片的深層語義（例如：這是在淺草寺拍的傳統建築）
        ///    - 生成具體且有用的標籤建議（例如：日本、東京、寺廟、文化）
        ///    - 判斷是否為知名旅遊景點
        /// 
        /// <b>階段三：儲存結果</b>
        /// 4. 將所有分析結果寫入資料庫：
        ///    - PhotoAIClassificationLog：完整的分析記錄
        ///    - PhotoAIClassificationSuggestion：AI 建議的標籤列表
        /// 
        /// <para><b>使用時機：</b></para>
        /// - 照片上傳時自動執行（背景任務）
        /// - 使用者手動觸發重新分析
        /// - 補充分析缺少 EXIF 資料的照片
        /// 
        /// <para><b>成本控制：</b></para>
        /// - Azure Vision：約 $0.001 / 張（1000 張 = $1）
        /// - Google Places：約 $0.017 / 張（58 張 = $1）
        /// - Claude API：約 $0.003 / 張（333 張 = $1）
        /// - 總成本：約 $0.021 / 張（48 張 = $1）
        /// 
        /// <para><b>錯誤處理：</b></para>
        /// 採用「優雅降級」策略：
        /// - 如果 Azure Vision 失敗，繼續執行 Google Places 和 Claude
        /// - 如果 Google Places 失敗（例如：沒有 GPS），Claude 會基於 Azure Vision 進行分析
        /// - 即使所有 API 都失敗，也會返回分析記錄（標記為失敗狀態）
        /// 
        /// <para><b>效能考量：</b></para>
        /// - 階段一並行執行：Azure Vision 和 Google Places 同時進行（節省 50% 時間）
        /// - 使用縮圖分析：降低傳輸時間和 API 成本
        /// - 平均處理時間：3-5 秒（取決於網路和 API 回應速度）
        /// </remarks>
        Task<PhotoAIAnalysisResponseDto> AnalyzePhotoWithAIAsync(PhotoAIAnalysisRequestDto request);

        #endregion

        #region 分階段執行

        /// <summary>
        /// 僅執行 Azure Computer Vision 分析
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <param name="useThumbnail">是否使用縮圖（節省成本）</param>
        /// <returns>Azure Vision 分析結果</returns>
        /// <remarks>
        /// <para><b>使用場景：</b></para>
        /// - 快速預覽照片內容（不需要景點識別）
        /// - 偵測不適當內容（成人、暴力）
        /// - 生成照片描述供搜尋使用
        /// 
        /// <para><b>分析內容：</b></para>
        /// - 物體識別（Objects）：照片中的實體物件
        /// - 標籤生成（Tags）：描述照片的關鍵字
        /// - 圖像描述（Caption）：一句話描述照片
        /// - 成人內容偵測（Adult）：是否包含敏感內容
        /// - 色彩分析（Color）：主色調、強調色
        /// 
        /// <para><b>處理速度：</b></para>
        /// - 使用縮圖：約 1-2 秒
        /// - 使用原圖：約 2-4 秒（如果超過 4MB 會自動使用縮圖）
        /// </remarks>
        Task<AzureVisionAnalysisDto> AnalyzeWithAzureVisionAsync(long photoId, bool useThumbnail = true);

        /// <summary>
        /// 僅執行 Google Places 景點查詢
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <param name="radius">搜尋半徑（公尺）</param>
        /// <returns>Google Places 查詢結果（如果沒有 GPS 則返回 null）</returns>
        /// <remarks>
        /// <para><b>前提條件：</b></para>
        /// 照片必須有 GPS 座標（EXIF 資料中的緯度/經度）。
        /// 如果沒有 GPS 資料，這個方法會返回 null。
        /// 
        /// <para><b>查詢內容：</b></para>
        /// 1. 反向地理編碼（Reverse Geocoding）：
        ///    - GPS 座標 → 地址（國家、城市、地區、街道）
        /// 
        /// 2. 附近景點搜尋（Nearby Search）：
        ///    - 搜尋指定半徑內的景點
        ///    - 過濾景點類型（tourist_attraction, museum 等）
        ///    - 按距離或重要性排序
        /// 
        /// 3. 景點識別邏輯：
        ///    - 如果找到 landmark（地標），優先判定為景點
        ///    - 如果附近有高評分的 tourist_attraction，判定為景點
        ///    - 綜合考慮距離、評分、類型、評論數
        /// 
        /// <para><b>典型使用：</b></para>
        /// - 判斷照片是否在旅遊景點拍攝
        /// - 取得照片的拍攝地點資訊
        /// - 為照片自動添加地點標籤
        /// 
        /// <para><b>搜尋半徑建議：</b></para>
        /// - 市區景點：300-500 公尺（預設）
        /// - 郊區景點：500-1000 公尺
        /// - 自然景觀：1000-2000 公尺
        /// </remarks>
        Task<TouristSpotIdentificationDto?> AnalyzeWithGooglePlacesAsync(long photoId, int radius = 500);

        /// <summary>
        /// 僅執行 Claude 語義分析
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <returns>Claude 分析結果</returns>
        /// <remarks>
        /// <para><b>前置作業：</b></para>
        /// Claude 分析需要基於其他 AI 服務的結果，因此建議先執行：
        /// 1. Azure Vision 分析（提供視覺內容描述）
        /// 2. Google Places 查詢（提供地點資訊）
        /// 
        /// 如果這些資訊不存在，Claude 會僅基於照片縮圖進行視覺分析，
        /// 但準確度會降低（無法判斷具體地點）。
        /// 
        /// <para><b>Claude 的角色：</b></para>
        /// Claude 是整個分析流程的「智能大腦」，它會：
        /// 
        /// 1. <b>語義整合</b>：
        ///    - 理解分散資訊的關聯性
        ///    - 例如：Azure Vision 說「建築物、傳統裝飾」+ Google Places 說「淺草寺」
        ///      → Claude 理解為「在東京淺草寺拍攝的傳統建築照片」
        /// 
        /// 2. <b>智能推理</b>：
        ///    - 補充缺失的資訊
        ///    - 例如：照片有「壽司、餐盤、木質桌面」但沒有 GPS
        ///      → Claude 推測「日式料理」但不會亂猜地點
        /// 
        /// 3. <b>生成有用的標籤</b>：
        ///    - 避免過於籠統（「建築物」→「傳統寺廟建築」）
        ///    - 提供層次化標籤（具體到抽象：「淺草寺」→「東京」→「日本」→「亞洲」）
        /// 
        /// 4. <b>判斷景點類型</b>：
        ///    - 是否為知名旅遊景點
        ///    - 景點類型（古蹟、博物館、自然景觀）
        ///    - 信心分數（Claude 對自己判斷的確信程度）
        /// 
        /// <para><b>輸出範例：</b></para>
        /// <code>
        /// {
        ///   "isTouristSpot": true,
        ///   "spotName": "淺草寺",
        ///   "confidence": 0.95,
        ///   "suggestedTags": ["日本", "東京", "淺草", "寺廟", "傳統建築", "文化", "旅遊"],
        ///   "description": "東京淺草寺的傳統建築，展現日本江戶時代的建築風格",
        ///   "category": "建築"
        /// }
        /// </code>
        /// 
        /// <para><b>Token 消耗：</b></para>
        /// - 輸入 token：約 1000-1500（包含圖片 + 上下文資訊）
        /// - 輸出 token：約 200-400（分析結果 JSON）
        /// - 總計：約 1200-1900 tokens / 張照片
        /// </remarks>
        Task<ClaudeAnalysisResultDto> AnalyzeWithClaudeAsync(long photoId);

        #endregion

        #region 套用 AI 建議

        /// <summary>
        /// 套用 AI 建議的標籤到照片
        /// </summary>
        /// <param name="request">套用請求（包含要套用的建議 ID）</param>
        /// <returns>套用結果</returns>
        /// <remarks>
        /// <para><b>使用場景：</b></para>
        /// 
        /// <b>場景一：全部採用</b>
        /// 使用者查看 AI 建議後，決定全部套用：
        /// - SuggestionIds 留空（表示全部）
        /// - 或設定 MinConfidence（只套用高信心度的標籤）
        /// 
        /// <b>場景二：選擇性採用</b>
        /// 使用者只想套用部分標籤：
        /// - 在 UI 上勾選想要的標籤
        /// - 傳入選中的 SuggestionIds
        /// 
        /// <b>場景三：批次套用</b>
        /// 使用者想要將標籤套用到其他相似照片：
        /// - ApplyToSimilarPhotos = true
        /// - 系統會找出同地點、同時間的照片並套用相同標籤
        /// 
        /// <para><b>套用邏輯：</b></para>
        /// 
        /// 1. <b>驗證建議</b>：
        ///    - 檢查建議是否存在
        ///    - 檢查建議是否屬於該照片
        ///    - 檢查建議是否已被採用
        /// 
        /// 2. <b>檢查重複</b>：
        ///    - 如果標籤已存在（任何來源），標記為 Skipped
        ///    - 避免重複標籤造成資料冗餘
        /// 
        /// 3. <b>套用標籤</b>：
        ///    - 新增到 PhotoPhotoTag 或 PhotoPhotoCategory
        ///    - 來源標記為 AI（SourceId = 3）
        ///    - 記錄信心分數
        /// 
        /// 4. <b>更新建議狀態</b>：
        ///    - IsAdopted = true
        ///    - AdoptedAt = 當前時間
        /// 
        /// <para><b>回傳資訊：</b></para>
        /// - AppliedCount：成功套用的數量
        /// - SkippedCount：跳過的數量（已存在）
        /// - FailedCount：失敗的數量
        /// - AppliedTags：詳細的套用結果列表
        /// 
        /// <para><b>錯誤處理：</b></para>
        /// - 部分失敗不影響其他標籤的套用
        /// - 錯誤訊息會記錄在 Errors 列表中
        /// - 即使全部失敗也會返回結果（不拋出例外）
        /// </remarks>
        Task<ApplyAITagsResponseDto> ApplyAITagsAsync(ApplyAITagsRequestDto request);

        #endregion

        #region 查詢 AI 結果

        /// <summary>
        /// 取得照片的完整 AI 分析結果
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <returns>完整的 AI 分析結果（如果沒有分析記錄則返回 null）</returns>
        /// <remarks>
        /// <para><b>返回內容：</b></para>
        /// 
        /// 1. <b>分析記錄</b>（PhotoAIClassificationLog）：
        ///    - 三個 AI 服務的原始回應
        ///    - 處理時間、token 使用量
        ///    - 成功/失敗狀態
        /// 
        /// 2. <b>AI 建議</b>（PhotoAIClassificationSuggestion）：
        ///    - 所有建議的標籤和分類
        ///    - 信心分數
        ///    - 是否已被採用
        /// 
        /// 3. <b>摘要資訊</b>：
        ///    - Azure Vision 摘要（物件數、標籤數、描述）
        ///    - Google Places 摘要（景點數、最近景點）
        ///    - Claude 摘要（是否為景點、景點名稱、信心分數）
        /// 
        /// <para><b>使用時機：</b></para>
        /// - 照片詳情頁顯示 AI 分析結果
        /// - 使用者查看和管理 AI 建議
        /// - 除錯和問題追蹤
        /// </remarks>
        Task<PhotoAIAnalysisResponseDto?> GetPhotoAIAnalysisAsync(long photoId);

        /// <summary>
        /// 取得照片的 AI 分析狀態（輕量級查詢）
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <returns>AI 分析狀態</returns>
        /// <remarks>
        /// <para><b>與 GetPhotoAIAnalysisAsync 的差異：</b></para>
        /// 
        /// - GetPhotoAIAnalysisAsync：返回完整的分析結果（包含所有 AI 回應）
        /// - GetPhotoAIStatusAsync：只返回狀態摘要（是否已分析、建議數量等）
        /// 
        /// <para><b>使用場景：</b></para>
        /// 
        /// 1. <b>照片列表</b>：
        ///    - 顯示哪些照片已經分析過
        ///    - 顯示 AI 建議數量的徽章
        /// 
        /// 2. <b>快速檢查</b>：
        ///    - 決定是否需要執行分析
        ///    - 檢查是否有待處理的建議
        /// 
        /// 3. <b>效能優化</b>：
        ///    - 避免載入完整的 AI 回應（可能很大）
        ///    - 減少資料傳輸量
        /// 
        /// <para><b>返回資訊：</b></para>
        /// - HasAnalysis：是否已分析過
        /// - SuggestionCount：建議總數
        /// - AdoptedCount：已採用數
        /// - PendingCount：待處理數
        /// - CanReanalyze：是否可以重新分析
        /// </remarks>
        Task<PhotoAIAnalysisStatusDto> GetPhotoAIStatusAsync(long photoId);

        /// <summary>
        /// 取得照片的待處理 AI 建議列表
        /// </summary>
        /// <param name="photoId">照片 ID</param>
        /// <param name="minConfidence">最低信心分數過濾（可選）</param>
        /// <returns>待處理的 AI 建議列表</returns>
        /// <remarks>
        /// <para><b>使用場景：</b></para>
        /// 
        /// <b>照片詳情頁的「AI 建議」區塊</b>：
        /// 1. 顯示所有待處理的標籤建議
        /// 2. 使用者可以逐一採用或忽略
        /// 3. 可以設定最低信心分數過濾（例如：只顯示 >0.7 的建議）
        /// 
        /// <para><b>過濾邏輯：</b></para>
        /// - IsAdopted = false（未採用）
        /// - Confidence >= minConfidence（如果有設定）
        /// - 按信心分數排序（高到低）
        /// 
        /// <para><b>返回資訊：</b></para>
        /// 每個建議包含：
        /// - 標籤/分類名稱
        /// - 信心分數
        /// - 來源（Azure, Google, Claude）
        /// - 建議 ID（用於後續採用）
        /// </remarks>
        Task<List<AITagSuggestionDto>> GetPendingAISuggestionsAsync(long photoId, double? minConfidence = null);

        #endregion

        #region 批次處理

        /// <summary>
        /// 批次分析多張照片
        /// </summary>
        /// <param name="request">批次分析請求</param>
        /// <returns>批次分析結果</returns>
        /// <remarks>
        /// <para><b>處理模式：</b></para>
        /// 
        /// <b>同步處理（ProcessAsync = false）</b>：
        /// - 適用於少量照片（1-10 張）
        /// - 等待所有分析完成後返回結果
        /// - 可以立即看到每張照片的分析結果
        /// - 缺點：如果照片很多，使用者需要等很久
        /// 
        /// <b>非同步處理（ProcessAsync = true，預設）</b>：
        /// - 適用於大量照片（>10 張）
        /// - 立即返回批次任務 ID
        /// - 背景執行分析任務
        /// - 使用者可以稍後查詢進度
        /// - 優點：不阻塞使用者操作
        /// 
        /// <para><b>批次策略：</b></para>
        /// 
        /// 1. <b>跳過已分析</b>：
        ///    - 如果 ForceReanalysis = false
        ///    - 已有分析記錄的照片會被跳過
        ///    - 節省 API 成本和時間
        /// 
        /// 2. <b>並行處理</b>：
        ///    - 同時處理多張照片（預設 3 張並行）
        ///    - 平衡速度和 API 速率限制
        ///    - 避免超過 API 的 QPS 限制
        /// 
        /// 3. <b>錯誤隔離</b>：
        ///    - 單張照片失敗不影響其他照片
        ///    - 繼續處理剩餘照片
        ///    - 記錄失敗原因供後續檢視
        /// 
        /// <para><b>使用場景：</b></para>
        /// - 使用者上傳大量照片後，批次執行分析
        /// - 補充分析舊照片（歷史資料）
        /// - 重新分析（例如：AI 模型更新後）
        /// 
        /// <para><b>成本控制：</b></para>
        /// - 假設批次分析 100 張照片
        /// - 成本約：$0.021 × 100 = $2.1
        /// - 處理時間約：3-5 秒/張 × 100 ÷ 3（並行）= 100-170 秒
        /// </remarks>
        Task<BatchPhotoAIAnalysisResponseDto> BatchAnalyzePhotosAsync(BatchPhotoAIAnalysisRequestDto request);

        #endregion

        #region 統計資訊

        /// <summary>
        /// 取得使用者的 AI 分析統計資訊
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>使用者的 AI 統計</returns>
        /// <remarks>
        /// <para><b>統計內容：</b></para>
        /// 
        /// 1. <b>使用量統計</b>：
        ///    - 總分析次數（累計呼叫 AI 的次數）
        ///    - 成功次數 vs 失敗次數
        ///    - 成功率（百分比）
        /// 
        /// 2. <b>配額追蹤</b>：
        ///    - 已使用的配額數量
        ///    - 剩餘配額（如果有限制）
        ///    - 配額重置時間
        /// 
        /// 3. <b>效能指標</b>：
        ///    - 平均處理時間
        ///    - 最快/最慢分析時間
        /// 
        /// <para><b>使用場景：</b></para>
        /// - 會員中心的「AI 使用統計」頁面
        /// - 配額管理和計費
        /// - 系統監控和優化
        /// 
        /// <para><b>配額限制範例：</b></para>
        /// - 免費會員：每月 100 次
        /// - 基礎會員：每月 1000 次
        /// - 進階會員：無限制
        /// </remarks>
        Task<UserAIAnalysisStatsDto> GetUserAIStatsAsync(long userId);

        #endregion
    }
}