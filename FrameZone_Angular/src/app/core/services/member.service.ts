import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
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
  UserLogStatsResponseDto,
  ThirdPartyAuthListResponseDto,
  BindThirdPartyDto,
  UnbindThirdPartyDto,
  MemberDashboardResponseDto,
  ApiResponse,
  GetUserSessionsResponseDto,
  LogoutSessionResponseDto,
  GetAccountLockStatusResponseDto,
  GetSecurityOverviewResponseDto
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
   * 取得所有登入裝置
   */
  getUserSessions(): Observable<GetUserSessionsResponseDto> {
    return this.http.get<GetUserSessionsResponseDto>(`${this.apiUrl}/security/sessions`);
  }

  /**
   * 登出特定裝置
   */
  logoutSession(sessionId: number): Observable<LogoutSessionResponseDto> {
    return this.http.delete<LogoutSessionResponseDto>(`${this.apiUrl}/security/sessions/${sessionId}`);
  }

  /**
   * 登出所有其他裝置
   */
  logoutOtherSessions(): Observable<LogoutSessionResponseDto> {
    return this.http.delete<LogoutSessionResponseDto>(`${this.apiUrl}/security/sessions/others`);
  }

  /**
   * 取得帳號鎖定狀態
   */
  getAccountLockStatus(): Observable<GetAccountLockStatusResponseDto> {
    return this.http.get<GetAccountLockStatusResponseDto>(`${this.apiUrl}/security/lock-status`);
  }

  /**
   * 取得安全性概覽
   */
  getSecurityOverview(): Observable<GetSecurityOverviewResponseDto> {
    return this.http.get<GetSecurityOverviewResponseDto>(`${this.apiUrl}/security/overview`);
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
   * 分頁查詢活動記錄
   */
  getUserLogs(params: UserLogQueryDto): Observable<UserLogPagedResponseDto> {
    // 清理空值參數
    const cleanParams = this.cleanParams(params);
    return this.http.get<UserLogPagedResponseDto>(`${this.apiUrl}/logs`, { params: cleanParams });
  }

  /**
   * 取得活動記錄統計資料
   */
  getUserLogStats(): Observable<UserLogStatsResponseDto> {
    return this.http.get<UserLogStatsResponseDto>(`${this.apiUrl}/logs/stats`);
  }

  /**
   * 取得單筆活動記錄詳細資料
   */
  getUserLogById(logId: number): Observable<UserLogPagedResponseDto> {
    return this.http.get<UserLogPagedResponseDto>(`${this.apiUrl}/logs/${logId}`);
  }

  /**
   * 取得最近登入記錄
   */
  getRecentLoginLogs(count: number = 5): Observable<UserLogPagedResponseDto> {
    const params = new HttpParams().set('count', count.toString());
    return this.http.get<UserLogPagedResponseDto>(`${this.apiUrl}/logs/recent-logins`, { params });
  }

  /**
   * 匯出活動記錄為 CSV
   */
  exportUserLogs(params: UserLogQueryDto): Observable<Blob> {
    const cleanParams = this.cleanParams(params);
    return this.http.get(`${this.apiUrl}/logs/export`, {
      params: cleanParams,
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

  // ============================================================================
  // Helper Methods
  // ============================================================================

  /**
   * 清理空值參數（避免傳送 undefined 或 null 到後端）
   */
  private cleanParams(params: any): HttpParams {
    let httpParams = new HttpParams();

    Object.keys(params).forEach(key => {
      const value = params[key];
      if (value !== null && value !== undefined && value !== '') {
        httpParams = httpParams.set(key, value.toString());
      }
    });

    return httpParams;
  }
}
