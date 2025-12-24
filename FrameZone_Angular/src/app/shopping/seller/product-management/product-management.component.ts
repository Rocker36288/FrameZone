import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Product, ProductStats, ProductStatus, SortOrder } from '../../interfaces/products';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-product-management',
  standalone: true,
  imports: [FormsModule, CommonModule, RouterLink],
  templateUrl: './product-management.component.html',
  styleUrl: './product-management.component.css'
})
export class ProductManagementComponent {
  products: Product[] = [];
  filteredProducts: Product[] = [];
  paginatedProducts: Product[] = [];

  currentFilter: ProductStatus = 'all';
  searchQuery: string = '';
  sortOrder: SortOrder = 'newest';

  // 分頁設定：每頁 20 筆
  currentPage: number = 1;
  pageSize: number = 20;
  totalPages: number = 1;

  tabs = [
    { key: 'all', label: '全部' },
    { key: 'active', label: '上架中' },
    { key: 'inactive', label: '未上架' },
    { key: 'violation', label: '違規/刪除' },
    { key: 'pending', label: '審核中' }
  ];

  private routeSub?: Subscription;

  constructor(private route: ActivatedRoute) { }

  ngOnInit(): void {
    this.loadMockData();
    this.routeSub = this.route.params.subscribe(params => {
      const status = params['status'] || 'all';
      this.currentFilter = status as ProductStatus;
      this.currentPage = 1;
      this.applyFilters();
    });
  }

  ngOnDestroy(): void { this.routeSub?.unsubscribe(); }

  loadMockData(): void {
    // 生成 60 筆資料 (足以呈現 3 頁)
    const baseProducts = [
      { name: '自製旅遊明信片', price: 300, stock: 220, sales: 50, status: 'active', image: 'https://images.unsplash.com/photo-1506905925346-21bda4d32df4?w=400' },
      { name: '復古底片相機', price: 2000, stock: 210, sales: 1, status: 'active', image: 'https://images.unsplash.com/photo-1526170375885-4d8ecf77b99f?w=400' },
      { name: '違規測試商品', price: 999, stock: 0, sales: 5, status: 'violation', image: 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=400' }
    ];

    this.products = Array.from({ length: 60 }, (_, i) => ({
      ...baseProducts[i % 3],
      id: `${i + 1}`,
      sku: `SKU-${1000 + i}`,
      updatedAt: new Date(),
      createdAt: new Date()
    })) as Product[];
  }

  applyFilters(): void {
    let result = [...this.products];
    if (this.currentFilter !== 'all') {
      result = result.filter(p => p.status === this.currentFilter);
    }
    if (this.searchQuery.trim()) {
      const query = this.searchQuery.toLowerCase();
      result = result.filter(p => p.name.toLowerCase().includes(query));
    }
    this.filteredProducts = result;
    this.totalPages = Math.ceil(this.filteredProducts.length / this.pageSize) || 1;
    this.updatePagination();
  }

  updatePagination(): void {
    const startIndex = (this.currentPage - 1) * this.pageSize;
    this.paginatedProducts = this.filteredProducts.slice(startIndex, startIndex + this.pageSize);
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.updatePagination();
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }

  getPageNumbers(): number[] {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }

  getStatusText(status: string): string {
    const map: any = { active: '上架中', inactive: '未上架', violation: '違規', sold_out: '售罄' };
    return map[status] || status;
  }

  onSearchChange(): void { this.currentPage = 1; this.applyFilters(); }

  setPage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    this.updatePagination();
  }

  get pageNumbers(): number[] {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }


}
