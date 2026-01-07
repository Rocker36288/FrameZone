import { Component, inject, signal } from '@angular/core';
import { AsyncPipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { SocialPostsListComponent } from '../social-posts-list/social-posts-list.component';
import { SocialPostsImagesComponent } from '../social-posts-images/social-posts-images.component';
import { AuthService } from '../../core/services/auth.service';
import { PostService } from '../services/post.service';
import { SocialProfileSummary } from '../models/social-profile.models';
import { distinctUntilChanged, map, of, switchMap } from 'rxjs';

@Component({
  selector: 'app-social-profile',
  standalone: true,
  imports: [AsyncPipe, RouterLink, SocialPostsListComponent, SocialPostsImagesComponent], // 在 Angular 19 中，內建控制流不需要 CommonModule
  templateUrl: './social-profile.component.html',
  styleUrl: './social-profile.component.css'
})
export class SocialProfileComponent {
  private authService = inject(AuthService);
  private postService = inject(PostService);
  private route = inject(ActivatedRoute);

  routeUserId$ = this.route.paramMap.pipe(
    map((params) => {
      const userId = params.get('userId');
      const parsedId = userId ? Number(userId) : null;
      return Number.isFinite(parsedId) ? parsedId : null;
    }),
    distinctUntilChanged()
  );

  profileUserId$ = this.routeUserId$.pipe(
    switchMap((userId) => {
      if (userId) return of(userId);
      return this.authService.currentUser$.pipe(
        map((user) => user?.userId ?? null)
      );
    })
  );

  profile$ = this.profileUserId$.pipe(
    switchMap((userId) => {
      if (!userId) return of(null);
      return this.postService.getUserProfile(userId);
    })
  );

  // 使用 Signal 管理目前檢視狀態，預設為 'all'
  currentView = signal<string>('all');

  addFriend() {
    console.log('已發送好友申請');
  }

  sendMessage() {
    console.log('跳轉至訊息頁面');
  }

  getDisplayName(profile: SocialProfileSummary | null): string {
    return profile?.displayName || '使用者';
  }

  getAvatarUrl(profile: SocialProfileSummary | null): string {
    if (profile?.avatar) return profile.avatar;
    const initial = this.getDisplayName(profile).charAt(0).toUpperCase();
    return `https://ui-avatars.com/api/?name=${encodeURIComponent(initial)}&background=667eea&color=fff&size=128`;
  }

  // 更新 Signal 的值
  setView(viewName: string) {
    this.currentView.set(viewName);
  }
}
