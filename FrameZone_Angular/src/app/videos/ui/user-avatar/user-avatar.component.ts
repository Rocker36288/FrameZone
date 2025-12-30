import { Component } from '@angular/core';
import { LoginResponseDto } from '../../../core/models/auth.models';
import { Subject, takeUntil } from 'rxjs';
import { AuthService } from '../../../core/services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-user-avatar',
  imports: [],
  templateUrl: './user-avatar.component.html',
  styleUrl: './user-avatar.component.css'
})
export class UserAvatarComponent {
  currentUser: LoginResponseDto | null = null;
  avatarUrl: string = '';

  private destroy$ = new Subject<void>();

  constructor(
    private authService: AuthService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        this.currentUser = user;
        this.avatarUrl = this.getAvatar();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private getAvatar(): string {
    if (this.currentUser?.avatar) {
      return this.currentUser.avatar;
    }
    // 預設未登入 icon
    return 'https://picsum.photos/seed/photo-channel/256/256';
  }

  onAvatarClick(): void {
    if (!this.currentUser) {
      // 未登入 → 導向登入頁
      this.router.navigate(['/login']);
    } else {
      // 已登入 → 可做其他操作，例如開啟個人頁面
      console.log('已登入使用者點擊頭像');
    }
  }
}
