import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { PhotographerReviewComponent } from "../photographer-review/photographer-review.component";
import { PhotographerBookingsidebarComponent } from "../photographer-bookingsidebar/photographer-bookingsidebar.component";
import { PhotographerServiceinfoComponent } from "../photographer-serviceinfo/photographer-serviceinfo.component";
import { PhotographerProfileComponent } from "../photographer-profile/photographer-profile.component";
import { PhotographerBookingService } from '../services/photographer-booking.service';
import { PhotographerDto, ServiceDto } from '../models/photographer-booking.models';

// Using local interfaces for reviews/faq/specialty if they are not in main models yet,
// or map them. For now, I'll keep local interfaces for parts NOT in DTO and map them if needed.
// But Photographer and Service come from DTO.

interface Review {
  reviewId: number;
  reviewerName: string;
  rating: number;
  reviewContent: string;
  createdAt: string;
  avatarUrl: string;
  photos: string[];
}

interface FAQ {
  question: string;
  answer: string;
}

@Component({
  selector: 'app-photographer-detail',
  imports: [CommonModule, PhotographerReviewComponent, PhotographerBookingsidebarComponent, PhotographerServiceinfoComponent, PhotographerProfileComponent],
  templateUrl: './photographer-detail.component.html',
  styleUrl: './photographer-detail.component.css',
})
export class PhotographerDetailComponent implements OnInit {
  photographer: PhotographerDto | null = null;
  specialties: string[] = [];
  services: ServiceDto[] = [];
  reviews: Review[] = [];
  portfolioImages: string[] = [];
  faqs: FAQ[] = [];

  selectedService: ServiceDto | null = null;

  constructor(
    private route: ActivatedRoute,
    private bookingService: PhotographerBookingService
  ) { }

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      const id = params['id'];
      if (id) {
        this.loadData(+id);
      }
    });
  }

  loadData(id: number): void {
    this.bookingService.getPhotographerById(id).subscribe({
      next: (data) => {
        this.photographer = data;
        this.services = data.services;
        this.specialties = data.specialties;

        // Mocking portfolio images if empty (since DTO has one portfolioUrl)
        if (data.portfolioUrl && data.portfolioUrl.startsWith('http')) {
          this.portfolioImages = [data.portfolioUrl];
          // Add some random placeholder images for "gallery" feel if only one image
          this.portfolioImages.push('https://images.unsplash.com/photo-1511285560929-80b456fea0bc?auto=format&fit=crop&w=400');
          this.portfolioImages.push('https://images.unsplash.com/photo-1520854221256-17451cc331bf?auto=format&fit=crop&w=400');
        } else {
          // Fallback
          this.portfolioImages = [
            'https://images.unsplash.com/photo-1519741497674-611481863552?auto=format&fit=crop&w=800',
            'https://images.unsplash.com/photo-1511285560929-80b456fea0bc?auto=format&fit=crop&w=400',
          ];
        }

        if (this.services.length > 0) {
          this.selectedService = this.services[0];
        }

        // Mock FAQ and Reviews (as backend doesn't provide them yet or I didn't add to DTO)
        this.loadMockFaqsAndReviews();
      },
      error: (err) => console.error('Error loading photographer', err)
    });
  }

  loadMockFaqsAndReviews(): void {
    this.faqs = [
      {
        question: '服務地區與交通費計算',
        answer: '大台北地區免交通費。桃園、宜蘭地區需加收 TWD 500,其他地區請私訊詢問。',
      },
      {
        question: '如何改期或取消預約?',
        answer: '拍攝日前 7 天可免費改期一次,3 天內改期或取消將扣除 30% 訂金。',
      },
    ];
    this.reviews = [
      {
        reviewId: 1,
        reviewerName: 'Wei-Ting',
        rating: 5,
        reviewContent: '攝影師非常專業,拍攝過程氣氛很輕鬆!',
        createdAt: '2024年12月',
        avatarUrl: 'https://i.pravatar.cc/100?u=jane',
        photos: []
      }
    ];
  }

  onServiceSelected(service: ServiceDto): void { // Start of unchanged methods
    this.selectedService = service;
  }

  goBack(): void {
    window.history.back();
  }

  sharePage(): void {
    // 實作分享功能
    console.log('分享頁面');
  }

  toggleFavorite(): void {
    // 實作收藏功能
    console.log('切換收藏');
  }

  showAllPhotos(): void {
    // 實作查看所有照片功能
    console.log('查看所有照片');
  }
}
