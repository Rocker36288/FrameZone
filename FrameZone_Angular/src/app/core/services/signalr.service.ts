import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject, Observable } from 'rxjs';
import { NotificationDto, UnreadCountDto } from '../models/notification.models';

/**
 * SignalR 連線狀態
 */
export enum SignalRConnectionState {
  Disconnected = 'Disconnected',
  Connecting = 'Connecting',
  Connected = 'Connected',
  Reconnecting = 'Reconnecting',
  Disconnecting = 'Disconnecting'
}

/**
 * SignalR 服務 - 管理即時通知推送
 */
@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection?: signalR.HubConnection;
  private connectionState: SignalRConnectionState = SignalRConnectionState.Disconnected;

  // 通知事件
  private notificationReceived$ = new Subject<NotificationDto>();
  private unreadCountUpdated$ = new Subject<UnreadCountDto>();
  private connectionStateChanged$ = new Subject<SignalRConnectionState>();

  // Hub URL
  private readonly hubUrl = 'https://localhost:7213/hubs/notification';

  constructor() {}

  /**
   * 建立 SignalR 連線
   */
  public startConnection(token?: string): Promise<void> {
    if (this.hubConnection && this.connectionState === SignalRConnectionState.Connected) {
      console.log('🔔 SignalR 已連線，跳過重複連線');
      return Promise.resolve();
    }

    this.updateConnectionState(SignalRConnectionState.Connecting);

    // 建立 HubConnection
    const connectionBuilder = new signalR.HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        accessTokenFactory: () => token || this.getStoredToken() || '',
        skipNegotiation: false,
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.ServerSentEvents
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext) => {
          // 重連延遲策略：0秒、2秒、10秒、30秒，之後每 60 秒重試
          if (retryContext.previousRetryCount === 0) return 0;
          if (retryContext.previousRetryCount === 1) return 2000;
          if (retryContext.previousRetryCount === 2) return 10000;
          if (retryContext.previousRetryCount === 3) return 30000;
          return 60000;
        }
      })
      .configureLogging(signalR.LogLevel.Information);

    this.hubConnection = connectionBuilder.build();

    // 註冊事件監聽
    this.registerEventHandlers();

    // 連線狀態監聽
    this.hubConnection.onreconnecting(() => {
      console.log('🔄 SignalR 重新連線中...');
      this.updateConnectionState(SignalRConnectionState.Reconnecting);
    });

    this.hubConnection.onreconnected(() => {
      console.log('✅ SignalR 重新連線成功');
      this.updateConnectionState(SignalRConnectionState.Connected);
    });

    this.hubConnection.onclose((error) => {
      console.error('🔴 SignalR 連線關閉', error);
      this.updateConnectionState(SignalRConnectionState.Disconnected);
    });

    // 開始連線
    return this.hubConnection
      .start()
      .then(() => {
        console.log('✅ SignalR 連線成功');
        this.updateConnectionState(SignalRConnectionState.Connected);
      })
      .catch((error) => {
        console.error('❌ SignalR 連線失敗:', error);
        this.updateConnectionState(SignalRConnectionState.Disconnected);
        throw error;
      });
  }

  /**
   * 停止連線
   */
  public stopConnection(): Promise<void> {
    if (!this.hubConnection || this.connectionState === SignalRConnectionState.Disconnected) {
      return Promise.resolve();
    }

    this.updateConnectionState(SignalRConnectionState.Disconnecting);

    return this.hubConnection
      .stop()
      .then(() => {
        console.log('🔕 SignalR 連線已關閉');
        this.updateConnectionState(SignalRConnectionState.Disconnected);
        this.hubConnection = undefined;
      })
      .catch((error) => {
        console.error('❌ SignalR 關閉連線失敗:', error);
        this.updateConnectionState(SignalRConnectionState.Disconnected);
        throw error;
      });
  }

  /**
   * 註冊 SignalR 事件處理器
   */
  private registerEventHandlers(): void {
    if (!this.hubConnection) return;

    // 監聽「收到新通知」事件
    this.hubConnection.on('ReceiveNotification', (notification: NotificationDto) => {
      console.log('🔔 收到新通知:', notification);
      this.notificationReceived$.next(notification);
    });

    // 監聽「未讀數更新」事件
    this.hubConnection.on('UnreadCountUpdated', (unreadCount: UnreadCountDto) => {
      console.log('🔢 未讀數更新:', unreadCount);
      this.unreadCountUpdated$.next(unreadCount);
    });
  }

  /**
   * 取得通知接收事件的 Observable
   */
  public onNotificationReceived(): Observable<NotificationDto> {
    return this.notificationReceived$.asObservable();
  }

  /**
   * 取得未讀數更新事件的 Observable
   */
  public onUnreadCountUpdated(): Observable<UnreadCountDto> {
    return this.unreadCountUpdated$.asObservable();
  }

  /**
   * 取得連線狀態變更事件的 Observable
   */
  public onConnectionStateChanged(): Observable<SignalRConnectionState> {
    return this.connectionStateChanged$.asObservable();
  }

  /**
   * 取得當前連線狀態
   */
  public getConnectionState(): SignalRConnectionState {
    return this.connectionState;
  }

  /**
   * 檢查是否已連線
   */
  public isConnected(): boolean {
    return this.connectionState === SignalRConnectionState.Connected;
  }

  /**
   * 更新連線狀態並發射事件
   */
  private updateConnectionState(state: SignalRConnectionState): void {
    this.connectionState = state;
    this.connectionStateChanged$.next(state);
  }

  /**
   * 從 localStorage 取得 JWT Token
   */
  private getStoredToken(): string | null {
    // 根據你的 AuthService 實際 token 儲存位置調整
    return localStorage.getItem('authToken');
  }
}
