import { Component, DestroyRef, HostListener, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { SocialMenuComponent } from "./social-menu/social-menu.component";
import { SocialFriendsComponent } from "./social-friends/social-friends.component";
import { SocialPostsSubmitComponent } from "./social-posts-submit/social-posts-submit.component";
import { SocialPostsComponent } from "./social-posts/social-posts.component";
import { NgClass } from '@angular/common';
import { RouterModule } from "@angular/router";
import { SocialIndexComponent } from "./social-index/social-index.component";
import { SocialChatComponent } from "./social-chat/social-chat.component";
import { SocialChatStateService } from './services/social-chat-state.service';

@Component({
  selector: 'app-social',
  imports: [SocialMenuComponent, SocialFriendsComponent, SocialPostsSubmitComponent, SocialPostsComponent, NgClass, RouterModule, SocialIndexComponent, SocialChatComponent],
  templateUrl: './social.component.html',
  styleUrl: './social.component.css'
})
export class SocialComponent {
  private chatState = inject(SocialChatStateService);
  private destroyRef = inject(DestroyRef);

  selectedFriend: { id: number; name: string; avatar?: string } | null = null;

  constructor() {
    this.chatState.openChat$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(friend => {
        this.selectedFriend = {
          id: friend.id,
          name: friend.name,
          avatar: friend.avatar ?? undefined
        };
      });
  }

  onFriendSelected(friend: { id: number; name: string }) {
    this.selectedFriend = friend;
  }

  onChatClosed() {
    this.selectedFriend = null;
  }


  // 偵測頁面捲動
  showBackToTop = false;
  @HostListener("window:scroll", [])
  onWindowScroll() {
    const scrollTop = document.documentElement.scrollTop || document.body.scrollTop;
    this.showBackToTop = scrollTop > 100;
  }
  // 回到最上面
  backToTop() {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }
}
