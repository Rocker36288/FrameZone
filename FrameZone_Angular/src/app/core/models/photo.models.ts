// ==================== 照片上傳相關 ====================

/**
 * 照片元數據 (EXIF)
 */
export interface PhotoMetadata {
  fileName: string;
  fileExtension: string;
  fileSize: number;
  hash?: string;

  // 相機資訊
  cameraMake?: string;
  cameraModel?: string;
  lensModel?: string;

  // 拍攝時間
  dateTaken?: string; // ISO 8601 格式

  // GPS 資訊
  gpsLatitude?: number;
  gpsLongitude?: number;

  // 拍攝參數
  focalLength?: number;
  aperture?: number;
  shutterSpeed?: string;
  iso?: number;
  exposureMode?: string;
  whiteBalance?: string;

  // 圖片資訊
  orientation?: number;
  width?: number;
  height?: number;

  // 自動標籤
  autoTags: string[];
}

/**
 * 照片上傳資料
 */
export interface PhotoUploadData {
  photoId: number;
  fileName: string;
  fileSize: number;
  metadata: PhotoMetadata;
  autoTags: string[];
  blobUrl?: string;
}

/**
 * 單張上傳回應
 */
export interface PhotoUploadResponse {
  success: boolean;
  message: string;
  data?: PhotoUploadData;
}

/**
 * 批次上傳結果
 */
export interface BatchUploadResult {
  fileName: string;
  success: boolean;
  photoId?: number;
  error?: string;
}

/**
 * 批次上傳回應
 */
export interface BatchUploadResponse {
  success: boolean;
  totalFiles: number;
  successCount: number;
  failedCount: number;
  results: BatchUploadResult[];
}

// ==================== 照片查詢相關 ====================

/**
 * 照片詳細資訊
 */
export interface PhotoDetail {
  photoId: number;
  userId: number;
  fileName: string;
  fileExtension: string;
  fileSize: number;
  uploadedAt: string;
  metadata?: PhotoMetadata;
  tags?: string[];
  blobUrl?: string;
}

/**
 * 照片列表項目
 */
export interface PhotoListItem {
  photoId: number;
  fileName: string;
  thumbnailUrl?: string;
  uploadedAt: string;
  dateTaken?: string;
  tags?: string[];
}

/**
 * 照片列表回應
 */
export interface PhotoListResponse {
  success: boolean;
  data: PhotoListItem[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
}

// ==================== 本地上傳狀態 ====================

/**
 * 上傳檔案項目
 */
export interface UploadFileItem {
  file: File;
  fileName: string;
  fileSize: number;
  preview?: string; // Base64 預覽圖
  status: 'pending' | 'uploading' | 'success' | 'error';
  progress: number;
  error?: string;
  photoId?: number;
  metadata?: PhotoMetadata;
  hash?: string;
}
