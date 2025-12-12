import { Component, Input } from '@angular/core';
import { ChannelData } from '../../../models/channel-data.interface';
import { VideoCardData } from '../../../models/video-data.interface';
import { NgIf, NgForOf } from "@angular/common";
import { VideoDurationPipe } from "../../../pipes/video-duration.pipe";

@Component({
  selector: 'app-video-creatorspotlight',
  imports: [NgIf, NgForOf, VideoDurationPipe],
  templateUrl: './video-creatorspotlight.component.html',
  styleUrl: './video-creatorspotlight.component.css'
})
export class VideoCreatorspotlightComponent {
  @Input() channel: ChannelData | undefined
  @Input() Videos: VideoCardData[] | undefined
  isLoaded = false;
  currentVideoIndex = 0;

  ngOnInit() {
    // 觸發進場動畫
    setTimeout(() => {
      this.isLoaded = true;
    }, 100);

    // 自動輪播特色影片（可選）
    this.startAutoRotate();
  }

  startAutoRotate() {
    setInterval(() => {
      if (this.Videos!.length > 1) {
        this.currentVideoIndex = (this.currentVideoIndex + 1) % this.Videos!.length;
      }
    }, 5000); // 每5秒切換
  }

  selectVideo(index: number) {
    this.currentVideoIndex = index;
  }

  toggleSubscribe() {
    // this.channel.isSubscribed = !this.channel.isSubscribed;
  }

}
