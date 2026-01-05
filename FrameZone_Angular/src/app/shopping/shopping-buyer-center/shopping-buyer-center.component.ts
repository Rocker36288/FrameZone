import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { FooterComponent } from "../../shared/components/footer/footer.component";
import { FavoriteService, FavoriteItem } from '../shared/services/favorite.service';
import { CartService } from '../shared/services/cart.service';
import { ToastService } from '../shared/services/toast.service';
import { CartItem } from '../interfaces/cart';
import { ToastNotificationComponent } from '../shared/components/toast-notification/toast-notification.component';

@Component({
  selector: 'app-shopping-buyer-center',
  standalone: true,
  imports: [FormsModule, CommonModule, RouterLink, FooterComponent, ToastNotificationComponent],
  templateUrl: './shopping-buyer-center.component.html',
  styleUrl: './shopping-buyer-center.component.css'
})
export class ShoppingBuyerCenterComponent {
  activeMenu: string = 'orders';
  searchText: string = '';
  showModal: boolean = false;
  currentOrderTab: string = 'all';
  couponCodeInput: string = '';

  // 1. 用戶資料
  userProfile = {
    username: 'Angular_User_001',
    name: '陳小明',
    password: 'password123',
    email: 'angular@example.com',
    phone: '0912-345-678',
    gender: '男',
    birthday: '1995-01-01',
    avatar: 'https://i.pravatar.cc/150?u=angular'
  };

  tempProfile = { ...this.userProfile };

  // 側邊欄
  sidebarItems = [
    { id: 'profile', label: '基本資料', icon: 'ti ti-user' },
    { id: 'orders', label: '我的訂單', icon: 'ti ti-clipboard-list' },
    { id: 'address', label: '預設收件地址', icon: 'ti ti-map-pin' },
    { id: 'store', label: '預設取貨門市', icon: 'ti ti-building-store' },
    { id: 'bank', label: '銀行帳號設定', icon: 'ti ti-building-bank' },
    { id: 'favorite', label: '我的收藏', icon: 'ti ti-heart' },
    { id: 'coupons', label: '優惠券', icon: 'ti ti-ticket' },
    { id: 'notifications', label: '通知總覽', icon: 'ti ti-bell' }
  ];

  // 2. 訂單資料
  allOrders = [
    {
      id: 'ORD001',
      shopName: '好物精選賣場',
      status: 'ship',
      statusText: '待出貨',
      totalAmount: 190,
      products: [{ name: '質感陶瓷馬克杯', spec: '簡約白', price: 100, quantity: 1, imageUrl: 'https://placehold.co/80x80/6c5ce7/fff?text=Cup' }]
    },
    {
      id: 'ORD002',
      shopName: '3C 科技生活館',
      status: 'pay',
      statusText: '待付款',
      totalAmount: 1250,
      products: [{ name: '無線藍牙耳機', spec: '太空灰', price: 1250, quantity: 1, imageUrl: 'https://placehold.co/80x80/6c5ce7/fff?text=Audio' }]
    },
    {
      id: 'ORD003',
      shopName: '生活百貨',
      status: 'done',
      statusText: '已完成',
      totalAmount: 500,
      products: [{ name: '香氛蠟燭', spec: '薰衣草', price: 500, quantity: 1, imageUrl: 'https://placehold.co/80x80/a29bfe/fff?text=Candle' }]
    }
  ];

  // 3. 優惠券資料
  coupons = [
    { id: 1, title: '全館滿千折百', desc: '全站商品滿 $1000 即可折抵', expiry: '2025-12-31', code: 'HAPPY2025', used: false },
    { id: 2, title: '免運優惠券', desc: '限超商取貨使用 (7-11/全家)', expiry: '2025-06-30', code: 'FREESHIP', used: false },
    { id: 3, title: '新戶首購禮', desc: '首筆訂單現打 9 折', expiry: '2025-03-01', code: 'NEWUSER', used: true }
  ];

  // 通知資料
  notifications = [
    { title: '訂單出貨通知', content: '您的訂單 ORD001 已經安排出貨！', time: '10 分鐘前' },
    { title: '系統維護', content: '系統將於凌晨 2 點進行維護', time: '3 小時前' }
  ];

