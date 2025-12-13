import { NgIf, NgForOf } from '@angular/common';
import { Component, Input } from '@angular/core';
import { VideoTimeagoPipe } from '../../../pipes/video-timeago.pipe';
import { CommentInputComponent } from "../video-comment-input/video-comment-input.component";
import { VideoCommentReplyComponent } from "../video-comment-reply/video-comment-reply.component";
import { VideoCommentModel } from '../../../models/video-comment.interface';

@Component({
  selector: 'app-video-comment-item',
  imports: [VideoTimeagoPipe, NgIf, NgForOf, CommentInputComponent, VideoCommentReplyComponent],
  templateUrl: './video-comment-item.component.html',
  styleUrl: './video-comment-item.component.css'
})
export class VideoCommentItemComponent {
  @Input() comment: VideoCommentModel | undefined;

  showReplyInput = false;



  toggleReply() {
    this.showReplyInput = !this.showReplyInput;
  }

  onReply(text: string) {
    this.comment?.replies
  }

  // onReply(text: string) {
  //   this.comment?.replies.push({
  //     id: 1,
  //     userName: '你',
  //     avatar: 'https://i.pravatar.cc/32',
  //     message: '',
  //     createdAt: new Date(),
  //   });

  // this.showReplyInput = false;
}
