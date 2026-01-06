/**
 * 標籤管理相關的 TypeScript 介面定義
 * 對應後端 TagManagementDtos.cs
 *
 * @description
 * 此文件包含標籤管理功能所需的所有資料型別定義
 * - 建立自訂標籤
 * - 批次添加標籤
 * - 搜尋標籤
 * - 獲取照片標籤
 * - 移除標籤
 * - 獲取可用分類
 */

// ============================================
// 1. 建立自訂標籤
// ============================================

/**
 * 建立自訂標籤請求
 */
export interface CreateCustomTagRequest {
  /**
   * 標籤名稱（必填）
   * @example "櫻花"
   */
  tagName: string;

  /**
   * 父標籤 ID（可選）
   * 用於建立階層式標籤，例如：「日本」的子標籤「東京」
   * @example 10
   */
  parentTagId?: number;

  /**
   * 分類 ID（可選）
   * 不提供時的處理邏輯：
   * - 如果有 parentTagId，繼承父標籤的分類
   * - 否則自動放入「用戶自定義」分類
   * @example 5
   */
  categoryId?: number;
}

/**
 * 建立自訂標籤回應
 */
export interface CreateCustomTagResponse {
  /**
   * 是否成功
   */
  success: boolean;

  /**
   * 訊息
   * @example "標籤建立成功"
   */
  message: string;

  /**
   * 建立的標籤資料（包含完整的標籤樹節點資訊）
   */
  tag: TagTreeNode | null;
}

/**
 * 標籤樹節點（用於階層式標籤顯示）
 */
export interface TagTreeNode {
  /**
   * 標籤 ID
   */
  tagId: number;

  /**
   * 標籤名稱
   */
  tagName: string;

  /**
   * 標籤類型
   * @example "SYSTEM" | "USER" | "CUSTOM"
   */
  tagType: string;

  /**
   * 所屬分類 ID
   */
  categoryId: number;

  /**
   * 所屬分類名稱
   */
  categoryName: string;

  /**
   * 父標籤 ID（可選）
   */
  parentTagId?: number;

  /**
   * 該標籤下的照片數量
   */
  photoCount: number;

  /**
   * 顯示順序
   */
  displayOrder: number;

  /**
   * 是否啟用
   */
  isActive: boolean;
}

// ============================================
// 2. 批次添加標籤
// ============================================

/**
 * 批次添加標籤請求
 * 用於編輯模式下，為多張照片批次添加標籤
 */
export interface BatchAddTagsRequest {
  /**
   * 照片 ID 列表（必填）
   * @example [1, 2, 3, 4, 5]
   */
  photoIds: number[];

  /**
   * 現有標籤 ID 列表（可選）
   * 從搜尋結果或標籤列表中選擇的標籤
   * @example [10, 20, 30]
   */
  existingTagIds?: number[];

  /**
   * 新建標籤列表（可選）
   * 用戶輸入的新標籤，系統會先建立這些標籤，再關聯到照片
   */
  newTags?: NewTagItem[];
}

/**
 * 新建標籤項目
 */
export interface NewTagItem {
  /**
   * 標籤名稱（必填）
   * @example "北海道之旅"
   */
  tagName: string;

  /**
   * 分類 ID（可選）
   * 不提供時，系統會自動判斷或放入「用戶自定義」分類
   */
  categoryId?: number;

  /**
   * 父標籤 ID（可選）
   * 用於建立階層式標籤
   */
  parentTagId?: number;
}

/**
 * 批次添加標籤回應
 */
export interface BatchAddTagsResponse {
  /**
   * 是否成功
   */
  success: boolean;

  /**
   * 訊息
   * @example "成功為 5 張照片添加標籤"
   */
  message: string;

  /**
   * 總共處理的照片數量
   */
  totalPhotos: number;

  /**
   * 成功處理的照片數量
   */
  successCount: number;

  /**
   * 失敗的照片數量
   */
  failedCount: number;

  /**
   * 新建立的標籤列表
   */
  createdTags: TagTreeNode[];

  /**
   * 處理結果詳細列表（可選，用於除錯或詳細報告）
   */
  results?: BatchAddTagResultItem[];
}

