import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState
} from '@microsoft/signalr';
import { ChatRoomDto } from '../models/ChatRoomDto';
import { MessageDto, SendShopMessageDto } from '../models/MessageDto';
import { AuthService } from '../../core/services/auth.service';
import { RecentChat } from '../models/recent-chat.models';
import { UnreadCount } from '../models/unread-count.models';

@Injectable({ providedIn: 'root' })
export class ChatService {
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  private apiUrl = 'https://localhost:7213/api/chat';
  private hubUrl = 'https://localhost:7213/hubs/chat';

  private connection: HubConnection | null = null;
  private messagesSubject = new BehaviorSubject<MessageDto | null>(null);
  messages$ = this.messagesSubject.asObservable();
  private stateSubject = new BehaviorSubject<'disconnected' | 'connecting' | 'connected'>('disconnected');
  state$ = this.stateSubject.asObservable();

  createSocialPrivateRoom(targetUserId: number): Observable<ChatRoomDto> {
    return this.http.post<ChatRoomDto>(`${this.apiUrl}/private/social`, { targetUserId });
  }

  createGroupRoom(targetUserId: number): Observable<ChatRoomDto> {
    return this.http.post<ChatRoomDto>(`${this.apiUrl}/group`, { targetUserId });
  }

  createShoppingPrivateRoom(targetUserId: number): Observable<ChatRoomDto> {
    return this.http.post<ChatRoomDto>(`${this.apiUrl}/private/shopping`, { targetUserId });
  }

  getRecentSocialChats(): Observable<RecentChat[]> {
    return this.http.get<RecentChat[]>(`${this.apiUrl}/recent/social`);
  }

  getUnreadCounts(): Observable<UnreadCount[]> {
    return this.http.get<UnreadCount[]>(`${this.apiUrl}/unread/social`);
  }

  getMessages(roomId: number): Observable<MessageDto[]> {
    return this.http.get<MessageDto[]>(`${this.apiUrl}/${roomId}/messages`);
  }

  markRoomRead(roomId: number): Observable<{ readCount: number }> {
    return this.http.post<{ readCount: number }>(`${this.apiUrl}/${roomId}/read`, {});
  }

  sendMessage(roomId: number, messageContent: string): Observable<MessageDto> {
    return this.http.post<MessageDto>(`${this.apiUrl}/${roomId}/messages`, { messageContent });
  }

  sendShopMessage(roomId: number, payload: SendShopMessageDto): Observable<MessageDto> {
    return this.http.post<MessageDto>(`${this.apiUrl}/${roomId}/messages/shop`, payload);
  }

  connectToRoom(roomId: number) {
    if (this.connection) {
      this.disconnect();
    }

    this.stateSubject.next('connecting');
    this.connection = new HubConnectionBuilder()
      .withUrl(`${this.hubUrl}?roomId=${roomId}`, {
        accessTokenFactory: () => this.authService.getToken() ?? ''
      })
      .withAutomaticReconnect()
      .build();

    this.connection.on('ReceiveMessage', (message: MessageDto) => {
      this.messagesSubject.next(message);
    });

    this.connection
      .start()
      .then(() => this.stateSubject.next('connected'))
      .catch(error => {
        console.error('SignalR connection failed', error);
        this.stateSubject.next('disconnected');
      });
  }

  disconnect() {
    if (!this.connection) return;

    const connection = this.connection;
    this.connection = null;
    this.stateSubject.next('disconnected');

    if (connection.state === HubConnectionState.Connected || connection.state === HubConnectionState.Connecting) {
      connection.stop().catch(() => { });
    }
    this.messagesSubject.next(null);
  }
}
