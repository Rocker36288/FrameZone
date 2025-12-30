import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MemberService } from '../../../core/services/member.service';
import {
  NotificationPreferenceDto,
  UpdateNotificationPreferenceDto
} from '../../../core/models/member.models';

@Component({
  selector: 'app-member-notifications',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './member-notifications.component.html',
  styleUrl: './member-notifications.component.css'
})
export class MemberNotificationsComponent implements OnInit {
  // 載入狀態
  isLoading = false;
  isSaving = false;

  // 錯誤訊息
  errorMessage = '';

  // 成功訊息
  successMessage = '';

  // 通知偏好設定
  preferences: NotificationPreferenceDto | null = null;

  // 表單資料（用於雙向綁定）
  form: UpdateNotificationPreferenceDto = {
    emailNotification: false,
    smsNotification: false,
    pushNotification: false,
    marketingEmail: false,
    orderUpdate: false,
    promotionAlert: false,
    systemAnnouncement: false
  };

  // 是否有變更（用於啟用/停用儲存按鈕）
  hasChanges = false;

  constructor(private memberService: MemberService) {}

  ngOnInit(): void {
    this.loadNotificationPreferences();
  }

  /**
   * 載入通知偏好設定
   */
  loadNotificationPreferences(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.memberService.getNotificationPreferences().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.preferences = response.data;
          this.initializeForm();
        } else {
          this.errorMessage = response.message;
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('載入通知偏好設定失敗:', error);
        this.errorMessage = '載入失敗，請稍後再試';
        this.isLoading = false;
      }
    });
  }

  /**
   * 初始化表單資料
   */
  initializeForm(): void {
    if (this.preferences) {
      this.form = {
        emailNotification: this.preferences.emailNotification,
        smsNotification: this.preferences.smsNotification,
        pushNotification: this.preferences.pushNotification,
        marketingEmail: this.preferences.marketingEmail,
        orderUpdate: this.preferences.orderUpdate,
        promotionAlert: this.preferences.promotionAlert,
        systemAnnouncement: this.preferences.systemAnnouncement
      };
      this.hasChanges = false;
    }
  }

  /**
   * 標記表單已變更
   */
  markAsChanged(): void {
    this.hasChanges = true;
    this.successMessage = '';
    this.errorMessage = '';
  }

  /**
   * 儲存通知偏好設定
   */
  savePreferences(): void {
    this.isSaving = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.memberService.updateNotificationPreferences(this.form).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.preferences = response.data;
          this.successMessage = response.message;
          this.hasChanges = false;

          // 3 秒後自動隱藏成功訊息
          setTimeout(() => {
            this.successMessage = '';
          }, 3000);
        } else {
          this.errorMessage = response.message;
        }
        this.isSaving = false;
      },
      error: (error) => {
        console.error('更新通知偏好設定失敗:', error);
        this.errorMessage = '更新失敗，請稍後再試';
        this.isSaving = false;
      }
    });
  }

  /**
   * 重設表單（恢復到上次儲存的狀態）
   */
  resetForm(): void {
    this.initializeForm();
    this.errorMessage = '';
    this.successMessage = '';
  }

  /**
   * 全部開啟
   */
  enableAll(): void {
    this.form.emailNotification = true;
    this.form.smsNotification = true;
    this.form.pushNotification = true;
    this.form.marketingEmail = true;
    this.form.orderUpdate = true;
    this.form.promotionAlert = true;
    this.form.systemAnnouncement = true;
    this.markAsChanged();
  }

  /**
   * 全部關閉
   */
  disableAll(): void {
    this.form.emailNotification = false;
    this.form.smsNotification = false;
    this.form.pushNotification = false;
    this.form.marketingEmail = false;
    this.form.orderUpdate = false;
    this.form.promotionAlert = false;
    this.form.systemAnnouncement = false;
    this.markAsChanged();
  }

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
}
