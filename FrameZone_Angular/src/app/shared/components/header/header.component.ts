import { Component, OnInit, OnDestroy, Inject, PLATFORM_ID, HostListener, AfterViewInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { AuthService } from './../../../core/services/auth.service';
import { LoginResponseDto } from '../../../core/models/auth.models';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

declare const bootstrap: any;

@Component({
  selector: 'app-header',
  imports: [RouterModule, CommonModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css'
})
export class HeaderComponent implements OnInit, OnDestroy {
  // ===== 主題相關 =====
  isDarkMode: boolean = false;
  private isBrowser: boolean;

  // ===== 使用者相關 =====
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
    @Inject(PLATFORM_ID) private platformId: Object,
    private authService: AuthService,
    private router: Router
  ) {
    this.isBrowser = isPlatformBrowser(this.platformId);
  }


  ngOnInit() {
    // 初始化主題
    this.initTheme();

    // 訂閱使用者狀態變化
    if (this.isBrowser) {
      this.authService.currentUser$
        .pipe(takeUntil(this.destroy$))
        .subscribe(user => {
          console.log('Header 收到使用者狀態更新:', user);
          this.updateUserState(user);
        });
    }
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


  // ========== 主題相關 ==========
  private initTheme() {
    if (!this.isBrowser) return;

    const saveTheme = localStorage.getItem('theme');
    this.isDarkMode = saveTheme === 'dark';
    this.applyTheme(this.isDarkMode ? 'dark' : 'light');
  }

  toggleTheme() {
    if (!this.isBrowser) return;

    this.isDarkMode = !this.isDarkMode;
    const newTheme = this.isDarkMode ? 'dark' : 'light';
    this.applyTheme(newTheme);

    // 儲存主題設定
    localStorage.setItem('theme', newTheme);
  }

  private applyTheme(theme: string) {
    if (!this.isBrowser) return;

    document.documentElement.setAttribute('data-bs-theme', theme);
  }

  // ========== 使用者相關 ==========
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
    if (!this.isBrowser) return;

    console.log('執行登出操作');

    this.closeDropdownMenu();

    this.authService.logout();

    this.router.navigate(['/']);
  }

  navigateToLogin(): void{
    console.log('導航到登入頁');
    this.router.navigate(['/login']).then(success => {
      if(success) {
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
    if (this.currentUser?.avatar) {
      return this.currentUser.avatar;
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
