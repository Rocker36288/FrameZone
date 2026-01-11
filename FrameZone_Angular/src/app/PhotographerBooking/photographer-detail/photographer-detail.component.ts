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
  mockRating: number = 5.0;
  mockReviewCount: number = 3;

  selectedService: ServiceDto | null = null;
  isFavorite: boolean = false;

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
          // 假設後端回傳的是相對路徑 (例如 /image/...)，加上 Base URL
          // 這裡假設 API Server 和圖片 Server 是同一個，或者前端有設定 Proxy
          // 如果是開發環境，通常是 http://localhost:5276 (根據 launchSettings.json)
          // 但這裡為了簡單，我們假設前端可以透過相對路徑存取 (如果有設定 proxy.conf.json)
          // 或者我們直接加上完整的 API URL 前綴。目前先假設直接使用路徑即可 (如果前端 server 有 proxy 或是同源)
          // 但通常 .NET Core Web API 的靜態檔案需要完整的 URL 或是前端 proxy
          // 為了保險起見，我們看看是否需要加 Base URL。
          // 原本的 portfolioUrl 也是 string，這裡我們試著直接用。
          // 根據使用者描述: "資料庫Photographers的PortfolioFile 裡/image/..."
          // 這看起來是 Server 的相對路徑。Angular 開發伺服器如果不設 Proxy 指向 API Server 的 wwwroot，會 404。
          // 假設 API 位置在 environment 裡，不過這裡沒有看到 environment 引用。
          // 先試著加上一個固定的 Base URL 常數，或者寫在 Service 裡比較好？
          // 暫時先 hardcode 一個常見的 localhost 端口，或者假設使用者有 proxy。
          // 觀察之前的 portfolioUrl 處理方式: `data.portfolioUrl.startsWith('http')`
          // 我們先加上 path。
          const apiBaseUrl = 'https://localhost:7213'; // 根據一般 .NET API 預設端口，或需確認
          this.portfolioImages = files.map(file => {
            if (file.startsWith('http')) return file;
            return `${apiBaseUrl}${file.trim()}`;
          });
        }
        // Fallback for demo if empty
        else if (data.portfolioUrl && data.portfolioUrl.startsWith('http')) {
          this.portfolioImages = [data.portfolioUrl];
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
        question: '交通費用如何計算？',
        answer: '大台北地區免交通費。桃園、宜蘭地區需加收 TWD 500,其他地區請私訊詢問。',
      },
      {
        question: '如何改期或取消預約？',
        answer: '拍攝日前 7 天可免費改期一次,3 天內改期或取消將扣除 30% 訂金。',
      },
      {
        question: '天氣不佳的改期政策',
        answer: '若氣象局發佈降雨機率 > 60%，可於拍攝前 24 小時免費改期一次。若因天災等不可APP力因素則無條件退還訂金。',
      },
    ];
    this.mockRating = 4.9;
    this.reviews = [
      {
        reviewId: 1,
        reviewerName: '簡宜君',
        rating: 5,
        reviewContent: '攝影師非常專業, 拍攝過程氣氛很輕鬆！對於不習慣面對鏡頭的我們提供了很好的引導，最後的作品色調非常優雅，家人都很滿意。',
        createdAt: '2025年12月',
        avatarUrl: 'https://i.pravatar.cc/150?u=jane',
        photos: [
          'https://images.unsplash.com/photo-1519741497674-611481863552?auto=format&fit=crop&w=400',
          'https://images.unsplash.com/photo-1511285560929-80b456fea0bc?auto=format&fit=crop&w=400'
        ]
      },
      {
        reviewId: 2,
        reviewerName: '陳志雄',
        rating: 5,
        reviewContent: '這次的戶外親子寫真拍得很棒。攝影師對小孩非常有耐性，能捕捉到很自然的互動瞬間。修圖速度也很快，值得推薦！',
        createdAt: '2025年11月',
        avatarUrl: 'https://i.pravatar.cc/150?u=chen',
        photos: [
          'https://images.unsplash.com/photo-1511285560929-80b456fea0bc?auto=format&fit=crop&w=400'
        ]
      },
      {
        reviewId: 3,
        reviewerName: 'Linda Wang',
        rating: 4,
        reviewContent: '整體的服務體驗很好，從諮詢到拍攝都很流暢。攝影師非常有美感，捕捉到很多我沒想過的視角。',
        createdAt: '2025年10月',
        avatarUrl: 'https://i.pravatar.cc/150?u=linda',
        photos: []
      }
    ];
    this.mockReviewCount = this.reviews.length;
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
