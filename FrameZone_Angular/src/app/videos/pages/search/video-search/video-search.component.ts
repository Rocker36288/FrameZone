import { MockChannelService } from './../../../service/mock-channel.service';
import { Component, Input, OnInit } from '@angular/core';
import { VideosListComponent } from "../../../ui/video/videos-list/videos-list.component";
import { SearchboxComponent } from "../../../ui/searchbox/searchbox.component";
import { VideoCardData } from '../../../models/video-model';
import { VideoService } from '../../../service/video.service';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-video-search',
  imports: [VideosListComponent, SearchboxComponent],
  templateUrl: './video-search.component.html',
  styleUrl: './video-search.component.css'
})
export class VideoSearchComponent {
  @Input() videos: VideoCardData[] | undefined
  loading = false;

  constructor(
    private route: ActivatedRoute,
    private videoService: VideoService
  ) { }

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.fetchVideos(params);
    });
  }

  fetchVideos(params: any) {
    this.loading = true;

    this.videoService.searchVideos({
      keyword: params['keyword'],
      sortBy: params['sortBy'],
      sortOrder: params['sortOrder'],
      take: 20
    }).subscribe({
      next: res => {
        this.videos = res;
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }
}
