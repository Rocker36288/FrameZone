import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { AuthService } from './../../../core/services/auth.service';
import { LoginResponseDto } from '../../../core/models/auth.models';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-user-menu',
  imports: [CommonModule, RouterModule],
  templateUrl: './user-menu.component.html',
  styleUrl: './user-menu.component.css'
})
export class UserMenuComponent implements OnInit, OnDestroy {
  isLoggedIn: boolean = false;
  showAdminPanel: boolean = false;
  isSuperAdmin: boolean = false;
  displayUserName: string = '';
  currentUser: LoginResponseDto | null = null;
  isDropdownOpen: boolean = false;

  userSession = {
    email: ''
  };

  private destroy$ = new Subject<void>();

  constructor(
    private authService: AuthService,
    private router: Router
  ) { }



  ngOnInit() {
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        this.updateUserState(user);
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  toggleDropdown(): void {
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  closeDropdownMenu(): void {
    this.isDropdownOpen = false;
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    const dropdown = document.querySelector('.nav-item.dropdown');

    // 如果點擊在下拉選單外面,關閉選單
    if (dropdown && !dropdown.contains(target)) {
      this.isDropdownOpen = false;
    }
  }

  private updateUserState(user: LoginResponseDto | null) {
    this.currentUser = user;

    if (user) {
      // 已登入
      this.isLoggedIn = true;
      this.displayUserName = user.displayName || user.account || '使用者';
      this.userSession.email = user.email || '';

      // TODO: 根據實際的角色欄位判斷
      this.showAdminPanel = false;
      this.isSuperAdmin = false;
    } else {
      // 未登入
      this.isLoggedIn = false;
      this.showAdminPanel = false;
      this.isSuperAdmin = false;
      this.displayUserName = '';
      this.userSession.email = '';
    }
  }

  /**
   * 登出
   */
  onLogout() {

    console.log('執行登出操作');
    this.closeDropdownMenu();
    this.authService.logout();
    this.router.navigate(['/']);
  }

  navigateToLogin(): void {
    console.log('導航到登入頁');
    this.router.navigate(['/login']).then(success => {
      if (success) {
        console.log('導航成功');
      } else {
        console.error('導航失敗');
      }
    });
  }

  /**
   * 取得使用者頭像 URL
   */
  getUserAvatar(): string {
    const avatarUrl = this.currentUser?.avatar;

    if (avatarUrl) {
      return avatarUrl;
    }

    return this.getDefaultAvatar();
  }

  /**
   * 產生預設頭像
   */
  private getDefaultAvatar(): string {
    const name = this.displayUserName || 'U';
    const initial = name.charAt(0).toUpperCase();

    return `https://ui-avatars.com/api/?name=${encodeURIComponent(initial)}&background=667eea&color=fff&size=128`;
  }
}
