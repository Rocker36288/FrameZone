import { Component } from '@angular/core';

@Component({
  selector: 'app-member-dashboard',
  imports: [],
  templateUrl: './member-dashboard.component.html',
  styleUrl: './member-dashboard.component.css'
})
export class MemberDashboardComponent {
  // 系統卡片資料
  systems = [
    {
      name: '照片分類系統',
      icon: '📷',
      description: '管理您的照片與相簿',
      path: '/photo-home',
      color: '#0054a6'
    },
    {
      name: '社群系統',
      icon: '👥',
      description: '與好友互動分享',
      path: '/social',
      color: '#7c3aed'
    },
    {
      name: '購物中心',
      icon: '🛒',
      description: '瀏覽與購買商品',
      path: '/shopping',
      color: '#dc2626'
    },
    {
      name: '影音平台',
      icon: '🎬',
      description: '觀看與上傳影片',
      path: '/videos',
      color: '#ea580c'
    },
    {
      name: '工作室預約',
      icon: '📅',
      description: '預約攝影工作室',
      path: '/photographer-bookinghome',
      color: '#059669'
    }
  ];
}
