import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import {
  CreateCustomTagRequest,
  CreateCustomTagResponse,
  BatchAddTagsRequest,
  BatchAddTagsResponse,
  SearchTagsRequest,
  SearchTagsResponse,
  PhotoTagsDetail,
  AvailableCategoriesResponse,
  RemoveTagResponse
} from '../models/tag-management.models';

/**
 * 標籤管理服務
 *
 * @description
 * 提供標籤管理相關的 API 調用功能：
 * - 建立自訂標籤
 * - 批次添加標籤
 * - 搜尋標籤
 * - 獲取照片標籤
 * - 移除標籤
 * - 獲取可用分類
 *
 * @example
 * constructor(private tagService: TagManagementService) {}
 *
 * this.tagService.searchTags({ keyword: '櫻花' })
 *   .subscribe(response => {
 *     console.log('搜尋結果:', response.tags);
 *   });
 */
@Injectable({
  providedIn: 'root'
})
export class TagManagementService {
  /**
   * API 基礎路徑
   * 🔧 根據後端 PhotosController 的路由：
   * [Route("api/[controller]")] => /api/Photos
   * 各標籤端點皆掛在 PhotosController 底下：
   * - GET  /api/Photos/tags/search
   * - POST /api/Photos/tags/custom
   * - POST /api/Photos/tags/batch-add
   * - GET  /api/Photos/categories/available
   */
  private readonly API_BASE = 'https://localhost:7213/api/photos';

  /**
   * API 端點定義
   */
  private readonly API_ENDPOINTS = {
    CREATE_CUSTOM_TAG: `${this.API_BASE}/tags/custom`,
    BATCH_ADD_TAGS: `${this.API_BASE}/tags/batch-add`,
    SEARCH_TAGS: `${this.API_BASE}/tags/search`,
    AVAILABLE_CATEGORIES: `${this.API_BASE}/categories/available`,
    PHOTO_TAGS: (photoId: number) => `${this.API_BASE}/${photoId}/tags`,
    REMOVE_TAG: (photoId: number, tagId: number) => `${this.API_BASE}/${photoId}/tags/${tagId}`
  };

  constructor(private http: HttpClient) {
    console.log('🏷️ TagManagementService initialized');
  }

  // ============================================
  // 1. 建立自訂標籤
  // ============================================

  /**
   * 建立自訂標籤
   *
   * @param request 建立標籤請求
   * @returns Observable<CreateCustomTagResponse>
   *
   * @example
   * const request: CreateCustomTagRequest = {
   *   tagName: '櫻花',
   *   categoryId: 5,
   *   parentTagId: 10
   * };
   *
   * this.tagService.createCustomTag(request).subscribe(
   *   response => {
   *     if (response.success) {
   *       console.log('標籤建立成功:', response.tag);
   *     }
   *   }
   * );
   */
  createCustomTag(request: CreateCustomTagRequest): Observable<CreateCustomTagResponse> {
    console.log('🏷️ [TagService] Creating custom tag:', request.tagName);

    return this.http.post<CreateCustomTagResponse>(
      this.API_ENDPOINTS.CREATE_CUSTOM_TAG,
      request
    ).pipe(
      tap(response => {
        if (response.success) {
          console.log('✅ [TagService] Tag created successfully:', response.tag?.tagName);
        } else {
          console.warn('⚠️ [TagService] Tag creation failed:', response.message);
        }
      }),
      catchError(error => this.handleError('createCustomTag', error))
    );
  }

  // ============================================
  // 2. 批次添加標籤
  // ============================================

  /**
   * 批次添加標籤到多張照片
   *
   * @param request 批次添加標籤請求
   * @returns Observable<BatchAddTagsResponse>
   *
   * @example
   * const request: BatchAddTagsRequest = {
   *   photoIds: [1, 2, 3, 4, 5],
   *   existingTagIds: [10, 20],
   *   newTags: [
   *     { tagName: '北海道之旅', categoryId: 5 }
   *   ]
   * };
   *
   * this.tagService.batchAddTags(request).subscribe(
   *   response => {
   *     console.log(`成功為 ${response.successCount} 張照片添加標籤`);
   *   }
   * );
   */
  batchAddTags(request: BatchAddTagsRequest): Observable<BatchAddTagsResponse> {
    console.log(`🏷️ [TagService] Batch adding tags to ${request.photoIds.length} photos`);
    console.log('📋 [TagService] Existing tags:', request.existingTagIds);
    console.log('➕ [TagService] New tags:', request.newTags);

    return this.http.post<BatchAddTagsResponse>(
      this.API_ENDPOINTS.BATCH_ADD_TAGS,
      request
    ).pipe(
      tap(response => {
        if (response.success) {
          console.log(`✅ [TagService] Successfully tagged ${response.successCount}/${response.totalPhotos} photos`);
          if (response.createdTags.length > 0) {
            console.log('🆕 [TagService] Created new tags:', response.createdTags.map(t => t.tagName));
          }
          if (response.failedCount > 0) {
            console.warn(`⚠️ [TagService] Failed to tag ${response.failedCount} photos`);
          }
        } else {
          console.warn('⚠️ [TagService] Batch tagging failed:', response.message);
        }
      }),
      catchError(error => this.handleError('batchAddTags', error))
    );
  }

