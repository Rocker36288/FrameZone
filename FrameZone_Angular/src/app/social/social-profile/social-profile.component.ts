import { Component, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { SocialPostsListComponent } from '../social-posts-list/social-posts-list.component';

@Component({
  selector: 'app-social-profile',
  standalone: true,
  imports: [RouterLink, SocialPostsListComponent], // 在 Angular 19 中，內建控制流不需要 CommonModule
  templateUrl: './social-profile.component.html',
  styleUrl: './social-profile.component.css'
})
export class SocialProfileComponent {
  // 使用 Signal 管理目前檢視狀態，預設為 'all'
  currentView = signal<string>('all');

  // 測試用的貼文資料
  posts = signal([
    { id: 1, author: '使用者名稱', content: '今天心情不錯！', date: '2小時前' },
    { id: 2, author: '使用者名稱', content: '分享一張漂亮的風景照', date: '昨天' }
  ]);


  addFriend() {
    console.log('已發送好友申請');
  }

  sendMessage() {
    console.log('跳轉至訊息頁面');
  }

  // 更新 Signal 的值
  setView(viewName: string) {
    this.currentView.set(viewName);
  }
}
