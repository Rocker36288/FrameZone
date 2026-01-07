import { Injectable } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { SignalRService } from './signalr.service';
import { NotificationService } from './notification.service';
import { NotificationDto, UnreadCountDto } from '../models/notification.models';
import { Subscription } from 'rxjs';

/**
 * 通知處理服務 - 統一管理通知推送邏輯
 * 負責：Toast 顯示 + 未讀數更新
 */
@Injectable({
  providedIn: 'root'
})
export class NotificationHandlerService {
  private notificationSubscription?: Subscription;
  private unreadCountSubscription?: Subscription;

  constructor(
    private signalRService: SignalRService,
    private notificationService: NotificationService,
    private toastr: ToastrService
  ) {}

  /**
   * 初始化通知處理器（訂閱 SignalR 事件）
   */
  public initialize(): void {
    console.log('🎯 NotificationHandlerService 初始化');

    // 訂閱「收到新通知」事件
    this.notificationSubscription = this.signalRService
      .onNotificationReceived()
      .subscribe((notification: NotificationDto) => {
        this.handleNewNotification(notification);
      });

    // 訂閱「未讀數更新」事件
    this.unreadCountSubscription = this.signalRService
      .onUnreadCountUpdated()
      .subscribe((unreadCount: UnreadCountDto) => {
        this.handleUnreadCountUpdate(unreadCount);
      });
  }

  /**
   * 清理訂閱
   */
  public destroy(): void {
    console.log('🗑️ NotificationHandlerService 清理');
    this.notificationSubscription?.unsubscribe();
    this.unreadCountSubscription?.unsubscribe();
  }

  /**
   * 處理新通知
   */
  private handleNewNotification(notification: NotificationDto): void {
    console.log('📩 處理新通知:', notification);

    // 1. 顯示 Toast 通知
    this.showToast(notification);

    // 2. 刷新未讀數量
    this.notificationService.refreshUnreadCount();
  }

  /**
   * 處理未讀數更新
   */
  private handleUnreadCountUpdate(unreadCount: UnreadCountDto): void {
    console.log('🔢 處理未讀數更新:', unreadCount);

    // 直接更新 NotificationService 的 BehaviorSubject
    // 這樣 NotificationBellComponent 會自動收到更新
    this.notificationService['unreadCountSubject'].next(unreadCount);
  }

  /**
   * 顯示 Toast 通知
   */
  private showToast(notification: NotificationDto): void {
    const title = notification.notificationTitle;
    const message = this.truncateMessage(notification.notificationContent, 80);
    const icon = notification.categoryIcon;

    // 根據優先級選擇 Toast 類型
    switch (notification.priorityCode) {
      case 'HIGH':
      case 'URGENT':
        this.toastr.error(message, `${icon} ${title}`, this.getToastConfig());
        break;

      case 'MEDIUM':
        this.toastr.info(message, `${icon} ${title}`, this.getToastConfig());
        break;

      case 'LOW':
      default:
        this.toastr.success(message, `${icon} ${title}`, this.getToastConfig());
        break;
    }
  }

  /**
   * 取得 Toast 配置
   */
  private getToastConfig() {
    return {
      timeOut: 5000,
      closeButton: true,
      progressBar: true,
      positionClass: 'toast-top-right',
      enableHtml: false,
      tapToDismiss: true,
      newestOnTop: true
    };
  }

  /**
   * 截斷過長的訊息
   */
  private truncateMessage(message: string, maxLength: number): string {
    if (message.length <= maxLength) {
      return message;
    }
    return message.substring(0, maxLength) + '...';
  }
}
