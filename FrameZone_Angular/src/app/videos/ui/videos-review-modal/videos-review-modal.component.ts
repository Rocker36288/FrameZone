import { Component, EventEmitter, Input, Output, OnChanges, SimpleChanges } from '@angular/core';
import { NgClass, NgIf, NgForOf, DecimalPipe } from "@angular/common";

@Component({
  selector: 'app-videos-review-modal',
  standalone: true,
  imports: [NgClass, NgIf, NgForOf, DecimalPipe],
  templateUrl: './videos-review-modal.component.html',
  styleUrl: './videos-review-modal.component.css'
})
export class VideosReviewModalComponent implements OnChanges {

  @Input() isLoading = false;

  // ⛔ 從父層來的是「string JSON」
  @Input() reviewData: string | null = null;

  // ✅ 真正給 template 用的「物件」
  parsedReviewData: any = null;

  @Output() close = new EventEmitter<void>();

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['reviewData'] && this.reviewData) {
      try {
        this.parsedReviewData = JSON.parse(this.reviewData);
        console.log('parsedReviewData', this.parsedReviewData);
      } catch (e) {
        console.error('reviewData JSON parse 失敗', e);
        this.parsedReviewData = null;
      }
    }
  }

  onBackdropClick() {
    this.close.emit();
  }

  nudityKeys(): string[] {
    const nudity = this.parsedReviewData?.sightengine?.nudity;
    if (!nudity) return [];

    return Object.keys(nudity).filter(
      k => typeof nudity[k] === 'number'
    );
  }

  contextKeys(): string[] {
    const context = this.parsedReviewData?.sightengine?.nudity?.context;
    if (!context) return [];

    return Object.keys(context);
  }
}
