import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core';
import { RouterLink } from "@angular/router";
import { VideoTimeagoPipe } from "../../../pipes/video-timeago.pipe";
import { VideoDurationPipe } from "../../../pipes/video-duration.pipe";
import { NgSwitch, NgSwitchCase } from '@angular/common';
import { VideoCardData } from '../../../models/video-model';
import { Router } from '@angular/router';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-video-card',
  imports: [DatePipe, VideoTimeagoPipe, VideoDurationPipe, NgSwitch, NgSwitchCase],
  templateUrl: './video-card.component.html',
  styleUrl: './video-card.component.css'
})
export class VideoCardComponent {

  @Input() video!: VideoCardData;
  @Input() variant:
    'small'
    | 'spotlight'
    | 'large'
    | 'creator-long'
    | 'search'
    | 'history'
    = 'small';
  @Input() showChannel: boolean = true;
  @Input() showDescription: boolean = true;
  @Input() lastPosition: number = 0;


  @Output() videoClick = new EventEmitter<string>();

  constructor(private router: Router) { }

  ngOnInit(): void {
    // 可以移除 thumbnail 相關邏輯
  }

  get thumbnailUrl(): string {
    return this.video?.thumbnail ||
      `https://localhost:7213/api/Videos/video-thumbnail/${this.video?.videoUri}`;
  }

  onVideoClick() {
    if (!this.video || !this.video.videoUri) return;

    this.router.navigate(['/watch', this.video.videoUri]);
  }

  onAvatarError(event: Event): void {
    const img = event.target as HTMLImageElement;
    img.src = 'favicon2.png';
  }

  onthumbnailError(event: Event): void {
    const img = event.target as HTMLImageElement;
    img.src = 'favicon2.png';
  }


}
