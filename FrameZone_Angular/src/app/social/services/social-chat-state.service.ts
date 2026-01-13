import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

export interface ChatFriend {
  id: number;
  name: string;
  avatar?: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class SocialChatStateService {
  private openChatSubject = new Subject<ChatFriend>();
  openChat$ = this.openChatSubject.asObservable();
  private unreadRefreshSubject = new Subject<void>();
  unreadRefresh$ = this.unreadRefreshSubject.asObservable();
  private recentRefreshSubject = new Subject<void>();
  recentRefresh$ = this.recentRefreshSubject.asObservable();
  private followRefreshSubject = new Subject<void>();
  followRefresh$ = this.followRefreshSubject.asObservable();

  openChat(friend: ChatFriend) {
    this.openChatSubject.next(friend);
  }

  requestUnreadRefresh() {
    this.unreadRefreshSubject.next();
  }

  requestRecentRefresh() {
    this.recentRefreshSubject.next();
  }

  requestFollowRefresh() {
    this.followRefreshSubject.next();
  }
}
