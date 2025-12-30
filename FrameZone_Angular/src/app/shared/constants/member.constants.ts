/**
 * Member Constants
 * 會員系統相關常數定義（對應後端 MemberConstants.cs）
 */

import { GenderOption, SelectOption } from "../../core/models/member.models";

// ============================================================================
// Field Length Limits
// ============================================================================

export const MEMBER_FIELD_LIMITS = {
  // 基本資料欄位長度限制
  DISPLAY_NAME_MAX_LENGTH: 30,
  BIO_MAX_LENGTH: 500,
  WEBSITE_MAX_LENGTH: 255,
  LOCATION_MAX_LENGTH: 100,
  PHONE_MAX_LENGTH: 50,

  // 私密資料欄位長度限制
  REAL_NAME_MAX_LENGTH: 100,
  FULL_ADDRESS_MAX_LENGTH: 200,
  COUNTRY_MAX_LENGTH: 50,
  CITY_MAX_LENGTH: 100,
  POSTAL_CODE_MAX_LENGTH: 20
} as const;

// ============================================================================
// Image Upload Limits
// ============================================================================

export const MEMBER_IMAGE_LIMITS = {
  // 頭像限制
  AVATAR_MAX_SIZE_BYTES: 5 * 1024 * 1024, // 5MB
  AVATAR_MAX_SIZE_MB: 5,
  AVATAR_RECOMMENDED_WIDTH: 200,
  AVATAR_RECOMMENDED_HEIGHT: 200,

  // 封面照片限制
  COVER_IMAGE_MAX_SIZE_BYTES: 10 * 1024 * 1024, // 10MB
  COVER_IMAGE_MAX_SIZE_MB: 10,
  COVER_IMAGE_RECOMMENDED_WIDTH: 1200,
  COVER_IMAGE_RECOMMENDED_HEIGHT: 400,

  // 允許的圖片格式
  ALLOWED_EXTENSIONS: ['.jpg', '.jpeg', '.png', '.gif', '.webp', '.bmp', '.heic', '.heif'] as readonly string[],
  ALLOWED_MIME_TYPES: ['image/jpeg', 'image/png', 'image/gif', 'image/webp', 'image/bmp', 'image/heic', 'image/heif', 'image/heic-sequence', 'image/heif-sequence'] as readonly string[]
} as const;

// ============================================================================
// Gender Options
// ============================================================================

export const GENDER_OPTIONS: readonly GenderOption[] = [
  { value: 'Male', label: '男性' },
  { value: 'Female', label: '女性' },
  { value: 'Other', label: '其他' },
  { value: 'PreferNotToSay', label: '不願透露' }
] as const;

// ============================================================================
// Privacy Visibility Options
// ============================================================================

export const PRIVACY_VISIBILITY_OPTIONS: readonly SelectOption[] = [
  { value: 'Public', label: '公開' },
  { value: 'Friends', label: '好友' },
  { value: 'Private', label: '僅自己' }
] as const;

// ============================================================================
// Privacy Field Names
// ============================================================================

export const PRIVACY_FIELDS = {
  PHONE: 'Phone',
  REAL_NAME: 'RealName',
  GENDER: 'Gender',
  BIRTH_DATE: 'BirthDate',
  LOCATION: 'Location',
  FULL_ADDRESS: 'FullAddress',
  EMAIL: 'Email'
} as const;

// ============================================================================
// Password Requirements
// ============================================================================

export const PASSWORD_REQUIREMENTS = {
  MIN_LENGTH: 8,
  MAX_LENGTH: 100,
  REQUIRE_UPPERCASE: true,
  REQUIRE_LOWERCASE: true,
  REQUIRE_DIGIT: true,
  REQUIRE_SPECIAL_CHAR: true,
  SPECIAL_CHARS: '!@#$%^&*()_+-=[]{}|;:,.<>?'
} as const;

// ============================================================================
// Notification Types
// ============================================================================

export const NOTIFICATION_TYPES = {
  EMAIL: 'Email',
  SMS: 'SMS',
  PUSH: 'Push',
  BELL: 'Bell'
} as const;

// ============================================================================
// Action Types (for UserLog)
// ============================================================================

export const USER_ACTION_TYPES = {
  // Profile Actions
  PROFILE_VIEW: 'ProfileView',
  PROFILE_UPDATE: 'ProfileUpdate',
  AVATAR_UPLOAD: 'AvatarUpload',
  AVATAR_REMOVE: 'AvatarRemove',
  COVER_IMAGE_UPLOAD: 'CoverImageUpload',
  COVER_IMAGE_REMOVE: 'CoverImageRemove',

  // Security Actions
  PASSWORD_CHANGE: 'PasswordChange',
  TWO_FACTOR_ENABLE: 'TwoFactorEnable',
  TWO_FACTOR_DISABLE: 'TwoFactorDisable',

  // Auth Actions
  LOGIN: 'Login',
  LOGOUT: 'Logout',
  LOGIN_FAILED: 'LoginFailed',

  // Settings Actions
  NOTIFICATION_PREFERENCE_UPDATE: 'NotificationPreferenceUpdate',
  PRIVACY_SETTINGS_UPDATE: 'PrivacySettingsUpdate'
} as const;

// ============================================================================
// Action Categories (for UserLog)
// ============================================================================

