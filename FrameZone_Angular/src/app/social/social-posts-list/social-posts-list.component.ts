import { PostService } from '../services/post.service';
import { Component, Input, OnChanges, OnDestroy, OnInit, SimpleChanges } from '@angular/core';
import { PostDto } from "../models/PostDto";
import { SocialPostsComponent } from '../social-posts/social-posts.component';
import { Subscription, map, of, switchMap } from 'rxjs';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { FollowService } from '../services/follow.service';

@Component({
  selector: 'app-social-posts-list',
  imports: [SocialPostsComponent],
  templateUrl: './social-posts-list.component.html',
  styleUrl: './social-posts-list.component.css'
})
export class SocialPostsListComponent implements OnInit, OnChanges, OnDestroy {
  @Input() userId: number | null = null;
  @Input() userIds: number[] | null = null;

  posts: PostDto[] = [];
  private refreshSub?: Subscription;
  private userSub?: Subscription;
  private isFollowingRoute = false;
  private isRecentRoute = false;
  private isLikedRoute = false;

  constructor(
    private postService: PostService,
    private authService: AuthService,
    private followService: FollowService,
    private route: ActivatedRoute
  ) {
    const routePath = this.route.snapshot.routeConfig?.path;
    this.isFollowingRoute = routePath === 'following';
    this.isRecentRoute = routePath === 'recent';
    this.isLikedRoute = routePath === 'liked';
  }

  ngOnInit(): void {
    // 1. ?濆?杓夊叆
    this.loadPosts();

    // 2. 瑷傞柋?岄??版暣?嗐€嶈???
    this.refreshSub = this.postService.refreshNeeded$.subscribe(() => {
      this.loadPosts();
    });

    if (this.isFollowingRoute || this.isRecentRoute || this.isLikedRoute) {
      this.userSub = this.authService.currentUser$.subscribe(() => {
        this.loadPosts();
      });
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if ((changes['userId'] && !changes['userId'].firstChange)
      || (changes['userIds'] && !changes['userIds'].firstChange)) {
      this.loadPosts();
    }
  }

  ngOnDestroy(): void {
    this.refreshSub?.unsubscribe();
    this.userSub?.unsubscribe();
  }

  loadPosts() {
    if (this.userIds && this.userIds.length === 0) {
      this.posts = [];
      return;
    }

    if (this.userIds && this.userIds.length > 0) {
      const idSet = new Set(this.userIds);
      this.postService.getPosts().subscribe(posts => {
        this.posts = posts.filter(post => idSet.has(post.userId));
        console.log(this.posts);
      });
      return;
    }

    if (this.userId) {
      this.postService.getPostsByUser(this.userId).subscribe(posts => {
        this.posts = posts;
        console.log(this.posts);
      });
      return;
    }

    if (this.isRecentRoute) {
      this.postService.getRecentViews(20).subscribe({
        next: (posts) => {
          this.posts = posts;
          console.log(this.posts);
        },
        error: () => {
          this.posts = [];
        }
      });
      return;
    }

    if (this.isLikedRoute) {
      this.postService.getLikedPosts(20).subscribe({
        next: (posts) => {
          this.posts = posts;
          console.log(this.posts);
        },
        error: () => {
          this.posts = [];
        }
      });
      return;
    }

    if (this.isFollowingRoute) {
      const currentUserId = this.authService.getCurrentUser()?.userId ?? null;
      if (!currentUserId) {
        this.posts = [];
        return;
      }
      this.followService.getFollowing(currentUserId).pipe(
        map(users => users.map(user => user.userId)),
        switchMap(ids => {
          if (ids.length === 0) return of([] as PostDto[]);
          const idSet = new Set(ids);
          return this.postService.getPosts().pipe(
            map(posts => posts.filter(post => idSet.has(post.userId)))
          );
        })
      ).subscribe(posts => {
        this.posts = posts;
        console.log(this.posts);
      });
      return;
    }

    this.postService.getPosts().subscribe(posts => {
      this.posts = posts;
      console.log(this.posts);
    });
  }

  onPostDeleted(postId: number) {
    this.posts = this.posts.filter(p => p.postId !== postId);
  }
}
