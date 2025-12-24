import { ChannelService } from '../../../service/channel.service';
import { Component, Input } from '@angular/core';
import { ChannelCard, ChannelHome, VideoCardData, VideoListCard } from '../../../models/video-model';
import { SubscribeButtonComponent } from "../../../ui/actions/subscribe-button/subscribe-button.component";
import { RouterOutlet, RouterModule, ActivatedRoute } from "@angular/router";
import { SidebarComponent } from "../../../../shared/components/sidebar/sidebar.component";
import { VideosSidebarComponent } from "../../../ui/videos-sidebar/videos-sidebar.component";
import { VideoService } from '../../../service/video.service';


@Component({
  selector: 'app-channel-layout',
  imports: [SubscribeButtonComponent, RouterOutlet, RouterModule, SidebarComponent, VideosSidebarComponent],
  templateUrl: './channel-layout.component.html',
  styleUrl: './channel-layout.component.css'
})
export class ChannelLayoutComponent {

  channelId: number | null = null;

  channelHome: ChannelHome | any;

  constructor(private ChannelService: ChannelService, private route: ActivatedRoute, private videoService: VideoService) { }

  ngOnInit(): void {

    /* 1️⃣ 取得路由中的影片 GUID */
    const idParam = this.route.snapshot.paramMap.get('id');
    if (!idParam) return;

    this.channelId = Number(idParam);

    if (Number.isNaN(this.channelId)) {
      console.error('channel id 不是有效的數字');
      return;
    }

    //讀取頻道
    this.videoService.getChannelHome(this.channelId).subscribe({
      next: (channel: ChannelHome) => {
        this.channelHome = channel;

        console.log(this.channelHome)
      },
      error: (err: any) => console.error(err)
    });

  }
}
