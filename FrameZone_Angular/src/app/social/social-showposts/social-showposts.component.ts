import { PostService } from '../services/post.service';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { PostDto } from '../services/post.service';
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

  constructor(private postService: PostService) { }

  startEdit() {
    this.isEditing = true;
    this.editContent = this.post.postContent;
  }

  cancelEdit() {
    this.isEditing = false;
  }

  saveEdit() {
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
