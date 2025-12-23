import { MockChannelService } from './../../../service/mock-channel.service';
import { Component, Input } from '@angular/core';
import { VideoCardData, VideoListCard } from '../../../models/video-model';
import { VideosListComponent } from "../../../ui/video/videos-list/videos-list.component";
import { ActivatedRoute, Route } from '@angular/router';
import { VideoService } from '../../../service/video.service';
@Component({
  selector: 'app-channel-home',
  imports: [VideosListComponent],
  templateUrl: './channel-home.component.html',
  styleUrl: './channel-home.component.css'
})
export class ChannelComponent {

  @Input() LastUploadVideos: VideoCardData[] | undefined

  @Input() PupularVideos: VideoCardData[] | undefined

  @Input() VideoPlaylists: VideoListCard[] = [{
    Id: 0,
    Title: '',
    Description: '',
    VideoCount: 0,
    thumbnail: ''
  }]

  channelId: number | null = null;

  constructor(private MockChannelService: MockChannelService, private route: ActivatedRoute, private videoService: VideoService) { }



  ngOnInit(): void {

    /* 1️⃣ 取得路由中ID */
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
        this.LastUploadVideos = videos;
      },
      error: (err: any) => console.error(err)
    });
  }
}
