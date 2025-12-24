import { Component, EventEmitter, Input, Output } from '@angular/core';
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-like-button',
  imports: [NgClass],
  templateUrl: './like-button.component.html',
  styleUrl: './like-button.component.css'
})
export class LikeButtonComponent {

  @Input() likes?: number = 0;  // 初始喜歡數，可從後端帶入
  @Input() isLiked: boolean = false; // 使用者是否已點擊
  @Output() likeToggled = new EventEmitter<boolean>();

  toggleLike() {
    this.isLiked = !this.isLiked;
    this.likes! += this.isLiked ? 1 : -1;
    this.likeToggled.emit(this.isLiked); // 通知父元件
  }
}