  // ============================================
  // 3. 搜尋標籤
  // ============================================

  /**
   * 搜尋標籤
   *
   * @param request 搜尋標籤請求
   * @returns Observable<SearchTagsResponse>
   *
   * @example
   * const request: SearchTagsRequest = {
   *   keyword: '櫻',
   *   limit: 20,
   *   includeSystemTags: true,
   *   includeUserTags: true
   * };
   *
   * this.tagService.searchTags(request).subscribe(
   *   response => {
   *     console.log(`找到 ${response.totalCount} 個標籤`);
   *     console.log('搜尋結果:', response.tags);
   *   }
   * );
   */
  searchTags(request: SearchTagsRequest): Observable<SearchTagsResponse> {
    console.log(`🔍 [TagService] Searching tags with keyword: "${request.keyword}"`);

    // 建立 HTTP 查詢參數
    let params = new HttpParams()
      .set('keyword', request.keyword);

    if (request.includeSystemTags !== undefined) {
      params = params.set('includeSystemTags', request.includeSystemTags.toString());
    }

    if (request.includeUserTags !== undefined) {
      params = params.set('includeUserTags', request.includeUserTags.toString());
    }

    if (request.limit !== undefined) {
      params = params.set('limit', request.limit.toString());
    }

    if (request.categoryId !== undefined) {
      params = params.set('categoryId', request.categoryId.toString());
    }

    return this.http.get<SearchTagsResponse>(
      this.API_ENDPOINTS.SEARCH_TAGS,
      { params }
    ).pipe(
      tap(response => {
        // 🔧 修正：使用可選鏈操作符，避免訪問不存在的屬性
        console.log('🔍 [TagService] Response body:', response);

        if (response?.success) {
          const totalCount = response.totalCount ?? response.tags?.length ?? 0;
          console.log(`✅ [TagService] Found ${totalCount} tags for "${request.keyword}"`);
          if (totalCount === 0) {
            console.log('💡 [TagService] No tags found - user may create new tag');
          }
        } else if (response?.tags) {
          // 🔧 修正：即使沒有 success 字段，但有 tags 數組，也視為成功
          console.log(`✅ [TagService] Found ${response.tags.length} tags (no success field)`);
        } else {
          console.warn('⚠️ [TagService] Tag search failed:', response?.message ?? 'Unknown error');
        }
      }),
      catchError(error => {
        console.error('❌ [TagService] searchTags error:', error);
        return this.handleError('searchTags', error);
      })
    );
  }

  // ============================================
  // 4. 獲取可用分類
  // ============================================

  /**
   * 獲取可用的標籤分類列表
   *
   * @returns Observable<AvailableCategoriesResponse>
   *
   * @example
   * this.tagService.getAvailableCategories().subscribe(
   *   response => {
   *     console.log('系統分類:', response.systemCategories);
   *     console.log('用戶分類:', response.userCategories);
   *   }
   * );
   */
  getAvailableCategories(): Observable<AvailableCategoriesResponse> {
    console.log('📁 [TagService] Fetching available categories');

    return this.http.get<AvailableCategoriesResponse>(
      this.API_ENDPOINTS.AVAILABLE_CATEGORIES
    ).pipe(
      tap(response => {
        if (response.success) {
          const systemCount = response.systemCategories.length;
          const userCount = response.userCategories.length;
          console.log(`✅ [TagService] Loaded ${systemCount} system categories and ${userCount} user categories`);
        } else {
          console.warn('⚠️ [TagService] Failed to load categories:', response.message);
        }
      }),
      catchError(error => this.handleError('getAvailableCategories', error))
    );
  }

  // ============================================
  // 5. 獲取照片標籤
  // ============================================

  /**
   * 獲取照片的所有標籤（按來源分類）
   *
   * @param photoId 照片 ID
   * @returns Observable<PhotoTagsDetail>
   *
   * @example
   * this.tagService.getPhotoTags(123).subscribe(
   *   detail => {
   *     console.log('相機資訊:', detail.exifTags);
   *     console.log('地點資訊:', detail.geocodingTags);
   *     console.log('我的標籤:', detail.manualTags);
   *     console.log('標籤總數:', detail.totalCount);
   *   }
   * );
   */
  getPhotoTags(photoId: number): Observable<PhotoTagsDetail> {
    console.log(`🏷️ [TagService] Fetching tags for photo ${photoId}`);

    return this.http.get<PhotoTagsDetail>(
      this.API_ENDPOINTS.PHOTO_TAGS(photoId)
    ).pipe(
      tap(detail => {
        console.log(`✅ [TagService] Loaded ${detail.totalCount} tags for photo ${photoId}`);
        console.log(`📸 [TagService] EXIF tags: ${detail.exifTags.length}`);
        console.log(`🌍 [TagService] Geocoding tags: ${detail.geocodingTags.length}`);
        console.log(`🏷️ [TagService] Manual tags: ${detail.manualTags.length}`);
        console.log(`🤖 [TagService] AI tags: ${detail.aiTags.length} (暫未啟用)`);
      }),
      catchError(error => this.handleError('getPhotoTags', error))
    );
  }

