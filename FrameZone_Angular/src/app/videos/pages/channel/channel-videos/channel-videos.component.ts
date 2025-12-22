import { Component, Input } from '@angular/core';
import { VideosListComponent } from "../../../ui/video/videos-list/videos-list.component";
import { VideoCardData } from '../../../models/video-model';
import { MockChannelService } from './../../../service/mock-channel.service';
import { ActivatedRoute } from '@angular/router';
import { VideoService } from '../../../service/video.service';

@Component({
  selector: 'app-channel-videos',
  imports: [VideosListComponent],
  templateUrl: './channel-videos.component.html',
  styleUrl: './channel-videos.component.css'
})
export class ChannelVideosComponent {
  @Input() UploadVideos: VideoCardData[] | undefined;
  channelId!: number;

  constructor(private MockChannelService: MockChannelService, private route: ActivatedRoute, private videoService: VideoService) { }

  ngOnInit(): void {
    this.route.parent?.paramMap.subscribe(params => {
      const idParam = params.get('id');
      if (!idParam) return;

      const channelId = Number(idParam);
      if (Number.isNaN(channelId)) {
        console.error('channel id 不是有效的數字');
        return;
      }

      this.channelId = channelId;
      console.log(this.channelId);
    });

    //讀取
    this.videoService.getChannelVideos(this.channelId!).subscribe({
      next: (videos: VideoCardData[]) => {
        console.log(videos)
        this.UploadVideos = videos;
      },
      error: (err: any) => console.error(err)
    });
  }
}
