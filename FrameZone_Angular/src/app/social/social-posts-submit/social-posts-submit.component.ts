import { PostService } from '../services/post.service';
import { Component } from '@angular/core';
import { FormsModule, ɵInternalFormsSharedModule } from "@angular/forms";

@Component({
  selector: 'app-social-posts-submit',
  imports: [ɵInternalFormsSharedModule, FormsModule],
  templateUrl: './social-posts-submit.component.html',
  styleUrl: './social-posts-submit.component.css'
})
export class SocialPostsSubmitComponent {
  postContent = "";

  constructor(private postService: PostService) { }

  submitPost() {
    this.postService.createPost(this.postContent)
      .subscribe({
        next: res => {
          // alert("發布成功");
          this.postContent = "";
        },
        error: err => {
          console.error(err);
          alert("發布失敗");
        }
      })
  }
}
