import { AuthService } from './../../../core/services/auth.service';
import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-reset-password',
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.css'
})
export class ResetPasswordComponent {
  resetForm!: FormGroup;
  isSubmitting = false;
  errorMessage = '';
  successMessage = '';
  token = '';
  isValidatingToken = true;
  isTokenValid = false;
  showNewPassword = false;
  showConfirmPassword = false;

  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.initForm();
  }

  ngOnInit(): void {
    // 從 URL query params 取得 token
    this.route.queryParams
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {
        this.token = params['token'] || '';
        if (this.token) {
          this.validateToken();
        } else {
          this.isValidatingToken = false;
          this.errorMessage = '無效的重設密碼連結';
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initForm(): void {
    this.resetForm = this.fb.group({
      newPassword: ['', [
        Validators.required,
        Validators.minLength(6),
        Validators.maxLength(50)
      ]],
      confirmPassword: ['', [Validators.required]],
    }, {
      validators: this.passwordMatchValidator
    });
  }

  private passwordMatchValidator(group: FormGroup): { [key: string]: boolean } | null {
    const newPassword = group.get('newPassword')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;

    if (newPassword && confirmPassword && newPassword !== confirmPassword) {
      return { passwordMismatch: true };
    }

    return null;
  }

  private validateToken(): void {
    this.isValidatingToken = true;
    this.authService.validateResetToken(this.token)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.isValidatingToken = false;
          this.isTokenValid = response.success;
          if(!response.success) {
            this.errorMessage = response.message || '此重設密碼連結已失效或已使用';
          }
        },
        error: (error) => {
          this.isValidatingToken = false;
          this.isTokenValid = false;
          this.errorMessage = error.error?.message || '驗證連結時發生錯誤';
        }
      });
  }

  toggleNewPasswordVisibility(): void {
    this.showNewPassword = !this.showNewPassword;
  }

  toggleConfirmPasswordVisibility(): void {
    this.showConfirmPassword = !this.showConfirmPassword;
  }

  hasError(fieldName: string, errorType?: string): boolean {
    const field = this.resetForm.get(fieldName);
    if (!field) return false;

    if (errorType) {
      return field.hasError(errorType) && (field.dirty || field.touched);
    }
    return field.invalid && (field.dirty || field.touched);
  }

  hasFormError(errorType: string): boolean {
    return this.resetForm.hasError(errorType) &&
           this.resetForm.get('confirmPassword')?.touched === true;
  }

  getPasswordStrengthClass(): string {
    const password = this.resetForm.get('newPassword')?.value || '';
    if (password.length === 0) return '';
    if (password.length < 6) return 'weak';

    const hasLetter = /[a-zA-Z]/.test(password);
    const hasNumber = /[0-9]/.test(password);
    const hasSpecial = /[!@#$%^&*(),.?":{}|<>]/.test(password);

    if (hasLetter && hasNumber && hasSpecial && password.Length >= 12) {
      return 'strong';
    } else if (hasLetter && hasNumber && password.length >= 8) {
      return 'medium';
    }
    return 'weak';
  }

  getPasswordStrengthText(): string {
    const strengthClass = this.getPasswordStrengthClass();
    switch (strengthClass) {
      case 'strong': return '強';
      case 'medium': return '中';
      case 'weak': return '弱';
      default: return '';
    }
  }

  onSubmit(): void {
    Object.keys(this.resetForm.controls).forEach(key => {
      this.resetForm.get(key)?.markAsTouched();
    });

    if (this.resetForm.invalid) {
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';
    this.successMessage = '';

    const formData = {
      token: this.token,
      newPassword: this.resetForm.value.newPassword,
      confirmPassword: this.resetForm.value.confirmPassword
    };

    this.authService.resetPassword(formData)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.handleSuccess(response.message || '密碼重設成功!');
        },
        error: (error) => {
          this.handleError(error);
        }
      });
  }

  private handleSuccess(message: string): void {
    this.isSubmitting = false;
    this.successMessage = message;
    this.resetForm.reset();

    setTimeout(() => {
      this.router.navigate(['/login'], {
        queryParams: { resetSuccess: 'true' }
      });
    }, 3000);
  }

  private handleError(error: any): void {
    this.isSubmitting = false;
    this.errorMessage = error.error?.message || '重設密碼時發生錯誤，請稍後在試';
  }

  dismissError(): void {
    this.errorMessage = '';
  }

  dismissSuccess(): void {
    this.successMessage = '';
  }

}
