import { AuthService } from './../../../core/services/auth.service';
import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { catchError, throwError, timeout } from 'rxjs';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-register',
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {
  // Logo
  logoName = "FrameZone";
  titleName = "建立新帳號";

  // 表單
  registerForm!: FormGroup;

  // 顯示密碼開關
  showPassword = false;
  showConfirmPassword = false;

  // 提交狀態
  isSubmitting = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private toastr: ToastrService
  ) {
    console.log('📦 ToastrService 注入狀態:', this.toastr ? '成功' : '失敗');
  }

  ngOnInit(): void {
    this.initForm();
  }

  /**
   * 初始化表單
   */
  private initForm(): void {
    this.registerForm = this.fb.group({
      account: ['', [
        Validators.required,
        Validators.minLength(3),
        Validators.maxLength(50),
        Validators.pattern(/^[a-zA-Z0-9_]+$/)
      ]],
      email: ['', [
        Validators.required,
        Validators.email
      ]],
      password: ['', [
        Validators.required,
        Validators.minLength(6),
        Validators.maxLength(50)
      ]],
      confirmPassword: ['', [
        Validators.required
      ]],
      phone: ['', [
        Validators.required,
        Validators.pattern(/^09\d{8}$/)
      ]],
    }, {
      validators: this.passwordMatchValidator
    });
  }

  /**
   * 檢查密碼與確認密碼是否相符
   */
  private passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password')?.value;
    const confirmPassword = control.get('confirmPassword')?.value;

    if (password && confirmPassword && password !== confirmPassword) {
      return { passwordMismatch: true };
    }

    return null;
  }

  /**
   * 切換密碼顯示/隱藏
   */
  togglePassword(): void {
    this.showPassword = !this.showPassword;
  }

  /**
   * 切換確認密碼顯示/隱藏
   */
  toggleConfirmPassword(): void {
    this.showConfirmPassword = !this.showConfirmPassword;
  }

  /**
   * 檢查欄位是否有錯誤
   */
  hasError(fieldName: string): boolean {
    const field = this.registerForm.get(fieldName);
    return !!(field && field.invalid && field.touched);
  }

  /**
   * 取得欄位的錯誤訊息
   */
  getErrorMessage(fieldName: string): string {
    const field = this.registerForm.get(fieldName);

    if (!field || !field.errors) {
      return '';
    }

    if (field.errors['required']) {
      const fieldNames: { [key: string]: string } = {
        'account': '帳號',
        'email': 'Email',
        'password': '密碼',
        'confirmPassword': '確認密碼',
        'phone': '手機號碼'
      };
      return `請輸入${fieldNames[fieldName] || fieldName}`;
    }

    if (field.errors['minlength']) {
      const requiredLength = field.errors['minlength'].requiredLength;
      return `至少需要${requiredLength}個字元`;
    }

    if (field.errors['maxlength']) {
      const requiredLength = field.errors['maxlength'].requiredLength;
      return `不能超過${requiredLength}個字元`;
    }

    if (field.errors['pattern']) {
      if (fieldName === 'account') {
        return '帳號只能包含英文字母、數字和底線';
      }
      if (fieldName === 'phone') {
        return '請輸入正確的手機號碼格式（09開頭共10碼）';
      }
    }

    if (field.errors['email']) {
      return 'Email 格式不正確';
    }

    return '輸入格式不正確';
  }

  /**
   * 檢查密碼是否不相符
   */
  hasPasswordMismatch(): boolean {
    const confirmPasswordField = this.registerForm.get('confirmPassword');
    return !!(
      this.registerForm.errors?.['passwordMismatch'] &&
      confirmPasswordField?.touched
    );
  }

  onSubmit(): void {
    // 如果表單無效，標記所有欄位為已觸碰並停止提交
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      this.toastr.warning('請檢查表單欄位', '⚠ 表單驗證失敗');
      return;
    }

    // 防止重複提交
    if (this.isSubmitting) {
      return;
    }

    // 開始提交
    this.isSubmitting = true;

    const registerData = {
      account: this.registerForm.value.account,
      email: this.registerForm.value.email,
      password: this.registerForm.value.password,
      confirmPassword: this.registerForm.value.confirmPassword,
      phone: this.registerForm.value.phone
    }

    console.log('開始註冊，資料：', {
      account: registerData.account,
      email: registerData.email,
      phone: registerData.phone
    });

    this.authService.register(registerData)
      .pipe(
        timeout(30000), // 30秒超時
        catchError(error => {
          console.error('註冊請求錯誤:', error);
          return throwError(() => error);
        })
      )
      .subscribe({
        next: (response) => {
          console.log('註冊回應:', response);

          if (response.success) {
            // 顯示成功訊息
            this.toastr.success(
              '帳號建立成功！即將前往登入頁面...',
              '✔ 註冊成功',
              { timeOut: 2000 }
            );

            // 延遲導向登入頁面
            setTimeout(() => {
              this.router.navigate(['/login'], {
                queryParams: { message: '註冊成功！請登入您的帳號' }
              });
            }, 1500);
          } else {
            // 後端回傳失敗訊息
            this.toastr.error(response.message || '註冊失敗', '✗ 註冊失敗');
            this.isSubmitting = false;
          }
        },
        error: (error) => {
          console.error('註冊錯誤:', error);
          console.log('錯誤類型:', error.name);
          console.log('錯誤狀態:', error.status);

          this.handleRegistrationError(error);
          this.isSubmitting = false;
        },
        complete: () => {
          console.log('註冊請求完成');
          // 防禦性程式設計：確保狀態被重設
          if (this.isSubmitting) {
            this.isSubmitting = false;
          }
        }
      });
  }

  /**
   * 處理註冊錯誤
   */
  private handleRegistrationError(error: any): void {
    // 處理超時錯誤
    if (error.name === 'TimeoutError') {
      this.toastr.error('請求超時，請檢查網路連線或稍後再試', '✗ 超時錯誤');
      return;
    }

    // 處理不同類型的錯誤訊息
    let errorMessage = '註冊失敗，請稍後再試';

    if (error.status === 400) {
      // 處理驗證錯誤
      if (error.error?.errors) {
        // ASP.NET Core 的 ModelState 驗證錯誤格式
        const validationErrors: string[] = [];

        Object.keys(error.error.errors).forEach(key => {
          const errors = error.error.errors[key];
          if (Array.isArray(errors)) {
            validationErrors.push(...errors);
          } else {
            validationErrors.push(errors);
          }
        });

        errorMessage = validationErrors.join('、');
      } else if (error.error?.message) {
        // 自訂錯誤訊息
        errorMessage = error.error.message;
      } else if (error.error?.title) {
        // 標準錯誤格式
        errorMessage = error.error.title;
      } else {
        errorMessage = '註冊失敗，請檢查輸入的資料';
      }
    } else if (error.status === 0) {
      // 網路連線錯誤
      errorMessage = '無法連線到伺服器，請檢查後端服務是否正在運行';
    } else if (error.status === 500) {
      // 伺服器錯誤
      errorMessage = '伺服器錯誤，請稍後再試';
    } else if (error.error?.message) {
      // 其他錯誤
      errorMessage = error.error.message;
    }

    this.toastr.error(errorMessage, '✗ 註冊失敗');
  }
}
