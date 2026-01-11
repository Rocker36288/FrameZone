import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
interface Review {
  reviewId: number;
  reviewerName: string;
  rating: number;
  reviewContent: string;
  createdAt: string;
  avatarUrl: string;
  photos: string[];
}
@Component({
  selector: 'app-photographer-review',
  imports: [CommonModule],
  templateUrl: './photographer-review.component.html',
  styleUrl: './photographer-review.component.css',
})
export class PhotographerReviewComponent {
  @Input() reviews: Review[] = [];
  @Input() rating: number = 0;
  @Input() reviewCount: number = 0;

  // 暫時寫死的細分評分項目 (精緻化 UI 用)
  ratingItems = [
    { label: '拍攝效果', score: 5.0, percent: 100 },
    { label: '溝通品質', score: 4.8, percent: 96 },
    { label: '服務態度', score: 4.9, percent: 98 },
    { label: '價格公道', score: 4.7, percent: 94 },
    { label: '準時與速度', score: 4.9, percent: 98 },
    { label: '設備專業', score: 5.0, percent: 100 },
  ];

  getStarArray(rating: number): number[] {
    return Array(Math.floor(rating)).fill(0);
  }
}
