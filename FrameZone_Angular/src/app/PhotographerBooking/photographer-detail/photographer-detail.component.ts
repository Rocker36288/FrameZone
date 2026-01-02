import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { PhotographerReviewComponent } from "../photographer-review/photographer-review.component";
import { PhotographerBookingsidebarComponent } from "../photographer-bookingsidebar/photographer-bookingsidebar.component";
import { PhotographerServiceinfoComponent } from "../photographer-serviceinfo/photographer-serviceinfo.component";
import { PhotographerProfileComponent } from "../photographer-profile/photographer-profile.component";
interface Photographer {
  photographerId: number;
  studioName: string;
  displayName: string;
  description: string;
  yearsOfExperience: number;
  avatarUrl: string;
  portfolioUrl: string;
  totalBookings: number;
  rating: number;
  reviewCount: number;
}

interface Specialty {
  specialtyId: number;
  specialtyName: string;
}

interface Service {
  serviceId: number;
  serviceName: string;
  description: string;
  basePrice: number;
  duration: number;
  maxRevisions: number;
  deliveryDays: number;
  includedPhotos: number;
  isPopular?: boolean;
  originalPrice?: number;
}

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
  photographer: Photographer | null = null;
  specialties: Specialty[] = [];
  services: Service[] = [];
  reviews: Review[] = [];
  portfolioImages: string[] = [];
  faqs: FAQ[] = [];

  selectedService: Service | null = null;

  constructor(private route: ActivatedRoute) {}

  ngOnInit(): void {
    // 模擬從 API 獲取數據
    this.loadMockData();
  }

  loadMockData(): void {
    // 攝影師資料
    this.photographer = {
      photographerId: 1,
      studioName: '影光工作室',
      displayName: 'Liam Chen',
      description: '捕捉光影中最真實的你,讓美在平凡中綻放。',
      yearsOfExperience: 8,
      avatarUrl: 'https://i.pravatar.cc/150?u=liam',
      portfolioUrl: 'https://instagram.com/liamchen',
      totalBookings: 450,
      rating: 4.9,
      reviewCount: 128,
    };

    // 作品集圖片
    this.portfolioImages = [
      'https://images.unsplash.com/photo-1519741497674-611481863552?auto=format&fit=crop&w=800',
      'https://images.unsplash.com/photo-1511285560929-80b456fea0bc?auto=format&fit=crop&w=400',
      'https://images.unsplash.com/photo-1520854221256-17451cc331bf?auto=format&fit=crop&w=400',
      'https://images.unsplash.com/photo-1519225421980-715cb0215aed?auto=format&fit=crop&w=400',
      'https://images.unsplash.com/photo-1532712938310-34cb3982ef74?auto=format&fit=crop&w=400',
    ];

    // 專長標籤
    this.specialties = [
      { specialtyId: 1, specialtyName: '#日系清透' },
      { specialtyId: 2, specialtyName: '#婚紗攝影' },
    ];

    // 服務方案
    this.services = [
      {
        serviceId: 1,
        serviceName: '個人輕寫真日常方案',
        description:
          '包含全數校色毛片 (約 80-100 張)、雲端空間提供保存 30 天、此價格不含妝髮加購',
        basePrice: 3600,
        duration: 90,
        maxRevisions: 2,
        deliveryDays: 7,
        includedPhotos: 12,
        isPopular: true,
        originalPrice: 4500,
      },
      {
        serviceId: 2,
        serviceName: '情侶寫真方案',
        description: '專業引導互動、精修 20 張、贈送底片風格調色',
        basePrice: 5800,
        duration: 120,
        maxRevisions: 3,
        deliveryDays: 10,
        includedPhotos: 20,
      },
    ];

    // 默認選擇第一個方案
    this.selectedService = this.services[0];

    // FAQ
    this.faqs = [
      {
        question: '服務地區與交通費計算',
        answer:
          '大台北地區免交通費。桃園、宜蘭地區需加收 TWD 500,其他地區請私訊詢問。',
      },
      {
        question: '如何改期或取消預約?',
        answer: '拍攝日前 7 天可免費改期一次,3 天內改期或取消將扣除 30% 訂金。',
      },
      {
        question: '下雨天怎麼辦?',
        answer: '若遇雨天可協調改期,或改為室內拍攝場景,不另收費用。',
      },
    ];

    // 評價
    this.reviews = [
      {
        reviewId: 1,
        reviewerName: 'Wei-Ting',
        rating: 5,
        reviewContent: '攝影師非常專業,拍攝過程氣氛很輕鬆!成品我們非常滿意。',
        createdAt: '2024年12月',
        avatarUrl: 'https://i.pravatar.cc/100?u=jane',
        photos: [
          'https://images.unsplash.com/photo-1519741497674-611481863552?auto=format&fit=crop&w=150',
          'https://images.unsplash.com/photo-1511285560929-80b456fea0bc?auto=format&fit=crop&w=150',
        ],
      },
      {
        reviewId: 2,
        reviewerName: 'Amy',
        rating: 5,
        reviewContent: '超級推薦!拍出來的照片質感很好,攝影師很會抓角度。',
        createdAt: '2024年11月',
        avatarUrl: 'https://i.pravatar.cc/100?u=amy',
        photos: [],
      },
    ];
  }

  onServiceSelected(service: Service): void {
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
