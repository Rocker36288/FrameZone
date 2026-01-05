import { computed, effect, inject, Injectable, PLATFORM_ID, signal } from '@angular/core';
import { CartItem, Coupon } from '../../interfaces/cart';
import { isPlatformBrowser } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';
import { CartApiService } from './cart-api.service';

@Injectable({
  providedIn: 'root'
})
export class CartService {
  // 注入平台 ID，用來判斷現在是瀏覽器還是伺服器 (SSR 必備)
  private platformId = inject(PLATFORM_ID);
  private readonly STORAGE_KEY = 'shopping_cart_data';
  private readonly COUPON_KEY = 'selected_coupon_data';

  //用localStorage如果F5刷新購物車資料還是會存在
  // 1. 核心資料：初始化時從 localStorage 讀取
  private cartItemsSignal = signal<CartItem[]>(this.loadFromLocalStorage() as CartItem[]);

  // 選中的優惠券 Signal
  selectedCouponSignal = signal<Coupon | null>(this.loadCouponFromLocalStorage());

  // 核心資料：使用 Signal 儲存
  // private cartItemsSignal = signal<CartItem[]>([
  //   { id: 1, name: '相機 A', price: 2500, quantity: 1, selected: false },
  //   { id: 2, name: '相機 B', price: 3500, quantity: 1, selected: false },
  //   { id: 3, name: '相機 C', price: 4500, quantity: 1, selected: false },
  //   { id: 4, name: '相機 D', price: 5500, quantity: 1, selected: false },
  //   { id: 5, name: '相機 E', price: 6500, quantity: 1, selected: false },
  //   { id: 6, name: '相機 F', price: 7500, quantity: 1, selected: false },
  // ]);

  // 2. 供元件使用的唯讀 Signal
  items = this.cartItemsSignal.asReadonly();
  selectedCoupon = this.selectedCouponSignal.asReadonly();

  private authService = inject(AuthService);
  private cartApiService = inject(CartApiService);

  constructor() {
    // 3. 關鍵功能：自動存檔
    // 每當購物車與優惠券變動，自動同步到 localStorage
    effect(() => {
      if (isPlatformBrowser(this.platformId)) {
        // 存購物車
        localStorage.setItem(this.STORAGE_KEY, JSON.stringify(this.cartItemsSignal()));
        // 存優惠券
        const coupon = this.selectedCouponSignal();
        if (coupon) {
          localStorage.setItem(this.COUPON_KEY, JSON.stringify(coupon));
        } else {
          localStorage.removeItem(this.COUPON_KEY);
        }
      }
    });

    // 監聽登入狀態：若登入則嘗試從資料庫同步，若登出則清空購物車
    this.authService.currentUser$.subscribe({
      next: (user) => {
        if (user) {
          this.loadFromApi();
        } else {
          this.clearCart();
        }
      }
    });
  }

  /** 從 API 載入購物車 */
  loadFromApi() {
    this.cartApiService.getCart().subscribe({
      next: (apiItems: any[]) => {
        const mappedItems: CartItem[] = apiItems.map(item => ({
          id: item.productId,
          specificationId: item.specificationId,
          name: item.productName,
          price: item.price,
          quantity: item.quantity,
          selected: true, // 預設勾選
          imageUrl: item.productImage,
          sellerId: item.sellerId,
          sellerName: item.sellerName,
          sellerAvatar: item.sellerAvatar
        }));
        this.cartItemsSignal.set(mappedItems);
      },
      error: (err: any) => console.error('Failed to load cart from API', err)
    });
  }

  /** 從 localStorage 載入資料的私有方法 */
  private loadFromLocalStorage(): CartItem[] {
    if (isPlatformBrowser(this.platformId)) {
      const saved = localStorage.getItem(this.STORAGE_KEY);
      return saved ? (JSON.parse(saved) as CartItem[]) : [];
    }
    return [];
  }

  /** 從 localStorage 載入優惠券 */
  private loadCouponFromLocalStorage(): Coupon | null {
    if (isPlatformBrowser(this.platformId)) {
      const saved = localStorage.getItem(this.COUPON_KEY);
      return saved ? (JSON.parse(saved) as Coupon) : null;
    }
    return null;
  }

  // 核心計算邏輯 (搬移自 Component)
  // 計算總數量 (導覽列紅點使用)
  navCartCount = computed(() => {
    return this.cartItemsSignal().reduce((total, item) => total + item.quantity, 0);
  });

