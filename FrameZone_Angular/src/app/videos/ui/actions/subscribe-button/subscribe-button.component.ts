import { Component, EventEmitter, Input, Output } from '@angular/core';
import { NgClass, NgIf } from "@angular/common";

@Component({
  selector: 'app-subscribe-button',
  imports: [NgClass, NgIf],
  templateUrl: './subscribe-button.component.html',
  styleUrl: './subscribe-button.component.css'
})
export class SubscribeButtonComponent {
  // 接收父元件傳入的初始狀態
  @Input() isFollowing: boolean = false;

  // 傳送事件給父元件
  @Output() followToggle = new EventEmitter<boolean>();

  toggleFollow() {
    // 切換內部狀態
    //this.isFollowing = !this.isFollowing;

    // 發送事件給父元件，傳回最新狀態
    this.followToggle.emit(this.isFollowing);
  }
}
