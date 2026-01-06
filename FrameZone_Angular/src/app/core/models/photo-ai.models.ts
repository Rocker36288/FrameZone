/**
 * 照片 AI 分析相關 Models
 *
 * 🔗 對應後端檔案：
 * - FrameZone_WebApi/DTOs/AI/PhotoAIAnalysisDto.cs
 * - FrameZone_WebApi/DTOs/AI/AIAnalysisStatsDto.cs
 *
 * ⚠️ 重要：此檔案的介面必須與後端 DTO 保持一致
 */

// ==================== AI 分析請求與回應 ====================

/**
 * 照片 AI 分析請求
 * 對應後端：PhotoAIAnalysisRequestDto
 */
export interface PhotoAIAnalysisRequest {
  /** 照片 ID（必填） */
  photoId: number;

  /** 使用者 ID（由前端自動填入，來自 JWT Token） */
  userId?: number;

  /** 是否使用縮圖進行分析（預設：true，節省 API 成本） */
  useThumbnail?: boolean;

  /** 最低信心分數（0.0-1.0，預設：0.6） */
  minConfidenceScore?: number;

  /** 是否啟用景點偵測（需要 GPS 資料，預設：true） */
  enableTouristSpotDetection?: boolean;

  /** 是否啟用物件偵測（Azure Vision，預設：true） */
  enableObjectDetection?: boolean;

  /** 景點搜尋半徑（公尺，預設：500） */
  placeSearchRadius?: number;

  /** 是否強制重新分析（預設：false，已分析過的照片不會重複分析） */
  forceReanalysis?: boolean;
}

/**
 * 照片 AI 分析回應
 * 對應後端：PhotoAIAnalysisResponseDto
 */
export interface PhotoAIAnalysisResponse {
  /** 分析記錄 ID */
  logId: number;

  /** 照片 ID */
  photoId: number;

  /** 分析狀態（Success, Failed, Pending, Processing） */
  status: string;

  /** 分析時間 */
  analyzedAt: string;

  /** Azure Vision 分析結果摘要 */
  azureVisionResult?: AzureVisionSummary;

  /** Google Places 分析結果摘要 */
  googlePlacesResult?: GooglePlacesSummary;

  /** Claude 語義分析結果摘要 */
  claudeSemanticResult?: ClaudeSemanticSummary;

  /** AI 標籤建議列表 */
  tagSuggestions: AITagSuggestion[];

  /** 總處理時間（毫秒） */
  totalProcessingTimeMs: number;

  /** 使用的配額（通常為 1） */
  quotaUsed: number;

  /** 錯誤訊息（如果失敗） */
  errorMessage?: string;

  /** 錯誤列表 */
  errors?: string[];
}

// ==================== AI 服務摘要 ====================

/**
 * Azure Vision 分析結果摘要
 * 對應後端：AzureVisionSummaryDto
 */
export interface AzureVisionSummary {
  /** 是否成功 */
  success: boolean;

  /** 識別的物件數量 */
  objectCount: number;

  /** 標籤數量 */
  tagCount: number;

  /** 前 5 個物件 */
  topObjects: string[];

  /** 前 10 個標籤 */
  topTags: string[];

  /** 照片描述（一句話） */
  description?: string;

  /** 是否包含成人內容 */
  hasAdultContent: boolean;

  /** 處理時間（毫秒） */
  processingTimeMs: number;

  /** 錯誤訊息 */
  errorMessage?: string;
}

/**
 * Google Places 分析結果摘要
 * 對應後端：GooglePlacesSummaryDto
 */
export interface GooglePlacesSummary {
  /** 是否成功 */
  success: boolean;

  /** 找到的景點數量 */
  placeCount: number;

  /** 最近的景點名稱 */
  nearestPlaceName?: string;

  /** 最近的景點距離（公尺） */
  nearestPlaceDistance?: number;

  /** 附近景點列表 */
  nearbyPlaces: string[];

  /** 處理時間（毫秒） */
  processingTimeMs: number;
}

/**
 * Claude 語義分析結果摘要
 * 對應後端：ClaudeSemanticSummaryDto
 */
export interface ClaudeSemanticSummary {
  /** 是否成功 */
  success: boolean;

  /** 是否為旅遊景點 */
  isTouristSpot: boolean;

  /** 景點名稱 */
  spotName?: string;

  /** 信心分數（0.0-1.0） */
  confidence: number;