  // ============================================
  // 6. 移除照片標籤
  // ============================================

  /**
   * 移除照片的標籤
   *
   * @param photoId 照片 ID
   * @param tagId 標籤 ID
   * @returns Observable<RemoveTagResponse>
   *
   * @description
   * 注意：只能移除 MANUAL 來源的標籤
   * EXIF、GEOCODING、AI 來源的標籤無法移除
   *
   * @example
   * this.tagService.removePhotoTag(123, 45).subscribe(
   *   response => {
   *     if (response.success) {
   *       console.log(`已移除標籤: ${response.tagName}`);
   *     }
   *   }
   * );
   */
  removePhotoTag(photoId: number, tagId: number): Observable<RemoveTagResponse> {
    console.log(`🗑️ [TagService] Removing tag ${tagId} from photo ${photoId}`);

    return this.http.delete<RemoveTagResponse>(
      this.API_ENDPOINTS.REMOVE_TAG(photoId, tagId)
    ).pipe(
      tap(response => {
        if (response.success) {
          console.log(`✅ [TagService] Successfully removed tag "${response.tagName}" from photo ${photoId}`);
        } else {
          console.warn(`⚠️ [TagService] Failed to remove tag: ${response.message}`);
        }
      }),
      catchError(error => this.handleError('removePhotoTag', error))
    );
  }

  // ============================================
  // 錯誤處理
  // ============================================

  /**
   * 統一的錯誤處理方法
   *
   * @param operation 操作名稱
   * @param error HTTP 錯誤物件
   * @returns Observable<never>
   */
  private handleError(operation: string, error: HttpErrorResponse): Observable<never> {
    console.error(`❌ [TagService] ${operation} failed:`, error);

    let errorMessage = '操作失敗，請稍後再試';

    if (error.error instanceof ErrorEvent) {
      // 客戶端錯誤
      errorMessage = `客戶端錯誤: ${error.error.message}`;
      console.error('🔴 [TagService] Client-side error:', error.error.message);
    } else {
      // 伺服器錯誤
      console.error(`🔴 [TagService] Server returned code ${error.status}`);
      console.error('🔴 [TagService] Response body:', error.error);

      switch (error.status) {
        case 400:
          errorMessage = error.error?.message || '請求參數錯誤';
          break;
        case 401:
          errorMessage = '未授權，請重新登入';
          break;
        case 403:
          errorMessage = '無權限執行此操作';
          break;
        case 404:
          errorMessage = '資源不存在';
          break;
        case 500:
          errorMessage = '伺服器錯誤';
          break;
        default:
          errorMessage = error.error?.message || '未知錯誤';
      }
    }

    // 返回包含錯誤訊息的 Observable
    return throwError(() => ({
      operation,
      status: error.status,
      message: errorMessage,
      originalError: error
    }));
  }

  // ============================================
  // 輔助方法（可選）
  // ============================================

  /**
   * 驗證標籤名稱是否有效
   *
   * @param tagName 標籤名稱
   * @returns 是否有效
   *
   * @example
   * if (!this.tagService.isValidTagName('')) {
   *   console.log('標籤名稱不能為空');
   * }
   */
  isValidTagName(tagName: string): boolean {
    if (!tagName || tagName.trim().length === 0) {
      console.warn('⚠️ [TagService] Tag name cannot be empty');
      return false;
    }

    if (tagName.length > 100) {
      console.warn('⚠️ [TagService] Tag name too long (max 100 characters)');
      return false;
    }

    return true;
  }

  /**
   * 驗證照片 ID 列表是否有效
   *
   * @param photoIds 照片 ID 列表
   * @returns 是否有效
   *
   * @example
   * if (!this.tagService.isValidPhotoIds([1, 2, 3])) {
   *   console.log('照片 ID 列表無效');
   * }
   */
  isValidPhotoIds(photoIds: number[]): boolean {
    if (!photoIds || photoIds.length === 0) {
      console.warn('⚠️ [TagService] Photo IDs array cannot be empty');
      return false;
    }

    if (photoIds.some(id => !id || id <= 0)) {
      console.warn('⚠️ [TagService] Invalid photo ID detected');
      return false;
    }

    return true;
  }

  /**
   * 驗證搜尋關鍵字是否有效
   *
   * @param keyword 搜尋關鍵字
   * @returns 是否有效
   *
   * @example
   * if (!this.tagService.isValidSearchKeyword('櫻')) {
   *   console.log('搜尋關鍵字太短');
   * }
   */
  isValidSearchKeyword(keyword: string): boolean {
    if (!keyword || keyword.trim().length === 0) {
      console.warn('⚠️ [TagService] Search keyword cannot be empty');
      return false;
    }

    if (keyword.trim().length < 1) {
      console.warn('⚠️ [TagService] Search keyword too short (min 1 character)');
      return false;
    }

    return true;
  }
}
