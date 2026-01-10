import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { FooterComponent } from "../../shared/components/footer/footer.component";
import { FavoriteService, FavoriteItem } from '../shared/services/favorite.service';
import { CartService } from '../shared/services/cart.service';
import { ToastService } from '../shared/services/toast.service';
import { CartItem } from '../interfaces/cart';
import { ReviewModalComponent } from '../shared/components/review-modal/review-modal.component';
import { ToastNotificationComponent } from '../shared/components/toast-notification/toast-notification.component';
import { OrderService } from '../shared/services/order.service';
import { ShoppingUserService } from '../shared/services/shopping-user.service';
import { AddressService } from '../shared/services/address.service';
import { StoreService } from '../shared/services/store.service';
import { ReceivingAddress, CreateAddressDto } from '../interfaces/address';
import { PickupStore, CreatePickupStoreDto } from '../interfaces/store';

@Component({
  selector: 'app-shopping-buyer-center',
  standalone: true,
  imports: [FormsModule, CommonModule, RouterLink, FooterComponent, ToastNotificationComponent, ReviewModalComponent],
  templateUrl: './shopping-buyer-center.component.html',
  styleUrl: './shopping-buyer-center.component.css'
})
export class ShoppingBuyerCenterComponent {
  activeMenu: string = 'orders';
  searchText: string = '';
  showModal: boolean = false;
  showAddressModal: boolean = false;
  showStoreModal: boolean = false;
  isEditingAddress: boolean = false;
  isEditingStore: boolean = false;
  editingAddressId: number | null = null;
  editingStoreId: number | null = null;

  // 用於控制地址/門市清單中的下拉選單
  activeDropdownType: string | null = null;
  activeDropdownId: number | null = null;

  // 評價 Modal 控制
  showReviewModal: boolean = false;
  currentReviewOrder: any = null;

  currentOrderTab: string = 'all';
  couponCodeInput: string = '';

  // ... (rest of the file) ...

  openReviewModal(order: any) {
    this.currentReviewOrder = order;
    this.showReviewModal = true;
  }

  onReviewSubmitted() {
    this.showReviewModal = false;
    this.loadOrders(); // 重新載入訂單以更新評價狀態
  }

  // ... (original methods) ...

  // 1. 用戶資料 (預設空)
  userProfile = {
    username: '',
    name: '',
    password: '',
    email: '',
    phone: '',
    gender: '',
    birthday: '',
    avatar: 'https://ui-avatars.com/api/?name=U&background=667eea&color=fff&size=128'
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

  // 2. 訂單資料 (從後端獲取)
  allOrders: any[] = [];

  // allOrders = [
  //   {
  //     id: 'ORD001',
  //     shopName: '好物精選賣場',
  //     status: 'ship',
  //     statusText: '待出貨',
  //     totalAmount: 190,
  //     products: [{ name: '質感陶瓷馬克杯', spec: '簡約白', price: 100, quantity: 1, imageUrl: 'https://placehold.co/80x80/6c5ce7/fff?text=Cup' }]
  //   },
  //   {
  //     id: 'ORD002',
  //     shopName: '3C 科技生活館',
  //     status: 'pay',
  //     statusText: '待付款',
  //     totalAmount: 1250,
  //     products: [{ name: '無線藍牙耳機', spec: '太空灰', price: 1250, quantity: 1, imageUrl: 'https://placehold.co/80x80/6c5ce7/fff?text=Audio' }]
  //   },
  //   {
  //     id: 'ORD003',
  //     shopName: '生活百貨',
  //     status: 'done',
  //     statusText: '已完成',
  //     totalAmount: 500,
  //     products: [{ name: '香氛蠟燭', spec: '薰衣草', price: 500, quantity: 1, imageUrl: 'https://placehold.co/80x80/a29bfe/fff?text=Candle' }]
  //   }
  // ];

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
    // { favoriteId: 1, productId: 1, name: '無線降噪耳機', price: 2990, imageUrl: 'https://placehold.co/400x300/6c5ce7/fff?text=Audio', date: '3 天前' },
    // { favoriteId: 2, productId: 2, name: '人體工學辦公椅', price: 4500, imageUrl: 'https://placehold.co/400x300/a29bfe/fff?text=Chair', date: '5 天前' }
  ];

  displayOrders: any[] = [];
  displayFavorites: FavoriteItem[] = [];

  // 地址與門市
  userAddresses: ReceivingAddress[] = [];
  userStores: PickupStore[] = [];

  newAddress: CreateAddressDto = {
    recipientName: '',
    phoneNumber: '',
    fullAddress: '',
    isDefault: false
  };

