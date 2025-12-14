import { Component, ElementRef, Input, input, ViewChild } from '@angular/core';
import { VideoCreatorspotlightComponent } from "../video-creatorspotlight/video-creatorspotlight.component";
import { ChannelCard, VideoCardData } from '../../../models/video-model';
import { VideoCardComponent } from '../../../ui/video/video-card/video-card.component';
import { VideosRecommendedListComponent } from "../../../ui/video/videos-recommended-list/videos-recommended-list.component";

@Component({
  selector: 'app-video-home',
  imports: [VideoCreatorspotlightComponent, VideoCardComponent, VideosRecommendedListComponent],
  templateUrl: './video-home.component.html',
  styleUrl: './video-home.component.css'
})
export class VideoHomeComponent {
  @Input() channel: ChannelCard | undefined
  @Input() videos: VideoCardData[] | undefined


}
