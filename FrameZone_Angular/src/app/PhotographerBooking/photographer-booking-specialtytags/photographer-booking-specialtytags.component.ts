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
      photographerId: 101,
      displayName: 'Mia Wedding',
      studioName: 'Mia Wedding Studio',
      studioAddress: '台北',
      studioType: '婚禮紀錄',
      description: '捕捉最真實的感動瞬間',
      specialties: ['黑白風格', '微距拍攝'],
      minPrice: 5000,
      rating: 4.8,
      reviewCount: 92,
      totalBookings: 150,
      avatarUrl: 'https://images.unsplash.com/photo-1512917774080-9991f1c4c750?w=400',
      portfolioUrl: '',
      services: []
    },
    {
      photographerId: 102,
      displayName: 'Sunny Studio',
      studioName: 'Sunny Creative',
      studioAddress: '台中',
      studioType: '商業廣告',
      description: '專業商業攝影與形象拍攝',
      specialties: ['極簡主義'],
      minPrice: 4200,
      rating: 5.0,
      reviewCount: 210,
      totalBookings: 300,
      avatarUrl: 'https://images.unsplash.com/photo-1554080353-a576cf803bda?w=400',
      portfolioUrl: '',
      services: []
    },
    {
      photographerId: 103,
      displayName: 'Alex Photo',
      studioName: 'Alex Photography',
      studioAddress: '台北',
      studioType: '人像寫真',
      description: '日系清新風格人像攝影',
      specialties: ['日系清新', '底片感'],
      minPrice: 3500,
      rating: 4.9,
      reviewCount: 156,
      totalBookings: 220,
      avatarUrl: 'https://images.unsplash.com/photo-1542038784456-1ea8e935640e?w=400',
      portfolioUrl: '',
      services: []
    },
    // Repeat for others if needed, using generic valid DTOs
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
        p.specialties.includes(tag)
      );
    }
  }
}
