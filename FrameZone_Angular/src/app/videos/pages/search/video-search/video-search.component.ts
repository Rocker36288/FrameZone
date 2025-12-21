import { MockChannelService } from './../../../service/mock-channel.service';
import { Component, Input, OnInit } from '@angular/core';
import { VideosListComponent } from "../../../ui/video/videos-list/videos-list.component";
import { SearchboxComponent } from "../../../ui/searchbox/searchbox.component";
import { VideoCardData } from '../../../models/video-model';

@Component({
  selector: 'app-video-search',
  imports: [VideosListComponent, SearchboxComponent],
  templateUrl: './video-search.component.html',
  styleUrl: './video-search.component.css'
})
export class VideoSearchComponent {
  @Input() videos: VideoCardData[] | undefined

}
