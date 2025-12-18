import CryptoJS from 'crypto-js';
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  PhotoUploadResponse,
  BatchUploadResponse,
  PhotoMetadata,
  PhotoDetail,
  PhotoListResponse
} from '../models/photo.models';

@Injectable({
  providedIn: 'root'
})
export class PhotoService {
  private apiUrl = 'https://localhost:7213/api/photos';

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
  getPhotosList(pageIndex: number = 1, pageSize: number = 20): Observable<PhotoListResponse> {
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

  /**
   * 驗證檔案格式
   * @param file 檔案
   */
  validateFile(file: File): { valid: boolean; error?: string } {
    const allowedExtensions = ['.jpg', '.jpeg', '.png', '.heic', '.gif', '.bmp'];
    const maxFileSize = 50 * 1024 * 1024;

    const fileExtension = file.name.substring(file.name.lastIndexOf('.')).toLowerCase();

    if (!allowedExtensions.includes(fileExtension)) {
      return {
        valid: false,
        error: `不支援的檔案格式，僅支援: ${allowedExtensions.join('.')}`
      };
    }

    if (file.size > maxFileSize) {
      return {
        valid: false,
        error: `檔案大小不能超過 ${maxFileSize / (1024 * 1024)} MB`
      };
    }

    return { valid: true };
  }

  /**
   * 產生圖片預覽
   * @param file 圖片檔案
   */
  generatePreview(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();

      reader.onload = (e: any) => {
        resolve(e.target.result);
      };

      reader.onerror = (error) => {
        reject(error);
      };

      reader.readAsDataURL(file);
    })
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
