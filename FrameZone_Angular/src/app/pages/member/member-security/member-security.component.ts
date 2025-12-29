import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MemberService } from '../../../core/services/member.service';
import {
  ChangePasswordDto,
  UserSessionDto,
  AccountLockStatusDto,
  UserLogDto,
  SecurityOverviewDto
} from '../../../core/models/member.models';

@Component({
  selector: 'app-member-security',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './member-security.component.html',
  styleUrl: './member-security.component.css'
})
export class MemberSecurityComponent implements OnInit {
  // 載入狀態
  isLoading = false;
  isLoadingSessions = false;
  isLoadingLockStatus = false;
  isLoadingRecentLogins = false;
  isChangingPassword = false;

  // 錯誤訊息
  errorMessage = '';
  passwordErrorMessage = '';
  sessionsErrorMessage = '';
  lockStatusErrorMessage = '';

  // 成功訊息
  successMessage = '';

  // 變更密碼表單
  changePasswordForm: ChangePasswordDto = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  };

  // 密碼強度
  passwordStrength = {
    score: 0,
    text: '',
    color: ''
  };

  // 登入裝置列表
  sessions: UserSessionDto[] = [];

  // 帳號鎖定狀態
  lockStatus: AccountLockStatusDto | null = null;

  // 最近登入記錄
  recentLogins: UserLogDto[] = [];

  // 安全性概覽
  securityOverview: SecurityOverviewDto | null = null;

  constructor(private memberService: MemberService) {}

  ngOnInit(): void {
    this.loadSecurityData();
  }

  /**
   * 載入所有安全性資料
   */
  loadSecurityData(): void {
    this.loadSessions();
    this.loadLockStatus();
    this.loadRecentLogins();
    this.loadSecurityOverview();
  }

  // ============================================================================
  // 變更密碼
  // ============================================================================

  /**
   * 變更密碼
   */
  changePassword(): void {
    this.passwordErrorMessage = '';
    this.successMessage = '';

    // 前端基本驗證
    if (!this.changePasswordForm.currentPassword) {
      this.passwordErrorMessage = '請輸入目前密碼';
      return;
    }

    if (!this.changePasswordForm.newPassword) {
      this.passwordErrorMessage = '請輸入新密碼';
      return;
    }

    if (!this.changePasswordForm.confirmPassword) {
      this.passwordErrorMessage = '請輸入確認密碼';
      return;
    }

    if (this.changePasswordForm.newPassword !== this.changePasswordForm.confirmPassword) {
      this.passwordErrorMessage = '新密碼與確認密碼不相符';
      return;
    }

    if (this.changePasswordForm.newPassword.length < 8) {
      this.passwordErrorMessage = '密碼長度至少需要 8 個字元';
      return;
    }

    this.isChangingPassword = true;

    this.memberService.changePassword(this.changePasswordForm).subscribe({
      next: (response) => {
        if (response.success) {
          this.successMessage = response.message;
          this.clearPasswordForm();
        } else {
          this.passwordErrorMessage = response.message;
        }
        this.isChangingPassword = false;
      },
      error: (error) => {
        console.error('變更密碼失敗:', error);
        this.passwordErrorMessage = '變更密碼失敗，請稍後再試';
        this.isChangingPassword = false;
      }
    });
  }

  /**
   * 清空密碼表單
   */
  clearPasswordForm(): void {
    this.changePasswordForm = {
      currentPassword: '',
      newPassword: '',
      confirmPassword: ''
    };
    this.passwordStrength = {
      score: 0,
      text: '',
      color: ''
    };
  }

  /**
   * 檢查密碼強度
   */
  checkPasswordStrength(): void {
    const password = this.changePasswordForm.newPassword;
    let score = 0;

    if (password.length >= 8) score++;
    if (password.length >= 12) score++;
    if (/[a-z]/.test(password)) score++;
    if (/[A-Z]/.test(password)) score++;
    if (/[0-9]/.test(password)) score++;
    if (/[^a-zA-Z0-9]/.test(password)) score++;

    if (score <= 2) {
      this.passwordStrength = { score, text: '弱', color: '#dc2626' };
    } else if (score <= 4) {
      this.passwordStrength = { score, text: '中等', color: '#f59e0b' };
    } else {
      this.passwordStrength = { score, text: '強', color: '#10b981' };
    }
  }

  // ============================================================================
  // 登入裝置管理
  // ============================================================================

  /**
   * 載入登入裝置列表
   */
  loadSessions(): void {
    this.isLoadingSessions = true;
    this.sessionsErrorMessage = '';

    this.memberService.getUserSessions().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.sessions = response.data;
        } else {
          this.sessionsErrorMessage = response.message;
        }
        this.isLoadingSessions = false;
      },
      error: (error) => {
        console.error('載入登入裝置失敗:', error);
        this.sessionsErrorMessage = '載入失敗，請稍後再試';
        this.isLoadingSessions = false;
      }
    });
  }

  /**
   * 登出特定裝置
   */
  logoutSession(sessionId: number): void {
    if (!confirm('確定要登出此裝置嗎？')) {
      return;
    }

    this.memberService.logoutSession(sessionId).subscribe({
      next: (response) => {
        if (response.success) {
          alert(response.message);
          this.loadSessions(); // 重新載入列表
        } else {
          alert(response.message);
        }
      },
      error: (error) => {
        console.error('登出裝置失敗:', error);
        alert('登出失敗，請稍後再試');
      }
    });
  }

  /**
   * 登出所有其他裝置
   */
  logoutOtherSessions(): void {
    if (!confirm('確定要登出所有其他裝置嗎？')) {
      return;
    }

    this.memberService.logoutOtherSessions().subscribe({
      next: (response) => {
        if (response.success) {
          alert(response.message);
          this.loadSessions(); // 重新載入列表
        } else {
          alert(response.message);
        }
      },
      error: (error) => {
        console.error('登出其他裝置失敗:', error);
        alert('登出失敗，請稍後再試');
      }
    });
  }

  /**
   * 取得裝置圖示
   */
  getDeviceIcon(deviceType: string): string {
    const icons: { [key: string]: string } = {
      'Desktop': '💻',
      'Mobile': '📱',
      'Tablet': '📲'
    };
    return icons[deviceType] || '💻';
  }

  // ============================================================================
  // 帳號鎖定狀態
  // ============================================================================

  /**
   * 載入帳號鎖定狀態
   */
  loadLockStatus(): void {
    this.isLoadingLockStatus = true;
    this.lockStatusErrorMessage = '';

    this.memberService.getAccountLockStatus().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.lockStatus = response.data;
        } else {
          this.lockStatusErrorMessage = response.message;
        }
        this.isLoadingLockStatus = false;
      },
      error: (error) => {
        console.error('載入帳號鎖定狀態失敗:', error);
        this.lockStatusErrorMessage = '載入失敗，請稍後再試';
        this.isLoadingLockStatus = false;
      }
    });
  }

  // ============================================================================
  // 最近登入記錄
  // ============================================================================

  /**
   * 載入最近登入記錄
   */
  loadRecentLogins(): void {
    this.isLoadingRecentLogins = true;

    this.memberService.getUserLogs({
      pageNumber: 1,
      pageSize: 10,
      actionType: 'Login'
    }).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.recentLogins = response.data.items;
        }
        this.isLoadingRecentLogins = false;
      },
      error: (error) => {
        console.error('載入最近登入記錄失敗:', error);
        this.isLoadingRecentLogins = false;
      }
    });
  }

  // ============================================================================
  // 安全性概覽
  // ============================================================================

  /**
   * 載入安全性概覽
   */
  loadSecurityOverview(): void {
    this.memberService.getSecurityOverview().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.securityOverview = response.data;
        }
      },
      error: (error) => {
        console.error('載入安全性概覽失敗:', error);
      }
    });
  }

  // ============================================================================
  // 輔助方法
  // ============================================================================

  /**
   * 格式化日期時間
   */
  formatDateTime(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleString('zh-TW', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit'
    });
  }

  /**
   * 格式化相對時間
   */
  formatRelativeTime(dateString: string): string {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) return '剛剛';
    if (diffMins < 60) return `${diffMins} 分鐘前`;
    if (diffHours < 24) return `${diffHours} 小時前`;
    if (diffDays < 7) return `${diffDays} 天前`;

    return this.formatDateTime(dateString);
  }

  /**
   * 取得登入狀態圖示
   */
  getStatusIcon(status: string): string {
    return status === 'Success' ? '✅' : '❌';
  }
}
