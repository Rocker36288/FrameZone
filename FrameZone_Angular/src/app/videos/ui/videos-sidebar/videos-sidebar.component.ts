import { Component } from '@angular/core';
import { NavigationEnd, Router, RouterLink } from '@angular/router';
import { filter, Subject, takeUntil } from 'rxjs';
import { NgSwitchCase, NgClass, NgSwitch, NgForOf, NgIf } from "@angular/common";
import { UserAvatarComponent } from "../user-avatar/user-avatar.component";
import { AuthService } from '../../../core/services/auth.service';
import { VideosSidebarMenuComponent } from "./videos-sidebar-menu/videos-sidebar-menu.component";
import { VideosSidebarCreatorComponent } from "./videos-sidebar-creator/videos-sidebar-creator.component";

@Component({
  selector: 'app-videos-sidebar',
  imports: [NgSwitchCase, RouterLink, NgClass, NgSwitch, NgForOf, UserAvatarComponent, NgIf, VideosSidebarMenuComponent, VideosSidebarCreatorComponent],
  templateUrl: './videos-sidebar.component.html',
  styleUrl: './videos-sidebar.component.css'
})
export class VideosSidebarComponent {
  isCollapsed = false;
  currentRoute = '';
  displayUserName: string = '未知登入';
  isLoggedIn: boolean = false;

  private destroy$ = new Subject<void>();


  welcomeMessage: any;

  constructor(
    private router: Router,
    private authService: AuthService
  ) { }


  ngOnInit(): void {
    // 1. 監聽登入狀態
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        if (user) {
          this.isLoggedIn = true;
          this.displayUserName = `你好! ${user.displayName || user.account || '使用者'}`;
        } else {
          this.isLoggedIn = false;
          this.displayUserName = '未登入';
        }
      });

    // 2. 監聽路由變化
    this.router.events
      .pipe(
        filter(event => event instanceof NavigationEnd),
        takeUntil(this.destroy$)
      )
      .subscribe((event: NavigationEnd) => {
        this.currentRoute = event.urlAfterRedirects;
      });

    this.currentRoute = this.router.url;

    // 3. 讀取側邊欄狀態
    const savedState = localStorage.getItem('sidebarCollapsed');
    if (savedState !== null) {
      this.isCollapsed = JSON.parse(savedState);
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  toggleSidebar(): void {
    this.isCollapsed = !this.isCollapsed;
    localStorage.setItem('sidebarCollapsed', JSON.stringify(this.isCollapsed));
  }

  isActive(route: string): boolean {
    return this.currentRoute === route || this.currentRoute.startsWith(route + '/');
  }

  get isCreatorPage(): boolean {
    // 判斷 URL 是否屬於創作者頁面
    return this.currentRoute.startsWith('/videos/videocreator');
  }

  login() {
    this.router.navigate(['/login']);
  }

  logout() {
    this.authService.logout()
    this.router.navigate(['/home']);
  }
}
