import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FooterComponent } from "../../shared/components/footer/footer.component";
import { ActivatedRoute } from '@angular/router';

interface Product {
  id: number;
  name: string;
  description: string;
  image: string;
  condition: string;
  price: number;
  seller: {
    name: string;
    avatar: string;
  };
  postedDate: string;
  isFavorite: boolean;
  isNew: boolean;
}

@Component({
  selector: 'app-shopping-products',
  standalone: true,
  imports: [FormsModule, CommonModule, FooterComponent],
  templateUrl: './shopping-products.component.html',
  styleUrl: './shopping-products.component.css'
})
export class ShoppingProductsComponent {

  constructor(private route: ActivatedRoute) { }

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
  filterNew: boolean = false;
  filterUsed: boolean = false;
  sortBy: string = 'default';

  ngOnInit(): void {
    this.generateProducts();

    // 接收 Header 搜尋參數
    this.route.queryParams.subscribe(params => {
      const keyword = (params['search'] || '').trim();
      this.searchKeyword = keyword;

      // 套用你原本的篩選邏輯
      this.applyFilters();
    });
  }

  generateProducts(): void {
    const productNames = ['相機', '鏡頭', '腳架', '記憶卡', '閃光燈', '背包', '濾鏡', '相機包'];
    const conditions = ['全新', '二手'];

    for (let i = 1; i <= 100; i++) {
      const isNew = Math.random() > 0.5;
      this.allProducts.push({
        id: i,
        name: productNames[Math.floor(Math.random() * productNames.length)],
        description: isNew ? '全新未拆封' : '二手使用過',
        image: `https://images.unsplash.com/photo-${this.getRandomPhotoId()}?w=400`,
        condition: isNew ? '全新' : '二手',
        price: Math.floor(Math.random() * 30000) + 1000,
        seller: {
          name: `賣家${i}`,
          avatar: `https://api.dicebear.com/7.x/avataaars/svg?seed=${i}`
        },
        postedDate: `${Math.floor(Math.random() * 30) + 1} 天前`,
        isFavorite: false,
        isNew: isNew
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

  applyFilters(): void {
    this.filteredProducts = this.allProducts.filter(product => {
      // 搜尋關鍵字
      if (this.searchKeyword) {
        const keyword = this.searchKeyword.toLowerCase();
        if (!product.name.toLowerCase().includes(keyword)) {
          return false;
        }
      }

      // 價格範圍
      if (this.minPrice !== null && product.price < this.minPrice) {
        return false;
      }
      if (this.maxPrice !== null && product.price > this.maxPrice) {
        return false;
      }

      // 商品狀況
      if (this.filterNew || this.filterUsed) {
        if (this.filterNew && !product.isNew) return false;
        if (this.filterUsed && product.isNew) return false;
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
    this.filterNew = false;
    this.filterUsed = false;
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
