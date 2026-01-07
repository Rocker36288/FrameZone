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

  openChat(friend: ChatFriend) {
    this.openChatSubject.next(friend);
  }
}