export const USER_ACTION_CATEGORIES = {
  PROFILE: 'Profile',
  SECURITY: 'Security',
  AUTH: 'Authentication',
  SETTINGS: 'Settings',
  SYSTEM: 'System'
} as const;

// ============================================================================
// Third Party Providers
// ============================================================================

export const THIRD_PARTY_PROVIDERS = {
  GOOGLE: 'Google',
  FACEBOOK: 'Facebook',
  LINE: 'Line',
  GITHUB: 'GitHub'
} as const;

// ============================================================================
// Validation Patterns
// ============================================================================

export const VALIDATION_PATTERNS = {
  // Email 格式
  EMAIL: /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/,

  // 電話格式（允許數字、空格、橫線、加號、括號）
  PHONE: /^[\d\s\-\+\(\)]+$/,

  // URL 格式
  URL: /^https?:\/\/.+/,

  // 密碼格式（至少8個字符，包含大小寫字母、數字、特殊字符）
  PASSWORD: /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/,

  // 郵遞區號（台灣5碼或3+2碼）
  POSTAL_CODE_TW: /^\d{3}(\d{2})?$/
} as const;

// ============================================================================
// Helper Functions
// ============================================================================

/**
 * 驗證性別值是否有效
 */
export function isValidGender(gender: string): boolean {
  return GENDER_OPTIONS.some(option => option.value === gender);
}

/**
 * 驗證圖片副檔名是否允許
 */
export function isValidImageExtension(extension: string): boolean {
  return MEMBER_IMAGE_LIMITS.ALLOWED_EXTENSIONS.includes(extension.toLowerCase());
}

/**
 * 驗證檔案大小
 */
export function isValidFileSize(fileSize: number, maxSizeBytes: number): boolean {
  return fileSize <= maxSizeBytes;
}

/**
 * 驗證頭像檔案
 */
export function isValidAvatarFile(file: File): { valid: boolean; error?: string } {
  const extension = '.' + file.name.split('.').pop()?.toLowerCase();

  if (!isValidImageExtension(extension)) {
    return {
      valid: false,
      error: `不支援的檔案格式。允許的格式: ${MEMBER_IMAGE_LIMITS.ALLOWED_EXTENSIONS.join(', ')}`
    };
  }

  if (!isValidFileSize(file.size, MEMBER_IMAGE_LIMITS.AVATAR_MAX_SIZE_BYTES)) {
    return {
      valid: false,
      error: `檔案大小超過限制 (${MEMBER_IMAGE_LIMITS.AVATAR_MAX_SIZE_MB}MB)`
    };
  }

  return { valid: true };
}

/**
 * 驗證封面照片檔案
 */
export function isValidCoverImageFile(file: File): { valid: boolean; error?: string } {
  const extension = '.' + file.name.split('.').pop()?.toLowerCase();

  if (!isValidImageExtension(extension)) {
    return {
      valid: false,
      error: `不支援的檔案格式。允許的格式: ${MEMBER_IMAGE_LIMITS.ALLOWED_EXTENSIONS.join(', ')}`
    };
  }

  if (!isValidFileSize(file.size, MEMBER_IMAGE_LIMITS.COVER_IMAGE_MAX_SIZE_BYTES)) {
    return {
      valid: false,
      error: `檔案大小超過限制 (${MEMBER_IMAGE_LIMITS.COVER_IMAGE_MAX_SIZE_MB}MB)`
    };
  }

  return { valid: true };
}

/**
 * 驗證 URL 格式
 */
export function isValidUrl(url: string): boolean {
  return VALIDATION_PATTERNS.URL.test(url);
}

/**
 * 驗證電話格式
 */
export function isValidPhone(phone: string): boolean {
  return VALIDATION_PATTERNS.PHONE.test(phone);
}

/**
 * 驗證 Email 格式
 */
export function isValidEmail(email: string): boolean {
  return VALIDATION_PATTERNS.EMAIL.test(email);
}

/**
 * 驗證密碼強度
 */
export function isValidPassword(password: string): boolean {
  return VALIDATION_PATTERNS.PASSWORD.test(password);
}

/**
 * 取得圖片預覽 URL
 */
export function getImagePreviewUrl(file: File): Promise<string> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = (e: ProgressEvent<FileReader>) => {
      resolve(e.target?.result as string);
    };
    reader.onerror = reject;
    reader.readAsDataURL(file);
  });
}

/**
 * 格式化檔案大小顯示
 */
export function formatFileSize(bytes: number): string {
  if (bytes === 0) return '0 Bytes';

  const k = 1024;
  const sizes = ['Bytes', 'KB', 'MB', 'GB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));

  return Math.round((bytes / Math.pow(k, i)) * 100) / 100 + ' ' + sizes[i];
}

/**
 * 取得性別標籤
 */
export function getGenderLabel(gender: string): string {
  const option = GENDER_OPTIONS.find(opt => opt.value === gender);
  return option?.label || gender;
}

/**
 * 取得隱私設定標籤
 */
export function getPrivacyVisibilityLabel(visibility: string): string {
  const option = PRIVACY_VISIBILITY_OPTIONS.find(opt => opt.value === visibility);
  return option?.label || visibility;
}

/**
 * 檢查是否為 HEIC/HEIF 格式
 */
export function isHeicFormat(file: File): boolean {
  const extension = '.' + file.name.split('.').pop()?.toLowerCase();
  return extension === '.heic' || extension === '.heif';
}
