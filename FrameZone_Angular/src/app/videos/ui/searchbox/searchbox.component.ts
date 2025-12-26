import { Component } from '@angular/core';
import { FormsModule } from "@angular/forms";
import { NgForOf } from "@angular/common";
import { VideoService } from '../../service/video.service';

@Component({
  selector: 'app-searchbox',
  imports: [FormsModule, NgForOf],
  templateUrl: './searchbox.component.html',
  styleUrl: './searchbox.component.css'
})
export class SearchboxComponent {
  keyword: string = '';
  selectedCategory: number | null = null;

  sortBy: string = 'date';

  constructor(private videoService: VideoService) { }

  onSearch() {
    // this.videoService.searchVideos({
    //   keyword: this.keyword,
    //   categoryId: this.selectedCategory ?? undefined,
    //   sortBy: this.sortBy,
    //   sortOrder: 'desc',
    //   take: 12
    // }).subscribe(res => {
    //   console.log(res);
    // });
  }

  setCategory(catId: number | null) {
    this.selectedCategory = catId;
    this.onSearch();
  }
}

