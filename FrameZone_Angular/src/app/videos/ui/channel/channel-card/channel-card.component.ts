import { NgSwitch, NgSwitchCase } from '@angular/common';
import { Component, Input } from '@angular/core';
import { SubscribeButtonComponent } from "../../actions/subscribe-button/subscribe-button.component";
import { ChannelCard } from '../../../models/video-model';

@Component({
  selector: 'app-channel-card',
  imports: [SubscribeButtonComponent, NgSwitch, NgSwitchCase],
  templateUrl: './channel-card.component.html',
  styleUrl: './channel-card.component.css'
})
export class ChannelCardComponent {
  @Input() channel!: ChannelCard
  @Input() variant: 'small' | 'middle' | 'large' = 'small';
  // channel: Partial<ChannelData> = {
  //   Name: '頻道名稱示例',
  //   UserAvatarUrl: 'https://placehold.co/80x80',
  //   Follows: 12345,
  // };

  FollowerBtn() {

  }
}
