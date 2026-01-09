import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { PhotographerDto } from '../models/photographer-booking.models';

@Component({
  selector: 'app-photographer-profile',
  imports: [CommonModule],
  templateUrl: './photographer-profile.component.html',
  styleUrl: './photographer-profile.component.css',
})
export class PhotographerProfileComponent {
  @Input() photographer: PhotographerDto | null = null;
  @Input() specialties: string[] = [];
  @Input() portfolioImages: string[] = [];
  @Output() showAllPhotos = new EventEmitter<void>();

  // 燈箱狀態控制
  isLightboxOpen = false;
  currentImageIndex = 0;
  isZoomed = false;
  zoomLevel = 1;

  onShowAllPhotos(): void {
    // 開啟燈箱並從第一張開始
    this.currentImageIndex = 0;
    this.isLightboxOpen = true;
    this.isZoomed = false;
    this.zoomLevel = 1;

    // 防止背景滾動
    document.body.style.overflow = 'hidden';

    // 如果你依然想通知父組件，保留這一行
    this.showAllPhotos.emit();
  }

  closeLightbox(): void {
    this.isLightboxOpen = false;
    this.isZoomed = false;
    this.zoomLevel = 1;

    // 恢復背景滾動
    document.body.style.overflow = '';
  }

  nextImage(): void {
    this.currentImageIndex = (this.currentImageIndex + 1) % this.portfolioImages.length;
    this.isZoomed = false;
    this.zoomLevel = 1;
  }

  prevImage(): void {
    this.currentImageIndex = (this.currentImageIndex - 1 + this.portfolioImages.length) % this.portfolioImages.length;
    this.isZoomed = false;
    this.zoomLevel = 1;
  }

  toggleZoom(): void {
    this.isZoomed = !this.isZoomed;
    this.zoomLevel = this.isZoomed ? 2 : 1;
  }

  onKeyDown(event: KeyboardEvent): void {
    if (!this.isLightboxOpen) return;

    switch (event.key) {
      case 'Escape':
        this.closeLightbox();
        break;
      case 'ArrowLeft':
        this.prevImage();
        break;
      case 'ArrowRight':
        this.nextImage();
        break;
    }
  }

  preventContextMenu(event: MouseEvent): boolean {
    event.preventDefault();
    return false;
  }
}
