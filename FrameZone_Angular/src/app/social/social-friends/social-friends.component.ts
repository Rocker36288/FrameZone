import { CommonModule } from '@angular/common';
import { Component, DestroyRef, EventEmitter, Output, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuthService } from '../../core/services/auth.service';
import { FollowUser } from '../models/follow.models';
import { RecentChat } from '../models/recent-chat.models';
import { ChatService } from '../services/chat.service';
import { FollowService } from '../services/follow.service';
interface Friend {
  id: number;
  name: string;
  avatar?: string | null;
}
@Component({
  selector: 'app-social-friends',
  imports: [CommonModule],
  templateUrl: './social-friends.component.html',
  styleUrl: './social-friends.component.css'
})

export class SocialFriendsComponent {
  @Output() friendSelected = new EventEmitter<Friend>();

  private authService = inject(AuthService);
  private followService = inject(FollowService);
  private chatService = inject(ChatService);
  private destroyRef = inject(DestroyRef);

  following: FollowUser[] = [];
  followers: FollowUser[] = [];
  recentChats: RecentChat[] = [];

  ngOnInit(): void {
    this.authService.currentUser$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(user => {
        if (!user?.userId) {
          this.following = [];
          this.followers = [];
          this.recentChats = [];
          return;
        }
        this.followService.getFollowing(user.userId).subscribe({
          next: (users) => {
            this.following = users;
          },
          error: () => {
            this.following = [];
          }
        });
        this.followService.getFollowers(user.userId).subscribe({
          next: (users) => {
            this.followers = users;
          },
          error: () => {
            this.followers = [];
          }
        });
        this.chatService.getRecentSocialChats().subscribe({
          next: (chats) => {
            this.recentChats = chats;
          },
          error: () => {
            this.recentChats = [];
          }
        });
      });
  }

  selectFriend(friend: Friend) {
    this.friendSelected.emit(friend);
  }

  getFollowAvatar(user: FollowUser): string {
    if (user.avatar) return user.avatar;
    const initial = (user.displayName || 'U').charAt(0).toUpperCase();
    return `https://ui-avatars.com/api/?name=${encodeURIComponent(initial)}&background=667eea&color=fff&size=128`;
  }

  getRecentAvatar(chat: RecentChat): string {
    if (chat.targetUserAvatar) return chat.targetUserAvatar;
    const initial = (chat.targetUserName || 'U').charAt(0).toUpperCase();
    return `https://ui-avatars.com/api/?name=${encodeURIComponent(initial)}&background=667eea&color=fff&size=128`;
  }
}
