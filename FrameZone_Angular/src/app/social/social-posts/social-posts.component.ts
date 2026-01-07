import { PostService } from '../services/post.service';
import { CommentService } from '../services/comment.service';
import { AfterViewInit, Component, ElementRef, EventEmitter, HostListener, Input, OnDestroy, Output, signal, inject } from '@angular/core';
import { PostDto } from "../models/PostDto";
import { CommentDto } from "../models/CommentDto";
import { DatePipe, SlicePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SocialCommentsComponent } from "../social-comments/social-comments.component";
import { AuthService } from '../../core/services/auth.service';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-social-posts',
  imports: [DatePipe, SlicePipe, FormsModule, SocialCommentsComponent, RouterLink],
  templateUrl: './social-posts.component.html',
  styleUrl: './social-posts.component.css'
})
export class SocialPostsComponent implements AfterViewInit, OnDestroy {
  @Input() post!: PostDto;
  @Output() postDeleted = new EventEmitter<number>();

  private authService = inject(AuthService);
  currentUserAvatar = this.authService.getCurrentUser()?.avatar || null;
  currentUserName = this.authService.getCurrentUser()?.displayName || null;

  // --- 貼文狀態 ---
  isEditing = false;
  editContent = "";
  isMenuOpen = false;
  isFullContent = false;

  // --- 留言狀態 ---
  isCommentShowed = false;
  comments = signal<CommentDto[]>([]); // 使用 Signal 管理留言列表
  newCommentContent = ""; // 綁定發布框
  isSubmittingComment = false; // 防止重複點擊
  private viewObserver?: IntersectionObserver;
  private hasTrackedView = false;
  isShareModalOpen = false;
  shareMessage = '';
  shareTarget: 'personal' | 'facebook' | 'twitter' | 'line' = 'personal';
  shareLink = window.location.href;

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

  ngAfterViewInit(): void {
    const currentUser = this.authService.getCurrentUser();
    if (!currentUser?.userId) return;

    this.viewObserver = new IntersectionObserver((entries) => {
      for (const entry of entries) {
        if (entry.isIntersecting && !this.hasTrackedView) {
          this.hasTrackedView = true;
          this.postService.recordView(this.post.postId).subscribe({
            next: () => { },
            error: () => { }
          });
          this.viewObserver?.disconnect();
          break;
        }
      }
    }, { threshold: 0.5 });

    this.viewObserver.observe(this.eRef.nativeElement);
  }

  ngOnDestroy(): void {
    this.viewObserver?.disconnect();
  }

  //頭像
  getUserAvatar(): string {
    if (this.currentUserAvatar) return this.currentUserAvatar;
    const initial = (this.currentUserName || 'U').charAt(0).toUpperCase();
    return `https://ui-avatars.com/api/?name=${encodeURIComponent(initial)}&background=667eea&color=fff&size=128`;
  }
  getPostUserAvatar(): string {
    if (this.post.avatar) return this.post.avatar;
    const initial = (this.post.userName || 'U').charAt(0).toUpperCase();
    return `https://ui-avatars.com/api/?name=${encodeURIComponent(initial)}&background=667eea&color=fff&size=128`;
  }
  getSharedUserAvatar(sharedPost: PostDto): string {
    if (sharedPost.avatar) return sharedPost.avatar;
    const initial = (sharedPost.userName || 'U').charAt(0).toUpperCase();
    return `https://ui-avatars.com/api/?name=${encodeURIComponent(initial)}&background=667eea&color=fff&size=128`;
  }

  //右上角選單
  toggleMenu() { this.isMenuOpen = !this.isMenuOpen; }
  closeMenu() { this.isMenuOpen = false; }

  // --- 貼文邏輯 ---
  toggleContent() { this.isFullContent = !this.isFullContent; }
  toggleLikes() {
    const wasLiked = !!this.post.isLiked;
    const currentCount = this.post.likeCount ?? 0;
    this.post.isLiked = !wasLiked;
    this.post.likeCount = wasLiked ? Math.max(0, currentCount - 1) : currentCount + 1;

    const request$ = wasLiked
      ? this.postService.unlikePost(this.post.postId)
      : this.postService.likePost(this.post.postId);

    request$.subscribe({
      next: () => { },
      error: () => {
        this.post.isLiked = wasLiked;
        this.post.likeCount = currentCount;
      }
    });
  }

  openShareModal() {
    this.shareMessage = '';
    this.shareTarget = 'personal';
    this.shareLink = window.location.href;
    this.isShareModalOpen = true;
  }

  closeShareModal() {
    this.isShareModalOpen = false;
  }

  submitShare() {
    const currentUser = this.authService.getCurrentUser();
    if (!currentUser?.userId) return;
    if (this.post.isShared && this.shareTarget === 'personal') return;

    if (this.shareTarget === 'personal') {
      const currentCount = this.post.shareCount ?? 0;
      this.post.isShared = true;
      this.post.shareCount = currentCount + 1;

      this.postService.recordShare(this.post.postId, this.shareMessage).subscribe({
        next: () => {
          this.closeShareModal();
        },
        error: () => {
          this.post.isShared = false;
          this.post.shareCount = currentCount;
        }
      });
      return;
    }

    const shareUrl = encodeURIComponent(window.location.href);
    const shareText = encodeURIComponent(this.shareMessage || this.post.postContent || '');

    if (this.shareTarget === 'facebook') {
      window.open(`https://www.facebook.com/sharer/sharer.php?u=${shareUrl}`, '_blank');
    } else if (this.shareTarget === 'twitter') {
      window.open(`https://twitter.com/intent/tweet?text=${shareText}&url=${shareUrl}`, '_blank');
    } else if (this.shareTarget === 'line') {
      window.open(`https://social-plugins.line.me/lineit/share?url=${shareUrl}&text=${shareText}`, '_blank');
    }

    this.closeShareModal();
  }

  copyShareLink() {
    const url = window.location.href;
    if (navigator.clipboard?.writeText) {
      navigator.clipboard.writeText(url).catch(() => { });
      return;
    }
    const textarea = document.createElement('textarea');
    textarea.value = url;
    textarea.setAttribute('readonly', '');
    textarea.style.position = 'fixed';
    textarea.style.opacity = '0';
    document.body.appendChild(textarea);
    textarea.select();
    document.execCommand('copy');
    document.body.removeChild(textarea);
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
        // 更新 UI：補上當前使用者資訊，避免一開始顯示為「新使用者」
        const currentUser = this.authService.getCurrentUser();
        const enrichedComment: CommentDto = {
          ...res,
          displayName: currentUser?.displayName ?? res.displayName,
          avatar: currentUser?.avatar ?? res.avatar,
          userId: currentUser?.userId ?? res.userId,
          isOwner: true
        };
        this.comments.update(old => [enrichedComment, ...old]);
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