  newStore: CreatePickupStoreDto = {
    recipientName: '',
    phoneNumber: '',
    convenienceStoreCode: '',
    convenienceStoreName: '',
    isDefault: false
  };

  constructor(
    private favoriteService: FavoriteService,
    private cartService: CartService,
    private toastService: ToastService,
    private orderService: OrderService,
    private shoppingUserService: ShoppingUserService,
    private addressService: AddressService,
    private storeService: StoreService,
    private route: ActivatedRoute
  ) { }

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      if (params['tab']) {
        this.activeMenu = params['tab'];
      }
    });

    this.loadProfile();
    this.loadOrders();
    this.loadFavorites();
    this.loadAddresses();
    this.loadStores();
  }

  loadProfile() {
    this.shoppingUserService.getUserProfile().subscribe({
      next: (profile: any) => {
        const name = profile.realName || profile.displayName || profile.account || 'U';
        this.userProfile = {
          username: profile.account,
          name: profile.realName || profile.displayName,
          password: '********',
          email: profile.email,
          phone: profile.phone,
          gender: profile.gender,
          birthday: profile.birthDate,
          avatar: profile.avatar
        };
        if (!profile.avatar) {
          const initial = name.charAt(0).toUpperCase();
          this.userProfile.avatar = `https://ui-avatars.com/api/?name=${encodeURIComponent(initial)}&background=667eea&color=fff&size=128`;
        }
        this.tempProfile = { ...this.userProfile };
      },
      error: (err: any) => {
        console.error('無法取得個人資料：', err);
      }
    });
  }

  loadOrders() {
    this.orderService.getMyOrders().subscribe({
      next: (orders) => {
        this.allOrders = orders;
        this.resetDisplay();
      },
      error: (err) => {
        console.error('無法取得訂單資料：', err);
      }
    });
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
    const term = (this.searchText || '').trim().toLowerCase();

    if (this.activeMenu === 'orders') {
      this.displayOrders = this.allOrders.filter(o => {
        const matchesTab = (this.currentOrderTab === 'all' || o.status === this.currentOrderTab);
        const shopMatch = (o.shopName || '').toLowerCase().includes(term);
        const productMatch = (o.products || []).some((p: any) => (p.name || '').toLowerCase().includes(term));
        return matchesTab && (shopMatch || productMatch);
      });
    } else if (this.activeMenu === 'favorite') {
      this.displayFavorites = this.allFavorites.filter(f =>
        (f.name || '').toLowerCase().includes(term)
      );
    }
  }

  resetDisplay() {
    this.filterOrders('all');
    this.displayFavorites = [...this.allFavorites];
  }

  filterOrders(tab: string) {
    this.currentOrderTab = tab;
    const term = (this.searchText || '').trim().toLowerCase();
    const baseOrders = tab === 'all' ? this.allOrders : this.allOrders.filter(o => o.status === tab);

    // 過濾時同步考慮搜尋字串
    this.displayOrders = baseOrders.filter(o => {
      const shopMatch = (o.shopName || '').toLowerCase().includes(term);
      const productMatch = (o.products || []).some((p: any) => (p.name || '').toLowerCase().includes(term));
      return shopMatch || productMatch;
    });
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
    // 轉換為後端需要的格式
    const updateDto = {
      email: this.tempProfile.email,
      phone: this.tempProfile.phone,
      displayName: this.tempProfile.name,
      avatar: this.tempProfile.avatar,
      realName: this.tempProfile.name, // 暫時一致
      gender: this.tempProfile.gender,
      birthDate: this.tempProfile.birthday // 這裡可能需要轉為 DateOnly 格式，但我們先試試字串
    };

    this.shoppingUserService.updateUserProfile(updateDto).subscribe({
      next: (res) => {
        if (res.success) {
          this.userProfile = { ...this.tempProfile };
          this.showModal = false;
          this.toastService.show('基本資料已更新！');
        } else {
          this.toastService.show(res.message || '更新失敗');
        }
      },
      error: (err) => {
        console.error('更新失敗：', err);
        this.toastService.show('連線錯誤，請稍後再試');
      }
    });
  }

  cancelEdit() {
    this.showModal = false;
    // 不再彈出確認視窗，減少干擾，或者直接關閉即可
  }

  // 地址處理邏輯擴展
  editAddress(addr: ReceivingAddress) {
    this.isEditingAddress = true;
    this.editingAddressId = addr.addressId;
    this.newAddress = {
      recipientName: addr.recipientName,
      phoneNumber: addr.phoneNumber,
      fullAddress: addr.fullAddress,
      isDefault: addr.isDefault
    };
    this.showAddressModal = true;
  }

  deleteAddress(addr: ReceivingAddress) {
    if (confirm(`確定要刪除「${addr.recipientName}」的地址嗎？`)) {
      this.addressService.deleteAddress(addr.addressId).subscribe({
        next: (res) => {
          if (res.success) {
            this.toastService.show('地址已刪除');
            this.loadAddresses();
          }
        }
      });
    }
  }

  setDefaultAddress(addr: ReceivingAddress) {
    this.addressService.setDefaultAddress(addr.addressId).subscribe({
      next: (res) => {
        if (res.success) {
          this.toastService.show('已設為預設地址');
          this.loadAddresses();
        }
      }
    });
  }

  saveAddress() {
    if (this.isEditingAddress && this.editingAddressId) {
      this.addressService.updateAddress(this.editingAddressId, this.newAddress).subscribe({
        next: (res) => {
          if (res.success) {
            this.toastService.show('地址更新成功');
            this.loadAddresses();
            this.showAddressModal = false;
          }
        }
      });
    } else {
      this.addressService.createAddress(this.newAddress).subscribe({
        next: (res) => {
          if (res.success) {
            this.toastService.show('地址新增成功');
            this.loadAddresses();
            this.showAddressModal = false;
          }
        }
      });
    }
  }

  // 門市處理邏輯擴展
  editStore(store: PickupStore) {
    this.isEditingStore = true;
    this.editingStoreId = store.convenienceStoreId;
    this.newStore = {
      recipientName: store.recipientName,
      phoneNumber: store.phoneNumber,
      convenienceStoreCode: store.convenienceStoreCode,
      convenienceStoreName: store.convenienceStoreName,
      isDefault: store.isDefault
    };
    this.showStoreModal = true;
  }

  deleteStore(store: PickupStore) {
    if (confirm(`確定要刪除門市「${store.convenienceStoreName}」嗎？`)) {
      this.storeService.deleteStore(store.convenienceStoreId).subscribe({
        next: (res) => {
          if (res.success) {
            this.toastService.show('門市已刪除');
            this.loadStores();
          }
        }
      });
    }
  }

  setDefaultStore(store: PickupStore) {
    this.storeService.setDefaultStore(store.convenienceStoreId).subscribe({
      next: (res) => {
        if (res.success) {
          this.toastService.show('已設為預設門市');
          this.loadStores();
        }
      }
    });
  }

  saveStore() {
    if (this.isEditingStore && this.editingStoreId) {
      this.storeService.updateStore(this.editingStoreId, this.newStore).subscribe({
        next: (res) => {
          if (res.success) {
            this.toastService.show('門市更新成功');
            this.loadStores();
            this.showStoreModal = false;
          }
        }
      });
    } else {
      this.storeService.createStore(this.newStore).subscribe({
        next: (res) => {
          if (res.success) {
            this.toastService.show('門市新增成功');
            this.loadStores();
            this.showStoreModal = false;
          }
        }
      });
    }
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
      sellerId: item.sellerId,
      sellerName: item.sellerName
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

  loadAddresses() {
    this.addressService.getUserAddresses().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.userAddresses = res.data;
        }
      },
      error: (err) => console.error('無法取得地址資料：', err)
    });
  }

  loadStores() {
    this.storeService.getUserStores().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.userStores = res.data;
        }
      },
      error: (err) => console.error('無法取得門市資料：', err)
    });
  }

  openAddressModal() {
    this.isEditingAddress = false;
    this.editingAddressId = null;
    this.newAddress = {
      recipientName: this.userProfile.name,
      phoneNumber: this.userProfile.phone,
      fullAddress: '',
      isDefault: this.userAddresses.length === 0
    };
    this.showAddressModal = true;
  }

  openStoreModal() {
    this.isEditingStore = false;
    this.editingStoreId = null;
    this.newStore = {
      recipientName: this.userProfile.name,
      phoneNumber: this.userProfile.phone,
      convenienceStoreCode: '',
      convenienceStoreName: '',
      isDefault: this.userStores.length === 0
    };
    this.showStoreModal = true;
  }

  toggleDropdown(type: string, id: number) {
    if (this.activeDropdownType === type && this.activeDropdownId === id) {
      this.activeDropdownType = null;
      this.activeDropdownId = null;
    } else {
      this.activeDropdownType = type;
      this.activeDropdownId = id;
    }
  }

  // 點擊空白處關閉選單 (如果在 HostListener 中實作會更完整，這裡先實作基本切換)
}
