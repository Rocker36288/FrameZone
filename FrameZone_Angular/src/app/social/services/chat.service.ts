import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ChatRoomDto {
  roomId: number;
  roomType: string;
  roomCategory: string;
  roomName?: string;
}

export interface MessageDto {
  messageId: number;
  senderUserId: number;
  messageContent: string;
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class ChatService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7213/api/chat';

  createSocialPrivateRoom(targetUserId: number): Observable<ChatRoomDto> {
    return this.http.post<ChatRoomDto>(`${this.apiUrl}/private/social`, { targetUserId });
  }

  getMessages(roomId: number): Observable<MessageDto[]> {
    return this.http.get<MessageDto[]>(`${this.apiUrl}/${roomId}/messages`);
  }

  sendMessage(roomId: number, messageContent: string): Observable<MessageDto> {
    return this.http.post<MessageDto>(`${this.apiUrl}/${roomId}/messages`, { messageContent });
  }
}
