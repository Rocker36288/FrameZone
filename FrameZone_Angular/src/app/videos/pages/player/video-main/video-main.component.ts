import { Component, Input } from '@angular/core';
import { VideoPlayerComponent } from '../../../ui/video/video-player/video-player.component';
import { VideoTimeagoPipe } from "../../../pipes/video-timeago.pipe";
import { VideoActionsBarComponent } from "../../../ui/actions/video-actions-bar/video-actions-bar.component";
import { ChannelCardComponent } from "../../../ui/channel/channel-card/channel-card.component";
import { NgIf } from '@angular/common';
import { VideoCommentListComponent } from "../../../ui/comments/video-comment-list/video-comment-list.component";
import { VideosListComponent } from "../../../ui/video/videos-list/videos-list.component";
import { ChannelCard, VideoCardData, VideoCommentModel } from '../../../models/video-model';
import { ActivatedRoute } from '@angular/router';
import { VideoWatchService } from '../../../service/video-watch.service';

@Component({
  selector: 'app-video-main',
  imports: [VideoPlayerComponent, VideoTimeagoPipe, VideoActionsBarComponent, ChannelCardComponent, NgIf, VideoCommentListComponent, VideosListComponent],
  templateUrl: './video-main.component.html',
  styleUrl: './video-main.component.css'
})
export class VideoMainComponent {

  // video: Partial<VideoCardData> = {
  //   title: '範例影片標題 Example Video Title',
  //   channelName: '範例頻道 Example Channel',
  //   thumbnail: 'https://picsum.photos/480/270', // 假圖片
  //   duration: 2158,
  //   views: 551,
  //   uploadDate: new Date('2002-02-07'),
  //   description: "我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明我是很長的說明"
  // };

  channel: ChannelCard = {
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

  video: VideoCardData | undefined
  @Input() videosRecommand: VideoCardData[] | undefined
  guid: string | null = null;

  constructor(private route: ActivatedRoute, private videoWatchService: VideoWatchService) { }


  isDescriptionExpanded = false;
  showExpandButton = false;

  isVideoLoaded = false;
  isPlayerHovered = false;
  private readonly MAX_DESCRIPTION_LENGTH = 200;


  // 影片位置
  VideoUrI = ''

  ngOnInit() {
    if (this.video?.description && this.video.description.length > this.MAX_DESCRIPTION_LENGTH) {
      this.showExpandButton = true;
    }

    // 取得路由參數
    this.guid = this.route.snapshot.paramMap.get('guid');

    // 呼叫服務去取得影片資料
    if (this.guid) {
      this.loadVideo(this.guid);
    }

    this.videoWatchService.getVideo(this.guid!).subscribe({
      next: (data) => {
        this.video = data;
        console.log('影片資料:', this.video);
      },
      error: (err) => {
        console.error('取得影片資料失敗', err);
      }
    });

    // 模擬影片載入動畫
    setTimeout(() => {
      this.isVideoLoaded = true;
    }, 300);


  }



  loadVideo(guid: string) {
    // 這裡放呼叫後端 API 或服務取得影片源頭的方法
    console.log('播放影片 GUID:', guid);
    this.VideoUrI = 'https://localhost:7213/api/videoplayer/' + this.guid


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
