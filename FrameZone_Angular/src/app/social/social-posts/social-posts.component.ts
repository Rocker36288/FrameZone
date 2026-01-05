import { PostService } from '../services/post.service';
import { CommentService } from '../services/comment.service';
import { Component, ElementRef, EventEmitter, HostListener, Input, Output, signal, inject } from '@angular/core';
import { PostDto } from "../models/PostDto";
import { CommentDto } from "../models/CommentDto";
import { DatePipe, SlicePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SocialCommentsComponent } from "../social-comments/social-comments.component";
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-social-posts',
  imports: [DatePipe, SlicePipe, FormsModule, SocialCommentsComponent],
  templateUrl: './social-posts.component.html',
  styleUrl: './social-posts.component.css'
})
export class SocialPostsComponent {
  @Input() post!: PostDto;
  @Output() postDeleted = new EventEmitter<number>();

  private authService = inject(AuthService);
  currentUserAvatar = this.authService.getCurrentUser()?.avatar || null;

  // --- 貼文狀態 ---
  isEditing = false;
  editContent = "";
  isMenuOpen = false;
  isFullContent = false;
  isLiked = false;
  likeCount = 4;

  // --- 留言狀態 ---
  isCommentShowed = false;
  comments = signal<CommentDto[]>([]); // 使用 Signal 管理留言列表
  newCommentContent = ""; // 綁定發布框
  isSubmittingComment = false; // 防止重複點擊

  // 注入 CommentService
  constructor(
    private postService: PostService,
    private commentService: CommentService,
    private eRef: ElementRef
  ) { }

  // 點擊選單以外的地方自動關閉
  @HostListener('document:click', ['$event'])
  clickout(event: any) {
    if (!this.eRef.nativeElement.contains(event.target)) {
      this.isMenuOpen = false;
    }
  }

  // --- 貼文邏輯 ---
  toggleMenu() { this.isMenuOpen = !this.isMenuOpen; }
  toggleContent() { this.isFullContent = !this.isFullContent; }
  toggleLikes() {
    this.isLiked = !this.isLiked;
    this.isLiked ? this.likeCount++ : this.likeCount--;
  }

  // --- 留言邏輯 ---
  toggleComments() {
    this.isCommentShowed = !this.isCommentShowed;
    // 如果展開且還沒有留言資料，就去抓取
    if (this.isCommentShowed && this.comments().length === 0) {
      this.loadComments();
    }
  }

  loadComments() {
    this.commentService.getCommentsByPost(this.post.postId).subscribe({
      next: (data) => this.comments.set(data),
      error: (err) => console.error('載入留言失敗', err)
    });
  }

  // 傳送頂層留言
  sendComment() {
    if (!this.newCommentContent.trim() || this.isSubmittingComment) return;

    this.isSubmittingComment = true;
    const dto = {
      postId: this.post.postId,
      parentCommentId: null, // 頂層留言
      commentContent: this.newCommentContent
    };

    this.commentService.createComment(dto).subscribe({
      next: (res) => {
        // 更新 UI：將新留言放到最前面
        this.comments.update(old => [res, ...old]);
        this.newCommentContent = ''; // 清空輸入框
        this.isSubmittingComment = false;
      },
      error: (err) => {
        console.error('發布失敗', err);
        this.isSubmittingComment = false;
      }
    });
  }

  // 接收子元件的回覆成功通知，重新整理留言列表
  refreshComments() {
    this.commentService.getCommentsByPost(this.post.postId).subscribe({
      next: (res) => {
        this.comments.set(res);
      },
      error: (err) => console.error('刷新留言失敗', err)
    });
  }

  // --- 編輯/刪除貼文邏輯 ---
  startEdit() {
    this.isEditing = true;
    this.editContent = this.post.postContent;
  }

  cancelEdit() { this.isEditing = false; }

  saveEdit() {
    if (!this.editContent?.trim()) {
      this.isEditing = false;
      return;
    }
    this.postService.editPost(this.post.postId, this.editContent).subscribe({
      next: (updatedPost: PostDto) => {
        this.post = updatedPost;
        this.isEditing = false;
        this.isMenuOpen = false;
      },
      error: (err) => alert('儲存失敗')
    });
  }

  deletePost() {
    if (!confirm("確定刪除貼文?")) return;
    this.postService.deletePost(this.post.postId).subscribe(() => {
      this.postDeleted.emit(this.post.postId);
    });
  }
}
