import { Component, Input } from '@angular/core';
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
  @Input() videoid: number = 0

}