  /** 照片描述（繁體中文） */
  description?: string;

  /** 輸入 Token 數 */
  inputTokens: number;

  /** 輸出 Token 數 */
  outputTokens: number;

  /** 處理時間（毫秒） */
  processingTimeMs: number;

  /** 錯誤訊息 */
  errorMessage?: string;
}

// ==================== AI 標籤建議 ====================

/**
 * AI 標籤建議
 * 對應後端：AITagSuggestionDto
 */
export interface AITagSuggestion {
  /** 建議 ID */
  suggestionId: number;

  /** 分析記錄 ID */
  logId: number;

  /** 分類 ID（如果是分類建議） */
  categoryId?: number;

  /** 分類名稱 */
  categoryName?: string;

  /** 分類類型 */
  categoryType?: string;

  /** 標籤 ID（如果是標籤建議） */
  tagId?: number;

  /** 標籤名稱 */
  tagName: string;

  /** 信心分數（0.0-1.0） */
  confidence: number;

  /** 是否已採用 */
  isAdopted: boolean;

  /** 來源（Azure, Google, Claude） */
  source: string;

  /** 建議時間 */
  suggestedAt: string;
}

/**
 * 套用 AI 標籤請求
 * 對應後端：ApplyAITagsRequestDto
 */
export interface ApplyAITagsRequest {
  /** 照片 ID */
  photoId: number;

  /** 要套用的建議 ID 列表（空陣列表示套用所有） */
  suggestionIds: number[];

  /** 最低信心分數過濾（可選） */
  minConfidence?: number;
}

/**
 * 套用 AI 標籤回應
 * 對應後端：ApplyAITagsResponseDto
 */
export interface ApplyAITagsResponse {
  /** 成功套用的數量 */
  appliedCount: number;

  /** 跳過的數量（已存在） */
  skippedCount: number;

  /** 失敗的數量 */
  failedCount: number;

  /** 套用詳情列表 */
  appliedTags: AppliedTagDetail[];

  /** 錯誤列表 */
  errors: string[];
}

/**
 * 已套用標籤詳情
 * 對應後端：AppliedTagDetailDto
 */
export interface AppliedTagDetail {
  /** 建議 ID */
  suggestionId: number;

  /** 標籤名稱 */
  tagName: string;

  /** 分類名稱 */
  categoryName: string;

  /** 信心分數 */
  confidence: number;

  /** 套用狀態（Applied, Skipped, Failed） */
  status: string;

  /** 備註（例如：標籤已存在） */
  note?: string;
}

// ==================== AI 分析狀態 ====================

/**
 * 照片 AI 分析狀態（輕量級查詢）
 * 對應後端：PhotoAIAnalysisStatusDto
 */
export interface PhotoAIAnalysisStatus {
  /** 照片 ID */
  photoId: number;

  /** 是否已有分析記錄 */
  hasAnalysis: boolean;

  /** 最後分析時間 */
  lastAnalyzedAt?: string;

  /** 最後分析狀態 */
  lastAnalysisStatus?: string;

  /** 建議總數 */
  suggestionCount: number;

  /** 已採用數量 */
  adoptedCount: number;

  /** 待處理數量 */
  pendingCount: number;

  /** 平均信心分數 */
  averageConfidence: number;

  /** 是否可以重新分析 */
  canReanalyze: boolean;

  /** 錯誤訊息 */
  errorMessage?: string;
}

// ==================== 批次 AI 分析 ====================

/**
 * 批次 AI 分析請求
 * 對應後端：BatchPhotoAIAnalysisRequestDto
 */
export interface BatchPhotoAIAnalysisRequest {
  /** 照片 ID 列表 */
  photoIds: number[];

  /** 使用者 ID（由前端自動填入） */
  userId?: number;

  /** 是否非同步處理（預設：false） */
  processAsync?: boolean;

  /** 分析選項 */
  options: PhotoAIAnalysisOptions;
}

/**
 * 照片 AI 分析選項
 * 對應後端：PhotoAIAnalysisOptionsDto
 */
export interface PhotoAIAnalysisOptions {
  /** 是否使用縮圖 */
  useThumbnail?: boolean;

  /** 最低信心分數 */
  minConfidenceScore?: number;

  /** 是否啟用景點偵測 */
  enableTouristSpotDetection?: boolean;

  /** 是否啟用物件偵測 */
  enableObjectDetection?: boolean;

