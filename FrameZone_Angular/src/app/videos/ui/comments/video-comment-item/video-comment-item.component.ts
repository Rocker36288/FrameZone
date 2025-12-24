import { NgIf, NgForOf } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { VideoTimeagoPipe } from '../../../pipes/video-timeago.pipe';
import { CommentInputComponent } from "../video-comment-input/video-comment-input.component";
import { VideoCommentReplyComponent } from "../video-comment-reply/video-comment-reply.component";
import { VideoCommentCard } from '../../../models/video-model';

@Component({
  selector: 'app-video-comment-item',
  imports: [VideoTimeagoPipe, NgIf, NgForOf, CommentInputComponent, VideoCommentReplyComponent],
  templateUrl: './video-comment-item.component.html',
  styleUrl: './video-comment-item.component.css'
})
export class VideoCommentItemComponent {
  @Input() comment!: VideoCommentCard;
  @Output() replyClicked = new EventEmitter<number>(); // 父留言ID
  @Output() submitReply = new EventEmitter<{ parentId: number, message: string }>();

  showReplyInput = false;

  toggleReply() {
    this.showReplyInput = !this.showReplyInput;
  }


  onReply(message: string) {
    this.submitReply.emit({
      parentId: this.comment.id, // ✅ 父留言 ID 就在這
      message
    });
    this.showReplyInput = false;
  }
}
