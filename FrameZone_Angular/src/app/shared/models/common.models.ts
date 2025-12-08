// ==================== 通用 API 回應 ====================
/**
 * API 回應格式
 */
export interface ApiResponse<T = any> {
  success: boolean;
  message: string;
  data?: T;
  errors?: ValidationErrors;
}

// ==================== 分頁相關 ====================
/**
 * 分頁請求參數 - 前端發送給後端
 */
export interface PaginationParams {
  pageNumber: number;
  pageSize: number;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

/**
 * 分頁回應 - 後端返回給前端
 */
export interface PaginationResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

// ==================== 驗證錯誤相關 ====================
/**
 * 後端驗證錯誤格式
 * 格式：{ "fieldName": ["error1", "error2"] }
 */
export interface ValidationErrors {
  [key: string]: string[];
}

/**
 * 前端表單錯誤格式
 * 格式：{ "fieldName": "error message" }
 */
export interface FormErrors {
  [key: string]: string;
}
