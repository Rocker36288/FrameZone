import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import {
  LoginRequestDto,
  LoginResponseDto,
  RegisterRequestDto,
  RegisterResponseDto,
  ChangePasswordRequestDto,
  ForgotPasswordRequestDto,
  ResetPasswordRequestDto
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
    return this.http.post<RegisterResponseDto>(`${this.apiUrl}/auth/register`, registerData);
  }

  /**
   * 登出
   */
  logout() {
    this.clearStorage();
    this.currentUserSubject.next(null);
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
  isAuthenticated() {
    return !!this.getToken();
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
  getCurrentUser() {
    return this.currentUserSubject.value;
  }

  /**
   * 忘記密碼
   * @param data
   * @returns
   */
  forgotPassword(data: ForgotPasswordRequestDto): Observable<any> {
    return this.http.post(`${this.apiUrl}/forgot-password`, data);
  }

  /**
   * 重設密碼
   * @param data
   * @returns
   */
  resetPassword(data: ResetPasswordRequestDto): Observable<any> {
    return this.http.post(`${this.apiUrl}/reset-password`, data);
  }


  changePassword(data: ChangePasswordRequestDto): Observable<any> {
    return this.http.post(`${this.apiUrl}/change-password`, data);
  }


  /**
   * 測試連線
   */
  testApi(): Observable<any> {
    return this.http.get(`${this.apiUrl}/test`);
  }

}
