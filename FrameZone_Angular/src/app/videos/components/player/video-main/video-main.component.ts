import { Component } from '@angular/core';
import { VideoPlayerComponent } from "../video-player/video-player.component";
import { VideoCardData } from '../../../models/video-data.interface';
import { ChannelData } from '../../../models/channel-data.interface';
import { VideoTimeagoPipe } from "../../../pipes/video-timeago.pipe";
import { VideoActionsBarComponent } from "../../../shared/actions/video-actions-bar/video-actions-bar.component";
import { ChannelCardComponent } from "../../../shared/channel/channel-card/channel-card.component";
import { NgIf } from '@angular/common';
import { CommentInputComponent } from "../../../shared/comments/video-comment-input/video-comment-input.component";
import { VideoCommentListComponent } from "../../../shared/comments/video-comment-list/video-comment-list.component";
import { VideoCommentModel } from '../../../models/video-comment.interface';
import { VideosRecommendedListComponent } from "../../../shared/video/videos-recommended-list/videos-recommended-list.component";

@Component({
  selector: 'app-video-main',
  imports: [VideoPlayerComponent, VideoTimeagoPipe, VideoActionsBarComponent, ChannelCardComponent, NgIf, CommentInputComponent, VideoCommentListComponent, VideosRecommendedListComponent],
  templateUrl: './video-main.component.html',
  styleUrl: './video-main.component.css'
})
export class VideoMainComponent {

  video: Partial<VideoCardData> = {
    title: '範例影片標題 Example Video Title',
    channelName: '範例頻道 Example Channel',
    thumbnail: 'https://picsum.photos/480/270', // 假圖片
    duration: 2158,
    views: 551,
    uploadDate: new Date('2002-02-07'),
    description: "我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明"
  };

  channel: ChannelData = {
    id: 1,
    Name: '頻道名稱示例',
    Avatar: 'https://i.pravatar.cc/48',
    Description: "這個人很懶，甚麼都沒留",
    Follows: 12345,
  };

  commet1: VideoCommentModel = {
    id: 1,
    userName: '我暴斃2',
    avatar: 'https://i.pravatar.cc/48',
    message: '這影片我一直調到快死RRRR',
    createdAt: new Date('2025-12-01'),
    likes: 100,
  }

  commentList: VideoCommentModel[] = [{
    id: 1,
    userName: '我暴斃',
    avatar: 'https://i.pravatar.cc/48',
    message: '這影片我一直調到快死',
    createdAt: new Date('2025-12-01'),
    likes: 100,
    replies: [this.commet1]
  }, {
    id: 2,
    userName: '我暴斃3',
    avatar: 'https://i.pravatar.cc/48',
    message: '這影片我一直調到快死',
    createdAt: new Date('2025-12-06'),
    likes: 150,
    replies: [this.commet1, this.commet1]
  }
  ]

  videos: VideoCardData[] = [{
    id: 1,
    userAvatarUrl: 'https://i.pravatar.cc/49',
    title: '範例影片標題 Example Video Title',
    channelName: '範例頻道 Example Channel',
    thumbnail: 'https://picsum.photos/480/270', // 假圖片
    duration: 2158,
    views: 551,
    uploadDate: new Date('2002-02-07'),
    description: "fff"
  }, {
    id: 1,
    userAvatarUrl: 'https://i.pravatar.cc/49',
    title: '範例影片標題 Example Video Title',
    channelName: '範例頻道 Example Channel',
    thumbnail: 'https://picsum.photos/480/270', // 假圖片
    duration: 21558,
    views: 551,
    uploadDate: new Date('2002-02-07'),
    description: "fff"
  }];



  isDescriptionExpanded = false;
  showExpandButton = false;

  isVideoLoaded = false;
  isPlayerHovered = false;
  private readonly MAX_DESCRIPTION_LENGTH = 200;

  ngOnInit() {
    if (this.video?.description && this.video.description.length > this.MAX_DESCRIPTION_LENGTH) {
      this.showExpandButton = true;
    }

    // 模擬影片載入動畫
    setTimeout(() => {
      this.isVideoLoaded = true;
    }, 300);
  }

  toggleDescription() {
    this.isDescriptionExpanded = !this.isDescriptionExpanded;
  }

  getDisplayDescription(): string {
    if (!this.video?.description) return '';

    if (this.isDescriptionExpanded || this.video.description.length <= this.MAX_DESCRIPTION_LENGTH) {
      return this.video.description;
    }

    return this.video.description.substring(0, this.MAX_DESCRIPTION_LENGTH) + '...';
  }

  onPlayerHover(state: boolean) {
    this.isPlayerHovered = state;
  }

  addComment($event: string) {
    throw new Error('Method not implemented.');
  }

}
