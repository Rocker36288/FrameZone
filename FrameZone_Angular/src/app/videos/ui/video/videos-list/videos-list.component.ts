import { VideoDetailData } from './../../../models/videocreator-model';
import { Component, ElementRef, Input, ViewChild } from '@angular/core';
import { VideoCardComponent } from "../video-card/video-card.component";
import { NgSwitch, NgSwitchCase, NgForOf } from "@angular/common";
import { VideoCardData, VideoListCard } from '../../../models/video-model';
import { VideolistCardComponent } from "../videolist-card/videolist-card.component";
import { VideocreatorVideocardComponent } from "../../creator/videocreator-videocard/videocreator-videocard.component";
@Component({
  selector: 'app-videos-list',
  imports: [VideoCardComponent, NgSwitch, NgSwitchCase, NgForOf, VideolistCardComponent, VideocreatorVideocardComponent],
  templateUrl: './videos-list.component.html',
  styleUrl: './videos-list.component.css',
})
export class VideosListComponent {
  @Input() videos: VideoCardData[] = []
  @Input() VideoPlaylists: VideoListCard[] = []
  @Input() VideoDetailData: VideoDetailData[] = []
  @Input() variant:
    | 'list'
    | 'creator-list'
    | 'search-list'

    | 'gridshow'
    | 'gridshow-scroll'

    | 'playlist-gridshow'
    | 'playlist-gridshow-scroll'

    = 'list';
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
