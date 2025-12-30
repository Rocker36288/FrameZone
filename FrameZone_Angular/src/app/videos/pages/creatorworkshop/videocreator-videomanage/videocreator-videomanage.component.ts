import { VideoDetailData } from './../../../models/videocreator-model';
import { Component, Input } from '@angular/core';
import { VideosListComponent } from "../../../ui/video/videos-list/videos-list.component";
import { VideoCardData, VideoListCard } from '../../../models/video-model';
import { MockChannelService } from './../../../service/mock-channel.service';
import { PrivacyStatus, ProcessStatus } from '../../../models/video.enum';
import { VideoCreatorService } from '../../../service/video-creator.service';
@Component({
  selector: 'app-videocreator-videomanage',
  imports: [VideosListComponent],
  templateUrl: './videocreator-videomanage.component.html',
  styleUrl: './videocreator-videomanage.component.css'
})
export class VideocreatorVideomanageComponent {

  @Input() VideoDetailsData: VideoDetailData[] | undefined


  constructor(private MockChannelService: MockChannelService, private videoCreatorService: VideoCreatorService) { }



  ngOnInit(): void {
    this.videoCreatorService.getRecentUploadVideos(5)
      .subscribe(videos => {
        this.VideoDetailsData = videos;
      });
  }

}