  // 收藏資料
  allFavorites: FavoriteItem[] = [
    { favoriteId: 1, productId: 1, name: '無線降噪耳機', price: 2990, imageUrl: 'https://placehold.co/400x300/6c5ce7/fff?text=Audio', date: '3 天前' },
    { favoriteId: 2, productId: 2, name: '人體工學辦公椅', price: 4500, imageUrl: 'https://placehold.co/400x300/a29bfe/fff?text=Chair', date: '5 天前' }
  ];

  displayOrders: any[] = [];
  displayFavorites: FavoriteItem[] = [];

  constructor(
    private favoriteService: FavoriteService,
    private cartService: CartService,
    private toastService: ToastService
  ) { }

  ngOnInit() {
    this.resetDisplay();
    this.loadFavorites();
  }

  loadFavorites() {
    this.favoriteService.getUserFavorites().subscribe({
      next: (data) => {
        this.allFavorites = data as any;
        this.displayFavorites = [...this.allFavorites];
      },
      error: (err) => {
        console.error('無法取得收藏資料：', err);
      }
    });
  }

  // 核心搜尋功能
  onSearch() {
    const term = this.searchText.trim().toLowerCase();

    if (this.activeMenu === 'orders') {
      this.displayOrders = this.allOrders.filter(o =>
        (this.currentOrderTab === 'all' || o.status === this.currentOrderTab) &&
        (o.shopName.toLowerCase().includes(term) || o.products.some(p => p.name.toLowerCase().includes(term)))
      );
    } else if (this.activeMenu === 'favorite') {
      this.displayFavorites = this.allFavorites.filter(f =>
        f.name.toLowerCase().includes(term)
      );
    }
  }

  resetDisplay() {
    this.filterOrders('all');
    this.displayFavorites = [...this.allFavorites];
  }

  filterOrders(tab: string) {
    this.currentOrderTab = tab;
    const term = this.searchText.trim().toLowerCase();
    const baseOrders = tab === 'all' ? this.allOrders : this.allOrders.filter(o => o.status === tab);

    // 過濾時同步考慮搜尋字串
    this.displayOrders = baseOrders.filter(o =>
      o.shopName.toLowerCase().includes(term) || o.products.some(p => p.name.toLowerCase().includes(term))
    );
  }

  onAvatarChange(event: any) {
    const file = event.target.files[0];
    if (file) {
      const reader = new FileReader();
      reader.onload = (e: any) => this.tempProfile.avatar = e.target.result;
      reader.readAsDataURL(file);
    }
  }

  openEditModal() {
    this.tempProfile = { ...this.userProfile };
    this.showModal = true;
  }

  saveProfile() {
    this.userProfile = { ...this.tempProfile };
    this.showModal = false;
    alert('基本資料已更新！');
  }

  cancelEdit() {
    if (confirm('確定取消嗎？未儲存的變更將遺失。')) this.showModal = false;
  }

  toggleFavorite(item: FavoriteItem) {
    if (confirm(`取消收藏「${item.name}」？`)) {
      this.favoriteService.toggleFavorite(item.productId).subscribe({
        next: () => {
          this.allFavorites = this.allFavorites.filter(f => f.favoriteId !== item.favoriteId);
          this.onSearch();
          this.toastService.show(`已從收藏移除：${item.name}`);
        },
        error: (err) => {
          console.error('取消收藏失敗：', err);
          this.toastService.show('取消操作失敗，請稍後再試');
        }
      });
    }
  }

  addToCart(item: FavoriteItem) {
    const cartItem: CartItem = {
      id: item.productId,
      name: item.name,
      price: item.price,
      quantity: 1,
      selected: true,
      imageUrl: item.imageUrl,
      sellerId: 0, // 收藏清單暫無賣家資訊，先給 0
      sellerName: '收藏商品'
    };
    this.cartService.addToCart(cartItem);
    this.toastService.show(`已將「${item.name}」加入購物車！`);
  }

  redeemCoupon() {
    if (this.couponCodeInput.trim()) {
      alert(`已領取優惠券：${this.couponCodeInput}`);
      this.couponCodeInput = '';
    }
  }
}
