import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ChatRoomDto } from '../models/ChatRoomDto';
import { MessageDto, SendShopMessageDto } from '../models/MessageDto';

@Injectable({ providedIn: 'root' })
export class ChatService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7213/api/chat';

  createSocialPrivateRoom(targetUserId: number): Observable<ChatRoomDto> {
    return this.http.post<ChatRoomDto>(`${this.apiUrl}/private/social`, { targetUserId });
  }

  createGroupRoom(targetUserId: number): Observable<ChatRoomDto> {
    return this.http.post<ChatRoomDto>(`${this.apiUrl}/group`, { targetUserId });
  }

  createShoppingPrivateRoom(targetUserId: number): Observable<ChatRoomDto> {
    return this.http.post<ChatRoomDto>(`${this.apiUrl}/private/shopping`, { targetUserId });
  }

  getMessages(roomId: number): Observable<MessageDto[]> {
    return this.http.get<MessageDto[]>(`${this.apiUrl}/${roomId}/messages`);
  }

  sendMessage(roomId: number, messageContent: string): Observable<MessageDto> {
    return this.http.post<MessageDto>(`${this.apiUrl}/${roomId}/messages`, { messageContent });
  }

  sendShopMessage(roomId: number, payload: SendShopMessageDto): Observable<MessageDto> {
    return this.http.post<MessageDto>(`${this.apiUrl}/${roomId}/messages/shop`, payload);
  }
}