/**
 * 批次添加標籤結果項目
 */
export interface BatchAddTagResultItem {
  /**
   * 照片 ID
   */
  photoId: number;

  /**
   * 是否成功
   */
  success: boolean;

  /**
   * 錯誤訊息（如果失敗）
   */
  errorMessage?: string;

  /**
   * 成功添加的標籤數量
   */
  tagsAdded: number;
}

// ============================================
// 3. 移除標籤
// ============================================

/**
 * 移除標籤回應
 */
export interface RemoveTagResponse {
  /**
   * 是否成功
   */
  success: boolean;

  /**
   * 訊息
   * @example "標籤移除成功"
   */
  message: string;

  /**
   * 照片 ID
   */
  photoId: number;

  /**
   * 被移除的標籤 ID
   */
  tagId: number;

  /**
   * 被移除的標籤名稱
   */
  tagName: string;
}

// ============================================
// 4. 搜尋標籤
// ============================================

/**
 * 搜尋標籤請求
 */
export interface SearchTagsRequest {
  /**
   * 搜尋關鍵字（必填）
   * 支援模糊搜尋，最少 1 個字元
   * @example "櫻"
   */
  keyword: string;

  /**
   * 是否包含系統標籤（預設：true）
   */
  includeSystemTags?: boolean;

  /**
   * 是否包含用戶自定義標籤（預設：true）
   */
  includeUserTags?: boolean;

  /**
   * 限制返回數量（預設：20，最大：100）
   */
  limit?: number;

  /**
   * 指定分類 ID（可選）
   * 只搜尋特定分類下的標籤
   */
  categoryId?: number;
}

/**
 * 搜尋標籤回應
 */
export interface SearchTagsResponse {
  /**
   * 是否成功
   */
  success: boolean;

  /**
   * 訊息
   */
  message: string;

  /**
   * 搜尋關鍵字
   */
  keyword: string;

  /**
   * 標籤列表
   */
  tags: TagItem[];

  /**
   * 總筆數
   */
  totalCount: number;
}

/**
 * 標籤項目（用於搜尋結果、列表顯示）
 */
export interface TagItem {
  /**
   * 標籤 ID
   */
  tagId: number;

  /**
   * 標籤名稱
   */
  tagName: string;

  /**
   * 標籤類型
   * @example "SYSTEM" | "USER" | "CUSTOM"
   */
  tagType: string;

  /**
   * 所屬分類 ID
   */
  categoryId: number;

  /**
   * 所屬分類名稱
   */
  categoryName: string;

  /**
   * 父標籤 ID（可選）
   */
  parentTagId?: number;

  /**
   * 父標籤名稱（可選）
   */
  parentTagName?: string;

  /**
   * 該標籤下的照片數量
   */
  photoCount: number;

  /**
   * 顯示順序
   */
  displayOrder: number;

  /**
   * 是否為使用者建立
   */
  isUserCreated: boolean;
}

// ============================================
// 5. 照片標籤詳細資訊
// ============================================

/**
 * 照片標籤詳細資訊
 * 用於顯示單張照片的所有標籤，並按來源分類
 */
export interface PhotoTagsDetail {
  /**
   * 照片 ID
   */
  photoId: number;

  /**
   * 所有標籤（合併後的完整列表）
   */
  allTags: PhotoTagItem[];

  /**
   * EXIF 自動標籤（來源：EXIF）
   * 相機型號、拍攝參數等
   */
  exifTags: PhotoTagItem[];

  /**
   * 地理編碼標籤（來源：GEOCODING）
   * 國家、城市、地點等
   */
  geocodingTags: PhotoTagItem[];

  /**
   * 用戶手動標籤（來源：MANUAL）
   * 用戶自行添加的標籤
   */
  manualTags: PhotoTagItem[];

  /**
   * AI 識別標籤（來源：AI）
   * 🚧 暫時保留，目前不會有資料
   * 未來 AI 功能上線後會使用
   */
  aiTags: PhotoTagItem[];

  /**
   * 標籤總數
   */
  totalCount: number;
}

