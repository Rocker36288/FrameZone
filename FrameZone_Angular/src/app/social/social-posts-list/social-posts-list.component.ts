import { PostService } from '../services/post.service';
import { Component, Input, OnChanges, OnDestroy, OnInit, SimpleChanges } from '@angular/core';
import { PostDto } from "../models/PostDto";
import { SocialPostsComponent } from '../social-posts/social-posts.component';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-social-posts-list',
  imports: [SocialPostsComponent],
  templateUrl: './social-posts-list.component.html',
  styleUrl: './social-posts-list.component.css'
})
export class SocialPostsListComponent implements OnInit, OnChanges, OnDestroy {
  @Input() userId: number | null = null;

  posts: PostDto[] = [];
  private refreshSub?: Subscription;

  constructor(private postService: PostService) { }

  ngOnInit(): void {
    // 1. 初始載入
    this.loadPosts();

    // 2. 訂閱「重新整理」訊號
    this.refreshSub = this.postService.refreshNeeded$.subscribe(() => {
      this.loadPosts();
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['userId'] && !changes['userId'].firstChange) {
      this.loadPosts();
    }
  }

  ngOnDestroy(): void {
    this.refreshSub?.unsubscribe();
  }

  loadPosts() {
    const request$ = this.userId
      ? this.postService.getPostsByUser(this.userId)
      : this.postService.getPosts();

    request$
      .subscribe(posts => {
        this.posts = posts;
        console.log(this.posts);
      });
  }

  onPostDeleted(postId: number) {
    this.posts = this.posts.filter(p => p.postId !== postId);
  }
}
