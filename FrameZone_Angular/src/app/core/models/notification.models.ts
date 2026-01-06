/**
 * 通知詳細資訊
 */
export interface NotificationDto {
  recipientId: number;
  notificationId: number;
  userId: number;
  systemCode: string;
  systemName: string;
  categoryCode: string;
  categoryName: string;
  categoryIcon: string;
  priorityCode: string;
  priorityLevel: number;
  notificationTitle: string;
  notificationContent: string;
  relatedObjectType?: string;
  relatedObjectId?: number;
  isRead: boolean;
  readAt?: string;
  createdAt: string;
  expiresAt?: string;
}

/**
 * 未讀數量統計
 */
export interface UnreadCountDto {
  totalCount: number;
  systemCounts: { [key: string]: number };
}

/**
 * 通知查詢參數
 */
export interface NotificationQueryDto {
  systemCode?: string;
  isUnreadOnly?: boolean;
  page: number;
  pageSize: number;
}

/**
 * 通知分頁結果
 */
export interface NotificationPagedResultDto {
  items: NotificationDto[];
  totalCount: number;
  currentPage: number;
  pageSize: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

/**
 * 標記已讀 DTO
 */
export interface MarkAsReadDto {
  recipientIds: number[];
}

/**
 * 刪除通知 DTO
 */
export interface DeleteNotificationDto {
  recipientIds: number[];
}

/**
 * 系統模組選項
 */
export interface SystemModuleDto {
  systemCode: string;
  systemName: string;
  unreadCount: number;
}

/**
 * API 回應包裝
 */
export interface ServiceResult<T> {
  success: boolean;
  message: string;
  data?: T;
  errorCode?: string;
}

/**
 * 系統常數
 */
export class NotificationConstants {
  static readonly SYSTEM_CODES = {
    PHOTO: 'PHOTO',
    SHOPPING: 'SHOPPING',
    SOCIAL: 'SOCIAL',
    VIDEO: 'VIDEO',
    PHOTOGRAPHER: 'PHOTOGRAPHER'
  };

  static readonly SYSTEM_NAMES: { [key: string]: string } = {
    'PHOTO': '照片系統',
    'SHOPPING': '購物系統',
    'SOCIAL': '社群系統',
    'VIDEO': '影片系統',
    'PHOTOGRAPHER': '攝影師預約'
  };

  static readonly DEFAULT_PAGE_SIZE = 20;
  static readonly BELL_DROPDOWN_SIZE = 20;
}
