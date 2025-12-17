import { Component, OnInit, Inject, PLATFORM_ID } from '@angular/core';
import { RouterModule } from '@angular/router';
import { isPlatformBrowser } from '@angular/common';


@Component({
  selector: 'app-header',
  imports: [RouterModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css'
})
export class HeaderComponent implements OnInit {
  // ===== 主題相關 =====
  isDarkMode: boolean = false;
  private isBrowser: boolean;

  // ===== 使用者相關 =====
  isLoggedIn: boolean = false;
  showAdminPanel: boolean = false;
  isSuperAdmin: boolean = false;
  displayUserName: string = '';
  userSession = {
    email: ''
  };

  constructor(@Inject(PLATFORM_ID) private platformId: Object) {
    this.isBrowser = isPlatformBrowser(this.platformId);
  }


  ngOnInit() {
    // 初始化主題
    this.initTheme();
    //初始化使用者狀態
    this.initUserSession();
  }

  // ========== 主題相關 ==========
  private initTheme() {
    if (!this.isBrowser) return;

    this.isDarkMode = false;
    this.applyTheme('light');
  }

  toggleTheme() {
    if (!this.isBrowser) return;

    this.isDarkMode = !this.isDarkMode;
    const newTheme = this.isDarkMode ? 'dark' : 'light';
    this.applyTheme(newTheme);
  }

  private applyTheme(theme: string) {
    if (!this.isBrowser) return;

    document.documentElement.setAttribute('data-bs-theme', theme);
  }

  // ========== 使用者相關 ==========
  private initUserSession() {
    if (!this.isBrowser) return;

    // TODO: 從 AuthService 讀取使用者狀態
    const token = localStorage.getItem('token');
    const userInfo = localStorage.getItem('userInfo');

    if (token && userInfo) {
      const user = JSON.parse(userInfo);
      this.isLoggedIn = true;
      this.displayUserName = user.name || user.username || '使用者';
      this.userSession.email = user.email || '';
      this.showAdminPanel = user.role === 'admin' || user.role === 'superAdmin';
      this.isSuperAdmin = user.role === 'superAdmin';
    }
  }

  onLogout() {
    if (!this.isBrowser) return;

    // TODO: 呼叫 AuthService 的 logout 方法
    localStorage.removeItem('token');
    localStorage.removeItem('userInfo');

    this.isLoggedIn = false;
    this.showAdminPanel = false;
    this.isSuperAdmin = false;
    this.displayUserName = '';
    this.userSession.email = '';

    window.location.href = '/login';
  }
}
