import { Component, Input } from '@angular/core';
import { VideoCardData } from '../../../models/video-data.interface';
import { VideoCardComponent } from "../video-card/video-card.component";
import { NgSwitch, NgSwitchCase } from "@angular/common";

@Component({
  selector: 'app-videos-recommended-list',
  imports: [VideoCardComponent, NgSwitch, NgSwitchCase],
  templateUrl: './videos-recommended-list.component.html',
  styleUrl: './videos-recommended-list.component.css'
})
export class VideosRecommendedListComponent {
  @Input() videos: VideoCardData[] = []
  @Input() variant: 'list' | 'gridshow' = 'list';

}
