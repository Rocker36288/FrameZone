import { Component, EventEmitter, Input, Output } from '@angular/core';
import { RouterLink } from "@angular/router";
import { VideoTimeagoPipe } from "../../../pipes/video-timeago.pipe";
import { VideoDurationPipe } from "../../../pipes/video-duration.pipe";
import { NgSwitch, NgSwitchCase } from '@angular/common';
import { VideoCardData } from '../../../models/video-model';
import { Router } from '@angular/router';

@Component({
  selector: 'app-video-card',
  imports: [VideoTimeagoPipe, VideoDurationPipe, NgSwitch, NgSwitchCase],
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
    = 'small';
  @Input() showChannel: boolean = true;
  @Input() showDescription: boolean = true;

  @Output() videoClick = new EventEmitter<string>();

  constructor(private router: Router) { }

  onVideoClick() {
    if (!this.video || !this.video.videoUri) return;

    this.router.navigate(['/watch', this.video.videoUri]);
  }



}
