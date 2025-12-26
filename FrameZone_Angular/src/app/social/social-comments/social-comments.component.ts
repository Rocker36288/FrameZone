import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CommentDto } from '../models/PostDto'; // 確保路徑正確

@Component({
  selector: 'app-social-comments',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './social-comments.component.html',
  styleUrl: './social-comments.component.css'
})
export class SocialCommentsComponent {
  // 接收從 SocialPostsComponent 傳進來的留言陣列
  @Input() comments: CommentDto[] = [];

  constructor() { }

  // 未來可以在這裡加入「按讚留言」或「刪除留言」的邏輯
}
