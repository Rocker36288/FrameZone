import { Component, Input } from '@angular/core';
import { VideosListComponent } from "../../../ui/video/videos-list/videos-list.component";
import { VideoListCard } from '../../../models/video-model';
import { MockChannelService } from './../../../service/mock-channel.service';
@Component({
  selector: 'app-channel-playlists',
  imports: [VideosListComponent],
  templateUrl: './channel-playlists.component.html',
  styleUrl: './channel-playlists.component.css'
})
export class ChannelPlaylistsComponent {
  @Input() VideoPlaylists: VideoListCard[] = [{
    Id: 0,
    Title: '',
    Description: '',
    VideoCount: 0,
    thumbnail: ''
  }]

  constructor(private MockChannelService: MockChannelService) { }
  ngOnInit(): void {
    this.MockChannelService.getChannelPlaylists().subscribe(data => { this.VideoPlaylists = data; })
  }
}
