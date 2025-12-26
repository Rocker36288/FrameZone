import { PostService } from '../services/post.service';
import { Component, ElementRef, EventEmitter, HostListener, Input, Output } from '@angular/core';
import { PostDto } from "../models/PostDto";
import { DatePipe, SlicePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SocialCommentsComponent } from "../social-comments/social-comments.component";

@Component({
  selector: 'app-social-posts',
  imports: [DatePipe, SlicePipe, FormsModule, SocialCommentsComponent],
  templateUrl: './social-posts.component.html',
  styleUrl: './social-posts.component.css'
})
export class SocialPostsComponent {
  @Input() post!: PostDto;
  @Output() postDeleted = new EventEmitter<number>();

  isEditing = false;  //是否正在編輯
  editContent = "";
  isMenuOpen = false; // 控制選單開關
  isFullContent = false; // 控制是否顯示全部內容
  isCommentShowed = false; // 控制是否顯示留言

  isLiked = false;  //是否按過讚
  likeCount = 132;  //按讚假資料

  constructor(private postService: PostService, private eRef: ElementRef) { }

  ngOnInit() {
    // 如果後端還沒實作留言功能（post.comments 為 undefined 或空）
    // 我們在這裡手動塞入假資料
    if (!this.post.comments || this.post.comments.length === 0) {
      this.post.comments = [
        {
          commentId: 101,
          userId: 55,
          content: '這則貼文的內容很有趣！',
          updatedAt: new Date().toISOString(),
          replies: [
            {
              commentId: 1011,
              userId: 1,
              content: '感謝支持，歡迎多加利用。',
              updatedAt: new Date().toISOString()
            }
          ]
        },
        {
          commentId: 102,
          userId: 66,
          content: '這段程式碼的邏輯還有優化空間喔。',
          updatedAt: new Date().toISOString(),
          replies: []
        }
      ];
    }
  }


  // 點擊選單以外的地方時，自動關閉選單
  @HostListener('document:click', ['$event'])
  clickout(event: any) {
    if (!this.eRef.nativeElement.contains(event.target)) {
      this.isMenuOpen = false;
    }
  }

  toggleMenu() {
    this.isMenuOpen = !this.isMenuOpen;
  }

  toggleContent() {
    this.isFullContent = !this.isFullContent;
  }

  toggleLikes() {
    this.isLiked = !this.isLiked; //先換狀態再加次數
    if (this.isLiked == true) {
      this.likeCount++;
    }
    else {
      this.likeCount--;
    }
  }

  toggleComments() {
    this.isCommentShowed = !this.isCommentShowed;
  }

  startEdit() {
    this.isEditing = true;
    this.editContent = this.post.postContent;
  }

  cancelEdit() {
    this.isEditing = false;
  }

  saveEdit() {
    // 1. 加上基本防呆，避免傳送空白內容
    if (!this.editContent || !this.editContent.trim()) {
      this.isEditing = false
      return;
    }
    // 2. 呼叫 Service 進行更新
    this.postService.editPost(this.post.postId, this.editContent)
      .subscribe({
        next: (updatedPost: PostDto) => {
          // 【關鍵修改】：直接將後端回傳的最新物件賦值給 this.post
          // 這樣 post.postContent 和 post.updatedAt 都會同步更新
          this.post = updatedPost;

          this.isEditing = false;
          this.isMenuOpen = false; // 存檔後建議一併關閉選單
          console.log('更新成功，最新時間為：', updatedPost.updatedAt);
        },
        error: (err) => {
          console.error('更新失敗：', err);
          alert('儲存失敗，請稍後再試');
        }
      });
  }

  deletePost() {
    if (!confirm("確定刪除貼文?")) {
      return;
    }
    this.postService.deletePost(this.post.postId)
      .subscribe(() => {
        this.postDeleted.emit(this.post.postId);
      })
  }
}
