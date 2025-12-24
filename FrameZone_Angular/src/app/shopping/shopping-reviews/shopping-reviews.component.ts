import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FooterComponent } from "../../shared/components/footer/footer.component";

interface Review {
  id: number;
  reviewer: {
    name: string;
    avatar: string;
  };
  rating: number;
  date: string;
  comment: string;
  productName: string;
  productImage: string;
  seller: string;
  type: 'buyer' | 'seller'; // 買家評價或賣家收到的評價
}

@Component({
  selector: 'app-shopping-reviews',
  standalone: true,
  imports: [FormsModule, CommonModule, FooterComponent],
  templateUrl: './shopping-reviews.component.html',
  styleUrl: './shopping-reviews.component.css'
})
export class ShoppingReviewsComponent {
  allReviews: Review[] = [];
  displayedReviews: Review[] = [];

  currentPage: number = 1;
  itemsPerPage: number = 30;
  totalPages: number = 1;
  pages: number[] = [];

  // 切換模式
  viewMode: 'buyer' | 'seller' = 'buyer'; // 預設顯示買家評價
  isSeller: boolean = true; // 是否為賣家身份

  averageRating: number = 4.5;
  totalReviews: number = 0;
  roundedRating: number = 5;

  ngOnInit(): void {
    this.generateReviews();
    this.switchViewMode('buyer');
  }

  generateReviews(): void {
    const buyerComments = [
      '賣家態度親切，商品包裝完整，物流速度快！',
      '商品品質很好，符合描述，很滿意！',
      '價格合理，商品狀況良好，推薦！',
      '賣家回覆很快速，商品比預期還要好！',
      '整體體驗不錯，下次還會再光顧！',
      '商品符合描述，運送快速，非常滿意！',
      '質量很好，賣家服務態度佳，值得購買！',
      '物超所值，商品狀態優良，推薦給大家！'
    ];

    const sellerComments = [
      '買家很客氣，付款迅速，推薦！',
      '溝通順暢，交易愉快，好買家！',
      '付款快速，態度良好，下次歡迎再來！',
      '很棒的買家，交易過程順利！',
      '買家很有禮貌，交易愉快！',
      '優質買家，值得推薦！'
    ];

    const productNames = ['相機', '鏡頭', '腳架', '記憶卡', '背包', '閃光燈', '濾鏡', '相機包'];

    // 生成買家評價（給賣家的評價）
    for (let i = 1; i <= 80; i++) {
      this.allReviews.push({
        id: i,
        reviewer: {
          name: '我',
          avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=me'
        },
        rating: Math.floor(Math.random() * 2) + 4,
        date: `${Math.floor(Math.random() * 60) + 1} 天前`,
        comment: buyerComments[Math.floor(Math.random() * buyerComments.length)],
        productName: productNames[Math.floor(Math.random() * productNames.length)],
        productImage: `https://images.unsplash.com/photo-${this.getRandomPhotoId()}?w=100`,
        seller: `賣家${i}`,
        type: 'buyer'
      });
    }

    // 生成賣家收到的評價（買家給的評價）
    for (let i = 81; i <= 150; i++) {
      this.allReviews.push({
        id: i,
        reviewer: {
          name: `買家${i - 80}`,
          avatar: `https://api.dicebear.com/7.x/avataaars/svg?seed=buyer${i}`
        },
        rating: Math.floor(Math.random() * 2) + 4,
        date: `${Math.floor(Math.random() * 60) + 1} 天前`,
        comment: buyerComments[Math.floor(Math.random() * buyerComments.length)],
        productName: productNames[Math.floor(Math.random() * productNames.length)],
        productImage: `https://images.unsplash.com/photo-${this.getRandomPhotoId()}?w=100`,
        seller: '我',
        type: 'seller'
      });
    }
  }

  getRandomPhotoId(): string {
    const photoIds = [
      '1606982801255-5ec8de5fee3f',
      '1526170375885-4d8ecf77b99f',
      '1495121605193-b116b5b9c5fe',
      '1502920917128-1aa500764cbd'
    ];
    return photoIds[Math.floor(Math.random() * photoIds.length)];
  }

  switchViewMode(mode: 'buyer' | 'seller'): void {
    this.viewMode = mode;
    this.currentPage = 1;
    const filteredReviews = this.allReviews.filter(review => review.type === mode);
    this.calculatePaginationForFiltered(filteredReviews);
    this.calculateAverageRating(filteredReviews);
  }

  calculateAverageRating(reviews: Review[]): void {
    if (reviews.length === 0) {
      this.averageRating = 0;
      this.totalReviews = 0;
      this.roundedRating = 0;
      return;
    }
    const sum = reviews.reduce((acc, review) => acc + review.rating, 0);
    this.averageRating = sum / reviews.length;
    this.roundedRating = Math.round(this.averageRating);
    this.totalReviews = reviews.length;
  }

  calculatePaginationForFiltered(filteredReviews: Review[]): void {
    this.totalPages = Math.ceil(filteredReviews.length / this.itemsPerPage);
    this.updatePageNumbers();
    this.updateDisplayedReviews(filteredReviews);
  }

  updatePageNumbers(): void {
    this.pages = [];
    const maxPagesToShow = 5;
    let startPage = Math.max(1, this.currentPage - 2);
    let endPage = Math.min(this.totalPages, startPage + maxPagesToShow - 1);

    if (endPage - startPage < maxPagesToShow - 1) {
      startPage = Math.max(1, endPage - maxPagesToShow + 1);
    }

    for (let i = startPage; i <= endPage; i++) {
      this.pages.push(i);
    }
  }

  updateDisplayedReviews(filteredReviews: Review[]): void {
    const startIndex = (this.currentPage - 1) * this.itemsPerPage;
    const endIndex = startIndex + this.itemsPerPage;
    this.displayedReviews = filteredReviews.slice(startIndex, endIndex);
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.updatePageNumbers();
      const filteredReviews = this.allReviews.filter(review => review.type === this.viewMode);
      this.updateDisplayedReviews(filteredReviews);
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }

  goToFirstPage(): void {
    this.goToPage(1);
  }

  goToLastPage(): void {
    this.goToPage(this.totalPages);
  }

  goToPreviousPage(): void {
    this.goToPage(this.currentPage - 1);
  }

  goToNextPage(): void {
    this.goToPage(this.currentPage + 1);
  }

  getStarArray(rating: number): boolean[] {
    return Array(5).fill(false).map((_, i) => i < rating);
  }

  getRatingDistribution(): { star: number; count: number; percentage: number }[] {
    const filteredReviews = this.allReviews.filter(review => review.type === this.viewMode);
    const total = filteredReviews.length;

    if (total === 0) return [];

    const distribution = [5, 4, 3, 2, 1].map(star => {
      const count = filteredReviews.filter(review => review.rating === star).length;
      return {
        star,
        count,
        percentage: (count / total) * 100
      };
    });

    return distribution;
  }
}
