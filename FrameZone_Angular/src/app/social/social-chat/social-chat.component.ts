import { Component, EventEmitter, inject, Input, Output } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';
import { ChatService } from '../services/chat.service';
import { ChatRoomDto } from '../models/ChatRoomDto';
import { MessageDto } from '../models/MessageDto';

@Component({
  selector: 'app-social-chat',
  imports: [],
  templateUrl: './social-chat.component.html',
  styleUrl: './social-chat.component.css'
})
export class SocialChatComponent {
  @Input() selectedFriend: { id: number; name: string } | null = null;
  @Output() closed = new EventEmitter<void>();

  private chatService = inject(ChatService);
  private authService = inject(AuthService);

  isOpen = false;
  room: ChatRoomDto | null = null;
  loading = false;
  error: string | null = null;
  messages: { content: string; isOwn: boolean; time: string }[] = [];
  private currentUserId: number | null = null;

  ngOnChanges() {
    if (this.selectedFriend) {
      this.openForFriend(this.selectedFriend);
    }
  }

  toggleChat() {
    this.isOpen = !this.isOpen;
    if (!this.isOpen) this.closed.emit();
  }

  private openForFriend(friend: { id: number; name: string }) {
    this.isOpen = true;
    this.loading = true;
    this.error = null;
    this.messages = [];
    this.currentUserId = this.authService.getCurrentUser()?.userId ?? null;
    this.chatService.createSocialPrivateRoom(friend.id).subscribe({
      next: room => {
        this.room = room;
        this.loadMessages(room.roomId);
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
      next: message => {
        this.messages = [...this.messages, this.toViewMessage(message)];
      },
      error: () => { }
    });
  }

  private toViewMessage(message: MessageDto) {
    const isOwn = this.currentUserId != null && message.senderUserId === this.currentUserId;
    return {
      content: message.messageContent,
      isOwn,
      time: new Date(message.createdAt).toLocaleTimeString()
    };
  }
}
