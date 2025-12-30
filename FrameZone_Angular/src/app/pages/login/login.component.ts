import { AuthService } from './../../core/services/auth.service';
import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, AfterViewInit, NgZone } from '@angular/core';
import { RouterLink, Router, ActivatedRoute } from "@angular/router";
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { LoginRequestDto, GoogleLoginRequestDto } from '../../core/models/auth.models';
import { ToastrService } from 'ngx-toastr';

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
    private route: ActivatedRoute,
    private ngZone: NgZone,
    private toastr: ToastrService
  ) {
    console.log('📦 ToastrService 注入狀態:', this.toastr ? '成功' : '失敗');
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
    // 延遲載入以確保 DOM 完全準備好
    setTimeout(() => {
      this.loadGoogleSignIn();
    }, 100);
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

    // 輸入時清除錯誤訊息（toastr 會自動消失，這裡保留以便未來擴展）
    this.loginForm.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        // 如果需要在輸入時執行其他動作，可以在此處添加
      });
  }

  /**
   * 檢查 URL 參數
   */
  private checkQueryParams(): void {
    this.route.queryParams.pipe(takeUntil(this.destroy$)).subscribe(params => {
      if (params['message']) {
        this.toastr.success(params['message'], '✔ 提示');
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
        ? `帳號或Email至少需要 ${minLength} 個字元`
        : `密碼至少需要 ${minLength} 個字元`;
    }

    if (field.errors['maxlength']) {
      const maxLength = field.errors['maxlength'].requiredLength;
      return `長度不可超過 ${maxLength} 個字元`;
    }

    return '';
  }

  onSubmit(): void {
    // 標記所有欄位為已觸碰，已顯示驗證錯誤
    this.markFormAsTouched();

    // 前端基本驗證失敗，不發送請求
    if (this.loginForm.invalid) {
      this.toastr.warning('請檢查表單欄位', '⚠ 表單驗證失敗');
      return;
    }

    // 開始提交
    this.isSubmitting = true;

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
      // 顯示成功訊息
      this.toastr.success(
        `歡迎回來，${response.displayName || response.account || '用戶'}！`,
        '✔ 登入成功'
      );

      // 短暫延遲後導向，讓用戶看到成功訊息
      setTimeout(() => {
        const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
        this.router.navigate([returnUrl]);
      }, 800);
    } else {
      // 後端回覆失敗，顯示訊息
      this.toastr.error(response.message || '登入失敗', '✗ 登入失敗');
    }
  }

  /**
   * 處理錯誤回應
   * @param error
   */
  private handleError(error: any): void {
    this.isSubmitting = false;

    console.error('登入錯誤:', error);

    // 處理不同類型的錯誤
    let errorMessage = '登入時發生錯誤，請稍後再試';

    if (error.error?.message) {
      // 後端返回的具體錯誤訊息
      errorMessage = error.error.message;
    } else if (error.status === 401) {
      errorMessage = '帳號或密碼錯誤';
    } else if (error.status === 0) {
      errorMessage = '無法連線到伺服器，請檢查網路連線';
    } else if (error.status === 500) {
      errorMessage = '伺服器錯誤，請稍後再試';
    }

    this.toastr.error(errorMessage, '✗ 登入失敗');
  }

  /**
   * 載入 Google Sign-In SDK
   */
  private loadGoogleSignIn(): void {
    console.log('🔍 開始載入 Google Sign-In SDK');

    if (typeof (window as any).google !== 'undefined') {
      console.log('✅ Google SDK 已載入');
      this.initializeGoogleSignIn();
    } else {
      console.log('⏳ 等待 Google SDK 載入...');
      setTimeout(() => this.loadGoogleSignIn(), 100);
    }
  }

  private initializeGoogleSignIn(): void {
    const google = (window as any).google;

    if (!google || !google.accounts) {
      console.error('❌ Google Identity Services 未載入');
      return;
    }

    console.log('🚀 初始化 Google Sign-In');

    // 初始化 Google Sign-In
    google.accounts.id.initialize({
      client_id: '836883046870-hl4oqsr1vatlgre0pfs7fn32ncpa6tkg.apps.googleusercontent.com',
      callback: (response: any) => {
        console.log('🔥 收到 Google 回應');
        // 使用 NgZone 確保在 Angular Zone 內執行
        this.ngZone.run(() => {
          this.handleGoogleSignIn(response);
        });
      },
      auto_select: false,
      cancel_on_tap_outside: true,
      // 重要：設定 ux_mode 為 popup 避免頁面重新整理
      ux_mode: 'popup',
      // 設定 context 為 signin
      context: 'signin'
    });

    // 渲染按鈕
    const buttonDiv = document.getElementById('googleSignInButton');
    if (buttonDiv) {
      // 清空容器
      buttonDiv.innerHTML = '';

      google.accounts.id.renderButton(
        buttonDiv,
        {
          theme: 'outline',
          size: 'large',
          width: buttonDiv.offsetWidth || 400,
          text: 'signin_with',
          shape: 'rectangular',
          logo_alignment: 'left'
        }
      );
      console.log('✅ Google 按鈕已渲染');
      this.googleButtonReady = true;
    } else {
      console.error('❌ 找不到 googleSignInButton 元素');
    }
  }

  /**
   * 處理 Google Sign-In 回應
   */
  private handleGoogleSignIn(response: any): void {
    console.log('🔍 開始處理 Google 登入');

    if (!response.credential) {
      console.error('❌ 沒有收到 Google credential');
      this.toastr.error('Google 登入失敗，請重試', '✗ 登入錯誤');
      return;
    }

    this.isSubmitting = true;

    const googleLoginData: GoogleLoginRequestDto = {
      idToken: response.credential,
      rememberMe: this.loginForm.get('rememberMe')?.value || false
    };

    console.log('📤 發送 Google 登入請求到後端');

    this.authService.googleLogin(googleLoginData)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          console.log('✅ 後端回應:', res);
          this.handleGoogleLoginSuccess(res);
        },
        error: (error) => {
          console.error('❌ 後端錯誤:', error);
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
      console.log('🎉 Google 登入成功');

      // 顯示歡迎訊息
      const welcomeMessage = response.isNewUser
        ? `歡迎加入 FrameZone，${response.displayName || response.account || '新用戶'}！`
        : `歡迎回來，${response.displayName || response.account || '用戶'}！`;

      this.toastr.success(welcomeMessage, response.isNewUser ? '✔ 註冊成功' : '✔ 登入成功');

      // 短暫延遲後導向
      setTimeout(() => {
        const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
        this.router.navigate([returnUrl]);
      }, 800);
    } else {
      console.warn('⚠️ Google 登入失敗:', response.message);
      this.toastr.error(response.message || 'Google 登入失敗', '✗ 登入失敗');
    }
  }

  /**
   * 處理 Google 登入錯誤
   */
  private handleGoogleLoginError(error: any): void {
    this.isSubmitting = false;

    console.error('Google 登入錯誤:', error);

    let errorMessage = 'Google 登入時發生錯誤，請稍後再試';

    if (error.error?.message) {
      errorMessage = error.error.message;
    } else if (error.status === 0) {
      errorMessage = '無法連線到伺服器，請檢查網路連線';
    } else if (error.status === 500) {
      errorMessage = '伺服器錯誤，請稍後再試';
    }

    this.toastr.error(errorMessage, '✗ Google 登入失敗');
  }
}
