import { Component, Inject, OnInit, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ThemeToggleComponent } from '../../shared/components/theme-toggle/theme-toggle.component';
import { UserMenuComponent } from '../../shared/components/user-menu/user-menu.component';
import { NotificationBellComponent } from '../../shared/components/notification-bell/notification-bell.component';

@Component({
  selector: 'app-photographerbooking-header',
  standalone: true,
  imports: [
    RouterModule,
    CommonModule,
    ThemeToggleComponent,
    UserMenuComponent,
    NotificationBellComponent,
  ],
  templateUrl: './photographerbooking-header.component.html',
  styleUrl: './photographerbooking-header.component.css',
})
export class PhotographerbookingHeaderComponent implements OnInit {
  isDarkMode: boolean = false;
  private isBrowser: boolean;

  constructor(@Inject(PLATFORM_ID) private platformId: Object) {
    this.isBrowser = isPlatformBrowser(this.platformId);
  }

  ngOnInit() {
    this.initTheme();
  }

  private initTheme() {
    if (!this.isBrowser) return;
    this.isDarkMode = false;
    this.applyTheme('light');
  }

  toggleTheme() {
    if (!this.isBrowser) return;
    this.isDarkMode = !this.isDarkMode;
    this.applyTheme(this.isDarkMode ? 'dark' : 'light');
  }

  private applyTheme(theme: string) {
    if (!this.isBrowser) return;
    document.documentElement.setAttribute('data-bs-theme', theme);
  }
}
