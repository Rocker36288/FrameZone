import { Component } from '@angular/core';

@Component({
  selector: 'app-social-posts-images',
  imports: [],
  templateUrl: './social-posts-images.component.html',
  styleUrl: './social-posts-images.component.css'
})
export class SocialPostsImagesComponent {
  // 模擬貼文資料
  posts = [
    { id: 1, imageUrl: 'https://picsum.photos/500/500?random=1' },
    { id: 2, imageUrl: 'https://picsum.photos/500/500?random=2' },
    { id: 3, imageUrl: 'https://picsum.photos/500/500?random=3' },
    { id: 4, imageUrl: 'https://picsum.photos/500/500?random=4' },
    { id: 5, imageUrl: 'https://picsum.photos/500/500?random=5' },
    { id: 6, imageUrl: 'https://picsum.photos/500/500?random=6' },
  ];

  viewPost(postId: number) {
    console.log('跳轉到貼文 ID:', postId);
    // 之後可以在這裡實作 Router.navigate
  }
}
