import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { Router } from '@angular/router';
import { Photographer } from '../models/photographer-booking.models';

@Component({
  selector: 'app-photographerbooking-card',
  imports: [CommonModule],
  templateUrl: './photographerbooking-card.component.html',
  styleUrl: './photographerbooking-card.component.css',
})
export class PhotographerbookingCardComponent {
  constructor(private router: Router) { }
  @Input() photographer!: Photographer;

  get formattedPrice(): string {
    // If we have services, show min price
    if (this.photographer.services && this.photographer.services.length > 0) {
      const min = Math.min(...this.photographer.services.map(s => s.basePrice));
      return `NT$ ${min.toLocaleString()} 起`;
    }
    return '價格詳談';
  }

  get cardImage(): string {
    const apiBaseUrl = 'https://localhost:7213';

    // 優先使用 portfolioFile 的第一張圖
    if (this.photographer.portfolioFile) {
      const files = this.photographer.portfolioFile.split(',');
      if (files.length > 0) {
        const firstImage = files[0].trim();
        if (firstImage.startsWith('http')) {
          return firstImage;
        }
        return `${apiBaseUrl}${firstImage}`;
      }
    }

    // Fallback 到 portfolioUrl
    if (this.photographer.portfolioUrl) {
      if (this.photographer.portfolioUrl.startsWith('http')) {
        return this.photographer.portfolioUrl;
      }
      return `${apiBaseUrl}${this.photographer.portfolioUrl}`;
    }

    // 最後 fallback 到預設圖片
    return '/images/Photographer/photographercard001.png';
  }

  onViewDetails(): void {
    this.router.navigate(['/photographer-detail'], { queryParams: { id: this.photographer.photographerId } });
  }
}
