/**
 * Member Models
 * 統一管理所有 Member 相關的 TypeScript 介面定義
 */

// ============================================================================
// Common Response Models
// ============================================================================

/**
 * 通用 API 回應介面
 */
export interface ApiResponse<T = any> {
  success: boolean;
  message: string;
  data?: T;
}

// ============================================================================
// Profile Models
// ============================================================================

/**
 * 使用者個人資料 DTO
 */
export interface UserProfileDto {
  userId: number;
  account: string;
  email: string;
  phone: string | null;
  displayName: string | null;
  avatar: string | null;
  coverImage: string | null;
  bio: string | null;
  website: string | null;
  location: string | null;
  realName: string | null;
  gender: string | null;
  birthDate: string | null;
  fullAddress: string | null;
  country: string | null;
  city: string | null;
  postalCode: string | null;
}

/**
 * 更新使用者個人資料 DTO
 */
export interface UpdateUserProfileDto {
  phone: string | null;
  displayName: string | null;
  bio: string | null;
  website: string | null;
  location: string | null;
  realName: string | null;
  gender: string | null;
  birthDate: string | null;
  fullAddress: string | null;
  country: string | null;
  city: string | null;
  postalCode: string | null;
  avatarFile: File | null;
  coverImageFile: File | null;
  removeAvatar: boolean;
  removeCoverImage: boolean;
}

/**
 * 取得個人資料回應 DTO
 */
export interface GetProfileResponseDto {
  success: boolean;
  message: string;
  data: UserProfileDto;
}

/**
 * 更新個人資料回應 DTO
 */
export interface UpdateProfileResponseDto {
  success: boolean;
  message: string;
  data: UserProfileDto;
}

/**
 * 性別選項
 */
export interface GenderOption {
  value: string;
  label: string;
}

/**
 * 下拉選項（通用）
 */
export interface SelectOption {
  value: string;
  label: string;
}

// ============================================================================
// Security Models
// ============================================================================

/**
 * 變更密碼 DTO
 */
export interface ChangePasswordDto {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

/**
 * 變更密碼回應 DTO
 */
export interface ChangePasswordResponseDto {
  success: boolean;
  message: string;
}

/**
 * 雙因素驗證設定 DTO
 */
export interface TwoFactorAuthDto {
  otpEnabled: boolean;
  otpSecret?: string;
}

/**
 * 啟用雙因素驗證 DTO
 */
export interface EnableTwoFactorDto {
  verificationCode: string;
}

/**
 * 驗證雙因素驗證碼 DTO
 */
export interface VerifyTwoFactorDto {
  verificationCode: string;
}

// ============================================================================
// Notification Preference Models
// ============================================================================

/**
 * 通知偏好設定 DTO
 */
export interface NotificationPreferenceDto {
  preferenceId: number;
  userId: number;
  emailNotification: boolean;
  smsNotification: boolean;
  pushNotification: boolean;
  marketingEmail: boolean;
  orderUpdate: boolean;
  promotionAlert: boolean;
  systemAnnouncement: boolean;
}

/**
 * 更新通知偏好設定 DTO
 */
export interface UpdateNotificationPreferenceDto {
  emailNotification: boolean;
  smsNotification: boolean;
  pushNotification: boolean;
  marketingEmail: boolean;
  orderUpdate: boolean;
  promotionAlert: boolean;
  systemAnnouncement: boolean;
}

/**
 * 通知偏好設定回應 DTO
 */
export interface NotificationPreferenceResponseDto {
  success: boolean;
  message: string;
  data: NotificationPreferenceDto;
}

// ============================================================================
// Privacy Settings Models
// ============================================================================

/**
 * 隱私設定項目 DTO
 */
export interface PrivacySettingDto {
  privacyId: number;
  userId: number;
  fieldName: string;
  visibility: string;
}

/**
 * 隱私設定回應 DTO
 */
export interface PrivacySettingsResponseDto {
  success: boolean;
  message: string;
  data: PrivacySettingDto[];
}

/**
 * 更新隱私設定 DTO
 */
export interface UpdatePrivacySettingDto {
  fieldName: string;
  visibility: string;
}

/**
 * 批次更新隱私設定 DTO
 */
export interface BatchUpdatePrivacySettingsDto {
  settings: UpdatePrivacySettingDto[];
}

// ============================================================================
// Activity Log Models（更新版）
// ============================================================================

/**
 * 使用者活動記錄 DTO
 */
export interface UserLogDto {
  logId: number;
  userId: number | null;
  status: string;
  actionType: string;
  actionCategory: string;
  actionDescription: string;
  targetType: string | null;
  targetId: number | null;
  oldValue: string | null;
  newValue: string | null;
  ipAddress: string | null;
  userAgent: string | null;
  deviceType: string | null;
  systemName: string;
  severity: string;
  errorMessage: string | null;
  executionTime: number | null;
  performedBy: string | null;
  createdAt: string;
}

/**
 * 活動記錄查詢參數
 */
export interface UserLogQueryDto {
  pageNumber: number;
  pageSize: number;
  actionType?: string;
  actionCategory?: string;
  startDate?: string;
  endDate?: string;
  status?: string;
  severity?: string;
}

/**
 * 分頁資料包裝
 */
export interface PagedData<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

/**
 * 活動記錄分頁回應 DTO
 */
export interface UserLogPagedResponseDto {
  success: boolean;
  message: string;
  data: PagedData<UserLogDto> | null;
}

/**
 * 活動記錄統計 DTO
 */
export interface UserLogStatsDto {
  totalLogs: number;
  successCount: number;
  failureCount: number;
  lastLoginAt: string | null;
  lastActivityAt: string | null;
  actionTypeStats: { [key: string]: number };
  actionCategoryStats: { [key: string]: number };
}

/**
 * 活動記錄統計回應 DTO
 */
export interface UserLogStatsResponseDto {
  success: boolean;
  message: string;
  data: UserLogStatsDto | null;
}

// ============================================================================
// Third Party Auth Models
// ============================================================================

/**
 * 第三方登入綁定 DTO
 */
export interface ThirdPartyAuthDto {
  authId: number;
  userId: number;
  provider: string;
  providerId: string;
  providerEmail: string | null;
  providerName: string | null;
  isActive: boolean;
  isPrimary: boolean;
  lastUsedAt: string | null;
  createdAt: string;
}

/**
 * 第三方登入綁定列表回應 DTO
 */
export interface ThirdPartyAuthListResponseDto {
  success: boolean;
  message: string;
  data: ThirdPartyAuthDto[];
}

/**
 * 綁定第三方帳號 DTO
 */
export interface BindThirdPartyDto {
  provider: string;
  authorizationCode: string;
}

/**
 * 解除綁定第三方帳號 DTO
 */
export interface UnbindThirdPartyDto {
  authId: number;
}

// ============================================================================
// Dashboard Models
// ============================================================================

/**
 * 會員儀表板統計資料 DTO
 */
export interface MemberDashboardDto {
  userId: number;
  displayName: string | null;
  avatar: string | null;
  memberSince: string;
  lastLoginAt: string | null;
  totalPhotos: number;
  totalAlbums: number;
  storageUsed: number;
  storageLimit: number;
  subscriptionPlan: string;
  subscriptionStatus: string;
  recentActivities: UserLogDto[];
}

/**
 * 會員儀表板回應 DTO
 */
export interface MemberDashboardResponseDto {
  success: boolean;
  message: string;
  data: MemberDashboardDto;
}
