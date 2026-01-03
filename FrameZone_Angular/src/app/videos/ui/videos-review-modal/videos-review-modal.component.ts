import { Component, EventEmitter, Input, Output } from '@angular/core';
import { NgClass, NgIf, NgForOf } from "@angular/common";
import { DecimalPipe } from "@angular/common";

@Component({
  selector: 'app-videos-review-modal',
  imports: [NgClass, NgIf, NgForOf, DecimalPipe],
  templateUrl: './videos-review-modal.component.html',
  styleUrl: './videos-review-modal.component.css'
})
export class VideosReviewModalComponent {
  @Input() reviewData: any; // 接收影片審核結果
  @Output() close = new EventEmitter<void>();

  onBackdropClick() {
    this.close.emit();
  }
  nudityKeys() {
    return this.reviewData?.sightengine?.nudity
      ? Object.keys(this.reviewData.sightengine.nudity).filter(k => typeof this.reviewData.sightengine.nudity[k] === 'number')
      : [];
  }

  contextKeys() {
    return this.reviewData?.sightengine?.nudity?.context
      ? Object.keys(this.reviewData.sightengine.nudity.context)
      : [];
  }

}
