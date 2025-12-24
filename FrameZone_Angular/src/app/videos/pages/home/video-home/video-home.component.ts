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

  constructor(private videoService: VideoService) {

  }

  ngOnInit() {
    this.videoService.getVideoRecommend().subscribe(data => { this.recommendVideos = data; })
  }

}