/**
 * 照片標籤項目
 */
export interface PhotoTagItem {
  /**
   * 標籤 ID
   */
  tagId: number;

  /**
   * 標籤名稱
   */
  tagName: string;

  /**
   * 標籤類型
   * @example "SYSTEM" | "USER" | "CUSTOM"
   */
  tagType: string;

  /**
   * 所屬分類名稱
   */
  categoryName: string;

  /**
   * 來源 ID
   */
  sourceId: number;

  /**
   * 來源名稱
   * @example "EXIF" | "MANUAL" | "GEOCODING" | "AI"
   */
  sourceName: string;

  /**
   * 信心度（AI 標籤專用，0-100）
   * 🚧 目前不會有值，AI 功能未實作
   */
  confidence?: number;

  /**
   * 添加時間
   * ISO 8601 格式字串
   * @example "2024-03-15T14:30:00Z"
   */
  addedAt: string;

  /**
   * 是否可移除
   * 只有 MANUAL 來源的標籤可移除
   */
  canRemove: boolean;
}

// ============================================
// 6. 分類列表
// ============================================

/**
 * 可用分類列表回應
 */
export interface AvailableCategoriesResponse {
  /**
   * 是否成功
   */
  success: boolean;

  /**
   * 訊息
   */
  message: string;

  /**
   * 系統分類列表
   */
  systemCategories: CategoryItem[];

  /**
   * 用戶自定義分類列表
   */
  userCategories: CategoryItem[];
}

/**
 * 分類項目
 */
export interface CategoryItem {
  /**
   * 分類 ID
   */
  categoryId: number;

  /**
   * 分類名稱
   */
  categoryName: string;

  /**
   * 分類代碼
   */
  categoryCode: string;

  /**
   * 是否為用戶自定義
   */
  isUserDefined: boolean;

  /**
   * 該分類下的標籤數量
   */
  tagCount: number;

  /**
   * 顯示順序
   */
  displayOrder: number;
}

// ============================================
// 7. 輔助型別與常數
// ============================================

/**
 * 標籤來源類型
 */
export enum TagSourceType {
  EXIF = 'EXIF',
  GEOCODING = 'GEOCODING',
  MANUAL = 'MANUAL',
  AI = 'AI'
}

/**
 * 標籤類型
 */
export enum TagType {
  SYSTEM = 'SYSTEM',
  USER = 'USER',
  CUSTOM = 'CUSTOM'
}

/**
 * 標籤來源顯示資訊
 */
export interface TagSourceInfo {
  code: TagSourceType;
  name: string;
  icon: string;
  description: string;
  canRemove: boolean;
}

/**
 * 標籤來源資訊對照表
 */
export const TAG_SOURCE_INFO: Record<TagSourceType, TagSourceInfo> = {
  [TagSourceType.EXIF]: {
    code: TagSourceType.EXIF,
    name: '相機資訊',
    icon: '📸',
    description: 'EXIF 資料自動提取',
    canRemove: false
  },
  [TagSourceType.GEOCODING]: {
    code: TagSourceType.GEOCODING,
    name: '地點資訊',
    icon: '🌍',
    description: '地理位置自動識別',
    canRemove: false
  },
  [TagSourceType.MANUAL]: {
    code: TagSourceType.MANUAL,
    name: '我的標籤',
    icon: '🏷️',
    description: '手動添加的標籤',
    canRemove: true
  },
  [TagSourceType.AI]: {
    code: TagSourceType.AI,
    name: 'AI 智能標籤',
    icon: '🤖',
    description: 'AI 自動識別（暫未啟用）',
    canRemove: false
  }
};

/**
 * 標籤操作選項
 */
export interface TagOperationOptions {
  /**
   * 是否顯示進度
   */
  showProgress?: boolean;

  /**
   * 是否顯示成功提示
   */
  showSuccessToast?: boolean;

  /**
   * 是否顯示錯誤提示
   */
  showErrorToast?: boolean;

  /**
   * 完成後的回調函數
   */
  onComplete?: (result: any) => void;

  /**
   * 錯誤處理回調函數
   */
  onError?: (error: any) => void;
}
