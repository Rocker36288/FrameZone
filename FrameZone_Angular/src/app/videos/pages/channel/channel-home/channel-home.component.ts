import { MockChannelService } from './../../../service/mock-channel.service';
import { Component, Input } from '@angular/core';
import { VideoCardData, VideoListCard } from '../../../models/video-model';
import { VideosListComponent } from "../../../ui/video/videos-list/videos-list.component";
@Component({
  selector: 'app-channel-home',
  imports: [VideosListComponent],
  templateUrl: './channel-home.component.html',
  styleUrl: './channel-home.component.css'
})
export class ChannelComponent {

  @Input() LastUploadVideos: VideoCardData[] = [{
    id: 0,
    title: '',
    thumbnail: '',
    duration: 0,
    views: 0,
    uploadDate: new Date(),
    description: '',
    channelName: '',
    Avatar: ''
  }]

  @Input() PupularVideos: VideoCardData[] = [{
    id: 0,
    title: '',
    thumbnail: '',
    duration: 0,
    views: 0,
    uploadDate: new Date(),
    description: '',
    channelName: '',
    Avatar: ''
  }]

  @Input() VideoPlaylists: VideoListCard[] = [{
    Id: 0,
    Title: '',
    Description: '',
    VideoCount: 0,
    thumbnail: ''
  }]

  constructor(private MockChannelService: MockChannelService) { }



  ngOnInit(): void {
    this.MockChannelService.getChannelVideos().subscribe(data => { this.LastUploadVideos = data; })
    this.MockChannelService.getChannelVideos().subscribe(data => { this.PupularVideos = data; })
    this.MockChannelService.getChannelPlaylists().subscribe(data => { this.VideoPlaylists = data; })
  }


}
