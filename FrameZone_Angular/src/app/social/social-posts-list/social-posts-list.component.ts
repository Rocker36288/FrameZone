import { PostService } from '../services/post.service';
import { Component } from '@angular/core';
import { PostDto } from "../models/PostDto";
import { SocialPostsComponent } from '../social-posts/social-posts.component';

@Component({
  selector: 'app-social-posts-list',
  imports: [SocialPostsComponent],
  templateUrl: './social-posts-list.component.html',
  styleUrl: './social-posts-list.component.css'
})
export class SocialPostsListComponent {
  posts: PostDto[] = [];

  constructor(private postService: PostService) { }

  ngOnInit(): void {
    // 1. 初始載入
    this.loadPosts();

    // 2. 訂閱「重新整理」訊號
    this.postService.refreshNeeded$.subscribe(() => {
      this.loadPosts();
    });
  }

  loadPosts() {
    this.postService.getPosts()
      .subscribe(posts => {
        this.posts = posts;
        console.log(this.posts);
      });
  }

  onPostDeleted(postId: number) {
    this.posts = this.posts.filter(p => p.postId !== postId);
  }
}
