import { VideoWatchHistoryDto } from '../../../models/video-model';
import { VideoService } from './../../../service/video.service';
import { Component } from '@angular/core';
import { NgIf, NgForOf } from "@angular/common";
import { VideosListComponent } from "../../../ui/video/videos-list/videos-list.component";
import { VideoCardComponent } from "../../../ui/video/video-card/video-card.component";
import { Router } from '@angular/router';

@Component({
  selector: 'app-channel-history',
  imports: [NgIf, NgForOf, VideosListComponent, VideoCardComponent],
  templateUrl: './channel-history.component.html',
  styleUrl: './channel-history.component.css'
})
export class ChannelHistoryComponent {
  watchHistory: VideoWatchHistoryDto[] = [];
  loading = true;

  constructor(private videoService: VideoService, private router: Router) { }

  ngOnInit(): void {
    this.videoService.GetWatchHistory().subscribe(res => {
      // 假設後端已排序（最新在前），如果沒有可自行 sort
      this.watchHistory = res;
      this.loading = false;
    });
  }

  /** 最近觀看（顯示成「X 小時前」） */
  get lastWatchedText(): string {
    if (!this.watchHistory.length) return '—';

    const last = new Date(this.watchHistory[0].lastWatchedAt);
    const diffMs = Date.now() - last.getTime();
    const diffHours = Math.floor(diffMs / (1000 * 60 * 60));

    if (diffHours < 1) return '剛剛';
    if (diffHours < 24) return `${diffHours} 小時前`;

    const diffDays = Math.floor(diffHours / 24);
    return `${diffDays} 天前`;
  }

  /** 已觀看影片數 */
  get watchedCount(): number {
    return this.watchHistory.length;
  }

  /** 總觀看時數（小時，取一位小數） */
  get totalWatchHours(): string {
    const totalSeconds = this.watchHistory
      .reduce((sum, x) => sum + x.lastPosition, 0);

    return (totalSeconds / 3600).toFixed(1);
  }

  /** 前往最後觀看影片 */
  gotoLastVideo() {
    if (!this.watchHistory.length) return;

    const last = this.watchHistory[0];
    // 範例：/videos/:id?start=秒數
    this.router.navigate(['/videos/watch', last.video.videoUri], {
      queryParams: { start: last.lastPosition }
    });
  }
}
