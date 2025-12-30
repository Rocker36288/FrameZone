import { Component } from '@angular/core';
import { Photographer } from '../models/photographer-booking.models';
import { PhotographerbookingCardComponent } from '../photographerbooking-card/photographerbooking-card.component';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-photographer-booking-specialtytags',
  imports: [PhotographerbookingCardComponent, CommonModule],
  templateUrl: './photographer-booking-specialtytags.component.html',
  styleUrl: './photographer-booking-specialtytags.component.css',
})
export class PhotographerBookingSpecialtytagsComponent {
  photographers: Photographer[] = [
    {
      name: 'Mia Wedding',
      loc: '台北',
      type: '婚禮紀錄',
      tags: ['黑白風格', '微距拍攝'],
      price: 5000,
      rating: 4.8,
      reviews: 92,
      img: 'https://images.unsplash.com/photo-1512917774080-9991f1c4c750?w=400',
    },
    {
      name: 'Sunny Studio',
      loc: '台中',
      type: '商業廣告',
      tags: ['極簡主義'],
      price: 4200,
      rating: 5.0,
      reviews: 210,
      img: 'https://images.unsplash.com/photo-1554080353-a576cf803bda?w=400',
    },
    {
      name: 'Alex Photo',
      loc: '台北',
      type: '人像寫真',
      tags: ['日系清新', '底片感'],
      price: 3500,
      rating: 4.9,
      reviews: 156,
      img: 'https://images.unsplash.com/photo-1542038784456-1ea8e935640e?w=400',
    },
    {
      name: 'Mia Wedding',
      loc: '台北',
      type: '婚禮紀錄',
      tags: ['黑白風格', '微距拍攝'],
      price: 5000,
      rating: 4.8,
      reviews: 92,
      img: 'https://images.unsplash.com/photo-1512917774080-9991f1c4c750?w=400',
    },
    {
      name: 'Sunny Studio',
      loc: '台中',
      type: '商業廣告',
      tags: ['極簡主義'],
      price: 4200,
      rating: 5.0,
      reviews: 210,
      img: 'https://images.unsplash.com/photo-1554080353-a576cf803bda?w=400',
    },
    {
      name: 'Alex Photo',
      loc: '台北',
      type: '人像寫真',
      tags: ['日系清新', '底片感'],
      price: 3500,
      rating: 4.9,
      reviews: 156,
      img: 'https://images.unsplash.com/photo-1542038784456-1ea8e935640e?w=400',
    },
    {
      name: 'Mia Wedding',
      loc: '台北',
      type: '婚禮紀錄',
      tags: ['黑白風格', '微距拍攝'],
      price: 5000,
      rating: 4.8,
      reviews: 92,
      img: 'https://images.unsplash.com/photo-1512917774080-9991f1c4c750?w=400',
    },
    {
      name: 'Sunny Studio',
      loc: '台中',
      type: '商業廣告',
      tags: ['極簡主義'],
      price: 4200,
      rating: 5.0,
      reviews: 210,
      img: 'https://images.unsplash.com/photo-1554080353-a576cf803bda?w=400',
    },

    // 可繼續新增其他攝影師
  ];

  tags: string[] = [
    '全部風格',
    '黑白風格',
    '微距拍攝',
    '極簡主義',
    '日系清新',
    '底片感',
  ];
  selectedTag = '全部風格';
  filteredPhotographers: Photographer[] = [];

  constructor() {
    this.filteredPhotographers = this.photographers;
  }

  filterByTag(tag: string) {
    this.selectedTag = tag;
    if (tag === '全部風格') {
      this.filteredPhotographers = this.photographers;
    } else {
      this.filteredPhotographers = this.photographers.filter((p) =>
        p.tags.includes(tag)
      );
    }
  }
}
