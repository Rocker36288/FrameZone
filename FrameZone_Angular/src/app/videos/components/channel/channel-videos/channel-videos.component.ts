import { Component, Input } from '@angular/core';
import { VideosRecommendedListComponent } from "../../../ui/video/videos-recommended-list/videos-recommended-list.component";
import { VideoCardData } from '../../../models/video-model';
import { MockChannelService } from './../../../service/mock-channel.service';

@Component({
  selector: 'app-channel-videos',
  imports: [VideosRecommendedListComponent],
  templateUrl: './channel-videos.component.html',
  styleUrl: './channel-videos.component.css'
})
export class ChannelVideosComponent {
  @Input() UploadVideos: VideoCardData[] | undefined;
  constructor(private MockChannelService: MockChannelService) { }
  ngOnInit(): void {
    this.MockChannelService.getChannelVideos().subscribe(data => { this.UploadVideos = data; })
  }
}
