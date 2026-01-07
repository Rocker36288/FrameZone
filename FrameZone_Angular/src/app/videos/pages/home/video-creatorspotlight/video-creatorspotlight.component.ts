import { Component, Input } from '@angular/core';
import { NgIf, NgForOf } from "@angular/common";
import { VideoDurationPipe } from "../../../pipes/video-duration.pipe";
import { ChannelCard, VideoCardData } from '../../../models/video-model';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-video-creatorspotlight',
  imports: [DatePipe, NgIf, NgForOf, VideoDurationPipe],
  templateUrl: './video-creatorspotlight.component.html',
  styleUrl: './video-creatorspotlight.component.css'
})
export class VideoCreatorspotlightComponent {
  @Input() channel: ChannelCard | undefined
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
