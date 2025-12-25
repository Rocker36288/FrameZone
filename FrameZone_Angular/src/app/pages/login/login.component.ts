import { AuthService } from './../../core/services/auth.service';
import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, AfterViewInit } from '@angular/core';
import { RouterLink, Router, ActivatedRoute } from "@angular/router";
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { LoginRequestDto, GoogleLoginRequestDto } from '../../core/models/auth.models';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent implements OnInit, OnDestroy, AfterViewInit {
  // 表單
  loginForm!: FormGroup;

  // UI 狀態
  successMessage: string = '';
  errorMessage: string = '';
  isSubmitting: boolean = false;
  showPassword: boolean = false;

  // LOGO 名稱
  logoName: string = "FrameZone";

  // Title 名稱
  titleName: string = "登入帳號";

  // Google 登入按鈕是否已準備好
  googleButtonReady: boolean = false;

  // 用於取消訂閱
  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.initializeForm();
  }

  ngOnInit(): void {
    this.checkQueryParams();
    this.redirectIfAuthenticated();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * 組件初始化完成後載入 Google Sign-In
   */
  ngAfterViewInit(): void {
    this.loadGoogleSignIn();
  }

  /**
   * 初始化表單
   */
  private initializeForm(): void {
    this.loginForm = this.fb.group({
      accountOrEmail: ['', [
        Validators.required,
        Validators.minLength(3),
        Validators.maxLength(100)
      ]],
      password: ['', [
        Validators.required,
        Validators.minLength(6),
        Validators.maxLength(50)
      ]],
      rememberMe: [false]
    });

    // 輸入時清除錯誤訊息
    this.loginForm.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.errorMessage = '');
  }

  /**
   * 檢查 URL 參數
   */
  private checkQueryParams(): void {
    this.route.queryParams.pipe(takeUntil(this.destroy$)).subscribe(params => {
      if (params['message']) {
        this.successMessage = params['message'];
      }
    });
  }

  /**
   * 如果已登入，導向首頁
   */
  private redirectIfAuthenticated(): void {
    const token = this.authService.getToken();
    const currentUser = this.authService.getCurrentUser();

    if (token && currentUser) {
      this.router.navigate(['/']);
    } else {
      console.log('未登入，顯示登入頁面');
    }
  }

  /**
   * 切換密碼顯示/隱藏
   */
  togglePassword(): void {
    this.showPassword = !this.showPassword;
    const passwordInput = document.getElementById('passwordInput') as HTMLInputElement;
    if (passwordInput) {
      passwordInput.type = this.showPassword ? 'text' : 'password';
    }
  }

  /**
   * 檢查欄位是否錯誤
   * @param fieldName
   * @returns
   */
  hasError(fieldName: string): boolean {
    const field = this.loginForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  /**
   * 取得欄位錯誤訊息
   * @param fieldName
   * @returns
   */
  getErrorMessage(fieldName: string): string {
    const field = this.loginForm.get(fieldName);

    if (!field || !field.errors || !field.touched) {
      return '';
    }

    // 基本驗證錯誤訊息
    if (field.errors['required']) {
      return fieldName === 'accountOrEmail' ? '請輸入帳號或Email' : '請輸入密碼';
    }

    if (field.errors['minlength']) {
      const minLength = field.errors['minlength'].requiredLength;
      return fieldName === 'accountOrEmail'
        ? `帳號或Email至少需要 ${minLength} 個字元 `
        : `密碼至少需要 ${minLength} 個字元 `;
    }

    if (field.errors['maxlength']) {
      const maxLength = field.errors['maxlength'].requiredLength;
      return `長度不可超過 ${maxLength} 個字元 `;
    }

    return '';
  }

  onSubmit(): void {
    // 標記所有欄位為已觸碰，已顯示驗證錯誤
    this.markFormAsTouched();

    // 前端基本驗證失敗，不發送請求
    if (this.loginForm.invalid) {
      return;
    }

    // 開始提交
    this.isSubmitting = true;
    this.errorMessage = '';

    const loginData: LoginRequestDto = this.loginForm.value;

    // 發送登入請求
    this.authService.login(loginData)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => this.handleSuccess(response),
        error: (error) => this.handleError(error)
      })
  }

  /**
   * 標記所有欄位為已觸碰
   */
  private markFormAsTouched(): void {
    Object.keys(this.loginForm.controls).forEach(key => {
      this.loginForm.get(key)?.markAsTouched();
    });
  }

  /**
   * 處理成功回應
   * @param response
   */
  private handleSuccess(response: any): void {
    this.isSubmitting = false;

    // 信任後端的 success 欄位
    if (response.success) {
      // 導向原本要去的頁面或首頁
      const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
      this.router.navigate([returnUrl]);
    } else {
      // 後端回覆失敗，顯示訊息
      this.errorMessage = response.message || '登入失敗';
    }
  }

  /**
   * 處理錯誤回應
   * @param error
   */
  private handleError(error: any): void {
    this.isSubmitting = false;

    // 直接使用後端返回的錯誤訊息
    if (error.error?.message) {
      this.errorMessage = error.error.message;
    } else {
      // 沒有具體訊息時的通用錯誤
      this.errorMessage = '登入時發生錯誤，請稍後在試';
    }
  }

  /**
   * 關閉成功訊息
   */
  dismissSuccessMessage(): void {
    this.successMessage = '';
  }

  /**
   * 關閉錯誤訊息
   */
  dismissErrorMessage(): void {
    this.errorMessage = '';
  }

  /**
   * 載入 Google Sign-In SDK
   */
  private loadGoogleSignIn(): void {
    if (typeof (window as any).google !== 'undefined') {
      this.initializeGoogleSignIn();
    } else {
      setTimeout(() => this.loadGoogleSignIn(), 100);
    }
  }

  private initializeGoogleSignIn(): void {
    const google = (window as any).google;

    if (!google || !google.accounts) {
      console.error('Google Identity Services 未載入');
      return;
    }

    // 初始化 Google Sign-In
    google.accounts.id.initialize({
      client_id: '836883046870-hl4oqsr1vatlgre0pfs7fn32ncpa6tkg.apps.googleusercontent.com',
      callback: (response: any) => {
        this.handleGoogleSignIn(response)
      },
      auto_select: false,
      cancel_on_tap_outside: true
    });

    // 渲染按鈕
    const buttonDiv = document.getElementById('googleSignInButton');
    if (buttonDiv) {
      google.accounts.id.renderButton(
        buttonDiv,
        {
          theme: 'outline',
          size: 'large',
          width: 400,
          text: 'signin_with',
          shape: 'rectangular',
          logo_alignment: 'left'
        }
      );
      console.log('Google 按鈕已渲染'); // 調試用
    } else {
      console.error('找不到 googleSignInButton 元素');
    }

    this.googleButtonReady = true;
  }

  /**
 * 處理 Google Sign-In 回應
 */
  private handleGoogleSignIn(response: any): void {
    if (!response.credential) {
      this.errorMessage = 'Google 登入失敗，請重試';
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    const googleLoginData: GoogleLoginRequestDto = {
      idToken: response.credential,
      rememberMe: this.loginForm.get('rememberMe')?.value || false
    };

    this.authService.googleLogin(googleLoginData)
    .pipe(takeUntil(this.destroy$))
    .subscribe({
      next: (res) => {
        console.log('後端回應:', res);
        this.handleGoogleLoginSuccess(res);
      },
      error: (error) => {
        console.error('後端錯誤:', error);
        this.handleGoogleLoginError(error);
      }
    });
  }

  /**
 * 處理 Google 登入成功
 */
  private handleGoogleLoginSuccess(response: any): void {
    this.isSubmitting = false;

    if (response.success) {
      // 顯示歡迎訊息（如果是新使用者）
      if (response.isNewUser) {
        this.successMessage = '歡迎加入 FrameZone！';
      }

      // 導向原本要去的頁面或首頁
      const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
      this.router.navigate([returnUrl]);
    } else {
      this.errorMessage = response.message || 'Google 登入失敗';
    }
  }

  /**
 * 處理 Google 登入錯誤
 */
  private handleGoogleLoginError(error: any): void {
    this.isSubmitting = false;

    if (error.error?.message) {
      this.errorMessage = error.error.message;
    } else {
      this.errorMessage = 'Google 登入時發生錯誤，請稍後再試';
    }
  }

  /**
   * 使用傳統方式觸發 Google 登入（點擊按鈕時）
   */
  onGoogleLogin(): void {
    const google = (window as any).google;
    if (google && google.accounts) {
      google.accounts.id.prompt();
    }
  }
}

