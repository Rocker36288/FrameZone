import { MemberService } from './../../../core/services/member.service';
import { Component, OnInit } from '@angular/core';
import { UserLogDto } from '../../../core/models/member.models';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-member-dashboard',
  imports: [CommonModule, RouterModule],
  templateUrl: './member-dashboard.component.html',
  styleUrl: './member-dashboard.component.css'
})
export class MemberDashboardComponent implements OnInit {
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

  // 最近活動
  recentActivities: UserLogDto[] = [];
  isLoadingActivities = true;
  activitiesError = '';

  constructor(private memberService: MemberService) {}

  ngOnInit(): void {
    this.loadRecentActivities();
  }

  /**
   * 載入最近活動記錄
   */
  loadRecentActivities(): void {
    this.isLoadingActivities = true;
    this.activitiesError = '';

    this.memberService.getUserLogs({
      pageNumber: 1,
      pageSize: 5
    }).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.recentActivities = response.data.items;
        } else {
          this.activitiesError = '載入失敗';
        }
        this.isLoadingActivities = false;
      },
      error: (error) => {
        console.error('載入最近活動失敗:', error);
        this.activitiesError = '載入失敗，請稍後在試';
        this.isLoadingActivities = false;
      }
    })
  }

  /**
   * 取得操作類型圖示
   */
  getActionIcon(actionType: string): string {
    const iconMap: { [key: string]: string } = {
      'Login': '🔐',
      'Logout': '👋',
      'ProfileUpdate': '✏️',
      'PasswordChange': '🔑',
      'AvatarUpload': '🖼️',
      'CoverImageUpload': '🎨',
      'SecurityUpdate': '🛡️',
      'SettingsUpdate': '⚙️'
    };
    return iconMap[actionType] || '📝';
  }

  /**
   * 取得狀態標籤樣式
   */
  getStatusClass(status: string): string {
    return status === 'Success' ? 'status-success' : 'status-failure';
  }

  /**
   * 格式化相對時間
   */
  formatRelativeTime(dateString: string): string {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) return '剛剛';
    if (diffMins < 60) return `${diffMins} 分鐘前`;
    if (diffHours < 24) return `${diffHours} 小時前`;
    if (diffDays < 7) return `${diffDays} 天前`;

    return date.toLocaleDateString('zh-TW', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit'
    });
  }

  /**
   * 取得操作類型的中文名稱
   */
  getActionTypeName(actionType: string): string {
    const nameMap: { [key: string]: string } = {
      'Login': '登入',
      'Logout': '登出',
      'ProfileUpdate': '更新個人資料',
      'PasswordChange': '變更密碼',
      'AvatarUpload': '上傳頭像',
      'CoverImageUpload': '上傳封面',
      'SecurityUpdate': '更新安全設定',
      'SettingsUpdate': '更新設定'
    };
    return nameMap[actionType] || actionType;
  }
}
