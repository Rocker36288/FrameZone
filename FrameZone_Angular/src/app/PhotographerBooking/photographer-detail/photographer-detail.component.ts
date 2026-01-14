import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { PhotographerReviewComponent } from "../photographer-review/photographer-review.component";
import { PhotographerBookingsidebarComponent } from "../photographer-bookingsidebar/photographer-bookingsidebar.component";
import { PhotographerServiceinfoComponent } from "../photographer-serviceinfo/photographer-serviceinfo.component";
import { PhotographerProfileComponent } from "../photographer-profile/photographer-profile.component";
import { PhotographerBookingService } from '../services/photographer-booking.service';
import { MockBookingService } from '../services/mock-booking.service';
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
  icon?: string;
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
  mockRating: number = 5.0;
  mockReviewCount: number = 3;

  selectedService: ServiceDto | null = null;
  isFavorite: boolean = false;

  constructor(
    private route: ActivatedRoute,
    private bookingService: PhotographerBookingService,
    private mockBookingService: MockBookingService
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
        const apiBaseUrl = 'https://localhost:7213';

        if (data.avatarUrl && !data.avatarUrl.startsWith('http')) {
          data.avatarUrl = `${apiBaseUrl}${data.avatarUrl}`;
        }

        this.photographer = data;
        this.services = data.services;
        this.specialties = data.specialties;

        // 如果後端回傳 portfolioFile，解析它
        if (data.portfolioFile) {
          const files = data.portfolioFile.split(',');
          const apiBaseUrl = 'https://localhost:7213';
          this.portfolioImages = files.map(file => {
            if (file.startsWith('http')) return file;
            return `${apiBaseUrl}${file.trim()}`;
          });
        }
        // Fallback for demo if empty
        else if (data.portfolioUrl && data.portfolioUrl.startsWith('http')) {
          this.portfolioImages = [data.portfolioUrl];
          this.portfolioImages.push('images/Photographer/Carousel02.png');
          this.portfolioImages.push('images/Photographer/Carousel03.png');
        } else {
          // Fallback
          this.portfolioImages = [
            'images/Photographer/Carousel01.png',
            'images/Photographer/Carousel02.png',
          ];
        }

        if (this.services.length > 0) {
          this.selectedService = this.services[0];
        }

        // Mock FAQ and Reviews (as backend doesn't provide them yet or I didn't add to DTO)
        this.loadMockFaqsAndReviews(id);
      },
      error: (err) => console.error('Error loading photographer', err)
    });
  }

  loadMockFaqsAndReviews(photographerId: number): void {
    this.faqs = [
      {
        question: '交通費用如何計算？',
        answer: '大台北地區免交通費。桃園、宜蘭地區需加收 TWD 500,其他地區請私訊詢問。',
        icon: 'ti-map-pin'
      },
      {
        question: '如何改期或取消預約？',
        answer: '拍攝日前 7 天可免費改期一次,3 天內改期或取消將扣除 30% 訂金。',
        icon: 'ti-calendar-event'
      },
      {
        question: '天氣不佳的改期政策',
        answer: '若氣象局發佈降雨機率 > 60%，可於拍攝前 24 小時免費改期一次。若因天災等不可APP力因素則無條件退還訂金。',
        icon: 'ti-cloud-storm'
      },
    ];

    // 從服務中讀取同步的模擬評價
    this.mockBookingService.getReviews(photographerId).subscribe(data => {
      this.reviews = data;
      this.mockRating = 4.9;
      this.mockReviewCount = this.reviews.length;
    });
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
    this.isFavorite = !this.isFavorite;
    console.log('切換收藏:', this.isFavorite);
  }

  showAllPhotos(): void {
    // 實作查看所有照片功能
    console.log('查看所有照片');
  }
}
