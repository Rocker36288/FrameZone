import { Component, Input } from '@angular/core';
import { VideoCardData, VideoListCard } from '../../../models/video-model';

@Component({
  selector: 'app-videolist-card',
  imports: [],
  templateUrl: './videolist-card.component.html',
  styleUrl: './videolist-card.component.css'
})
export class VideolistCardComponent {

  @Input() videosPlaylist: VideoListCard = {
    Id: 0,
    Title: '',
    Description: '',
    VideoCount: 0,
    thumbnail: ''
  }



}
