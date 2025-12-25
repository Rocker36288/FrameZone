/**
 * 照片系統常數定義（前端）
 *
 * ⚠️ 重要：此檔案的值必須與後端 PhotoConstants.cs 保持一致
 *
 * 後端檔案位置: FrameZone_WebApi/Helpers/PhotoConstants.cs
 * 前端檔案位置: src/app/shared/constants/photo.constants.ts
 */

export class PhotoConstants {
  // ==================== 檔案上傳限制 ====================

  /**
   * 單一檔案大小上限 (MB)
   * 後端對應: PhotoConstants.MAX_FILE_SIZE_MB
   */
  static readonly MAX_FILE_SIZE_MB = 50;

  /**
   * 單一檔案大小上限 (Bytes)
   * 後端對應: PhotoConstants.MAX_FILE_SIZE_BYTES
   */
  static readonly MAX_FILE_SIZE_BYTES = 50 * 1024 * 1024;

  /**
   * 單次批次上傳的檔案數量上限
   * 後端對應: PhotoConstants.MAX_BATCH_UPLOAD_COUNT
   */
  static readonly MAX_BATCH_UPLOAD_COUNT = 50;

  /**
   * 批次上傳總大小上限 (MB)
   * 後端對應: PhotoConstants.MAX_BATCH_TOTAL_SIZE_BYTES
   */
  static readonly MAX_BATCH_TOTAL_SIZE_MB = 500;

  /**
   * 批次上傳總大小上限 (Bytes)
   * 用於前端計算
   */
  static readonly MAX_BATCH_TOTAL_SIZE_BYTES = 500 * 1024 * 1024;

  /**
   * 允許的圖片副檔名
   * 後端對應: PhotoConstants.ALLOWED_IMAGE_EXTENSIONS
   */
  static readonly ALLOWED_IMAGE_EXTENSIONS = [
    '.jpg', '.jpeg', '.png', '.heic', '.gif', '.bmp', '.webp', '.tiff', '.raw'
  ];

  // ==================== 縮圖配置 ====================

  /**
   * 縮圖寬度 (像素)
   * 後端對應: PhotoConstants.THUMBNAIL_WIDTH
   */
  static readonly THUMBNAIL_WIDTH = 600;

  /**
   * 縮圖高度 (像素)
   * 後端對應: PhotoConstants.THUMBNAIL_HEIGHT
   */
  static readonly THUMBNAIL_HEIGHT = 450;

  // ==================== 分頁配置 ====================

  /**
   * 預設每頁顯示數量
   * 後端對應: PhotoConstants.DEFAULT_PAGE_SIZE
   */
  static readonly DEFAULT_PAGE_SIZE = 20;

  /**
   * 最大每頁顯示數量
   * 後端對應: PhotoConstants.MAX_PAGE_SIZE
   */
  static readonly MAX_PAGE_SIZE = 100;

  /**
   * 最小每頁顯示數量
   * 後端對應: PhotoConstants.MIN_PAGE_SIZE
   */
  static readonly MIN_PAGE_SIZE = 1;

  // ==================== 錯誤訊息 ====================

  /**
   * 取得檔案大小超過限制的錯誤訊息
   */
  static getFileSizeExceededMessage(): string {
    return `檔案大小不能超過 ${this.MAX_FILE_SIZE_MB} MB`;
  }

  /**
   * 取得不支援的檔案格式錯誤訊息
   */
  static getUnsupportedFileFormatMessage(): string {
    return `不支援的檔案格式，僅支援: ${this.ALLOWED_IMAGE_EXTENSIONS.join(', ')}`;
  }

  /**
   * 取得批次上傳數量超過限制的錯誤訊息
   */
  static getBatchCountExceededMessage(): string {
    return `單次最多只能上傳 ${this.MAX_BATCH_UPLOAD_COUNT} 個檔案`;
  }

  /**
   * 取得批次上傳總大小超過限制的錯誤訊息
   */
  static getBatchSizeExceededMessage(): string {
    return `批次上傳總大小不能超過 ${this.MAX_BATCH_TOTAL_SIZE_MB} MB`;
  }

  // ==================== 輔助方法 ====================

  /**
   * 驗證檔案大小
   */
  static isFileSizeValid(fileSize: number): boolean {
    return fileSize <= this.MAX_FILE_SIZE_BYTES;
  }

  /**
   * 驗證檔案副檔名
   */
  static isFileExtensionValid(fileName: string): boolean {
    const ext = '.' + fileName.split('.').pop()?.toLowerCase();
    return this.ALLOWED_IMAGE_EXTENSIONS.includes(ext);
  }

  /**
   * 驗證批次上傳數量
   */
  static isBatchCountValid(count: number): boolean {
    return count <= this.MAX_BATCH_UPLOAD_COUNT && count > 0;
  }

  /**
   * 格式化檔案大小顯示
   */
  static formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';

    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));

    return Math.round((bytes / Math.pow(k, i)) * 100) / 100 + ' ' + sizes[i];
  }
}

/**
 * 認證相關常數
 */
export class AuthConstants {
  /**
   * 密碼最小長度
   * 後端對應: AuthConstants.PASSWORD_MIN_LENGTH
   */
  static readonly PASSWORD_MIN_LENGTH = 8;

  /**
   * 密碼最大長度
   * 後端對應: AuthConstants.PASSWORD_MAX_LENGTH
   */
  static readonly PASSWORD_MAX_LENGTH = 100;

  /**
   * JWT Token 儲存的 Key
   */
  static readonly TOKEN_STORAGE_KEY = 'authToken';

  /**
   * 使用者資訊儲存的 Key
   */
  static readonly USER_INFO_STORAGE_KEY = 'currentUser';

  /**
   * JWT Token 預設有效期（天）- 不記住我
   * 後端對應: AuthConstants.JWT_EXPIRY_DAYS_DEFAULT
   */
  static readonly JWT_EXPIRY_DAYS_DEFAULT = 1;

  /**
   * JWT Token 延長有效期（天）- 記住我
   * 後端對應: AuthConstants.JWT_EXPIRY_DAYS_REMEMBER
   */
  static readonly JWT_EXPIRY_DAYS_REMEMBER = 7;

  /**
   * 計算 Token 過期時間
   */
  static calculateTokenExpiry(rememberMe: boolean): Date {
    const days = rememberMe
      ? this.JWT_EXPIRY_DAYS_REMEMBER
      : this.JWT_EXPIRY_DAYS_DEFAULT;

    return new Date(Date.now() + days * 24 * 60 * 60 * 1000);
  }
}
