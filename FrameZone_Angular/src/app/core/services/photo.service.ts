import heic2any from 'heic2any';
import CryptoJS from 'crypto-js';
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  PhotoUploadResponse,
  BatchUploadResponse,
  PhotoMetadata,
  PhotoDetail,
  PhotoListResponse,
  TagHierarchyResponse,
  PhotoQueryRequest,
  PhotoQueryResponse,
  CreateCustomTagRequest,
  CreateCustomTagResponse
} from '../models/photo.models';
import { PhotoConstants } from '../../shared/constants/photo.constants';
import { splitNsName } from '@angular/compiler';
import { AITagSuggestion, ApplyAITagsRequest, ApplyAITagsResponse, BatchPhotoAIAnalysisRequest, BatchPhotoAIAnalysisResponse, PhotoAIAnalysisRequest, PhotoAIAnalysisResponse, PhotoAIAnalysisStatus, UserAIAnalysisStats } from '../models/photo-ai.models';

@Injectable({
  providedIn: 'root'
})
export class PhotoService {
  private apiUrl = 'https://localhost:7213/api/photos';

  private isHeic(file: File): boolean {
    const ext = file.name.split('.').pop()?.toLocaleLowerCase();
    const t = (file.type || '').toLocaleLowerCase();

    return ext === 'heic' || ext === 'heif' || t === 'image/heic' || t === 'image/heif';
  }

