import { Component, OnInit, OnDestroy, HostListener, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { NotificationService } from '../../../core/services/notification.service';
import {
  NotificationDto,
  UnreadCountDto,
  NotificationQueryDto,
  SystemModuleDto,
  NotificationConstants
} from '../../../core/models/notification.models';

@Component({
  selector: 'app-notification-bell',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notification-bell.component.html',
  styleUrl: './notification-bell.component.css'
})
export class NotificationBellComponent implements OnInit, OnDestroy {
  isDropdownOpen = false;
  isLoading = false;
  isProcessing = false; // 新增：防止重複操作
  unreadCount = 0;
  unreadCountBySystem: { [key: string]: number } = {};
  notifications: NotificationDto[] = [];
  systemModules: SystemModuleDto[] = [];
  currentSystemFilter: string = 'ALL';
  currentPage = 1;
  pageSize = NotificationConstants.BELL_DROPDOWN_SIZE;
  hasMore = false;
  private unreadCountSubscription?: Subscription;
  readonly SYSTEM_NAMES = NotificationConstants.SYSTEM_NAMES;

  constructor(
    private notificationService: NotificationService,
    private router: Router,
    private elementRef: ElementRef
  ) { }

  ngOnInit(): void {
    this.unreadCountSubscription = this.notificationService.unreadCount$.subscribe(
      (count: UnreadCountDto) => {
        this.unreadCount = count.totalCount;
        this.unreadCountBySystem = count.systemCounts;
        this.updateSystemModulesUnreadCount();
      }
    );
    this.notificationService.refreshUnreadCount();
  }

  ngOnDestroy(): void {
    if (this.unreadCountSubscription) {
      this.unreadCountSubscription.unsubscribe();
    }
  }

  toggleDropdown(): void {
    this.isDropdownOpen = !this.isDropdownOpen;
    if (this.isDropdownOpen) {
      this.loadSystemModules();
      this.loadNotifications();
    } else {
      this.resetState();
    }
  }

  closeDropdown(): void {
    if (this.isDropdownOpen) {
      this.isDropdownOpen = false;
      this.resetState();
    }
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    const clickedInside = this.elementRef.nativeElement.contains(target);
    if (!clickedInside && this.isDropdownOpen) {
      this.closeDropdown();
    }
  }

  @HostListener('document:keydown.escape')
  onEscapeKey(): void {
    if (this.isDropdownOpen) {
      this.closeDropdown();
    }
  }

  loadSystemModules(): void {
    this.notificationService.getSystemModules().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.systemModules = response.data;
          this.updateSystemModulesUnreadCount();
        }
      },
      error: (error) => {
        console.error('載入系統模組失敗:', error);
      }
    });
  }

  private updateSystemModulesUnreadCount(): void {
    if (this.systemModules.length > 0) {
      this.systemModules.forEach(module => {
        module.unreadCount = this.unreadCountBySystem[module.systemCode] || 0;
      });
    }
  }

  loadNotifications(append: boolean = false): void {
    if (this.isLoading) return;
    this.isLoading = true;

    const query: NotificationQueryDto = {
      page: this.currentPage,
      pageSize: this.pageSize,
      systemCode: this.currentSystemFilter === 'ALL' ? undefined : this.currentSystemFilter
    };

    this.notificationService.getNotifications(query).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          if (append) {
            this.notifications = [...this.notifications, ...response.data.items];
          } else {
            this.notifications = response.data.items;
          }
          this.hasMore = response.data.hasNext;
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('載入通知清單失敗:', error);
        this.isLoading = false;
      }
    });
  }

  changeSystemFilter(systemCode: string): void {
    if (this.currentSystemFilter === systemCode) return;
    this.currentSystemFilter = systemCode;
    this.currentPage = 1;
    this.notifications = [];
    this.hasMore = false;
    this.loadNotifications();
  }

  loadMore(): void {
    if (this.hasMore && !this.isLoading) {
      this.currentPage++;
      this.loadNotifications(true);
    }
  }

  onScroll(event: Event): void {
    const element = event.target as HTMLElement;
    const threshold = 50;
    const atBottom = element.scrollHeight - element.scrollTop - element.clientHeight < threshold;
    if (atBottom && !this.isLoading && this.hasMore) {
      this.loadMore();
    }
  }

  markAsRead(recipientId: number): void {
    const notification = this.notifications.find(n => n.recipientId === recipientId);
    if (!notification || notification.isRead) return;

    this.notificationService.markAsRead(recipientId).subscribe({
      next: (response) => {
        if (response.success) {
          notification.isRead = true;
          notification.readAt = new Date().toISOString();
          this.notificationService.refreshUnreadCount();
        }
      },
      error: (error) => {
        console.error('標記已讀失敗:', error);
      }
    });
  }

  /**
   * 標記所有通知為已讀
   */
  markAllAsRead(): void {
    // 防止重複操作
    if (this.isProcessing) {
      console.log('操作進行中，請稍候...');
      return;
    }

    // 檢查是否有通知
    if (this.notifications.length === 0) {
      console.log('沒有通知可標記');
      this.showMessage('目前沒有通知');
      return;
    }

    // 檢查是否有未讀通知
    const unreadNotifications = this.notifications.filter(n => !n.isRead);
    if (unreadNotifications.length === 0) {
      console.log('所有通知都已讀');
      this.showMessage('所有通知都已讀');
      return;
    }

    console.log('開始標記所有通知為已讀...');
    this.isProcessing = true;

    const systemCode = this.currentSystemFilter === 'ALL' ? undefined : this.currentSystemFilter;

    this.notificationService.markAllAsRead(systemCode).subscribe({
      next: (response) => {
        console.log('標記已讀 API 回應:', response);

        if (response.success) {
          // 更新本地狀態
          this.notifications.forEach(n => {
            n.isRead = true;
            n.readAt = new Date().toISOString();
          });

          // 重新載入未讀數量
          this.notificationService.refreshUnreadCount();

          const message = response.message || `已標記 ${response.data} 則通知為已讀`;
          console.log('成功:', message);
          this.showMessage(message);
        } else {
          console.error('標記已讀失敗:', response.message);
          this.showMessage(response.message || '標記已讀失敗');
        }

        this.isProcessing = false;
      },
      error: (error) => {
        console.error('全部標記已讀失敗:', error);
        this.showMessage('標記已讀失敗，請稍後再試');
        this.isProcessing = false;
      }
    });
  }

  /**
   * 清空所有通知
   */
  clearAll(): void {
    // 防止重複操作
    if (this.isProcessing) {
      console.log('操作進行中，請稍候...');
      return;
    }

    // 檢查是否有通知
    if (this.notifications.length === 0) {
      console.log('沒有通知可清空');
      this.showMessage('目前沒有通知');
      return;
    }

    // 確認操作
    if (!confirm('確定要清空所有通知嗎？此操作無法復原。')) {
      return;
    }

    console.log('開始清空通知...');
    this.isProcessing = true;

    const systemCode = this.currentSystemFilter === 'ALL' ? undefined : this.currentSystemFilter;

    this.notificationService.clearAllNotifications(systemCode).subscribe({
      next: (response) => {
        console.log('清空通知 API 回應:', response);

        if (response.success) {
          // 清空本地通知列表
          this.notifications = [];
          this.currentPage = 1;
          this.hasMore = false;

          // 重新載入未讀數量
          this.notificationService.refreshUnreadCount();

          const message = response.message || `已清空 ${response.data} 則通知`;
          console.log('成功:', message);
          this.showMessage(message);
        } else {
          console.error('清空通知失敗:', response.message);
          this.showMessage(response.message || '清空通知失敗');
        }

        this.isProcessing = false;
      },
      error: (error) => {
        console.error('清空通知失敗:', error);
        this.showMessage('清空通知失敗，請稍後再試');
        this.isProcessing = false;
      }
    });
  }

  /**
   * 顯示訊息提示（簡易版，可替換為 toast 組件）
   */
  private showMessage(message: string): void {
    // 這裡可以替換為你的 toast/notification 組件
    // 暫時使用 alert 作為示範
    console.log('[訊息]', message);

    // 如果你有 toast 服務，可以這樣用：
    // this.toastService.show(message);
  }

  formatRelativeTime(dateString: string): string {
    return this.notificationService.formatRelativeTime(dateString);
  }

  getUnreadCountDisplay(): string {
    if (this.unreadCount === 0) return '';
    if (this.unreadCount > 99) return '99+';
    return this.unreadCount.toString();
  }

  /**
   * 檢查是否有未讀通知（用於 UI 樣式）
   */
  get hasUnreadNotifications(): boolean {
    return this.notifications.some(n => !n.isRead);
  }

  /**
   * 檢查所有通知是否都已讀
   */
  get isAllNotificationsRead(): boolean {
    return this.notifications.length === 0 || this.notifications.every(n => n.isRead);
  }

  /**
   * 檢查是否有通知
   */
  get hasNotifications(): boolean {
    return this.notifications.length > 0;
  }

  trackByRecipientId(index: number, notification: NotificationDto): number {
    return notification.recipientId;
  }

  private resetState(): void {
    this.currentSystemFilter = 'ALL';
    this.currentPage = 1;
    this.notifications = [];
    this.systemModules = [];
    this.hasMore = false;
    this.isProcessing = false;
  }
}
