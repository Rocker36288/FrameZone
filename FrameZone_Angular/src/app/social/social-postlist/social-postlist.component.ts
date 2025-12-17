import { PostService } from '../services/post.service';
import { Component } from '@angular/core';
import { PostDto } from '../services/post.service';
import { SocialShowpostsComponent } from '../social-showposts/social-showposts.component';

@Component({
  selector: 'app-social-postlist',
  imports: [SocialShowpostsComponent],
  templateUrl: './social-postlist.component.html',
  styleUrl: './social-postlist.component.css'
})
export class SocialPostlistComponent {
  posts: PostDto[] = [];

  constructor(private postService: PostService) { }

  ngOnInit(): void {
    this.loadPosts();
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