  // 購物車結帳頁面使用的「已勾選」總數量
  selectedTotalQuantity = computed(() => {
    return this.cartItemsSignal().reduce((total, item) =>
      item.selected ? total + item.quantity : total, 0);
  });

  // 購物車商品總額 (未扣折扣)
  totalAmount = computed(() => {
    return this.cartItemsSignal().reduce((total, item) =>
      item.selected ? total + (item.price * item.quantity) : total, 0);
  });

  // 計算目前套用的折扣金額
  appliedDiscount = computed(() => {
    const coupon = this.selectedCouponSignal();
    return coupon ? coupon.discount : 0;
  });

  /** 設定選用的優惠券 */
  applyCoupon(coupon: Coupon | null) {
    this.selectedCouponSignal.set(coupon);
  }

  /**
   * 加入購物車方法
   * @param product 傳入從詳細頁封裝好的商品物件
   */
  addToCart(product: CartItem) {
    this.cartItemsSignal.update(items => {
      const existing = items.find(i =>
        i.id === product.id && i.specificationId === product.specificationId
      );

      if (existing) {
        const newQuantity = existing.quantity + product.quantity;
        const specId = product.specificationId;
        if (this.authService.isAuthenticated() && specId !== undefined) {
          this.cartApiService.updateCartItem(specId, newQuantity).subscribe();
        }
        return items.map(i =>
          (i.id === product.id && i.specificationId === product.specificationId)
            ? { ...i, quantity: newQuantity }
            : i
        );
      }

      const specId = product.specificationId;
      if (this.authService.isAuthenticated() && specId !== undefined) {
        this.cartApiService.addToCart(specId, product.quantity).subscribe();
      }
      return [...items, { ...product, selected: true }];
    });
  }

  updateItemQuantity(id: number, quantity: number, specificationId?: number) {
    const finalQuantity = Math.max(1, quantity);
    this.cartItemsSignal.update(items =>
      items.map(i => (i.id === id && i.specificationId === specificationId)
        ? { ...i, quantity: finalQuantity }
        : i)
    );

    const sId = specificationId;
    if (this.authService.isAuthenticated() && sId !== undefined) {
      this.cartApiService.updateCartItem(sId, finalQuantity).subscribe();
    }
  }

  toggleItemSelection(id: number, specificationId?: number) {
    this.cartItemsSignal.update(items =>
      items.map(i => (i.id === id && i.specificationId === specificationId)
        ? { ...i, selected: !i.selected }
        : i)
    );
  }

  toggleAll(isChecked: boolean) {
    this.cartItemsSignal.update(items => items.map(i => ({ ...i, selected: isChecked })));
  }

  toggleSellerItems(sellerId: string | number, isChecked: boolean) {
    this.cartItemsSignal.update(items =>
      items.map(i => i.sellerId === sellerId ? { ...i, selected: isChecked } : i)
    );
  }

  removeFromCart(id: number, specificationId?: number) {
    this.cartItemsSignal.update(items =>
      items.filter(i => !(i.id === id && i.specificationId === specificationId))
    );

    const sId = specificationId;
    if (this.authService.isAuthenticated() && sId !== undefined) {
      this.cartApiService.removeFromCart(sId).subscribe();
    }
  }

  /** 清空購物車方法 (通常用於結帳完成後) */
  clearCart() {
    // 只有在登入狀態下才需要呼叫 API 清空
    if (this.authService.isAuthenticated()) {
      // 注意：這裏可能會有循環調用風險，如果 logout 觸發 clearCart 而 clearCart 檢查 isAuthenticated
      // 但 logout 會先把 user 設為 null，所以 isAuthenticated 會回傳 false。
      this.cartApiService.clearCart().subscribe();
    }
    // 清空商品
    this.cartItemsSignal.set([]);

    // 清空優惠券
    this.selectedCouponSignal.set(null);

    // 清空 localStorage (使用正確的 Key)
    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem(this.STORAGE_KEY);
      localStorage.removeItem(this.COUPON_KEY);
    }
  }

  orderCompletedSignal = signal(false);

  markOrderCompleted(): void {
    this.orderCompletedSignal.set(true);
  }

  resetOrderCompleted(): void {
    this.orderCompletedSignal.set(false);
  }
}
