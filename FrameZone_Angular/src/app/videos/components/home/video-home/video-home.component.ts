import { Component, ElementRef, Input, input, ViewChild } from '@angular/core';
import { VideoCreatorspotlightComponent } from "../video-creatorspotlight/video-creatorspotlight.component";
import { VideoCardData } from '../../../models/video-data.interface';
import { ChannelData } from '../../../models/channel-data.interface';
import { VideoCardComponent } from "../../../shared/video/video-card/video-card.component";
import { NgForOf } from '@angular/common';

@Component({
  selector: 'app-video-home',
  imports: [VideoCreatorspotlightComponent, VideoCardComponent, NgForOf],
  templateUrl: './video-home.component.html',
  styleUrl: './video-home.component.css'
})
export class VideoHomeComponent {
  @Input() channel: ChannelData | undefined
  @Input() videos: VideoCardData[] | undefined
  @ViewChild('scrollContainer', { static: false })
  scrollContainer!: ElementRef;

  items = Array(10).fill(0); // 自己換成影片資料

  scrollAmount = 300; // 每次滑動 300px，可自行調整

  scrollLeft() {
    this.scrollContainer.nativeElement.scrollBy({
      left: -this.scrollAmount,
      behavior: 'smooth'
    });
  }

  scrollRight() {
    this.scrollContainer.nativeElement.scrollBy({
      left: this.scrollAmount,
      behavior: 'smooth'
    });
  }
}
