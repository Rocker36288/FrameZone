import { DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-social-comments',
  imports: [DatePipe],
  templateUrl: './social-comments.component.html',
  styleUrl: './social-comments.component.css'
})
export class SocialCommentsComponent {
  // 接收父組件傳進來的留言資料
  @Input() data: any;

  // 判斷是否為目前登入者，以便顯示刪除按鈕 (範例)
  @Input() isOwner: boolean = false;

  // 傳遞刪除事件給父組件
  @Output() delete = new EventEmitter<number>();

  onDeleteClick() {
    this.delete.emit(this.data.id);
  }
}
