import { MockChannelService } from './../service/mock-channel.service';
import { Component } from '@angular/core';
import { VideosSidebarComponent } from "../ui/videos-sidebar/videos-sidebar.component";
import { VideoHomeComponent } from "./home/video-home/video-home.component";
import { ChannelCard, VideoCardData } from '../models/video-model';
import { VideoSearchComponent } from "./search/video-search/video-search.component";

@Component({
  selector: 'app-videos-shell',
  imports: [VideosSidebarComponent, VideoHomeComponent, VideoSearchComponent],
  templateUrl: './videos-shell.component.html',
  styleUrl: './videos-shell.component.css'
})
export class VideosShellComponent {
  videos: VideoCardData[] | undefined;
  channel: ChannelCard | undefined;

  constructor(private MockChannelService: MockChannelService) {

  }
  ngOnInit(): void {
    this.MockChannelService.getChannelVideos().subscribe(data => { this.videos = data; })
    this.MockChannelService.getChannelCard().subscribe(data => { this.channel = data; })
  }

}