  private readAsDataUrl(blob: Blob): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = (e) => resolve(e.target?.result as string);
      reader.onerror = () => reject(new Error('讀取失敗'));
      reader.readAsDataURL(blob);
    });
  }

  constructor(private http: HttpClient) { }

  /**
   * 測試 EXIF 解析
   * @param file 照片檔案
   */
  testExif(file: File): Observable<{ success: boolean; metadata: PhotoMetadata }> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<{ success: boolean; metadata: PhotoMetadata }>(
      `${this.apiUrl}/test-exif`,
      formData
    );
  }

  /**
   * 上傳單張照片
   * @param file 照片檔案
   */
  uploadPhoto(file: File): Observable<PhotoUploadResponse> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<PhotoUploadResponse>(
      `${this.apiUrl}/upload`,
      formData
    );
  }

  /**
   * 批次上傳照片
   * @param files 照片檔案陣列
   */
  batchUpload(files: File[]): Observable<BatchUploadResponse> {
    const formData = new FormData();

    files.forEach(file => {
      formData.append('files', file);
    });

    return this.http.post<BatchUploadResponse>(
      `${this.apiUrl}/batch-upload`,
      formData
    );
  }

  /**
   * 取得照片詳細資訊
   * @param photoId 照片 ID
   */
  getPhotoById(photoId: number): Observable<{ success: boolean; data: PhotoDetail }> {
    return this.http.get<{ success: boolean; data: PhotoDetail }>(
      `${this.apiUrl}/${photoId}`
    );
  }

  /**
   * 刪除照片
   * @param photoId 照片 ID
   */
  deletePhoto(photoId: number): Observable<{ success: boolean; message: string }> {
    return this.http.delete<{ success: boolean; message: string }>(
      `${this.apiUrl}/${photoId}`
    );
  }

  /**
   * 取得照片列表
   * @param pageIndex 頁碼
   * @param pageSize 每頁筆數
   */
  getPhotosList(
    pageIndex: number = 1,
    pageSize: number = PhotoConstants.DEFAULT_PAGE_SIZE
  ): Observable<PhotoListResponse> {
    return this.http.get<PhotoListResponse>(
      `${this.apiUrl}/list`,
      {
        params: {
          pageIndex: pageIndex.toString(),
          pageSize: pageSize.toString()
        }
      }
    );
  }

  // ==================== 標籤階層與篩選 ====================

  /**
   * 取得標籤階層（用於 Sidebar）
   */
  getTagHierarchy(): Observable<TagHierarchyResponse> {
    return this.http.get<TagHierarchyResponse>(
      `${this.apiUrl}/tags/hierarchy`
    );
  }

  /**
   * 查詢照片（支援標籤篩選、多條件篩選）
   * @param request 查詢請求
   */
  queryPhotos(request: PhotoQueryRequest): Observable<PhotoQueryResponse> {
    return this.http.post<PhotoQueryResponse>(
      `${this.apiUrl}/query`,
      request
    );
  }

  /**
   * 根據標籤 ID 篩選照片（便捷方法）
   * @param tagIds 標籤 ID 陣列
   * @param pageNumber 頁碼
   * @param pageSize 每頁筆數
   */
  getPhotosByTags(
    tagIds: number[],
    pageNumber: number = 1,
    pageSize: number = PhotoConstants.DEFAULT_PAGE_SIZE
  ): Observable<PhotoQueryResponse> {
    const request: PhotoQueryRequest = {
      tagIds: tagIds,
      pageNumber: pageNumber,
      pageSize: pageSize,
      sortBy: 'DateTaken',
      sortOrder: 'desc'
    };

    return this.queryPhotos(request);
  }

  /**
   * 建立自訂標籤
   * @param request 建立自訂標籤請求
   */
  createCustomTag(request: CreateCustomTagRequest): Observable<CreateCustomTagResponse> {
    return this.http.post<CreateCustomTagResponse>(
      `${this.apiUrl}/tags/custom`,
      request
    );
  }

  /**
   * 驗證檔案格式
   * @param file 檔案
   */
  validateFile(file: File): { valid: boolean; error?: string } {
    // 檔案格式驗證
    if (!PhotoConstants.isFileExtensionValid(file.name)) {
      return {
        valid: false,
        error: PhotoConstants.getUnsupportedFileFormatMessage()
      };
    }

    // 檔案大小驗證
    if (!PhotoConstants.isFileSizeValid(file.size)) {
      return {
        valid: false,
        error: PhotoConstants.getFileSizeExceededMessage()
      };
    }

    return { valid: true };
  }

  /**
   * 產生圖片預覽
   * @param file 圖片檔案
   */
  async generatePreview(file: File): Promise<string> {
    // HEIC/HEIF：先轉成 JPEG 再給 <img>
    if (this.isHeic(file)) {
      // heic2any 可能回 Blob 或 Blob[]
      const output = await heic2any({
        blob: file,
        toType: 'image/jpeg',
        quality: 0.95
      });

      const jpegBlob = Array.isArray(output) ? output[0] : output;

      return this.readAsDataUrl(jpegBlob);
    }

    // 其他常見格式：直接讀成 DataURL
    return this.readAsDataUrl(file);
  }

  /**
   * 計算檔案 Hash
   * @param file 檔案
   */
  async calculateFileHash(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();

      reader.onload = (e: any) => {
        const wordArray = CryptoJS.lib.WordArray.create(e.target.result);
        const hash = CryptoJS.SHA256(wordArray).toString();
        resolve(hash);
      };

      reader.onerror = (error) => reject(error);

      reader.readAsArrayBuffer(file);
    });
  }

  /**
   * 檢查重複照片
   * ⚠️ 注意：此方法已不再使用，重複檢查統一由後端在上傳時處理
   * @param hash 檔案 Hash
   */
  checkDuplicateByHash(hash: string): Observable<{ exists: boolean; photoId?: number }> {
    return this.http.get<{ exists: boolean; photoId?: number }>(
      `${this.apiUrl}/check-duplicate/${hash}`
    );
  }

  /**
 * 📸 分析單張照片（完整 AI 分析）
 *
 * 執行照片的完整 AI 分析，包含三個階段：
 * 1. Azure Vision 物件識別和場景分析
 * 2. Google Places 景點識別（如果有 GPS）
 * 3. Claude 語義整合和標籤建議
 *
 * @param request AI 分析請求參數
 * @returns AI 分析完整結果
 *
 * @example
 * ```typescript
 * const request: PhotoAIAnalysisRequest = {
 *   photoId: 12345,
 *   useThumbnail: true,
 *   minConfidenceScore: 0.7,
 *   enableTouristSpotDetection: true,
 *   enableObjectDetection: true
 * };
 *
 * this.photoService.analyzePhoto(request).subscribe({
 *   next: (result) => {
 *     console.log('分析完成！建議標籤：', result.tagSuggestions);
 *   },
 *   error: (error) => {
 *     console.error('分析失敗：', error);
 *   }
 * });
 * ```
 */
  analyzePhoto(request: PhotoAIAnalysisRequest): Observable<PhotoAIAnalysisResponse> {
    return this.http.post<PhotoAIAnalysisResponse>(
      `${this.apiUrl}/ai/analyze`,
      request
    );
  }

  /**
   * 🔍 取得照片的 AI 分析狀態（輕量級查詢）
   *
   * 快速查詢照片是否已分析過，以及 AI 建議的摘要資訊。
   * 這是一個輕量級的查詢，不會返回完整的分析結果。
   *
   * 適用場景：
   * - 照片列表：顯示哪些照片已經分析過
   * - 決定是否需要執行分析
   * - 顯示 AI 建議數量的徽章
   *
   * @param photoId 照片 ID
   * @returns 分析狀態摘要
   *
   * @example
   * ```typescript
   * this.photoService.getPhotoAIStatus(12345).subscribe({
   *   next: (status) => {
   *     if (status.hasAnalysis) {
   *       console.log(`有 ${status.pendingCount} 個待處理的 AI 建議`);
   *     } else {
   *       console.log('此照片尚未分析');
   *     }
   *   }
   * });
   * ```
   */
  getPhotoAIStatus(photoId: number): Observable<PhotoAIAnalysisStatus> {
    return this.http.get<PhotoAIAnalysisStatus>(
      `${this.apiUrl}/${photoId}/ai/status`
    );
  }

  /**
   * 📊 取得照片的完整 AI 分析結果
   *
   * 返回照片的完整 AI 分析結果，包含：
   * - Azure Vision 分析摘要
   * - Google Places 景點資訊
   * - Claude 語義分析結果
   * - 所有 AI 標籤建議（包含已採用和待處理）
   *
   * 適用場景：
   * - 照片詳情頁：顯示完整的 AI 分析資訊
   * - 查看 AI 的分析過程和推理
   * - 除錯和問題追蹤
   *
   * @param photoId 照片 ID
   * @returns 完整的 AI 分析結果
   *
   * @example
   * ```typescript
   * this.photoService.getPhotoAIAnalysis(12345).subscribe({
   *   next: (analysis) => {
   *     console.log('Azure Vision:', analysis.azureVisionResult);
   *     console.log('Google Places:', analysis.googlePlacesResult);
   *     console.log('Claude 分析:', analysis.claudeSemanticResult);
   *   }
   * });
   * ```
   */
  getPhotoAIAnalysis(photoId: number): Observable<PhotoAIAnalysisResponse> {
    return this.http.get<PhotoAIAnalysisResponse>(
      `${this.apiUrl}/${photoId}/ai/analysis`
    );
  }

  /**
   * 💡 取得照片的待處理 AI 建議
   *
   * 返回照片的所有待處理 AI 建議（尚未被使用者採用的標籤）。
   * 可以使用 minConfidence 參數過濾低信心分數的建議。
   *
   * 適用場景：
   * - 照片詳情頁的「AI 建議」區塊
   * - 使用者查看並決定是否採用標籤
   * - 信心分數過濾：只顯示高品質建議
   *
   * @param photoId 照片 ID
   * @param minConfidence 最低信心分數過濾（可選，0.0 - 1.0）
   * @returns 待處理的標籤建議列表
   *
   * @example
   * ```typescript
   * // 取得所有待處理建議
   * this.photoService.getAISuggestions(12345).subscribe({
   *   next: (suggestions) => {
   *     console.log(`共有 ${suggestions.length} 個建議`);
   *   }
   * });
   *
   * // 只取得信心分數 > 0.8 的建議
   * this.photoService.getAISuggestions(12345, 0.8).subscribe({
   *   next: (suggestions) => {
   *     console.log('高信心建議：', suggestions);
   *   }
   * });
   * ```
   */
  getAISuggestions(
    photoId: number,
    minConfidence?: number
  ): Observable<AITagSuggestion[]> {
    const params: any = {};
    if (minConfidence !== undefined) {
      params.minConfidence = minConfidence.toString();
    }

    return this.http.get<AITagSuggestion[]>(
      `${this.apiUrl}/${photoId}/ai/suggestions`,
      { params }
    );
  }

  /**
   * ✅ 套用 AI 標籤建議到照片
   *
   * 將 AI 建議的標籤實際套用到照片上。使用者可以選擇：
   * - 套用所有建議（suggestionIds 為空陣列）
   * - 套用特定建議（指定 suggestionIds）
   * - 按信心分數過濾（設定 minConfidence）
   *
   * 套用邏輯：
   * - 檢查標籤是否已存在（避免重複）
   * - 標記建議為已採用（isAdopted = true）
   * - 記錄來源為 AI（sourceId = 3）
   *
   * @param photoId 照片 ID
   * @param request 套用請求
   * @returns 套用結果（成功/跳過/失敗數量）
   *
   * @example
   * ```typescript
   * // 套用特定建議
   * const request: ApplyAITagsRequest = {
   *   photoId: 12345,
   *   suggestionIds: [100, 101, 102]
   * };
   *
   * this.photoService.applyAITags(12345, request).subscribe({
   *   next: (result) => {
   *     console.log(`成功套用 ${result.appliedCount} 個標籤`);
   *     console.log(`跳過 ${result.skippedCount} 個（已存在）`);
   *     console.log(`失敗 ${result.failedCount} 個`);
   *   }
   * });
   *
   * // 套用所有信心分數 > 0.7 的建議
   * const requestAll: ApplyAITagsRequest = {
   *   photoId: 12345,
   *   suggestionIds: [],
   *   minConfidence: 0.7
   * };
   *
   * this.photoService.applyAITags(12345, requestAll).subscribe({
   *   next: (result) => {
   *     console.log('批次套用完成！', result);
   *   }
   * });
   * ```
   */
  applyAITags(
    photoId: number,
    request: ApplyAITagsRequest
  ): Observable<ApplyAITagsResponse> {
    // 確保 photoId 一致
    request.photoId = photoId;

    return this.http.post<ApplyAITagsResponse>(
      `${this.apiUrl}/${photoId}/ai/apply-tags`,
      request
    );
  }

  /**
   * 📦 批次分析多張照片
   *
   * 一次分析多張照片，支援兩種模式：
   * - 同步模式（processAsync = false）：等待所有照片分析完成後返回（適合少量照片，1-10 張）
   * - 非同步模式（processAsync = true）：立即返回任務 ID，背景執行（適合大量照片，>10 張）
   *
   * 批次限制：
   * - 最大批次大小：50 張（AIAnalysisDefaults.BATCH_MAX_SIZE）
   * - 超過 10 張建議使用非同步模式（AIAnalysisDefaults.BATCH_ASYNC_THRESHOLD）
   *
   * @param request 批次分析請求
   * @returns 批次分析結果或任務 ID
   *
   * @example
   * ```typescript
   * // 同步批次分析（少量照片）
   * const request: BatchPhotoAIAnalysisRequest = {
   *   photoIds: [12345, 12346, 12347],
   *   processAsync: false,
   *   options: {
   *     useThumbnail: true,
   *     minConfidenceScore: 0.7,
   *     enableTouristSpotDetection: true,
   *     enableObjectDetection: true
   *   }
   * };
   *
   * this.photoService.batchAnalyzePhotos(request).subscribe({
   *   next: (result) => {
   *     console.log(`成功 ${result.successCount} 張，失敗 ${result.failedCount} 張`);
   *     result.results?.forEach(r => {
   *       console.log(`照片 ${r.photoId}:`, r.tagSuggestions);
   *     });
   *   }
   * });
   *
   * // 非同步批次分析（大量照片）
   * const requestAsync: BatchPhotoAIAnalysisRequest = {
   *   photoIds: [12345, 12346, ...], // 50 張照片
   *   processAsync: true,
   *   options: { ... }
   * };
   *
   * this.photoService.batchAnalyzePhotos(requestAsync).subscribe({
   *   next: (result) => {
   *     console.log('批次任務已建立，Job ID:', result.batchJobId);
   *     console.log('預計完成時間:', result.estimatedCompletionTime);
   *     // TODO: 輪詢任務狀態
   *   }
   * });
   * ```
   */
  batchAnalyzePhotos(
    request: BatchPhotoAIAnalysisRequest
  ): Observable<BatchPhotoAIAnalysisResponse> {
    return this.http.post<BatchPhotoAIAnalysisResponse>(
      `${this.apiUrl}/ai/batch-analyze`,
      request
    );
  }

  /**
   * 📈 取得使用者的 AI 使用統計
   *
   * 返回目前登入使用者的 AI 功能使用統計，包含：
   * - 總分析次數
   * - 成功/失敗次數
   * - 使用的配額
   * - 平均處理時間
   * - 成功率
   *
   * 適用場景：
   * - 會員中心的「AI 使用統計」頁面
   * - 配額管理和提醒
   * - 系統監控和優化
   *
   * @returns 使用者 AI 使用統計資訊
   *
   * @example
   * ```typescript
   * this.photoService.getUserAIStats().subscribe({
   *   next: (stats) => {
   *     console.log(`總共分析了 ${stats.totalAnalysisCount} 張照片`);
   *     console.log(`成功率: ${stats.successRate.toFixed(2)}%`);
   *     console.log(`平均處理時間: ${stats.averageProcessingTime}ms`);
   *   }
   * });
   * ```
   */
  getUserAIStats(): Observable<UserAIAnalysisStats> {
    return this.http.get<UserAIAnalysisStats>(
      `${this.apiUrl}/ai/stats`
    );
  }

  // ==================== 輔助方法 ====================

  /**
   * 🎯 根據信心分數過濾 AI 建議（前端過濾）
   *
   * 這是一個前端輔助方法，用於在本地過濾已取得的 AI 建議列表。
   * 如果需要從後端過濾，請使用 getAISuggestions(photoId, minConfidence)。
   *
   * @param suggestions AI 建議列表
   * @param minConfidence 最低信心分數
   * @returns 過濾後的建議列表
   */
  filterSuggestionsByConfidence(
    suggestions: AITagSuggestion[],
    minConfidence: number
  ): AITagSuggestion[] {
    return suggestions.filter(s => s.confidence >= minConfidence);
  }

  /**
   * 📂 根據來源分組 AI 建議（前端輔助）
   *
   * 將 AI 建議按照來源（Azure, Google, Claude）分組，
   * 方便在 UI 中分別顯示不同來源的建議。
   *
   * @param suggestions AI 建議列表
   * @returns 按來源分組的建議
   */
  groupSuggestionsBySource(
    suggestions: AITagSuggestion[]
  ): { [source: string]: AITagSuggestion[] } {
    const grouped: { [source: string]: AITagSuggestion[] } = {
      Azure: [],
      Google: [],
      Claude: []
    };

    suggestions.forEach(s => {
      const source = s.source || 'Unknown';
      if (!grouped[source]) {
        grouped[source] = [];
      }
      grouped[source].push(s);
    });

    return grouped;
  }

  /**
   * 🎨 取得來源的顏色標記（前端 UI 輔助）
   *
   * 根據 AI 服務來源返回對應的 CSS class 或顏色，
   * 用於在 UI 中以不同顏色標記不同來源的標籤。
   *
   * @param source AI 服務來源
   * @returns Tabler 的 badge 顏色 class
   */
  getSourceBadgeColor(source: string): string {
    const colorMap: { [key: string]: string } = {
      'Azure': 'azure',      // 藍色
      'Google': 'success',   // 綠色
      'Claude': 'purple',    // 紫色
      'Combined': 'info'     // 資訊色
    };

    return colorMap[source] || 'secondary';
  }

  /**
   * ⏱️ 格式化處理時間顯示（前端 UI 輔助）
   *
   * 將毫秒數轉換為人類可讀的時間格式。
   *
   * @param milliseconds 毫秒數
   * @returns 格式化的時間字串
   *
   * @example
   * ```typescript
   * formatProcessingTime(1234)  // "1.23 秒"
   * formatProcessingTime(62000) // "1 分 2 秒"
   * ```
   */
  formatProcessingTime(milliseconds: number): string {
    if (milliseconds < 1000) {
      return `${milliseconds} 毫秒`;
    }

    const seconds = Math.floor(milliseconds / 1000);
    const ms = milliseconds % 1000;

    if (seconds < 60) {
      return `${seconds}.${Math.floor(ms / 100)} 秒`;
    }

    const minutes = Math.floor(seconds / 60);
    const remainingSeconds = seconds % 60;

    return `${minutes} 分 ${remainingSeconds} 秒`;
  }

  /**
   * 📊 計算信心分數的進度條百分比（前端 UI 輔助）
   *
   * 將 0.0 - 1.0 的信心分數轉換為 0 - 100 的百分比，
   * 用於顯示進度條。
   *
   * @param confidence 信心分數 (0.0 - 1.0)
   * @returns 百分比 (0 - 100)
   */
  confidenceToPercentage(confidence: number): number {
    return Math.round(confidence * 100);
  }

  /**
   * 🎯 取得信心分數的評級（前端 UI 輔助）
   *
   * 根據信心分數返回評級（高/中/低），用於 UI 顯示。
   *
   * @param confidence 信心分數 (0.0 - 1.0)
   * @returns 評級字串
   */
  getConfidenceRating(confidence: number): string {
    if (confidence >= 0.95) return '高';
    if (confidence >= 0.7) return '中';
    return '低';
  }

  /**
   * 🎨 取得信心分數的顏色（前端 UI 輔助）
   *
   * 根據信心分數返回對應的顏色 class。
   *
   * @param confidence 信心分數 (0.0 - 1.0)
   * @returns Tabler 顏色 class
   */
  getConfidenceColor(confidence: number): string {
    if (confidence >= 0.95) return 'success';  // 綠色
    if (confidence >= 0.7) return 'warning';   // 黃色
    return 'danger';                            // 紅色
  }
}
