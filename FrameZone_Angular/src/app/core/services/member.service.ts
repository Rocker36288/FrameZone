import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  GetProfileResponseDto,
  UpdateUserProfileDto,
  UpdateProfileResponseDto,
  ChangePasswordDto,
  ChangePasswordResponseDto,
  NotificationPreferenceDto,
  NotificationPreferenceResponseDto,
  UpdateNotificationPreferenceDto,
  PrivacySettingsResponseDto,
  BatchUpdatePrivacySettingsDto,
  UserLogQueryDto,
  UserLogPagedResponseDto,
  ThirdPartyAuthListResponseDto,
  BindThirdPartyDto,
  UnbindThirdPartyDto,
  MemberDashboardResponseDto,
  TwoFactorAuthDto,
  EnableTwoFactorDto,
  VerifyTwoFactorDto,
  ApiResponse
} from '../models/member.models';

/**
 * Member Service
 * 統一管理所有會員相關的 API 呼叫
 */
@Injectable({
  providedIn: 'root'
})
export class MemberService {
  private apiUrl = 'https://localhost:7213/api/member';

  constructor(private http: HttpClient) {}

  // ============================================================================
  // Profile Management
  // ============================================================================

  /**
   * 取得會員個人資料
   */
  getProfile(): Observable<GetProfileResponseDto> {
    return this.http.get<GetProfileResponseDto>(`${this.apiUrl}/profile`);
  }

  /**
   * 更新會員個人資料
   */
  updateProfile(dto: UpdateUserProfileDto): Observable<UpdateProfileResponseDto> {
    const formData = new FormData();

    // 基本資料
    if (dto.phone) formData.append('phone', dto.phone);
    if (dto.displayName) formData.append('displayName', dto.displayName);
    if (dto.bio) formData.append('bio', dto.bio);
    if (dto.website) formData.append('website', dto.website);
    if (dto.location) formData.append('location', dto.location);

    // 私密資料
    if (dto.realName) formData.append('realName', dto.realName);
    if (dto.gender) formData.append('gender', dto.gender);
    if (dto.birthDate) formData.append('birthDate', dto.birthDate);
    if (dto.fullAddress) formData.append('fullAddress', dto.fullAddress);
    if (dto.country) formData.append('country', dto.country);
    if (dto.city) formData.append('city', dto.city);
    if (dto.postalCode) formData.append('postalCode', dto.postalCode);

    // 圖片檔案
    if (dto.avatarFile) {
      formData.append('avatarFile', dto.avatarFile);
    }
    if (dto.coverImageFile) {
      formData.append('coverImageFile', dto.coverImageFile);
    }

    // 刪除標記
    formData.append('removeAvatar', dto.removeAvatar.toString());
    formData.append('removeCoverImage', dto.removeCoverImage.toString());

    return this.http.put<UpdateProfileResponseDto>(`${this.apiUrl}/profile`, formData);
  }

  // ============================================================================
  // Security Management
  // ============================================================================

  /**
   * 變更密碼
   */
  changePassword(dto: ChangePasswordDto): Observable<ChangePasswordResponseDto> {
    return this.http.post<ChangePasswordResponseDto>(`${this.apiUrl}/security/change-password`, dto);
  }

  /**
   * 取得雙因素驗證狀態
   */
  getTwoFactorAuthStatus(): Observable<ApiResponse<TwoFactorAuthDto>> {
    return this.http.get<ApiResponse<TwoFactorAuthDto>>(`${this.apiUrl}/security/two-factor`);
  }

  /**
   * 啟用雙因素驗證
   */
  enableTwoFactorAuth(dto: EnableTwoFactorDto): Observable<ApiResponse<TwoFactorAuthDto>> {
    return this.http.post<ApiResponse<TwoFactorAuthDto>>(`${this.apiUrl}/security/two-factor/enable`, dto);
  }

  /**
   * 停用雙因素驗證
   */
  disableTwoFactorAuth(): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(`${this.apiUrl}/security/two-factor/disable`, {});
  }

  /**
   * 驗證雙因素驗證碼
   */
  verifyTwoFactorAuth(dto: VerifyTwoFactorDto): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(`${this.apiUrl}/security/two-factor/verify`, dto);
  }

  // ============================================================================
  // Notification Preferences
  // ============================================================================

  /**
   * 取得通知偏好設定
   */
  getNotificationPreferences(): Observable<NotificationPreferenceResponseDto> {
    return this.http.get<NotificationPreferenceResponseDto>(`${this.apiUrl}/notifications/preferences`);
  }

  /**
   * 更新通知偏好設定
   */
  updateNotificationPreferences(dto: UpdateNotificationPreferenceDto): Observable<NotificationPreferenceResponseDto> {
    return this.http.put<NotificationPreferenceResponseDto>(`${this.apiUrl}/notifications/preferences`, dto);
  }

  // ============================================================================
  // Privacy Settings
  // ============================================================================

  /**
   * 取得隱私設定
   */
  getPrivacySettings(): Observable<PrivacySettingsResponseDto> {
    return this.http.get<PrivacySettingsResponseDto>(`${this.apiUrl}/privacy/settings`);
  }

  /**
   * 批次更新隱私設定
   */
  updatePrivacySettings(dto: BatchUpdatePrivacySettingsDto): Observable<PrivacySettingsResponseDto> {
    return this.http.put<PrivacySettingsResponseDto>(`${this.apiUrl}/privacy/settings`, dto);
  }

  // ============================================================================
  // Activity Logs
  // ============================================================================

  /**
   * 查詢活動記錄
   */
  getUserLogs(params: UserLogQueryDto): Observable<UserLogPagedResponseDto> {
    return this.http.get<UserLogPagedResponseDto>(`${this.apiUrl}/logs`, { params: params as any });
  }

  /**
   * 匯出活動記錄
   */
  exportUserLogs(params: UserLogQueryDto): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/logs/export`, {
      params: params as any,
      responseType: 'blob'
    });
  }

  // ============================================================================
  // Third Party Auth
  // ============================================================================

  /**
   * 取得已綁定的第三方帳號列表
   */
  getThirdPartyAuths(): Observable<ThirdPartyAuthListResponseDto> {
    return this.http.get<ThirdPartyAuthListResponseDto>(`${this.apiUrl}/third-party-auth`);
  }

  /**
   * 綁定第三方帳號
   */
  bindThirdPartyAuth(dto: BindThirdPartyDto): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(`${this.apiUrl}/third-party-auth/bind`, dto);
  }

  /**
   * 解除綁定第三方帳號
   */
  unbindThirdPartyAuth(dto: UnbindThirdPartyDto): Observable<ApiResponse> {
    return this.http.delete<ApiResponse>(`${this.apiUrl}/third-party-auth/${dto.authId}`);
  }

  // ============================================================================
  // Dashboard
  // ============================================================================

  /**
   * 取得會員儀表板資料
   */
  getDashboard(): Observable<MemberDashboardResponseDto> {
    return this.http.get<MemberDashboardResponseDto>(`${this.apiUrl}/dashboard`);
  }
}
