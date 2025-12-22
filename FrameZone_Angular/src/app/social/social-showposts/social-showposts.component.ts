import { PostService } from '../services/post.service';
import { Component, ElementRef, EventEmitter, HostListener, Input, Output } from '@angular/core';
import { PostDto } from "../models/PostDto";
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-social-showposts',
  imports: [DatePipe, FormsModule],
  templateUrl: './social-showposts.component.html',
  styleUrl: './social-showposts.component.css'
})
export class SocialShowpostsComponent {
  @Input() post!: PostDto;
  @Output() postDeleted = new EventEmitter<number>();

  isEditing = false;
  editContent = "";
  isMenuOpen = false; // 控制選單開關

  constructor(private postService: PostService, private eRef: ElementRef) { }

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
    this.postService.editPost(this.post.postId, this.editContent)
      .subscribe(updatePost => {
        this.post.postContent = updatePost.postContent;
        this.isEditing = false;
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
