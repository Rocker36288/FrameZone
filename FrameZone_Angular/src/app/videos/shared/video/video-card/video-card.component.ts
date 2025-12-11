import { Component, EventEmitter, Input, Output } from '@angular/core';
import { VideoCardData } from '../../../models/video-data.interface';
import { RouterLink } from "@angular/router";
import { VideoTimeagoPipe } from "../../../pipes/video-timeago.pipe";
import { VideoDurationPipe } from "../../../pipes/video-duration.pipe"; // 步驟 1 定義的 Interface
@Component({
  selector: 'app-video-card',
  imports: [VideoTimeagoPipe, VideoDurationPipe],
  templateUrl: './video-card.component.html',
  styleUrl: './video-card.component.css'
})
export class VideoCardComponent {
  @Input() video!: VideoCardData;
  @Input() variant: 'grid' | 'list' | 'compact' | 'sidebar' = 'grid';
  @Input() showChannel: boolean = true;
  @Input() showDescription: boolean = true;

  @Output() videoClick = new EventEmitter<string>();



  onVideoClick(): void {
    if (this.video) {
    }
  }

  constructor() {

  }
}
