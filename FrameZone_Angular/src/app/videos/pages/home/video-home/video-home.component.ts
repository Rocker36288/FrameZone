import { MockChannelService } from './../../../service/mock-channel.service';
import { VideoService } from './../../../service/video.service';
import { Component, ElementRef, Input, input, ViewChild, OnInit } from '@angular/core';
import { VideoCreatorspotlightComponent } from "../video-creatorspotlight/video-creatorspotlight.component";
import { ChannelCard, VideoCardData } from '../../../models/video-model';
import { VideoCardComponent } from '../../../ui/video/video-card/video-card.component';
import { VideosListComponent } from "../../../ui/video/videos-list/videos-list.component";
import { SearchboxComponent } from "../../../ui/searchbox/searchbox.component";

@Component({
  selector: 'app-video-home',
  imports: [VideoCreatorspotlightComponent, VideoCardComponent, VideosListComponent, SearchboxComponent],
  templateUrl: './video-home.component.html',
  styleUrl: './video-home.component.css'
})
export class VideoHomeComponent {
  @Input() channel: ChannelCard | undefined
  @Input() videos: VideoCardData[] | undefined

  recommendVideos: VideoCardData[] | undefined

  popularVideos: VideoCardData[] | undefined

  spotlightVideos: VideoCardData[] | undefined
  spotlightChannel: ChannelCard | undefined

  constructor(private videoService: VideoService, private mockChannelService: MockChannelService) {

  }

  ngOnInit(): void {
    this.videoService.getVideoRecommend().subscribe(apiVideos => {
      this.popularVideos = [
        ...apiVideos,
        ...this.mockChannelService.Videos2
      ];
    });
    this.videoService.getVideoRecommend().subscribe(apiVideos => {
      this.recommendVideos = [
        ...apiVideos,
        ...this.mockChannelService.videos,
        ...this.mockChannelService.Videos3
      ];
    });

    this.spotlightVideos = this.mockChannelService.videos
    this.spotlightChannel = this.mockChannelService.channel
  }

}
