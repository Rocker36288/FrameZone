import { Component } from '@angular/core';
import { NavigationEnd, Router, RouterLink } from '@angular/router';
import { filter } from 'rxjs';
import { NgSwitchCase, NgClass, NgSwitch, NgForOf } from "@angular/common";
import { UserMenuComponent } from "../../../shared/components/user-menu/user-menu.component";

@Component({
  selector: 'app-videos-sidebar',
  imports: [NgSwitchCase, RouterLink, NgClass, NgSwitch, NgForOf, UserMenuComponent],
  templateUrl: './videos-sidebar.component.html',
  styleUrl: './videos-sidebar.component.css'
})
export class VideosSidebarComponent {
  isCollapsed = false;
  currentRoute = '';

  menuItems = [
    {
      title: '影片首頁',
      route: '/videos',
      icon: 'home'
    },
    {
      title: '假頻道',
      route: '/videos/channel/1',
      icon: 'barrier-block'
    },
    {
      title: '創作者工作室',
      route: '/videos/videocreator/1',
      icon: 'video'
    }
  ];

  constructor(private router: Router) { }

  ngOnInit(): void {
    // 監聽路由變化
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event: NavigationEnd) => {
        this.currentRoute = event.urlAfterRedirects;
      });

    // 初始化當前路由
    this.currentRoute = this.router.url;

    // 從 localStorage 讀取側邊欄狀態
    const savedState = localStorage.getItem('sidebarCollapsed');
    if (savedState !== null) {
      this.isCollapsed = JSON.parse(savedState);
    }
  }

  toggleSidebar(): void {
    this.isCollapsed = !this.isCollapsed;
    localStorage.setItem('sidebarCollapsed', JSON.stringify(this.isCollapsed));
  }

  isActive(route: string): boolean {
    return this.currentRoute === route || this.currentRoute.startsWith(route + '/');
  }

}
