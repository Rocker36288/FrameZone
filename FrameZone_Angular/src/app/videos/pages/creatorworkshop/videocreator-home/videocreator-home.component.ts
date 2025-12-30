import { Component } from '@angular/core';
import { VideocreatorEditvideoComponent } from "../videocreator-editvideo/videocreator-editvideo.component";
import { VideosSidebarComponent } from "../../../ui/videos-sidebar/videos-sidebar.component";
import { AuthService } from '../../../../core/services/auth.service';
import { VideosListComponent } from "../../../ui/video/videos-list/videos-list.component";
import { MockChannelService } from './../../../service/mock-channel.service';
import { VideoCardData } from '../../../models/video-model';
import { VideoDetailData } from '../../../models/videocreator-model';
import { VideoCreatorService } from '../../../service/video-creator.service';

@Component({
  selector: 'app-videocreator-home',
  imports: [VideocreatorEditvideoComponent, VideosSidebarComponent, VideosListComponent],
  templateUrl: './videocreator-home.component.html',
  styleUrl: './videocreator-home.component.css'
})
export class VideocreatorHomeComponent {
  userId: string | null = null;
  VideoDetailsData: VideoDetailData[] = [
  ]

  constructor(private authService: AuthService, private MockChannelService: MockChannelService, private videoCreatorService: VideoCreatorService) { }

  ngOnInit(): void {
    this.videoCreatorService.getRecentUploadVideos(5)
      .subscribe(videos => {
        this.VideoDetailsData = videos;
      });
  }
}
