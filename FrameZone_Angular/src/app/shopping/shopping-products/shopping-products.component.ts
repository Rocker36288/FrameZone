import { ProductApiService } from './../shared/services/product-api.service';
import { CommonModule } from '@angular/common';
import { Component, effect } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FooterComponent } from "../../shared/components/footer/footer.component";
import { ActivatedRoute } from '@angular/router';
import { SearchService } from '../shared/services/search.service';
import { ProductCardComponent } from "../shared/components/product-card/product-card.component";
import { ToastNotificationComponent } from "../shared/components/toast-notification/toast-notification.component";
import { debounceTime, distinctUntilChanged, Subject } from 'rxjs';
import { ShopProduct } from '../interfaces/products';

@Component({
  selector: 'app-shopping-products',
  standalone: true,
  imports: [FormsModule, CommonModule, FooterComponent, ProductCardComponent, ToastNotificationComponent],
  templateUrl: './shopping-products.component.html',
  styleUrl: './shopping-products.component.css'
})
export class ShoppingProductsComponent {
  constructor(
    private searchService: SearchService,
    private productApiService: ProductApiService,
    private route: ActivatedRoute
  ) {
    // 將 effect 放在 constructor 或屬性初始化層級 (Angular 16/17+ 推薦)
    effect(() => {
      const keyword = this.searchService.keyword().toLowerCase();

      // 檢查 URL 是否有 keyword 參數，如果有就不要覆蓋
      const urlKeyword = this.route.snapshot.queryParams['keyword'];
      if (!urlKeyword) {
        // 只有在 URL 沒有 keyword 時，才使用 searchService 的值
        this.searchKeyword = keyword; // 同步內部的搜尋框
        this.applyFilters(); // 當全站搜尋變動時，觸發篩選
      }
    });
  }

  allProducts: ShopProduct[] = [];
  filteredProducts: ShopProduct[] = [];
  displayedProducts: ShopProduct[] = [];

  currentPage: number = 1;
  itemsPerPage: number = 30;
  totalPages: number = 1;
  pages: number[] = [];

  // 篩選條件
  searchKeyword: string = '';
  minPrice: number | null = null;
  maxPrice: number | null = null;
  sortBy: string = 'default';
  isLoading: boolean = true;


  ngOnInit(): void {
    // 1. 載入商品資料
    this.loadProductsFromApi();

    // 2. 訂閱 URL 參數變更，當參數改變時更新搜尋關鍵字並重新篩選
    this.route.queryParams.subscribe(params => {
      const keyword = params['keyword'];
      console.log('URL 參數變更，關鍵字:', keyword); // 除錯用

      // 更新搜尋關鍵字
      this.searchKeyword = keyword || '';

      // 如果商品已載入，立即重新套用篩選
      if (this.allProducts.length > 0) {
        console.log('商品已載入，套用篩選，關鍵字:', this.searchKeyword); // 除錯用
        this.applyFilters();
      }
    });
  }

  loadProductsFromApi(): void {
    console.log('開始載入商品資料...'); // 除錯用
    this.isLoading = true;
    this.productApiService.getProducts().subscribe({
      next: (data) => {
        this.isLoading = false;
        console.log('API 回傳資料:', data); // 除錯用

        // 2. 將 API 回傳的 DTO 對應到你的前端介面
        this.allProducts = data.map(item => ({
          productId: item.productId,
          name: item.productName,
          image: item.mainImageUrl || 'assets/images/default.jpg', // 預防沒圖片
          description: item.description,
          price: Number(item.price),
          seller: {
            name: item.seller ? item.seller.displayName : '未知賣家',
            avatar: item.seller ? item.seller.avatar : ''
          },
          postedDate: this.formatDate(item.createdAt),
          sales: item.salesCount || 0,
          categoryId: item.categoryId,
          sellerCategoryIds: item.sellerCategoryIds || [],
          isFavorite: item.isFavorite || false,
          averageRating: item.averageRating || 0,
          reviewCount: item.reviewCount || 0
        }));

        console.log('轉換後商品:', this.allProducts); // 除錯用

        // 3. 資料載入後執行篩選與分頁
        this.applyFilters();
      },
      error: (err) => {
        this.isLoading = false;
        console.error('API 讀取失敗', err);
        console.error('錯誤詳情', err.error);
        console.error('狀態碼:', err.status);
      }
    });
  }

  // generateProducts(): void {
  //   const productNames = ['相機', '鏡頭', '腳架', '記憶卡', '閃光燈', '背包', '濾鏡', '相機包'];

  //   for (let i = 1; i <= 100; i++) {
  //     //先取出這一次迴圈要用的名稱
  //     const name = productNames[Math.floor(Math.random() * productNames.length)];
  //     //根據名稱決定圖片關鍵字
  //     let keyword = 'camera';
  //     if (name === '相機包') keyword = 'camera-bag';
  //     if (name === '腳架') keyword = 'tripod';

