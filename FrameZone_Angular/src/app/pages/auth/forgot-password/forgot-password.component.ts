import { Router } from '@angular/router';
import { AuthService } from './../../../core/services/auth.service';
import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { ForgotPasswordRequestDto } from '../../../core/models/auth.models';

@Component({
  selector: 'app-forgot-password',
  imports: [CommonModule, RouterLink, ReactiveFormsModule],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.css'
})
export class ForgotPasswordComponent implements OnInit, OnDestroy {
  // 表單
  forgotPasswordForm!: FormGroup;

  // UI 狀態
  successMessage: string = '';
  errorMessage: string = '';
  isSubmitting: boolean = false;

  // 用於取消訂閱
  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.initializeForm();
  }

  ngOnInit(): void {

  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * 初始化表單
   */
  private initializeForm(): void {
    this.forgotPasswordForm = this.fb.group({
      email: ['', [
        Validators.required,
        Validators.email,
        Validators.maxLength(100)
      ]]
    });

    this.forgotPasswordForm.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.errorMessage = '';
      });
  }

  /**
   * 檢查欄位是否有錯誤
   * @param fieldName 欄位名稱
   * @returns 是否有錯誤
   */
  hasError(fieldName: string): boolean {
    const field = this.forgotPasswordForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  /**
   * 取得欄位錯誤訊息
   */
  getErrorMessage(fieldName: string): string {
    const field = this.forgotPasswordForm.get(fieldName);

    if (!field || !field.errors || !field.touched) {
      return '';
    }

    // 必須驗證
    if (field.errors['required']) {
      return '請輸入 Email';
    }

    // Email 格式驗證
    if (field.errors['email']) {
      return '請輸入有效的 Email 格式';
    }

    // 長度驗證
    if (field.errors['maxLength']) {
      const maxLength = field.errors['maxlength'].requiredLength;
      return `Email 長度不可超過 ${maxLength} 個字`;
    }

    return '';
  }

  /**
   * 提交表單
   */
  onSubmit(): void {
    // 標記所有欄位為已觸碰，已顯示驗證錯誤
    this.markFormAsTouched();

    // 前端基本驗證失敗，不發送請求
    if (this.forgotPasswordForm.invalid) {
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';
    this.successMessage = '';

    // 準備要發送的資料
    const requestData: ForgotPasswordRequestDto = {
      email: this.forgotPasswordForm.value.email
    };

    // 發送 API 請求
    this.authService.forgotPassword(requestData)
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
    Object.keys(this.forgotPasswordForm.controls).forEach(key => {
      this.forgotPasswordForm.get(key)?.markAsTouched();
    });
  }

  /**
   * 處理成功回應
   * @param response
   */
  private handleSuccess(response: any): void {
    this.isSubmitting = false;

    if (response.success) {
      // 顯示成功訊息
      this.successMessage = response.message || '重設密碼連結已發送到您的信箱，請查收';

      // 清空表單
      this.forgotPasswordForm.reset();

      setTimeout(() => {
        this.router.navigate(['/login']);
      }, 3000);
    } else {
      this.errorMessage = response.message || '發送失敗，請稍後再試';
    }
  }

  /**
   * 處理錯誤回應
   * @param error 錯誤物件
   */
  private handleError(error: any): void {
    this.isSubmitting = false;

    if (error.error?.message) {
      this.errorMessage = error.error.message;
    } else {
      this.errorMessage = '系統錯誤，請稍後再試';
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
