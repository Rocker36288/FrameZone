import { computed, effect, inject, Injectable, PLATFORM_ID, signal } from '@angular/core';
import { CartItem, Coupon } from '../../interfaces/cart';
import { isPlatformBrowser } from '@angular/common';

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
  private cartItemsSignal = signal<CartItem[]>(this.loadFromLocalStorage());

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
  }

  /** 從 localStorage 載入資料的私有方法 */
  private loadFromLocalStorage(): CartItem[] {
    if (isPlatformBrowser(this.platformId)) {
      const saved = localStorage.getItem(this.STORAGE_KEY);
      return saved ? JSON.parse(saved) : [];
    }
    return [];
  }

  /** 從 localStorage 載入優惠券 */
  private loadCouponFromLocalStorage(): Coupon | null {
    if (isPlatformBrowser(this.platformId)) {
      const saved = localStorage.getItem(this.COUPON_KEY);
      return saved ? JSON.parse(saved) : null;
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

  /**
   * 加入購物車方法
   * @param product 傳入從詳細頁封裝好的商品物件
   */

  /** 設定選用的優惠券 */
  applyCoupon(coupon: Coupon | null) {
    this.selectedCouponSignal.set(coupon);
  }

  addToCart(product: CartItem) {
    this.cartItemsSignal.update(items => {
      const existing = items.find(i => i.id === product.id);
      if (existing) {
        // 加上 product.quantity 而不是固定的 1
        return items.map(i =>
          i.id === product.id
            ? { ...i, quantity: i.quantity + product.quantity }
            : i
        );
      }
      // 使用傳入的 quantity，並確保預設為勾選
      return [...items, { ...product, selected: true }];
    });
  }

  updateItemQuantity(id: number, quantity: number) {
    this.cartItemsSignal.update(items =>
      items.map(i => i.id === id ? { ...i, quantity: Math.max(1, quantity) } : i)
    );
  }

  toggleItemSelection(id: number) {
    this.cartItemsSignal.update(items =>
      items.map(i => i.id === id ? { ...i, selected: !i.selected } : i)
    );
  }

  toggleAll(isChecked: boolean) {
    this.cartItemsSignal.update(items => items.map(i => ({ ...i, selected: isChecked })));
  }

  removeFromCart(id: number) {
    this.cartItemsSignal.update(items => items.filter(i => i.id !== id));
  }

  /** 清空購物車方法 (通常用於結帳完成後) */
  clearCart() {
    // 清空商品
    this.cartItemsSignal.set([]);

    // 清空優惠券
    this.selectedCouponSignal.set(null);

    // 清空 localStorage
    localStorage.removeItem('cart_items');
    localStorage.removeItem('selected_coupon_data');
  }

  orderCompletedSignal = signal(false);

  markOrderCompleted(): void {
    this.orderCompletedSignal.set(true);
  }

  resetOrderCompleted(): void {
    this.orderCompletedSignal.set(false);
  }
}
