import { VideoWatchHistoryDto } from '../../../models/video-model';
import { VideoService } from './../../../service/video.service';
import { Component } from '@angular/core';
import { NgIf, NgForOf } from "@angular/common";
import { VideosListComponent } from "../../../ui/video/videos-list/videos-list.component";
import { VideoCardComponent } from "../../../ui/video/video-card/video-card.component";

@Component({
  selector: 'app-channel-history',
  imports: [NgIf, NgForOf, VideosListComponent, VideoCardComponent],
  templateUrl: './channel-history.component.html',
  styleUrl: './channel-history.component.css'
})
export class ChannelHistoryComponent {

  watchHistory: VideoWatchHistoryDto[] = [];
  loading = true;

  constructor(private videoService: VideoService) { }

  ngOnInit(): void {
    this.videoService.GetWatchHistory().subscribe(res => {
      this.watchHistory = res;
      this.loading = false;
    });
  }

  formatTime(seconds: number): string {
    const mins = Math.floor(seconds / 60);
    const secs = Math.floor(seconds % 60);
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  }

  gotoLastVideo() {

  }
}
