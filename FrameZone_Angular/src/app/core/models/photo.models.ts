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

// ==================== 標籤階層相關 ====================

/**
 * 標籤節點（支援階層結構）
 */
export interface TagNode {
  tagId: number;
  tagName: string;
  tagType: string;
  categoryId: number;
  parentTagId: number | null;
  photoCount: number;
  displayOrder: number;
  children: TagNode[];

  // 前端使用的狀態（不來自後端）
  isExpanded?: boolean;   // 是否展開
  isSelected?: boolean;   // 是否被選中
}

/**
 * 分類與標籤（用於 Sidebar）
 */
export interface CategoryWithTags {
  categoryId: number;
  categoryName: string;
  categoryCode: string;
  icon: string;
  displayOrder: number;
  isDefaultExpanded: boolean;
  uiType: string;
  isComingSoon: boolean;
  tags: TagNode[];

  // 前端使用的狀態
  isExpanded?: boolean;   // 分類是否展開
}

/**
 * 標籤階層回應
 */
export interface TagHierarchyResponse {
  success: boolean;
  message?: string;
  categories: CategoryWithTags[];
  totalPhotoCount: number;
}

/**
 * 照片查詢請求（支援標籤篩選）
 */
export interface PhotoQueryRequest {
  // 分頁參數
  pageNumber?: number;
  pageSize?: number;

  // 標籤篩選 ⭐ 新增
  tagIds?: number[];

  // 時間篩選
  startDate?: string;
  endDate?: string;
  years?: number[];
  months?: number[];

  // 地點篩選
  country?: string;
  city?: string;
  district?: string;
  placeName?: string;
  hasLocation?: boolean;

  // 相機篩選
  cameraMake?: string;
  cameraModel?: string;
  lensModel?: string;

  // 拍攝參數篩選
  minISO?: number;
  maxISO?: number;
  minAperture?: number;
  maxAperture?: number;
  minFocalLength?: number;
  maxFocalLength?: number;

  // 檔案屬性篩選
  fileNameKeyword?: string;
  minFileSize?: number;
  maxFileSize?: number;
  fileExtensions?: string[];

  // 排序參數
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';

  // 進階篩選
  hasExif?: boolean;
  isCategorized?: boolean;
  hasTags?: boolean;
  uploadedAfter?: string;
  uploadedBefore?: string;
}

/**
 * 照片查詢回應
 */
export interface PhotoQueryResponse {
  success: boolean;
  message?: string;
  photos: PhotoListItem[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
  executionTimeMs: number;
}

/**
 * 建立自訂標籤請求
 */
export interface CreateCustomTagRequest {
  tagName: string;
  parentTagId?: number | null;
}

/**
 * 建立自訂標籤回應
 */
export interface CreateCustomTagResponse {
  success: boolean;
  message: string;
  tag?: TagNode;
}
