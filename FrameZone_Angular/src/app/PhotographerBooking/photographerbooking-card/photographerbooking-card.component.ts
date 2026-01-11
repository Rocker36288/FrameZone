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

  // 新增：追蹤圖片載入狀態
  isLoaded: boolean = false;

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
    // 預設圖片路徑
    let imageUrl = '/images/Photographer/photographercard001.png';

    if (this.photographer.portfolioFile) {
      const firstImage = this.photographer.portfolioFile.split(',')[0].trim();
      imageUrl = firstImage.startsWith('http') ? firstImage : `${apiBaseUrl}${firstImage}`;
    } else if (this.photographer.portfolioUrl) {
      imageUrl = this.photographer.portfolioUrl.startsWith('http') ?
        this.photographer.portfolioUrl : `${apiBaseUrl}${this.photographer.portfolioUrl}`;
    }

    return imageUrl;
  }

  // 新增：圖片載入完成的處理函式
  onImageLoad(): void {
    this.isLoaded = true;
  }


  onViewDetails(): void {
    this.router.navigate(['/photographer-detail'], { queryParams: { id: this.photographer.photographerId } });
  }
}
