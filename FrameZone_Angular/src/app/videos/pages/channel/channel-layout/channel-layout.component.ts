import { AuthService } from './../../../../core/services/auth.service';
import { ChannelService } from '../../../service/channel.service';
import { Component, Input, EventEmitter } from '@angular/core';
import { ChannelCard, ChannelHome, VideoCardData, VideoListCard } from '../../../models/video-model';
import { SubscribeButtonComponent } from "../../../ui/actions/subscribe-button/subscribe-button.component";
import { RouterOutlet, RouterModule, ActivatedRoute } from "@angular/router";
import { SidebarComponent } from "../../../../shared/components/sidebar/sidebar.component";
import { VideosSidebarComponent } from "../../../ui/videos-sidebar/videos-sidebar.component";
import { VideoService } from '../../../service/video.service';
import { VideosNotloginyetModalComponent } from "../../../ui/videos-notloginyet-modal/videos-notloginyet-modal.component";


@Component({
  selector: 'app-channel-layout',
  imports: [SubscribeButtonComponent, RouterOutlet, RouterModule, VideosNotloginyetModalComponent],
  templateUrl: './channel-layout.component.html',
  styleUrl: './channel-layout.component.css'
})
export class ChannelLayoutComponent {

  channelId: number | null = null;

  channelHome: ChannelHome | any;
  userIsFollowing: boolean = false;
  loginRequired: any;

  //===============
  showLoginModal = false; // 控制 Modal 顯示

  userLoggedIn = false; // 假設是否登入


  constructor(private ChannelService: ChannelService, private route: ActivatedRoute, private videoService: VideoService, private authService: AuthService) { }

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

    if (this.authService.getCurrentUser()) {
      this.videoService.checkChannelFollow(this.channelId).subscribe(isFollowing => {
        console.log('是否追隨:', isFollowing);
        this.userIsFollowing = isFollowing
      });
    } else {
      console.log('未登入');

    }
  }
  onthumbnailError(event: Event): void {
    const img = event.target as HTMLImageElement;
    img.src = 'favicon2.png';
  }

  onFollowToggled(newStatus: boolean) {
    if (this.authService.getCurrentUser()) {
      this.videoService.toggleChannelFollow(this.channelId!).subscribe(isFollowing => {
        console.log('最新追隨狀態:', isFollowing);
        this.userIsFollowing = isFollowing
      });
    } else {
      this.loginRequired.emit();
    }
  }
}
