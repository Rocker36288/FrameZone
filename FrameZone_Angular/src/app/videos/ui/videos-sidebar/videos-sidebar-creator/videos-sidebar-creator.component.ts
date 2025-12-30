import { Component, Input } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { NgForOf, NgSwitchCase, NgSwitch } from "@angular/common";

@Component({
  selector: 'app-videos-sidebar-creator',
  imports: [NgForOf, RouterLink, NgSwitchCase, NgSwitch],
  templateUrl: './videos-sidebar-creator.component.html',
  styleUrl: './videos-sidebar-creator.component.css'
})
export class VideosSidebarCreatorComponent {
  @Input() menuItems = [
    { title: '回到影片首頁', route: '/videos/home', icon: 'home' },
    { title: '創作者管理首頁', route: '/videos/videocreator/home', icon: 'dashboard' },
    { title: '影片上傳', route: '/videos/videocreator/upload', icon: 'upload' },
    { title: '影片管理', route: '/videos/videocreator/videos', icon: 'film' },
    { title: '數據顯示', route: '/videos/videocreator/stats', icon: 'dashboard' }
  ];

  @Input() isCollapsed: boolean = true;

  constructor(private router: Router) {
  }

  isActive(route: string): boolean {
    return this.router.url === route || this.router.url.startsWith(route + '/');
  }
}