  /** 景點搜尋半徑 */
  placeSearchRadius?: number;

  /** 是否強制重新分析 */
  forceReanalysis?: boolean;
}

/**
 * 批次 AI 分析回應
 * 對應後端：BatchPhotoAIAnalysisResponseDto
 */
export interface BatchPhotoAIAnalysisResponse {
  /** 照片總數 */
  totalPhotos: number;

  /** 成功數量 */
  successCount: number;

  /** 失敗數量 */
  failedCount: number;

  /** 跳過數量 */
  skippedCount: number;

  /** 是否為非同步處理 */
  isAsync: boolean;

  /** 批次任務 ID（非同步模式） */
  batchJobId?: string;

  /** 預計完成時間（非同步模式） */
  estimatedCompletionTime?: string;

  /** 分析結果列表（同步模式） */
  results?: PhotoAIAnalysisResponse[];

  /** 錯誤列表 */
  errors: string[];
}

// ==================== AI 使用統計 ====================

/**
 * 使用者 AI 分析統計
 * 對應後端：UserAIAnalysisStatsDto
 */
export interface UserAIAnalysisStats {
  /** 總分析次數 */
  totalAnalysisCount: number;

  /** 成功次數 */
  successCount: number;

  /** 失敗次數 */
  failedCount: number;

  /** 使用的總配額 */
  totalQuotaUsed: number;

  /** 平均處理時間（毫秒） */
  averageProcessingTime: number;

  /** 成功率（百分比） */
  successRate: number;

  /** 最後分析時間 */
  lastAnalysisAt?: string;

  /** Azure Vision 使用次數 */
  azureUsageCount?: number;

  /** Google Places 使用次數 */
  googleUsageCount?: number;

  /** Claude 使用次數 */
  claudeUsageCount?: number;

  /** 總輸入 Token 數 */
  totalInputTokens?: number;

  /** 總輸出 Token 數 */
  totalOutputTokens?: number;
}

// ==================== 前端使用的 UI 狀態 ====================

/**
 * AI 分析 UI 狀態（前端專用）
 */
export interface PhotoAIUIState {
  /** 是否正在分析 */
  isAnalyzing: boolean;

  /** 分析進度（0-100） */
  progress: number;

  /** 當前階段（準備中、Azure 分析、Google 查詢、Claude 分析、完成） */
  currentStage: 'preparing' | 'azure' | 'google' | 'claude' | 'completed' | 'error';

  /** 階段描述 */
  stageDescription: string;

  /** 錯誤訊息 */
  error?: string;
}

/**
 * AI 建議篩選選項（前端專用）
 */
export interface AISuggestionFilter {
  /** 最低信心分數 */
  minConfidence?: number;

  /** 來源篩選（Azure, Google, Claude） */
  sources?: string[];

  /** 是否只顯示未採用 */
  onlyPending?: boolean;
}

// ==================== 常數定義 ====================

/**
 * AI 分析狀態常數
 */
export const AIAnalysisStatus = {
  SUCCESS: 'Success',
  FAILED: 'Failed',
  PENDING: 'Pending',
  PROCESSING: 'Processing'
} as const;

/**
 * AI 來源常數
 */
export const AISource = {
  AZURE: 'Azure',
  GOOGLE: 'Google',
  CLAUDE: 'Claude',
  COMBINED: 'Combined'
} as const;

/**
 * 標籤套用狀態常數
 */
export const TagApplyStatus = {
  APPLIED: 'Applied',
  SKIPPED: 'Skipped',
  FAILED: 'Failed'
} as const;

/**
 * AI 分析預設配置
 */
export const AIAnalysisDefaults = {
  /** 預設使用縮圖 */
  USE_THUMBNAIL: true,

  /** 預設最低信心分數 */
  MIN_CONFIDENCE: 0.9,

  /** 預設啟用物件識別 */
  ENABLE_OBJECT_DETECTION: true,

  /** 預設啟用景點識別 */
  ENABLE_TOURIST_SPOT_DETECTION: true,

  /** 預設搜尋半徑（公尺） */
  PLACE_SEARCH_RADIUS: 500,

  /** 批次非同步處理閾值（超過此數量使用非同步） */
  BATCH_ASYNC_THRESHOLD: 10,

  /** 批次最大數量 */
  BATCH_MAX_SIZE: 50
} as const;
