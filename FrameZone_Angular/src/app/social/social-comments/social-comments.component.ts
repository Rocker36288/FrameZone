import { AuthService } from './../../core/services/auth.service';
import { Component, Input, Output, EventEmitter, inject, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CommentDto } from '../models/CommentDto';
import { CommentService } from '../services/comment.service';

@Component({
  selector: 'app-social-comments',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './social-comments.component.html',
  styleUrl: './social-comments.component.css'
})
export class SocialCommentsComponent {
  @Input() comments: CommentDto[] = [];
  @Input() postId!: number; // 從父元件傳進來，知道是哪則貼文
  @Output() refresh = new EventEmitter<void>(); // 用於通知父元件重新整理列表

  private commentService = inject(CommentService);

  // 追蹤目前哪則留言顯示輸入框 (存儲 commentId)
  activeReplyId: number | null = null;
  replyContent: string = "";

  activeMenuId: number | null = null;
  editingCommentId: number | null = null;
  editContent = "";


  constructor() { }
  ngOnInit(): void {

  }


  @HostListener('document:click')
  onDocumentClick() {
    this.closeMenu();
    this.activeReplyId = null;
  }

  @HostListener('document:keydown.escape')
  onEsc() {
    this.activeReplyId = null;
  }

  getCommentUserAvatar(comment: CommentDto): string {
    if (comment.avatar) return comment.avatar;
    const initial = (comment.displayName || 'U').charAt(0).toUpperCase();
    return `https://ui-avatars.com/api/?name=${encodeURIComponent(initial)}&background=667eea&color=fff&size=128`;
  }

  toggleMenu(commentId: number, event: MouseEvent) {
    event.stopPropagation();
    this.activeMenuId =
      this.activeMenuId === commentId ? null : commentId;
  }

  closeMenu() {
    this.activeMenuId = null;
  }

  toggleReply(commentId: number) {
    this.activeReplyId =
      this.activeReplyId === commentId ? null : commentId;
  }

  submitReply(parentCommentId: number) {
    if (!this.replyContent.trim()) return;

    const dto = {
      postId: this.postId,
      parentCommentId: parentCommentId, // 關鍵：指向父留言
      commentContent: this.replyContent
    };

    this.commentService.createComment(dto).subscribe({
      next: () => {
        this.activeReplyId = null;
        this.replyContent = "";
        this.refresh.emit(); // 通知最上層父元件重新抓取資料
      },
      error: (err) => console.error('回覆失敗', err)
    });

    this.replyContent = '';
    this.activeReplyId = null;
  }

  // 遞迴中轉：讓每一層的 refresh 都能往上傳遞到父元件
  onChildRefresh() {
    this.refresh.emit();
  }

  startEdit(comment: any) {
    this.editingCommentId = comment.commentId;
    this.editContent = comment.commentContent;
  }

  submitEdit(commentId: number) {
    if (!this.editContent.trim()) return;

    this.commentService
      .updateComment(commentId, this.editContent)
      .subscribe(() => {
        this.editingCommentId = null;
        this.refresh.emit(); // 通知父層重抓留言
      });
  }

  cancelEdit() {
    this.editingCommentId = null;
  }

  deleteComment(commentId: number) {
    if (!confirm('確定要刪除這則留言嗎？')) return;

    this.commentService.deleteComment(commentId).subscribe(() => {
      this.refresh.emit();
    });
  }

}
