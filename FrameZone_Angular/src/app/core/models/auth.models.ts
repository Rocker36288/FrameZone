// ==================== 登入相關 ====================
/**
 * 登入請求 - 前端發送給後端的資料
 */
export interface LoginRequestDto {
  accountOrEmail: string;
  password: string;
  rememberMe: boolean;
}

/**
 * 登入回應 - 後端返回給前端的資料
 */
export interface LoginResponseDto {
  success: boolean;
  message: string;
  token?: string;
  userId?: number;
  account?: string;
  email?: string;
  displayName?: string;
  avatar?: string;
}

// ==================== 註冊相關 ====================
/**
 * 註冊請求
 */
export interface RegisterRequestDto {
  account: string;
  email: string;
  password: string;
  confirmPassword: string;
  phone?: string;
}

/**
 * 註冊回應
 */
export interface RegisterResponseDto {
  success: boolean;
  message: string;
  userId?: number;
}

// ==================== 密碼相關 ====================
/**
 * 忘記密碼請求
 */
export interface ForgotPasswordRequestDto {
  email: string;
}

/**
 * 忘記密碼回應
 */
export interface ForgotPasswordResponseDto {
  success: boolean;
  message: string;
}

/**
 * 驗證重設 Token 回應
 */
export interface ValidateResetTokenResponseDto {
  success: boolean;
  message: string;
}

/**
 * 重設密碼請求
 */
export interface ResetPasswordRequestDto {
  token: string;
  newPassword: string;
  confirmPassword: string;
}

/**
 * 重設密碼回應
 */
export interface ResetPasswordResponseDto {
  success: boolean;
  message: string;
}

/**
 * 變更密碼請求
 */
export interface ChangePasswordRequestDto {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

/**
 * 變更密碼回應
 */
export interface ChangePasswordResponseDto {
  success: boolean;
  message: string;
}

/**
 * 用戶資訊
 */
export interface UserInfo {
  userId: number;
  account: string;
  email: string;
  displayName?: string;
  avatar?: string;
}
