import { Component, EventEmitter, Input, OnDestroy, Output, inject } from '@angular/core';
import { Subscription } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { ChatService } from '../services/chat.service';
import { ChatRoomDto } from '../models/ChatRoomDto';
import { MessageDto } from '../models/MessageDto';

interface ViewMessage {
  id?: number;
  content: string;
  isOwn: boolean;
  time: string;
  createdAt?: string;
  senderId?: number;
  displayName: string;
  avatar: string;
}

@Component({
  selector: 'app-social-chat',
  imports: [],
  templateUrl: './social-chat.component.html',
  styleUrl: './social-chat.component.css'
})
export class SocialChatComponent implements OnDestroy {
  @Input() selectedFriend: { id: number; name: string; avatar?: string } | null = null;
  @Output() closed = new EventEmitter<void>();

  private chatService = inject(ChatService);
  private authService = inject(AuthService);

  isOpen = false;
  room: ChatRoomDto | null = null;
  loading = false;
  error: string | null = null;
  messages: ViewMessage[] = [];
  private currentUserId: number | null = null;
  private messageSub = new Subscription();

  ngOnChanges() {
    if (this.selectedFriend) {
      this.openForFriend(this.selectedFriend);
    }
  }

  toggleChat() {
    this.isOpen = !this.isOpen;
    if (!this.isOpen) {
      this.teardownRealtime();
      this.closed.emit();
    }
  }

  private openForFriend(friend: { id: number; name: string }) {
    this.teardownRealtime();
    this.isOpen = true;
    this.loading = true;
    this.error = null;
    this.messages = [];
    this.currentUserId = this.authService.getCurrentUser()?.userId ?? null;
    this.chatService.createSocialPrivateRoom(friend.id).subscribe({
      next: room => {
        this.room = room;
        this.loadMessages(room.roomId);
        this.chatService.connectToRoom(room.roomId);
        this.listenRealtime();
        this.loading = false;
      },
      error: () => {
        this.error = '開啟聊天室失敗';
        this.loading = false;
      }
    });
  }

  private loadMessages(roomId: number) {
    this.chatService.getMessages(roomId).subscribe({
      next: messages => {
        this.messages = messages.map(message => this.toViewMessage(message));
      },
      error: () => { }
    });
  }

  sendMessage(content: string) {
    if (!this.room) return;
    const text = content.trim();
    if (!text) return;
    this.chatService.sendMessage(this.room.roomId, text).subscribe({
      next: () => { },
      error: () => { }
    });
  }

  private listenRealtime() {
    this.messageSub.unsubscribe();
    this.messageSub = new Subscription();
    this.messageSub.add(
      this.chatService.messages$.subscribe(message => {
        if (!message) return;
        this.messages = [...this.messages, this.toViewMessage(message)];
      })
    );
  }

  private teardownRealtime() {
    this.messageSub.unsubscribe();
    this.messageSub = new Subscription();
    this.chatService.disconnect();
  }

  private toViewMessage(message: MessageDto) {
    const isOwn = !!message.isOwner;
    const profile = this.getSenderProfile(isOwn);
    return {
      id: message.messageId,
      content: message.messageContent,
      isOwn,
      time: new Date(message.createdAt).toLocaleTimeString(),
      createdAt: message.createdAt,
      senderId: message.senderUserId,
      displayName: profile.name,
      avatar: profile.avatar
    };
  }

  ngOnDestroy(): void {
    this.teardownRealtime();
  }

  private getSenderProfile(isOwn: boolean) {
    const user = this.authService.getCurrentUser();
    const defaultAvatar = 'https://i.pravatar.cc/100?u=default';
    console.log(isOwn);
    if (isOwn) {
      return {
        name: user?.displayName || user?.account || '我',
        avatar: user?.avatar || defaultAvatar
      };
    }

    return {
      name: this.selectedFriend?.name || '好友',
      avatar: this.selectedFriend?.avatar || defaultAvatar
    };
  }

}
