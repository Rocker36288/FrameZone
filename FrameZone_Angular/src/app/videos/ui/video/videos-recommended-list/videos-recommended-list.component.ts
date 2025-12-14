import { Component, ElementRef, Input, ViewChild } from '@angular/core';
import { VideoCardComponent } from "../video-card/video-card.component";
import { NgSwitch, NgSwitchCase, NgForOf } from "@angular/common";
import { VideoCardData, VideoListCard } from '../../../models/video-model';
import { VideolistCardComponent } from "../videolist-card/videolist-card.component";
@Component({
  selector: 'app-videos-recommended-list',
  imports: [VideoCardComponent, NgSwitch, NgSwitchCase, NgForOf, VideolistCardComponent],
  templateUrl: './videos-recommended-list.component.html',
  styleUrl: './videos-recommended-list.component.css',
})
export class VideosRecommendedListComponent {
  @Input() videos: VideoCardData[] = []
  @Input() VideoPlaylists: VideoListCard[] = []
  @Input() variant: 'list' | 'gridshow-scroll' | 'Playlist-gridshow-scroll' | 'Playlist-gridshow' | 'gridshow' = 'list';
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
