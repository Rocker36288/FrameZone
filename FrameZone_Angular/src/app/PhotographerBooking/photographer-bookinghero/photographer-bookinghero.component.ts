import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { PhotographerBookingService } from '../services/photographer-booking.service';
interface CarouselImage {
  url: string;
  author: string;
  style: string;
}
@Component({
  selector: 'app-photographer-bookinghero',
  imports: [CommonModule, FormsModule],
  templateUrl: './photographer-bookinghero.component.html',
  styleUrl: './photographer-bookinghero.component.css',
})
export class PhotographerBookingheroComponent implements OnInit {
  dbImages: CarouselImage[] = [
    {
      url: '/images/Photographer/Carousel01.png',
      author: '林曉美',
      style: '空間攝影',
    },
    {
      url: '/images/Photographer/Carousel02.png',
      author: '張大衛',
      style: '活動紀錄',
    },
    {
      url: '/images/Photographer/Carousel03.png',
      author: '陳小雅',
      style: '商業產品',
    },
  ];

  currentIndex = 0;

  // 搜尋欄
  city = '';
  date = '';
  type = '';

  // 快速標籤
  tags = ['韓系婚紗', '寵物攝影', '日系寫真'];

  constructor(private router: Router, private bookingService: PhotographerBookingService) { }
  ngOnInit() {
    setInterval(() => this.nextSlide(), 5000);

    // 從後端載入熱門標籤顯示3
    this.bookingService.getPopularTags(3).subscribe({
      next: (tags) => {
        this.tags = tags;
      },
      error: (err) => {
        console.error('Error loading popular tags', err);
        // 保留預設標籤作為 fallback
      }
    });
  }

  nextSlide() {
    this.currentIndex = (this.currentIndex + 1) % this.dbImages.length;
  }

  onSearch() {
    const queryParams: any = {};

    if (this.city) queryParams.city = this.city;
    if (this.date) queryParams.date = this.date;
    if (this.type) queryParams.type = this.type;

    // 沒條件也可以 → {}
    this.router.navigate(['/photographerbooking-page-search'], { queryParams });
  }

  quickSearch(tag: string) {
    this.router.navigate(['/photographerbooking-page-search'], {
      queryParams: { tag },
    });
  }
}
