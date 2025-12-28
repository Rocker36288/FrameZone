import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-member-layout',
  imports: [CommonModule, RouterModule],
  templateUrl: './member-layout.component.html',
  styleUrl: './member-layout.component.css'
})
export class MemberLayoutComponent {
  // 側邊欄展開/收合狀態
  isSidebarOpen = false;

  // 導覽選項項目
  menuItems = [
    {
      path: '/member/dashboard',
      label: '總覽',
      icon: `<svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <path d="M3 3v18h18"></path>
        <path d="M18 17v-5"></path>
        <path d="M14 17v-9"></path>
        <path d="M10 17v-3"></path>
        <path d="M6 17v-7"></path>
      </svg>`
    },
    {
      path: '/member/profile',
      label: '個人資料',
      icon: `<svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <path d="M8 7a4 4 0 1 0 8 0a4 4 0 0 0 -8 0"></path>
        <path d="M6 21v-2a4 4 0 0 1 4 -4h4a4 4 0 0 1 4 4v2"></path>
      </svg>`
    },
    {
      path: '/member/security',
      label: '帳號安全',
      icon: `<svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <path d="M12 3a12 12 0 0 0 8.5 3a12 12 0 0 1 -8.5 15a12 12 0 0 1 -8.5 -15a12 12 0 0 0 8.5 -3"></path>
      </svg>`
    },
    {
      path: '/member/notifications',
      label: '通知設定',
      icon: `<svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <path d="M10 5a2 2 0 1 1 4 0a7 7 0 0 1 4 6v3a4 4 0 0 0 2 3h-16a4 4 0 0 0 2 -3v-3a7 7 0 0 1 4 -6"></path>
        <path d="M9 17v1a3 3 0 0 0 6 0v-1"></path>
      </svg>`
    },
    {
      path: '/member/privacy',
      label: '隱私設定',
      icon: `<svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <path d="M5 13a2 2 0 0 1 2 -2h10a2 2 0 0 1 2 2v6a2 2 0 0 1 -2 2h-10a2 2 0 0 1 -2 -2v-6z"></path>
        <path d="M11 16a1 1 0 1 0 2 0a1 1 0 0 0 -2 0"></path>
        <path d="M8 11v-4a4 4 0 1 1 8 0v4"></path>
      </svg>`
    },
    {
      path: '/member/logs',
      label: '操作記錄',
      icon: `<svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <path d="M3 12a9 9 0 1 0 18 0a9 9 0 0 0 -18 0"></path>
        <path d="M12 7v5l3 3"></path>
      </svg>`
    }
  ];

  /**
   * 切換側邊欄展開/收合狀態
   */
  toggleSidebar(): void {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  /**
   * 關閉側邊攔
   */
  closeSidebar(): void {
    this.isSidebarOpen = false;
  }

}
