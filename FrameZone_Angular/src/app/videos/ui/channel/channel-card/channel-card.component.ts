import { AuthService } from './../../../../core/services/auth.service';
import { VideoService } from './../../../service/video.service';
import { NgSwitch, NgSwitchCase } from '@angular/common';
import { Component, Input, OnInit } from '@angular/core';
import { SubscribeButtonComponent } from "../../actions/subscribe-button/subscribe-button.component";
import { ChannelCard } from '../../../models/video-model';
import { Router } from '@angular/router';

@Component({
  selector: 'app-channel-card',
  imports: [SubscribeButtonComponent, NgSwitch, NgSwitchCase],
  templateUrl: './channel-card.component.html',
  styleUrl: './channel-card.component.css'
})
export class ChannelCardComponent {

  @Input() channel!: ChannelCard
  @Input() variant: 'small' | 'middle' | 'large' = 'small';

  userIsFollowing: boolean = false

  constructor(private router: Router, private videoService: VideoService, private authService: AuthService) { }

  OnInit() {
    // Channel
    this.videoService.checkChannelFollow(this.channel.id).subscribe(isFollowing => {
      console.log('是否追隨:', isFollowing);
    });
  }

  onFollowToggled(newStatus: boolean) {
    if (this.authService.getCurrentUser()) {
      this.videoService.toggleChannelFollow(this.channel.id).subscribe(isFollowing => {
        console.log('最新追隨狀態:', isFollowing);
      });
    } else {
      console.log('未登入');
    }
  }

  goToChannelHome(): void {
    const url = `/videos/channel/${this.channel.id}/home`;
    window.open(url, '_blank');
  }

  onthumbnailError(event: Event): void {
    const img = event.target as HTMLImageElement;
    img.src = 'favicon2.png';
  }
}
