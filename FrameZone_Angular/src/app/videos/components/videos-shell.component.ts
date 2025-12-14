import { MockChannelService } from './../service/mock-channel.service';
import { Component } from '@angular/core';
import { HeaderComponent } from "../../shared/components/header/header.component";
import { VideosSidebarComponent } from "../ui/videos-sidebar/videos-sidebar.component";
import { VideoHomeComponent } from "./home/video-home/video-home.component";
import { VideolistCardComponent } from "../ui/video/videolist-card/videolist-card.component";
import { ChannelComponent } from "./channel/channel-home/channel-home.component";
import { ChannelLayoutComponent } from "./channel/channel-layout/channel-layout.component";
import { ChannelCard, VideoCardData } from '../models/video-model';

@Component({
  selector: 'app-videos-shell',
  imports: [HeaderComponent, VideosSidebarComponent, VideoHomeComponent, VideolistCardComponent, ChannelComponent, ChannelLayoutComponent],
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
