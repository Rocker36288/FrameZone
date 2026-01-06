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

  getStarArray(rating: number): number[] {
    return Array(Math.floor(rating)).fill(0);
  }
}
