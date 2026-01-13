import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AsyncPipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { SocialPostsListComponent } from '../social-posts-list/social-posts-list.component';
import { AuthService } from '../../core/services/auth.service';
import { PostService } from '../services/post.service';
import { SocialProfileSummary } from '../models/social-profile.models';
import { FollowService } from '../services/follow.service';
import { FollowUser } from '../models/follow.models';
import { SocialChatStateService } from '../services/social-chat-state.service';
import { combineLatest, distinctUntilChanged, map, of, startWith, switchMap, take, Subject } from 'rxjs';

@Component({
  selector: 'app-social-profile',
  standalone: true,
  imports: [AsyncPipe, RouterLink, SocialPostsListComponent], // 在 Angular 19 中，內建控制流不需要 CommonModule
  templateUrl: './social-profile.component.html',
  styleUrl: './social-profile.component.css'
})
export class SocialProfileComponent {
  private authService = inject(AuthService);
  private postService = inject(PostService);
  private followService = inject(FollowService);
  private chatState = inject(SocialChatStateService);
  private route = inject(ActivatedRoute);
  private destroyRef = inject(DestroyRef);
  private profileRefresh$ = new Subject<void>();

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

  profile$ = combineLatest([
    this.profileUserId$,
    this.profileRefresh$.pipe(startWith(null))
  ]).pipe(
    map(([userId]) => userId),
    switchMap((userId) => {
      if (!userId) return of(null);
      return this.postService.getUserProfile(userId);
    })
  );

  following$ = combineLatest([
    this.profileUserId$,
    this.profileRefresh$.pipe(startWith(null))
  ]).pipe(
    map(([userId]) => userId),
    switchMap((userId) => {
      if (!userId) return of<FollowUser[]>([]);
      return this.followService.getFollowing(userId);
    })
  );

  followers$ = combineLatest([
    this.profileUserId$,
    this.profileRefresh$.pipe(startWith(null))
  ]).pipe(
    map(([userId]) => userId),
    switchMap((userId) => {
      if (!userId) return of<FollowUser[]>([]);
      return this.followService.getFollowers(userId);
    })
  );

  isOwnProfile$ = this.profileUserId$.pipe(
    switchMap((userId) =>
      this.authService.currentUser$.pipe(
        map((user) => !!userId && user?.userId === userId)
      )
    )
  );

  isFollowing$ = combineLatest([
    this.profileUserId$,
    this.isOwnProfile$,
    this.profileRefresh$.pipe(startWith(null))
  ]).pipe(
    map(([userId, isOwn]) => ({ userId, isOwn })),
    switchMap(({ userId, isOwn }) => {
      if (!userId || isOwn) return of(false);
      return this.followService.getFollowStatus(userId).pipe(
        map((res) => res.isFollowing)
      );
    })
  );

  // 使用 Signal 管理目前檢視狀態，預設為 'all'
  currentView = signal<string>('all');


  private normalizeView(view: string | null): string {
    switch (view) {
      case 'all':
      case 'following':
      case 'followers':
        return view;
      default:
        return 'all';
    }
  }
  constructor() {
    combineLatest([
      this.routeUserId$,
      this.route.queryParamMap.pipe(
        map((params) => params.get('view')),
        distinctUntilChanged()
      )
    ])
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(([_, view]) => {
        this.currentView.set(this.normalizeView(view));
      });
  }

  addFollow() {
    this.profileUserId$
      .pipe(
        take(1),
        switchMap((userId) => {
          if (!userId) return of(null);
          return this.followService.addFollow(userId);
        })
      )
      .subscribe({
        next: () => {
          this.profileRefresh$.next();
          this.chatState.requestFollowRefresh();
          console.log('已追蹤');
        },
        error: (err) => {
          console.error('追蹤失敗', err);
        }
      });
  }

  removeFollow() {
    this.profileUserId$
      .pipe(
        take(1),
        switchMap((userId) => {
          if (!userId) return of(null);
          return this.followService.removeFollow(userId);
        })
      )
      .subscribe({
        next: () => {
          this.profileRefresh$.next();
          this.chatState.requestFollowRefresh();
          console.log('已取消追蹤');
        },
        error: (err) => {
          console.error('取消追蹤失敗', err);
        }
      });
  }

  getFollowAvatar(user: FollowUser): string {
    if (user.avatar) return user.avatar;
    const initial = (user.displayName || 'U').charAt(0).toUpperCase();
    return `https://ui-avatars.com/api/?name=${encodeURIComponent(initial)}&background=667eea&color=fff&size=128`;
  }

  sendMessage() {
    combineLatest([this.profileUserId$, this.profile$])
      .pipe(take(1))
      .subscribe(([userId, profile]) => {
        if (!userId) return;
        this.chatState.openChat({
          id: userId,
          name: this.getDisplayName(profile),
          avatar: profile?.avatar ?? null
        });
      });
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




