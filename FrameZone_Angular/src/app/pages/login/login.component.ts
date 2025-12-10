import { AuthService } from './../../core/services/auth.service';
import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { RouterLink, Router, ActivatedRoute } from "@angular/router";
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { LoginRequestDto } from '../../core/models/auth.models';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent implements OnInit, OnDestroy {
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
}

