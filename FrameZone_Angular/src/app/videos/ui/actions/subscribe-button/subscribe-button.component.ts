import { Component, Input } from '@angular/core';
import { NgClass } from "@angular/common";

@Component({
  selector: 'app-subscribe-button',
  imports: [NgClass],
  templateUrl: './subscribe-button.component.html',
  styleUrl: './subscribe-button.component.css'
})
export class SubscribeButtonComponent {
  isFollowing: boolean = false;

  toggleFollow() {
    this.isFollowing = !this.isFollowing;

    // 這裡可呼叫 API 更新後端跟隨狀態
    // this.userService.toggleFollow(userId, this.isFollowing).subscribe();
  }
}
