import { Component, ElementRef, Input, input, ViewChild } from '@angular/core';
import { VideoCreatorspotlightComponent } from "../video-creatorspotlight/video-creatorspotlight.component";
import { ChannelCard, VideoCardData } from '../../../models/video-model';
import { VideoCardComponent } from '../../../ui/video/video-card/video-card.component';
import { VideosListComponent } from "../../../ui/video/videos-list/videos-list.component";

@Component({
  selector: 'app-video-home',
  imports: [VideoCreatorspotlightComponent, VideoCardComponent, VideosListComponent],
  templateUrl: './video-home.component.html',
  styleUrl: './video-home.component.css'
})
export class VideoHomeComponent {
  @Input() channel: ChannelCard | undefined
  @Input() videos: VideoCardData[] | undefined


}
