import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import {
  LoginRequestDto,
  LoginResponseDto,
  RegisterRequestDto,
  RegisterResponseDto,
  ForgotPasswordRequestDto,
  ForgotPasswordResponseDto,
  ValidateResetTokenResponseDto,
  ResetPasswordRequestDto,
  ResetPasswordResponseDto,
  ChangePasswordRequestDto,
  ChangePasswordResponseDto,
  UserInfo,
  LinkGoogleAccountRequestDto,
  GoogleLoginRequestDto,
  GoogleLoginResponseDto,
  UnlinkGoogleAccountRequestDto,
  GoogleLinkedStatusResponseDto
} from '../models/auth.models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'https://localhost:7213/api/auth';

  private currentUserSubject = new BehaviorSubject<LoginResponseDto | null>(null);

  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    this.loadStoredUser();
  }

  /**
   * 從 storage 載入已儲存的用戶資訊
   */
  private loadStoredUser() {
    const storedUser = localStorage.getItem('currentUser') || sessionStorage.getItem('currentUser');
    if (storedUser) {
      try {
        const user = JSON.parse(storedUser);
        this.currentUserSubject.next(user);
      } catch (error) {
        console.error('Failed to parse stored user:', error);
        this.clearStorage();
      }
    }
  }

  /**
   * 登入
   * @param loginData
   * @returns
   */
  login(loginData: LoginRequestDto): Observable<LoginResponseDto> {
    return this.http.post<LoginResponseDto>(`${this.apiUrl}/login`, loginData)
      .pipe(
        tap(response => {
          if (response.success && response.token) {
            this.handleLoginSuccess(response, loginData.rememberMe);
          }
        })
      )
  }

  /**
   * 登入成功後的本地操作
   * @param response
   * @param rememberMe
   */
  private handleLoginSuccess(response: LoginResponseDto, rememberMe: boolean): void {
    // 儲存 Token
    if (response.token) {
      localStorage.setItem('authToken', response.token);
    }

    // 根據「記住我」決定儲存位置
    const storage = rememberMe ? localStorage : sessionStorage;
    storage.setItem('currentUser', JSON.stringify(response));

    // 更新狀態
    this.currentUserSubject.next(response);
  }

  /**
   * 註冊
   * @param registerData
   * @returns
   */
  register(registerData: RegisterRequestDto): Observable<RegisterResponseDto> {
    return this.http.post<RegisterResponseDto>(`${this.apiUrl}/register`, registerData);
  }

  /**
   * 登出
   */
  logout() {
    this.clearStorage();
    this.currentUserSubject.next(null);
  }

  /**
   * 更新用戶 Session 資料（用於個人資料更新後同步）
   * @param updatedUser 更新後的用戶資料
   */
  updateUserSession(updatedUser: Partial<LoginResponseDto>): void {
    const currentUser = this.currentUserSubject.value;

    if (currentUser) {
      // 合併更新的資料
      const mergedUser: LoginResponseDto = {
        ...currentUser,
        ...updatedUser
      };

      // 更新 localStorage（檢查是否存在於 localStorage，若不存在則檢查 sessionStorage）
      const isInLocalStorage = localStorage.getItem('currentUser');
      const isInSessionStorage = sessionStorage.getItem('currentUser');

      if (isInLocalStorage) {
        localStorage.setItem('currentUser', JSON.stringify(mergedUser));
      } else if (isInSessionStorage) {
        sessionStorage.setItem('currentUser', JSON.stringify(mergedUser));
      }

      // 更新 BehaviorSubject，觸發所有訂閱者更新
      this.currentUserSubject.next(mergedUser);

      console.log('用戶 Session 已更新:', mergedUser);
    }
  }

  /**
   * 清除所有儲存
   */
  private clearStorage() {
    localStorage.removeItem('authToken');
    localStorage.removeItem('currentUser');
    sessionStorage.removeItem('currentUser');
  }

  /**
   * 檢查是否已登入
   */
  isAuthenticated(): boolean {
    const token = this.getToken();
    const user = this.getCurrentUser();

    const isAuth = !!(token && user);

    console.log('isAuthenticated 檢查:', {
      hasToken: !!token,
      hasUser: !!user,
      result: isAuth
    });

    return isAuth;
  }

  /**
   * 取得當前 Token
   */
  getToken() {
    return localStorage.getItem('authToken');
  }

  /**
   * 取得當前用戶資訊
   */
  getCurrentUser(): LoginResponseDto | null {
    return this.currentUserSubject.value;
  }

  /**
   * 忘記密碼
   * @param data
   * @returns
   */
  forgotPassword(data: ForgotPasswordRequestDto): Observable<any> {
    return this.http.post<ForgotPasswordResponseDto>(`${this.apiUrl}/forgot-password`, data);
  }

  /**
   * 驗證重設密碼 Token
   * @param token
   * @returns
   */
  validateResetToken(token: string): Observable<ValidateResetTokenResponseDto> {
    const params = new HttpParams().set('token', token);
    return this.http.get<ValidateResetTokenResponseDto>(
      `${this.apiUrl}/validate-reset-token`,
      { params }
    );

  }

  /**
   * 重設密碼
   * @param data
   * @returns
   */
  resetPassword(data: ResetPasswordRequestDto): Observable<any> {
    return this.http.post<ResetPasswordRequestDto>(`${this.apiUrl}/reset-password`, data);
  }

  /**
   * 修改密碼
   * @param data
   * @returns
   */
  changePassword(data: ChangePasswordRequestDto): Observable<any> {
    return this.http.post<ChangePasswordRequestDto>(`${this.apiUrl}/change-password`, data);
  }


  /**
   * 測試連線
   */
  testApi(): Observable<any> {
    return this.http.get(`${this.apiUrl}/test`);
  }


  /**
   * 使用 Google 登入
   * @param googleLoginData Google 登入資料
   * @returns 登入結果
   */
  googleLogin(googleLoginData: GoogleLoginRequestDto): Observable<GoogleLoginResponseDto> {
    return this.http.post<GoogleLoginResponseDto>(`${this.apiUrl}/google-login`, googleLoginData)
      .pipe(
        tap(response => {
          if (response.success && response.token) {
            this.handleLoginSuccess(response, googleLoginData.rememberMe);
          }
        })
      );
  }

  /**
   * 綁定 Google 帳號
   * @param request 綁定請求資料
   * @returns 處理結果
   */
  linkGoogleAccount(request: LinkGoogleAccountRequestDto): Observable<any> {
    return this.http.post(`${this.apiUrl}/link-google`, request);
  }

  /**
   * 解除 Google 帳號綁定
   * @param request 解除綁定請求資料
   * @returns 處理結果
   */
  unlinkGoogleAccount(request: UnlinkGoogleAccountRequestDto): Observable<any> {
    return this.http.post(`${this.apiUrl}/unlink-google`, request);
  }

  /**
   * 檢查是否已綁定 Google 帳號
   * @returns 綁定狀態
   */
  isGoogleLinked(): Observable<GoogleLinkedStatusResponseDto> {
    return this.http.get<GoogleLinkedStatusResponseDto>(`${this.apiUrl}/google-linked`);
  }

}
