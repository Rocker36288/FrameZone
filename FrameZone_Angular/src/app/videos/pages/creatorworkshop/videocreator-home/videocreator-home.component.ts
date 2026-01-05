import { Component } from '@angular/core';
import { AuthService } from '../../../../core/services/auth.service';
import { VideosListComponent } from "../../../ui/video/videos-list/videos-list.component";
import { MockChannelService } from './../../../service/mock-channel.service';
import { VideoDetailData } from '../../../models/videocreator-model';
import { VideoCreatorService } from '../../../service/video-creator.service';
import { DecimalPipe } from '@angular/common';

@Component({
  selector: 'app-videocreator-home',
  imports: [DecimalPipe, VideosListComponent],
  templateUrl: './videocreator-home.component.html',
  styleUrl: './videocreator-home.component.css'
})
export class VideocreatorHomeComponent {
  userId: string | null = null;
  VideoDetailsData: VideoDetailData[] = [
  ]

  constructor(private authService: AuthService, private MockChannelService: MockChannelService, private videoCreatorService: VideoCreatorService) { }

  ngOnInit(): void {
    this.videoCreatorService.getRecentUploadVideos(5)
      .subscribe(videos => {
        this.VideoDetailsData = videos;
      });
  }


  /** 📊 總觀看數 */
  get totalViews(): number {
    return this.VideoDetailsData.reduce(
      (sum, v) => sum + (v.viewsCount ?? 0),
      0
    );
  }

  /** 🎬 影片數量（含草稿） */
  get totalVideos(): number {
    return this.VideoDetailsData.length;
  }

  /** ❤️ 總讚數 */
  get totalLikes(): number {
    return this.VideoDetailsData.reduce(
      (sum, v) => sum + (v.likesCount ?? 0),
      0
    );
  }

  /** 💬 總留言數 */
  get totalComments(): number {
    return this.VideoDetailsData.reduce(
      (sum, v) => sum + (v.commentCount ?? 0),
      0
    );
  }
}
