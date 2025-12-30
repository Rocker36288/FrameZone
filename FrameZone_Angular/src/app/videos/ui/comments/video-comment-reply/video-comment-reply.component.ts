import { Component, Input } from '@angular/core';
import { VideoCommentCard } from '../../../models/video-model';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-video-comment-reply',
  imports: [DatePipe],
  templateUrl: './video-comment-reply.component.html',
  styleUrl: './video-comment-reply.component.css'
})
export class VideoCommentReplyComponent {
  @Input() comment!: VideoCommentCard;

  onAvatarlError(event: ErrorEvent) {
    const img = event.target as HTMLImageElement;
    img.src = 'favicon2.png';
  }
}


