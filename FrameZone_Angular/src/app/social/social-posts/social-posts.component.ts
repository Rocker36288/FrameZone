import { PostService } from '../services/post.service';
import { Component } from '@angular/core';
import { FormsModule, ɵInternalFormsSharedModule } from "@angular/forms";

@Component({
  selector: 'app-social-posts',
  imports: [ɵInternalFormsSharedModule, FormsModule],
  templateUrl: './social-posts.component.html',
  styleUrl: './social-posts.component.css'
})
export class SocialPostsComponent {
  postContent = "";

  constructor(private postService: PostService) { }

  submitPost() {
    this.postService.createPost(this.postContent)
      .subscribe({
        next: res => {
          alert("發布成功");
          this.postContent = "";
        },
        error: err => {
          console.error(err);
          alert("發布失敗");
        }
      })
  }
}
