
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { VideoCommentItemComponent } from "../video-comment-item/video-comment-item.component";
import { NgForOf, NgIf } from '@angular/common';
import { VideoCommentCard } from '../../../models/video-model';

@Component({
  selector: 'app-video-comment-list',
  imports: [VideoCommentItemComponent, NgForOf, NgIf],
  templateUrl: './video-comment-list.component.html',
  styleUrl: './video-comment-list.component.css'
})
export class VideoCommentListComponent {
  @Input() comments: VideoCommentCard[] = [];
  @Output() submitReply = new EventEmitter<{ parentId: number; message: string }>();

  onSubmitReply(event: { parentId: number; message: string }) {
    this.submitReply.emit(event);
  }
}
