import { CommonModule } from '@angular/common';
import { Component, DestroyRef, EventEmitter, Output, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuthService } from '../../core/services/auth.service';
import { FollowUser } from '../models/follow.models';
import { RecentChat } from '../models/recent-chat.models';
import { UnreadCount } from '../models/unread-count.models';
import { ChatService } from '../services/chat.service';
import { FollowService } from '../services/follow.service';
import { SocialChatStateService } from '../services/social-chat-state.service';
interface Friend {
  id: number;
  name: string;
  avatar?: string | null;
}
@Component({
  selector: 'app-social-friends',
  imports: [CommonModule, FormsModule],
  templateUrl: './social-friends.component.html',
  styleUrl: './social-friends.component.css'
})

export class SocialFriendsComponent {
  @Output() friendSelected = new EventEmitter<Friend>();

  private authService = inject(AuthService);
  private followService = inject(FollowService);
  private chatService = inject(ChatService);
  private chatState = inject(SocialChatStateService);
  private destroyRef = inject(DestroyRef);

  following: FollowUser[] = [];
  followers: FollowUser[] = [];
  recentChats: RecentChat[] = [];
  unreadMap = new Map<number, number>();
  openSection: { recent: boolean; following: boolean; followers: boolean } = {
    recent: true,
    following: false,
    followers: false
  };
  searchTerm = '';
  private currentUserId: number | null = null;

  ngOnInit(): void {
    this.authService.currentUser$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(user => {
        this.currentUserId = user?.userId ?? null;
        if (!this.currentUserId) {
          this.following = [];
          this.followers = [];
          this.recentChats = [];
          this.unreadMap.clear();
          return;
        }
        this.refreshFollowLists(this.currentUserId);
        this.refreshRecentChats();
        this.refreshUnreadCounts();
      });

    this.chatState.unreadRefresh$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.refreshUnreadCounts();
      });

    this.chatState.recentRefresh$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.refreshRecentChats();
      });

    this.chatState.followRefresh$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        if (this.currentUserId) {
          this.refreshFollowLists(this.currentUserId);
        }
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

  getUnreadCount(userId: number): number {
    return this.unreadMap.get(userId) ?? 0;
  }

  private refreshUnreadCounts() {
    this.chatService.getUnreadCounts().subscribe({
      next: (items: UnreadCount[]) => {
        this.unreadMap = new Map(items.map(item => [item.targetUserId, item.unreadCount]));
      },
      error: () => {
        this.unreadMap.clear();
      }
    });
  }

  private refreshRecentChats() {
    this.chatService.getRecentSocialChats().subscribe({
      next: (chats) => {
        this.recentChats = chats;
      },
      error: () => {
        this.recentChats = [];
      }
    });
  }

  private refreshFollowLists(userId: number) {
    this.followService.getFollowing(userId).subscribe({
      next: (users) => {
        this.following = users;
      },
      error: () => {
        this.following = [];
      }
    });
    this.followService.getFollowers(userId).subscribe({
      next: (users) => {
        this.followers = users;
      },
      error: () => {
        this.followers = [];
      }
    });
  }

  toggleSection(section: 'recent' | 'following' | 'followers') {
    this.openSection[section] = !this.openSection[section];
  }

  getFilteredRecentChats(): RecentChat[] {
    const keyword = this.searchTerm.trim().toLowerCase();
    if (!keyword) return this.recentChats;
    return this.recentChats.filter(chat =>
      (chat.targetUserName || '').toLowerCase().includes(keyword)
    );
  }

  getFilteredFollowing(): FollowUser[] {
    const keyword = this.searchTerm.trim().toLowerCase();
    if (!keyword) return this.following;
    return this.following.filter(user =>
      (user.displayName || '').toLowerCase().includes(keyword)
    );
  }

  getFilteredFollowers(): FollowUser[] {
    const keyword = this.searchTerm.trim().toLowerCase();
    if (!keyword) return this.followers;
    return this.followers.filter(user =>
      (user.displayName || '').toLowerCase().includes(keyword)
    );
  }
}
