import { VideoDetailData } from './../../../models/videocreator-model';
import { Component, Input } from '@angular/core';
import { VideosListComponent } from "../../../ui/video/videos-list/videos-list.component";
import { VideoCardData, VideoListCard } from '../../../models/video-model';
import { MockChannelService } from './../../../service/mock-channel.service';
import { PrivacyStatus, ProcessStatus } from '../../../models/video.enum';
@Component({
  selector: 'app-videocreator-videomanage',
  imports: [VideosListComponent],
  templateUrl: './videocreator-videomanage.component.html',
  styleUrl: './videocreator-videomanage.component.css'
})
export class VideocreatorVideomanageComponent {

  @Input() VideoDetailsData: VideoDetailData[] = [{
    id: 0,
    title: '',
    description: '',
    thumbnail: '',
    duration: 0,
    publishDate: new Date(),
    viewsCount: 0,
    likesCount: 0,
    commentCount: 0,
    videoUrl: '',
    processStatus: ProcessStatus.UPLOADING,
    privacyStatus: PrivacyStatus.PUBLIC
  }]


  constructor(private MockChannelService: MockChannelService) { }



  ngOnInit(): void {
    this.MockChannelService.getVideoDetailsData().subscribe(data => { this.VideoDetailsData = data; })
  }

}
