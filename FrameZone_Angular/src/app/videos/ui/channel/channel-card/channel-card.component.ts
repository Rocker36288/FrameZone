import { NgSwitch, NgSwitchCase } from '@angular/common';
import { Component, Input } from '@angular/core';
import { ChannelData } from '../../../models/channel-data.interface';
import { SubscribeButtonComponent } from "../../actions/subscribe-button/subscribe-button.component";

@Component({
  selector: 'app-channel-card',
  imports: [SubscribeButtonComponent, NgSwitch, NgSwitchCase],
  templateUrl: './channel-card.component.html',
  styleUrl: './channel-card.component.css'
})
export class ChannelCardComponent {
  @Input() channel!: ChannelData
  @Input() variant: 'small' | 'middle' | 'large' = 'small';
  // channel: Partial<ChannelData> = {
  //   Name: '頻道名稱示例',
  //   UserAvatarUrl: 'https://placehold.co/80x80',
  //   Follows: 12345,
  // };

  FollowerBtn() {

  }
}
