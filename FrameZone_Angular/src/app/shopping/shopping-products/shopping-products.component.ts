import { CommonModule } from '@angular/common';
import { Component, effect } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FooterComponent } from "../../shared/components/footer/footer.component";
import { ActivatedRoute } from '@angular/router';
import { SearchService } from '../shared/services/search.service';
import { ProductCardComponent } from "../shared/components/product-card/product-card.component";
import { ToastNotificationComponent } from "../shared/components/toast-notification/toast-notification.component";
import { debounceTime, distinctUntilChanged, Subject } from 'rxjs';

interface Product {
  id: number;
  name: string;
  image: string;
  description: string;
  price: number;
  seller: {
    name: string;
    avatar: string;
  };
  postedDate: string;
  sales: number;
  categoryId: number;
  isFavorite: boolean;
}

@Component({
  selector: 'app-shopping-products',
  standalone: true,
  imports: [FormsModule, CommonModule, FooterComponent, ProductCardComponent, ToastNotificationComponent],
  templateUrl: './shopping-products.component.html',
  styleUrl: './shopping-products.component.css'
})
export class ShoppingProductsComponent {
  constructor(
    private searchService: SearchService
  ) {
    // 將 effect 放在 constructor 或屬性初始化層級 (Angular 16/17+ 推薦)
    effect(() => {
      const keyword = this.searchService.keyword().toLowerCase();
      this.searchKeyword = keyword; // 同步內部的搜尋框
      this.applyFilters(); // 當全站搜尋變動時，觸發篩選
    });
  }

  allProducts: Product[] = [];
  filteredProducts: Product[] = [];
  displayedProducts: Product[] = [];

  currentPage: number = 1;
  itemsPerPage: number = 30;
  totalPages: number = 1;
  pages: number[] = [];

  // 篩選條件
  searchKeyword: string = '';
  minPrice: number | null = null;
  maxPrice: number | null = null;
  sortBy: string = 'default';

  ngOnInit(): void {
    this.generateProducts(); // 1. 先生成資料

    // 2. 獲取 Service 的初始值並執行第一次篩選
    this.searchKeyword = this.searchService.keyword();
    this.applyFilters();
  }

  generateProducts(): void {
    const productNames = ['相機', '鏡頭', '腳架', '記憶卡', '閃光燈', '背包', '濾鏡', '相機包'];

    for (let i = 1; i <= 100; i++) {
      //先取出這一次迴圈要用的名稱
      const name = productNames[Math.floor(Math.random() * productNames.length)];
      //根據名稱決定圖片關鍵字
      let keyword = 'camera';
      if (name === '相機包') keyword = 'camera-bag';
      if (name === '腳架') keyword = 'tripod';

      this.allProducts.push({
        id: i,
        name: name,
        description: '全新未拆封',
        image: `https://loremflickr.com/400/300/${keyword}?lock=${i}`,
        price: Math.floor(Math.random() * 30000) + 1000,
        seller: {
          name: `賣家${i}`,
          avatar: `https://api.dicebear.com/7.x/avataaars/svg?seed=${i}`
        },
        postedDate: `${Math.floor(Math.random() * 30) + 1} 天前`,
        sales: 45,
        categoryId: 1,
        isFavorite: false,
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
      // 搜尋關鍵字
      if (finalKeyword) {
        // 建議：同時搜尋名稱與描述，增加「閃光燈」這類關鍵字的命中率
        const isMatch = product.name.toLowerCase().includes(finalKeyword) ||
          product.description.toLowerCase().includes(finalKeyword);
        if (!isMatch) return false;
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
        this.filteredProducts.sort((a, b) => a.id - b.id);
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

  toggleFavorite(product: Product): void {
    product.isFavorite = !product.isFavorite;
  }
}
