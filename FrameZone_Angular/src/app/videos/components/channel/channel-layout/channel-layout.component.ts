import { ChannelService } from './../../../service/channel.service';
import { Component, Input } from '@angular/core';
import { ChannelHome, VideoCardData, VideoListCard } from '../../../models/video-model';
import { SubscribeButtonComponent } from "../../../ui/actions/subscribe-button/subscribe-button.component";
import { RouterOutlet, RouterModule } from "@angular/router";
import { SidebarComponent } from "../../../../shared/components/sidebar/sidebar.component";
import { VideosSidebarComponent } from "../../../ui/videos-sidebar/videos-sidebar.component";


@Component({
  selector: 'app-channel-layout',
  imports: [SubscribeButtonComponent, RouterOutlet, RouterModule, SidebarComponent, VideosSidebarComponent],
  templateUrl: './channel-layout.component.html',
  styleUrl: './channel-layout.component.css'
})
export class ChannelLayoutComponent {

  //呼叫假資料
  channelHome!: any;

  constructor(private ChannelService: ChannelService) { }

  ngOnInit(): void {
    this.ChannelService.getChannel().subscribe(data => { this.channelHome = data; })
  }
}
