import { Component, Input } from '@angular/core';
import { VideoTimeagoPipe } from '../../../pipes/video-timeago.pipe';
import { VideoCommentModel } from '../../../models/video-comment.interface';

@Component({
  selector: 'app-video-comment-reply',
  imports: [VideoTimeagoPipe],
  templateUrl: './video-comment-reply.component.html',
  styleUrl: './video-comment-reply.component.css'
})
export class VideoCommentReplyComponent {
  @Input() comment: VideoCommentModel | undefined;
}
