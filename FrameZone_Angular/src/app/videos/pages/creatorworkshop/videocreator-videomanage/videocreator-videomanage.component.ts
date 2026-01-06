import { Component, Input, OnInit } from '@angular/core';
import { VideosListComponent } from "../../../ui/video/videos-list/videos-list.component";
import { VideoDetailData } from './../../../models/videocreator-model';
import { VideoCreatorService } from '../../../service/video-creator.service';
import { MockChannelService } from './../../../service/mock-channel.service';
import { NgForOf, NgIf } from "@angular/common";
import { FormsModule } from "@angular/forms";

@Component({
  selector: 'app-videocreator-videomanage',
  templateUrl: './videocreator-videomanage.component.html',
  styleUrls: ['./videocreator-videomanage.component.css'],
  imports: [VideosListComponent, NgForOf, NgIf, FormsModule]
})
export class VideocreatorVideomanageComponent implements OnInit {
  @Input() VideoDetailsData: VideoDetailData[] = [];
  currentPage: number = 1;
  totalPages: number = 1;
  itemsPerPage: number = 5;
  totalItems: number = 0; // 新增：總影片數

  // 篩選與排序
  selectedStatus: string = '';
  selectedSort: string = 'newest';

  constructor(
    private MockChannelService: MockChannelService,
    private videoCreatorService: VideoCreatorService
  ) { }

  ngOnInit(): void {
    this.loadVideos(1);
  }

  get pages(): number[] {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }

  // 計算起始索引
  getStartIndex(): number {
    if (!this.VideoDetailsData || this.VideoDetailsData.length === 0) return 0;
    return (this.currentPage - 1) * this.itemsPerPage + 1;
  }

  // 計算結束索引
  getEndIndex(): number {
    if (!this.VideoDetailsData || this.VideoDetailsData.length === 0) return 0;
    return (this.currentPage - 1) * this.itemsPerPage + this.VideoDetailsData.length;
  }

  loadVideos(page: number): void {
    // 防止超過範圍
    if (page < 1 || (this.totalPages && page > this.totalPages)) return;

    this.videoCreatorService.getRecentUploadVideos(page)
      .subscribe(res => {
        this.VideoDetailsData = res.videos;
        this.currentPage = res.currentPage;
        this.totalPages = res.totalPages;
        // 計算總影片數
        this.totalItems = this.totalPages * this.itemsPerPage;
      });
  }

  onFilterChange(): void {
    this.loadVideos(1);
  }

  onPageSizeChange(event: any): void {
    this.itemsPerPage = +event.target.value;
    this.loadVideos(1);
  }
}
