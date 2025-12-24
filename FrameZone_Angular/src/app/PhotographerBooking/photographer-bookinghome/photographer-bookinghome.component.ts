import { PhotographerBookingheroComponent } from './../photographer-bookinghero/photographer-bookinghero.component';
import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PhotographerbookingSearchComponent } from '../photographerbooking-search/photographerbooking-search.component';
import { PhotographerbookingCardComponent } from '../photographerbooking-card/photographerbooking-card.component';
import { PhotographerbookingHeaderComponent } from '../photographerbooking-header/photographerbooking-header.component';

@Component({
  selector: 'app-photographer-bookinghome',
  imports: [
    CommonModule,
    FormsModule,
    PhotographerbookingSearchComponent,
    PhotographerbookingCardComponent,
    PhotographerbookingHeaderComponent,
    PhotographerBookingheroComponent,
  ],
  templateUrl: './photographer-bookinghome.component.html',
  styleUrl: './photographer-bookinghome.component.css',
})
export class PhotographerBookinghomeComponent {
  // Search 狀態
  keyword = '';
  filterLocation = '';
  filterService = '';
  filterPrice = '';

  // 原始資料
  photographers = [
    {
      name: 'Alex Studio',
      rating: 4.9,
      location: '台北',
      services: ['人像', '商業'],
      price: 3500,
      availability: '本週末',
      tags: ['自然光', '清新'],
      img: '/images/Photographer/photographercard001',
    },
    {
      name: 'Mia Photo',
      rating: 4.8,
      location: '新北',
      services: ['婚紗'],
      price: 6000,
      availability: '下週',
      tags: ['唯美', '棚拍'],
      img: '/images/Photographer/photographercard001',
    },
    {
      name: 'Leo Works',
      rating: 4.7,
      location: '台中',
      services: ['活動'],
      price: 2800,
      availability: '今日',
      tags: ['紀實', '快速交件'],
      img: '/images/Photographer/photographercard001',
    },
    {
      name: 'Nina Studio',
      rating: 5.0,
      location: '高雄',
      services: ['形象照'],
      price: 4200,
      availability: '本月',
      tags: ['專業修圖', '商務'],
      img: '/images/Photographer/photographercard001',
    },
  ];

  // 濾後結果filter 邏輯
  get filteredPhotographers() {
    return this.photographers.filter((p) => {
      // 關鍵字搜尋（名稱 / 服務 / 標籤）
      const keywordMatch = this.keyword
        ? p.name.includes(this.keyword) ||
          p.services.some((s) => s.includes(this.keyword)) ||
          p.tags.some((t) => t.includes(this.keyword))
        : true;

      // 地區
      const locationMatch = this.filterLocation
        ? p.location === this.filterLocation
        : true;

      // 服務
      const serviceMatch = this.filterService
        ? p.services.includes(this.filterService)
        : true;

      // 價格
      let priceMatch = true;
      if (this.filterPrice === '2k-5k') {
        priceMatch = p.price >= 2000 && p.price <= 5000;
      }
      if (this.filterPrice === '5k+') {
        priceMatch = p.price > 5000;
      }

      return keywordMatch && locationMatch && serviceMatch && priceMatch;
    });
  }
}
