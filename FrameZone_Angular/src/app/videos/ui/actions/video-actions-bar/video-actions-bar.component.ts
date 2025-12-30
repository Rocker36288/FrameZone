import { Component, EventEmitter, Input, Output } from '@angular/core';
import { LikeButtonComponent } from "../like-button/like-button.component";
import { ReportButtonComponent } from "../report-button/report-button.component";
import { SaveButtonComponent } from "../save-button/save-button.component";
import { ShareButtonComponent } from "../share-button/share-button.component";

@Component({
  selector: 'app-video-actions-bar',
  imports: [LikeButtonComponent, ReportButtonComponent, SaveButtonComponent, ShareButtonComponent],
  templateUrl: './video-actions-bar.component.html',
  styleUrl: './video-actions-bar.component.css'
})
export class VideoActionsBarComponent {
  @Output() likeChanged = new EventEmitter<boolean>();
  @Input() isLiked: boolean = false;
  @Input() likes?: number = 0;

  onLikeToggled(liked: boolean) {
    this.likeChanged.emit(liked);
  }

  @Output() share = new EventEmitter<boolean>();

  onShare() {
    console.log('➡ ActionBar emit share');
    this.share.emit();
  }

}
