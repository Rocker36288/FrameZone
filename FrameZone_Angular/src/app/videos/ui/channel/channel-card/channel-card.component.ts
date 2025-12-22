import { NgSwitch, NgSwitchCase } from '@angular/common';
import { Component, Input } from '@angular/core';
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


  constructor(private router: Router) { }

  goToChannelHome(): void {
    const url = `/videos/channel/${this.channel.id}/home`;
    window.open(url, '_blank');
  }

  FollowerBtn() {

  }
}
