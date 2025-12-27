import { Component, Input } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { NgForOf, NgSwitch, NgSwitchCase } from "@angular/common";

@Component({
  selector: 'app-videos-sidebar-menu',
  imports: [NgForOf, RouterLink, NgSwitch, NgSwitchCase],
  templateUrl: './videos-sidebar-menu.component.html',
  styleUrl: './videos-sidebar-menu.component.css'
})
export class VideosSidebarMenuComponent {
  @Input() menuItems = [
    { title: '回到影片首頁', route: '/videos/home', icon: 'home' },
    { title: '創作者工作室', route: '/videos/videocreator', icon: 'video' }
  ];

  @Input() isCollapsed: boolean = true;

  constructor(private router: Router) {
  }

  isActive(route: string): boolean {
    return this.router.url === route || this.router.url.startsWith(route + '/');
  }

}
