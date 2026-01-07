import { Component, ElementRef, EventEmitter, HostListener, Input, OnDestroy, Output, ViewChild, inject } from '@angular/core';
import { Subscription } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { ChatService } from '../services/chat.service';
import { ChatRoomDto } from '../models/ChatRoomDto';
import { MessageDto } from '../models/MessageDto';
import { SocialChatStateService } from '../services/social-chat-state.service';

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
  @ViewChild('chatBody') chatBody?: ElementRef<HTMLElement>;
  @ViewChild('chatWindow') chatWindow?: ElementRef<HTMLElement>;
  @ViewChild('chatToggle') chatToggle?: ElementRef<HTMLElement>;

  private chatService = inject(ChatService);
  private authService = inject(AuthService);
  private chatState = inject(SocialChatStateService);

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

  private closeChat() {
    if (!this.isOpen) return;
    this.isOpen = false;
    this.teardownRealtime();
    this.closed.emit();
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    if (!this.isOpen) return;
    const target = event.target as Node | null;
    const chatEl = this.chatWindow?.nativeElement;
    const toggleEl = this.chatToggle?.nativeElement;
    if (chatEl?.contains(target) || toggleEl?.contains(target)) return;
    this.closeChat();
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
        this.scrollToBottom();
        this.markRoomRead(roomId);
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
        this.scrollToBottom();
        if (this.room) {
          this.markRoomRead(this.room.roomId);
        }
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
    const name = isOwn
      ? (user?.displayName || user?.account || '我')
      : (this.selectedFriend?.name || '好友');
    const initial = name.charAt(0).toUpperCase();
    const defaultAvatar = `https://ui-avatars.com/api/?name=${encodeURIComponent(initial)}&background=667eea&color=fff&size=128`;
    console.log(isOwn);
    if (isOwn) {
      return {
        name,
        avatar: user?.avatar || defaultAvatar
      };
    }

    return {
      name,
      avatar: this.selectedFriend?.avatar || defaultAvatar
    };
  }

  private scrollToBottom() {
    const el = this.chatBody?.nativeElement;
    if (!el) return;
    requestAnimationFrame(() => {
      requestAnimationFrame(() => {
        el.scrollTop = el.scrollHeight;
      });
    });
  }

  private markRoomRead(roomId: number) {
    this.chatService.markRoomRead(roomId).subscribe({
      next: () => this.chatState.requestUnreadRefresh(),
      error: () => { }
    });
  }

}
