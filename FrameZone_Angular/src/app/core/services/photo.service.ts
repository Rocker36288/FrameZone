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
        quality: 0.85
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
}
