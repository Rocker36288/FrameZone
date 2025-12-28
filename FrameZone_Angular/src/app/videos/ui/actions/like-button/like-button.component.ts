import { Component, EventEmitter, Input, Output } from '@angular/core';
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-like-button',
  imports: [NgClass],
  templateUrl: './like-button.component.html',
  styleUrl: './like-button.component.css'
})
export class LikeButtonComponent {
  @Input() likes?: number = 0;  // 初始喜歡數
  @Input() isLiked: boolean = false; // 父元件控制
  @Output() likeToggled = new EventEmitter<void>(); // 只發射事件，交給父元件決定

  toggleLike() {
    this.likeToggled.emit();
  }
}