  //     this.allProducts.push({
  //       id: i,
  //       name: name,
  //       description: '全新未拆封',
  //       image: `https://loremflickr.com/400/300/${keyword}?lock=${i}`,
  //       price: Math.floor(Math.random() * 30000) + 1000,
  //       seller: {
  //         name: `賣家${i}`,
  //         avatar: `https://api.dicebear.com/7.x/avataaars/svg?seed=${i}`
  //       },
  //       postedDate: `${Math.floor(Math.random() * 30) + 1} 天前`,
  //       sales: 45,
  //       categoryId: 1,
  //       isFavorite: false,
  //     });
  //   }
  // }

  // getRandomPhotoId(): string {
  //   const photoIds = [
  //     '1606982801255-5ec8de5fee3f',
  //     '1526170375885-4d8ecf77b99f',
  //     '1495121605193-b116b5b9c5fe',
  //     '1502920917128-1aa500764cbd'
  //   ];
  //   return photoIds[Math.floor(Math.random() * photoIds.length)];
  // }

  // 即時驗證最低價格（防止負數）
  validateMinPrice(): void {
    if (this.minPrice !== null && this.minPrice !== undefined) {
      const value = Number(this.minPrice);
      if (isNaN(value) || value < 0) {
        this.minPrice = 0;
      } else {
        this.minPrice = Math.floor(value); // 只保留整數
      }
    }
  }

  // 即時驗證最高價格（防止負數）
  validateMaxPrice(): void {
    if (this.maxPrice !== null && this.maxPrice !== undefined) {
      const value = Number(this.maxPrice);
      if (isNaN(value) || value < 0) {
        this.maxPrice = 0;
      } else {
        this.maxPrice = Math.floor(value); // 只保留整數
      }
    }
  }

  applyFilters(): void {
    // 優先判斷目前的關鍵字來源
    // 如果 Service 有值且本地沒值，使用 Service 值
    // 這裡我們確保 searchKeyword 始終反映最新的搜尋意圖
    const finalKeyword = this.searchKeyword.trim().toLowerCase();

    // 強制轉為數字，解決字串比較導致的錯誤
    const min = (this.minPrice !== null && this.minPrice !== undefined && this.minPrice !== ('' as any)) ? Number(this.minPrice) : null;
    const max = (this.maxPrice !== null && this.maxPrice !== undefined && this.maxPrice !== ('' as any)) ? Number(this.maxPrice) : null;

    this.filteredProducts = this.allProducts.filter(product => {
      // 搜尋關鍵字 (支援多關鍵字，用 | 分隔)
      if (finalKeyword) {
        const keywords = finalKeyword.split('|').map(k => k.trim()).filter(k => k !== '');
        if (keywords.length > 0) {
          const isMatch = keywords.some(k =>
            product.name.toLowerCase().includes(k) ||
            product.description.toLowerCase().includes(k)
          );
          if (!isMatch) return false;
        }
      }

      // 價格範圍篩選 (使用轉換過的 min, max)
      if (min !== null && product.price < min) return false;
      if (max !== null && product.price > max) return false;

      // 如果最高價低於最低價，點擊搜尋時自動清空最高價或給予提示
      if (min !== null && max !== null && max < min) {
        // 這裡可以選擇清空 maxPrice 或不執行搜尋
        // this.maxPrice = null;
      }

      return true;

    });

    // 排序
    this.sortProducts();

    // 重新計算分頁
    this.currentPage = 1;
    this.calculatePagination();
    this.updateDisplayedProducts();
  }

  sortProducts(): void {
    switch (this.sortBy) {
      case 'price-low':
        this.filteredProducts.sort((a, b) => a.price - b.price);
        break;
      case 'price-high':
        this.filteredProducts.sort((a, b) => b.price - a.price);
        break;
      case 'newest':
        this.filteredProducts.sort((a, b) => a.productId - b.productId);
        break;
      default:
        // 預設排序
        break;
    }
  }

  setQuickPrice(min: number, max: number): void {
    this.minPrice = min;
    this.maxPrice = max;
    this.applyFilters();
  }

  clearFilters(): void {
    this.searchKeyword = '';
    this.minPrice = null;
    this.maxPrice = null;
    this.sortBy = 'default';
    this.applyFilters();
  }

  calculatePagination(): void {
    this.totalPages = Math.ceil(this.filteredProducts.length / this.itemsPerPage);
    this.updatePageNumbers();
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

  updateDisplayedProducts(): void {
    const startIndex = (this.currentPage - 1) * this.itemsPerPage;
    const endIndex = startIndex + this.itemsPerPage;
    this.displayedProducts = this.filteredProducts.slice(startIndex, endIndex);
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.updatePageNumbers();
      this.updateDisplayedProducts();
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

  toggleFavorite(product: ShopProduct): void {
    product.isFavorite = !product.isFavorite;
  }

  private formatDate(dateString: string): string {
    if (!dateString) return '';
    const date = new Date(dateString);
    const now = new Date();
    const diff = now.getTime() - date.getTime();

    // Convert to days
    const diffDays = Math.floor(diff / (1000 * 3600 * 24));

    if (diffDays <= 7) {
      return `${diffDays} 天前`;
    } else if (diffDays <= 30) {
      return `${Math.floor(diffDays / 7)} 週前`;
    } else {
      return `${Math.floor(diffDays / 30)} 個月前`;
    }
  }
}
