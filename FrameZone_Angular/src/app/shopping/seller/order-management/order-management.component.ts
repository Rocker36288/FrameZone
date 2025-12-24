import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Subscription } from 'rxjs';

interface OrderItem {
  name: string;
  image: string;
  spec: string;
  price: number;
  quantity: number;
}

interface Order {
  id: string;
  orderNumber: string;
  customer: string;
  items: OrderItem[];
  totalAmount: number;
  status: 'unpaid' | 'pending' | 'shipping' | 'completed' | 'cancelled';
  orderDate: Date;
}

// 定義統一的狀態類型
type OrderStatusFilter = 'all' | 'unpaid' | 'pending' | 'shipping' | 'completed' | 'cancelled';

@Component({
  selector: 'app-order-management',
  standalone: true,
  imports: [FormsModule, CommonModule, RouterLink],
  templateUrl: './order-management.component.html',
  styleUrl: './order-management.component.css'
})
export class OrderManagementComponent {

  orders: Order[] = [];
  filteredOrders: Order[] = [];
  paginatedOrders: Order[] = [];

  searchQuery: string = '';
  currentFilter: string = 'all';
  currentPage: number = 1;
  pageSize: number = 5; // 每頁 5 筆
  totalPages: number = 1;

  showShipModal: boolean = false;
  selectedOrderForShip: Order | null = null;
  shippingNumber: string = '';

  tabs = [
    { key: 'all', label: '全部' },
    { key: 'unpaid', label: '尚未付款' },
    { key: 'pending', label: '待出貨' },
    { key: 'shipping', label: '運送中' },
    { key: 'completed', label: '已完成' },
    { key: 'cancelled', label: '不成立' }
  ];

  private routeSub: Subscription | undefined;

  constructor(private route: ActivatedRoute) { } // 注入路由服務

  ngOnInit(): void {
    this.loadMockData();

    //監聽路由參數變化
    this.routeSub = this.route.params.subscribe(params => {
      const statusFromRoute = params['status'];
      if (statusFromRoute) {
        // 將網址參數同步到目前的過濾狀態
        this.currentFilter = statusFromRoute;
        this.applyFilters(true);
      }
    });
  }

  ngOnDestroy(): void {
    if (this.routeSub) {
      this.routeSub.unsubscribe();
    }
  }

  loadMockData(): void {
    const data: Order[] = [];

    // --- 第一頁 (ID 1-5): 混合狀態 ---
    for (let i = 1; i <= 5; i++) {
      data.push({
        id: i.toString(),
        orderNumber: `ORD-2025122200${i}`,
        customer: `買家 ${i}`,
        status: i === 1 ? 'completed' : 'unpaid',
        totalAmount: 1000 * i,
        orderDate: new Date(),
        items: [{ name: '商品 A', image: `https://picsum.photos/id/${i}/200/200`, spec: '預設', price: 1000 * i, quantity: 1 }]
      });
    }

    // --- 第二頁 (ID 6-10): 集中「安排出貨」(待出貨) 訂單 ---
    for (let i = 6; i <= 10; i++) {
      data.push({
        id: i.toString(),
        orderNumber: `ORD-202512220${i.toString().padStart(2, '0')}`,
        customer: `待出貨買家 ${i}`,
        status: 'pending', // 全部設定為待出貨
        totalAmount: 2500 * i,
        orderDate: new Date(),
        items: [
          { name: '多品項商品 A', image: `https://picsum.photos/id/${10 + i}/200/200`, spec: '藍色', price: 1500 * i, quantity: 1 },
          { name: '多品項商品 B', image: `https://picsum.photos/id/${20 + i}/200/200`, spec: '配件', price: 1000, quantity: 1 }
        ]
      });
    }

    // --- 第三頁 (ID 11-15): 混合狀態 ---
    for (let i = 11; i <= 15; i++) {
      data.push({
        id: i.toString(),
        orderNumber: `ORD-202512220${i}`,
        customer: `買家 ${i}`,
        status: 'cancelled',
        totalAmount: 300 * i,
        orderDate: new Date(),
        items: [{ name: '歷史商品', image: `https://picsum.photos/id/${30 + i}/200/200`, spec: '無', price: 300 * i, quantity: 1 }]
      });
    }

    this.orders = data;
    this.applyFilters();
  }

  applyFilters(resetPage: boolean = true): void {
    let result = [...this.orders];
    if (this.currentFilter !== 'all') {
      result = result.filter(o => o.status === this.currentFilter);
    }
    if (this.searchQuery.trim()) {
      const q = this.searchQuery.toLowerCase();
      result = result.filter(o => o.orderNumber.toLowerCase().includes(q) || o.customer.toLowerCase().includes(q));
    }
    this.filteredOrders = result;
    if (resetPage) this.currentPage = 1;
    this.updatePagination();
  }

  updatePagination(): void {
    this.totalPages = Math.ceil(this.filteredOrders.length / this.pageSize) || 1;
    const start = (this.currentPage - 1) * this.pageSize;
    this.paginatedOrders = this.filteredOrders.slice(start, start + this.pageSize);
  }

  onFilterChange(status: string): void {
    this.currentFilter = status;
    this.applyFilters(true);
  }

  setPage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    this.updatePagination();
  }

  openShipModal(order: Order): void {
    this.selectedOrderForShip = order;
    this.shippingNumber = '';
    this.showShipModal = true;
  }

  closeShipModal(): void { this.showShipModal = false; }

  confirmShip(): void {
    if (!this.shippingNumber) return;
    if (this.selectedOrderForShip) {
      const o = this.orders.find(item => item.id === this.selectedOrderForShip?.id);
      if (o) o.status = 'shipping';
      this.closeShipModal();
      this.applyFilters(false); // 保持在當前頁面
    }
  }

  getStatusText(status: string): string {
    const map: any = { unpaid: '尚未付款', pending: '待出貨', shipping: '運送中', completed: '已完成', cancelled: '不成立' };
    return map[status] || status;
  }

  get pageNumbers(): number[] {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }
}
