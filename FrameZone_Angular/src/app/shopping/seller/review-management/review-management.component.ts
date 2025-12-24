import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';

interface Review {
  id: string;
  productName: string;
  productImage: string;
  customerName: string;
  rating: number;
  comment: string;
  reviewDate: Date;
  replied: boolean;
  reply?: string;
}

interface ReviewStats {
  total: number;
  pending: number;
  replied: number;
  averageRating: number;
}

@Component({
  selector: 'app-review-management',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './review-management.component.html',
  styleUrl: './review-management.component.css'
})
export class ReviewManagementComponent {
  stats: ReviewStats = {
    total: 0,
    pending: 0,
    replied: 0,
    averageRating: 0
  };

  reviews: Review[] = [];
  filteredReviews: Review[] = [];
  currentFilter: 'all' | 'pending' | 'replied' = 'all';
  searchQuery: string = '';

  replyText: { [key: string]: string } = {};

  ngOnInit(): void {
    this.loadMockData();
    this.calculateStats();
    this.applyFilters();
  }

  loadMockData(): void {
    this.reviews = [
      {
        id: '1',
        productName: 'iPhone 15 Pro Max 256GB',
        productImage: 'https://images.unsplash.com/photo-1592286927505-c80e44641c1c?w=100',
        customerName: '王小明',
        rating: 5,
        comment: '商品品質非常好，包裝完整，賣家出貨速度快，非常滿意！',
        reviewDate: new Date('2024-12-20'),
        replied: false
      },
      {
        id: '2',
        productName: 'AirPods Pro 第二代',
        productImage: 'https://images.unsplash.com/photo-1606841837239-c5a1a4a07af7?w=100',
        customerName: '李美華',
        rating: 4,
        comment: '音質很棒，降噪效果不錯，但價格有點貴。',
        reviewDate: new Date('2024-12-19'),
        replied: true,
        reply: '感謝您的購買與評價！很高興您喜歡我們的商品。'
      },
      {
        id: '3',
        productName: 'MacBook Pro 14吋 M3',
        productImage: 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=100',
        customerName: '張大偉',
        rating: 5,
        comment: '效能強大，螢幕顯示超棒，很推薦！',
        reviewDate: new Date('2024-12-18'),
        replied: true,
        reply: '感謝您的五星好評！祝您使用愉快。'
      },
      {
        id: '4',
        productName: '旅遊明信片',
        productImage: 'https://images.unsplash.com/photo-1506905925346-21bda4d32df4?w=100',
        customerName: '陳小玲',
        rating: 3,
        comment: '明信片品質普通，印刷有些模糊。',
        reviewDate: new Date('2024-12-17'),
        replied: false
      },
      {
        id: '5',
        productName: '復古底片相機',
        productImage: 'https://images.unsplash.com/photo-1526170375885-4d8ecf77b99f?w=100',
        customerName: '林志明',
        rating: 5,
        comment: '相機保存狀況很好，很有復古感，超喜歡！',
        reviewDate: new Date('2024-12-16'),
        replied: false
      }
    ];
  }

  calculateStats(): void {
    this.stats.total = this.reviews.length;
    this.stats.pending = this.reviews.filter(r => !r.replied).length;
    this.stats.replied = this.reviews.filter(r => r.replied).length;

    const totalRating = this.reviews.reduce((sum, r) => sum + r.rating, 0);
    this.stats.averageRating = this.reviews.length > 0 ? totalRating / this.reviews.length : 0;
  }

  applyFilters(): void {
    let result = [...this.reviews];

    if (this.currentFilter === 'pending') {
      result = result.filter(r => !r.replied);
    } else if (this.currentFilter === 'replied') {
      result = result.filter(r => r.replied);
    }

    if (this.searchQuery.trim()) {
      const query = this.searchQuery.toLowerCase();
      result = result.filter(r =>
        r.productName.toLowerCase().includes(query) ||
        r.customerName.toLowerCase().includes(query) ||
        r.comment.toLowerCase().includes(query)
      );
    }

    this.filteredReviews = result;
  }

  onFilterChange(filter: 'all' | 'pending' | 'replied'): void {
    this.currentFilter = filter;
    this.applyFilters();
  }

  onSearchChange(): void {
    this.applyFilters();
  }

  submitReply(review: Review): void {
    const reply = this.replyText[review.id];
    if (reply && reply.trim()) {
      review.replied = true;
      review.reply = reply.trim();
      this.replyText[review.id] = '';
      this.calculateStats();
      alert('回覆已送出！');
    }
  }

  getStars(rating: number): string[] {
    return Array(5).fill('').map((_, i) => i < rating ? 'star-filled' : 'star');
  }
}
