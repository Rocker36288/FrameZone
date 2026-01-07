import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import {
  NotificationDto,
  UnreadCountDto,
  NotificationQueryDto,
  NotificationPagedResultDto,
  MarkAsReadDto,
  DeleteNotificationDto,
  SystemModuleDto,
  ServiceResult
} from '../models/notification.models';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private apiUrl = 'https://localhost:7213/api/notification';

  // 未讀數量快取（用於即時更新小鈴鐺）
  private unreadCountSubject = new BehaviorSubject<UnreadCountDto>({
    totalCount: 0,
    systemCounts: {}
  });
  public unreadCount$ = this.unreadCountSubject.asObservable();

  // ❌ 移除輪詢相關變數
  // private readonly POLL_INTERVAL = 30000;
  // private pollSubscription: any = null;

  constructor(private http: HttpClient) {}

  /**
   * 取得未讀通知數量
   */
  getUnreadCount(): Observable<ServiceResult<UnreadCountDto>> {
    return this.http.get<ServiceResult<UnreadCountDto>>(`${this.apiUrl}/unread-count`)
      .pipe(
        tap(response => {
          if (response.success && response.data) {
            this.unreadCountSubject.next(response.data);
          }
        })
      );
  }

  /**
   * 取得通知清單（分頁）
   */
  getNotifications(query: NotificationQueryDto): Observable<ServiceResult<NotificationPagedResultDto>> {
    let params = new HttpParams()
      .set('page', query.page.toString())
      .set('pageSize', query.pageSize.toString());

    if (query.systemCode) {
      params = params.set('systemCode', query.systemCode);
    }

    if (query.isUnreadOnly !== undefined) {
      params = params.set('isUnreadOnly', query.isUnreadOnly.toString());
    }

    return this.http.get<ServiceResult<NotificationPagedResultDto>>(`${this.apiUrl}`, { params });
  }

  /**
   * 取得單筆通知詳細資訊
   */
  getNotificationById(recipientId: number): Observable<ServiceResult<NotificationDto>> {
    return this.http.get<ServiceResult<NotificationDto>>(`${this.apiUrl}/${recipientId}`);
  }

  /**
   * 標記單筆通知為已讀
   */
  markAsRead(recipientId: number): Observable<ServiceResult<boolean>> {
    return this.http.put<ServiceResult<boolean>>(`${this.apiUrl}/${recipientId}/read`, null)
      .pipe(
        tap(response => {
          if (response.success) {
            this.refreshUnreadCount();
          }
        })
      );
  }

  /**
   * 批次標記通知為已讀
   */
  markBatchAsRead(dto: MarkAsReadDto): Observable<ServiceResult<number>> {
    return this.http.put<ServiceResult<number>>(`${this.apiUrl}/read-batch`, dto)
      .pipe(
        tap(response => {
          if (response.success) {
            this.refreshUnreadCount();
          }
        })
      );
  }

  /**
   * 標記所有通知為已讀
   */
  markAllAsRead(systemCode?: string): Observable<ServiceResult<number>> {
    let params = new HttpParams();
    if (systemCode) {
      params = params.set('systemCode', systemCode);
    }

    return this.http.put<ServiceResult<number>>(`${this.apiUrl}/read-all`, null, { params })
      .pipe(
        tap(response => {
          if (response.success) {
            this.refreshUnreadCount();
          }
        })
      );
  }

  /**
   * 刪除單筆通知
   */
  deleteNotification(recipientId: number): Observable<ServiceResult<boolean>> {
    return this.http.delete<ServiceResult<boolean>>(`${this.apiUrl}/${recipientId}`)
      .pipe(
        tap(response => {
          if (response.success) {
            this.refreshUnreadCount();
          }
        })
      );
  }

  /**
   * 批次刪除通知
   */
  deleteBatchNotifications(dto: DeleteNotificationDto): Observable<ServiceResult<number>> {
    return this.http.delete<ServiceResult<number>>(`${this.apiUrl}/delete-batch`, { body: dto })
      .pipe(
        tap(response => {
          if (response.success) {
            this.refreshUnreadCount();
          }
        })
      );
  }

  /**
   * 清空所有通知
   */
  clearAllNotifications(systemCode?: string): Observable<ServiceResult<number>> {
    let params = new HttpParams();
    if (systemCode) {
      params = params.set('systemCode', systemCode);
    }

    return this.http.delete<ServiceResult<number>>(`${this.apiUrl}/clear-all`, { params })
      .pipe(
        tap(response => {
          if (response.success) {
            this.refreshUnreadCount();
          }
        })
      );
  }

  /**
   * 取得系統模組清單（含未讀數）
   */
  getSystemModules(): Observable<ServiceResult<SystemModuleDto[]>> {
    return this.http.get<ServiceResult<SystemModuleDto[]>>(`${this.apiUrl}/systems`);
  }

  /**
   * 手動刷新未讀數量
   */
  refreshUnreadCount(): void {
    this.getUnreadCount().subscribe({
      error: (error) => {
        console.error('刷新未讀數量失敗:', error);
      }
    });
  }

  // ❌ 移除輪詢方法
  // startPolling(): void { ... }
  // stopPolling(): void { ... }

  /**
   * 取得當前未讀數量（同步）
   */
  getCurrentUnreadCount(): UnreadCountDto {
    return this.unreadCountSubject.value;
  }

  /**
   * 格式化相對時間
   */
  formatRelativeTime(dateString: string): string {
    const date = new Date(dateString);
    const now = new Date();
    const diffInSeconds = Math.floor((now.getTime() - date.getTime()) / 1000);

    if (diffInSeconds < 60) {
      return '剛剛';
    } else if (diffInSeconds < 3600) {
      const minutes = Math.floor(diffInSeconds / 60);
      return `${minutes} 分鐘前`;
    } else if (diffInSeconds < 86400) {
      const hours = Math.floor(diffInSeconds / 3600);
      return `${hours} 小時前`;
    } else if (diffInSeconds < 604800) {
      const days = Math.floor(diffInSeconds / 86400);
      return `${days} 天前`;
    } else {
      return date.toLocaleDateString('zh-TW', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit'
      });
    }
  }

  /**
   * 格式化完整日期時間
   */
  formatDateTime(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleString('zh-TW', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    });
  }
}
