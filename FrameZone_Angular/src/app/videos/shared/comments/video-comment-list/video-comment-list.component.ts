
import { Component, Input } from '@angular/core';
import { VideoCommentModel } from '../../../models/video-comment.interface';
import { VideoCommentItemComponent } from "../video-comment-item/video-comment-item.component";
import { NgForOf, NgIf } from '@angular/common';

@Component({
  selector: 'app-video-comment-list',
  imports: [VideoCommentItemComponent, NgForOf, NgIf],
  templateUrl: './video-comment-list.component.html',
  styleUrl: './video-comment-list.component.css'
})
export class VideoCommentListComponent {
  @Input() comments: VideoCommentModel[] = [];
}
