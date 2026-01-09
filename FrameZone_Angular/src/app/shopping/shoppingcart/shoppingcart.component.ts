import { CommonModule } from '@angular/common';
import { Component, computed, effect, ElementRef, inject, ViewChild } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HeaderComponent } from "../../shared/components/header/header.component";
import { CartItem, Coupon } from '../interfaces/cart';
import { CartService } from '../shared/services/cart.service';
import { AuthService } from '../../core/services/auth.service';
import { ToastService } from '../shared/services/toast.service';
import { Router, RouterLink } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-shoppingcart',
  standalone: true,
  imports: [FormsModule, CommonModule, ReactiveFormsModule, HeaderComponent, RouterLink],
  templateUrl: './shoppingcart.component.html',
  styleUrl: './shoppingcart.component.css'
})


export class ShoppingcartComponent {

  // 注入服務，設為 public 讓 HTML 可以直接使用 cartService.items() 等
  public cartService = inject(CartService);

  @ViewChild('couponModalElement') couponModalElement!: ElementRef;

  private authService = inject(AuthService);
  private toastService = inject(ToastService);
  private router = inject(Router);
  private destroy$ = new Subject<void>();

  currentStep: number = 1;

  // 2. 屬性(Property): 模擬購物車資料已移轉到 Service
  // 組件端透過 getter 或是直接在 HTML 使用 cartService.items()
  groupedItems = computed(() => {
    const items = this.cartService.items();
    const groups: { sellerId: string | number; sellerName: string; sellerAvatar?: string; items: CartItem[] }[] = [];

    items.forEach(item => {
      let group = groups.find(g => g.sellerId === item.sellerId);
      if (!group) {
        group = {
          sellerId: item.sellerId,
          sellerName: item.sellerName || '官方賣場',
          sellerAvatar: item.sellerAvatar || `https://ui-avatars.com/api/?name=${encodeURIComponent((item.sellerName || 'S').charAt(0).toUpperCase())}&background=667eea&color=fff&size=128`,
          items: []
        };
        groups.push(group);
      }
      group.items.push(item);
    });

    return groups;
  });

  constructor() {
    effect(() => {
      const appliedCoupon = this.cartService.selectedCoupon();

      if (!appliedCoupon) {
        // 沒使用優惠券
        this.availableCoupons.forEach(c => c.isSelected = false);
        this.finalAppliedDiscount = 0;
        return;
      }

      // 有使用優惠券 → 對齊 UI
      this.availableCoupons.forEach(coupon => {
        coupon.isSelected = coupon.id === appliedCoupon.id;
      });

      this.finalAppliedDiscount = appliedCoupon.discount;
    });
  }

  ngOnInit(): void {
    if (!this.authService.isAuthenticated()) {
      this.toastService.show('請先登入會員', 'top');
      setTimeout(() => {
        this.router.navigate(['/login'], { queryParams: { returnUrl: '/shoppingcart' } });
      }, 1000);
      return;
    }
    // 可以在這裡執行元件初始化時的邏輯，例如從服務中載入資料

    // 從 CartService 讀取「已套用的優惠券」
    const appliedCoupon = this.cartService.selectedCoupon();

    if (!appliedCoupon) {
      // 沒有使用優惠券 → 全部取消勾選
      this.availableCoupons.forEach(c => c.isSelected = false);
      this.finalAppliedDiscount = 0;
    }

    // 訂閱會員資料
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        if (user) {
          this.memberName = user.account || user.displayName || '會員';
          this.memberAvatarUrl = user.avatar || `https://ui-avatars.com/api/?name=${encodeURIComponent(this.memberName.charAt(0).toUpperCase())}&background=667eea&color=fff&size=128`;
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * 某個賣家的勾選框被點擊時，連動該賣家的所有商品
   */
  toggleSellerItems(sellerId: string | number, isChecked: boolean): void {
    this.cartService.toggleSellerItems(sellerId, isChecked);
  }

  /**
   * 賣家勾選框被點擊時，連動所有商品
   * @param isChecked 賣家勾選框的狀態 (true/false)
   */
  toggleAllItems(isChecked: boolean): void {
    this.cartService.toggleAll(isChecked);
  }

  // 4. 方法 (Method): 實現「商品點擊 -> 賣家勾勾連動」邏輯
  /**
   * 當任一商品勾選框被點擊時，更新賣家勾選框的狀態。
   * * 備註：這個方法主要觸發 Angular 偵測，讓 areAllItemsChecked() 重新計算。
   */
  updateMasterCheckbox(): void {
    // 檢查邏輯實際上在 areAllItemsChecked() 內執行，這裡無需重複計算。
    console.log('商品狀態已變動，觸發檢查主勾選框狀態。');
  }

  /**
   * 判斷某個賣家的所有商品是否都被勾選
   */
  isSellerChecked(sellerId: string | number): boolean {
    const sellerItems = this.cartService.items().filter(i => i.sellerId === sellerId);
    return sellerItems.length > 0 && sellerItems.every(i => i.selected);
  }

  /**
   * 判斷是否所有商品都被勾選。
   * * @returns boolean
   */
  areAllItemsChecked(): boolean {
    const items = this.cartService.items();
    return items.length > 0 && items.every(item => item.selected);
  }

  getTotalQuantity(): number {
    return this.cartService.selectedTotalQuantity();
  }

  getTotalAmount(): number {
    return this.cartService.totalAmount();
  }

  couponCodeInput: string = ''; // 用於綁定輸入框
  selectedDiscount: number = 0; // 儲存最終選定的折扣金額
  private tempFinalAppliedDiscount: number = 0;// 新增一個屬性來儲存 Modal 開啟前的 finalAppliedDiscount
  finalAppliedDiscount: number = 0;   // 儲存最終套用的折扣金額，這個值才真正影響購物車小計

  availableCoupons: Coupon[] = [
    { id: 1, name: '運費抵用券', discount: 60, code: 'FREESHIP', expiryDate: new Date('2025/12/31'), isSelected: false },
    { id: 2, name: '滿千折百券', discount: 100, code: 'SAVE100', expiryDate: new Date('2025/11/30'), isSelected: false },
    // ... 更多優惠券
  ];

  // 處理點擊優惠券列表中的勾選框
  selectCoupon(clickedCoupon: Coupon): void {
    // 實現單選邏輯：只允許勾選一個優惠券
    this.availableCoupons.forEach(coupon => {
      if (coupon.id === clickedCoupon.id) {
        coupon.isSelected = !coupon.isSelected; // 切換自身狀態
      } else {
        coupon.isSelected = false; // 取消其他優惠券的選取
      }
    });
  }

  // 處理手動輸入代碼 (這裡僅為範例，實際邏輯需連線後端驗證)
  applyCouponCode(): void {
    const enteredCode = this.couponCodeInput.trim().toUpperCase();
    const foundCoupon = this.availableCoupons.find(c => c.code === enteredCode);

    if (foundCoupon) {
      this.selectCoupon(foundCoupon); // 如果代碼匹配，則視為選中該優惠券
      alert(`折扣碼 ${enteredCode} 已選取!`);
    } else {
      alert(`折扣碼 ${enteredCode} 無效或不可用。`);
    }
  }

  // 計算最終總折扣
  getAppliedDiscount(): number {
    const selectedCoupon = this.availableCoupons.find(c => c.isSelected);
    // 如果有選中的優惠券，則使用其折扣金額，否則為 0
    return selectedCoupon ? selectedCoupon.discount : 0;
  }

  // 計算最終小計
  getFinalTotal(): number {
    const totalAmount = this.getTotalAmount(); // 購物車商品總額

    // 使用已確認的最終折扣
    const discount = this.finalAppliedDiscount;

    // 確保最終金額不為負數
    return Math.max(0, totalAmount - discount);
  }

  // 更新小計區域的優惠券金額
  getDiscountDisplay(): string {
    const discount = this.finalAppliedDiscount;
    return discount > 0 ? `-$${discount}` : '選擇/輸入折扣碼';
  }

  calculateFinalTotal(): void {
    const selectedCoupon =
      this.availableCoupons.find(c => c.isSelected) || null;

    // 關鍵：寫回 CartService
    this.cartService.applyCoupon(selectedCoupon);

    // 本地只做顯示用（可留可不留）
    this.finalAppliedDiscount = selectedCoupon ? selectedCoupon.discount : 0;
  }

  private tempCouponState: Coupon[] = [];

  /** * 在 Modal 開啟前呼叫，儲存當前的優惠券選取狀態。*/
  saveCouponState(): void {
    // 深層複製當前狀態，避免直接引用
    this.tempCouponState = this.availableCoupons.map(coupon => ({ ...coupon }));

    // ***儲存當前已套用的最終折扣 ***
    this.tempFinalAppliedDiscount = this.finalAppliedDiscount;
  }

  /*** 在 Modal 被取消時呼叫，還原優惠券選取狀態。*/
  restoreCouponState(): void {
    // 將暫存的狀態寫回實際的 availableCoupons 陣列
    this.availableCoupons = this.tempCouponState.map(coupon => ({ ...coupon }));

    // *** 還原最終套用的折扣金額 ***
    this.finalAppliedDiscount = this.tempFinalAppliedDiscount;
  }

  /** 檢查是否有任何優惠券被選中 (供 HTML 按鈕判斷用) */
  anyCouponSelected(): boolean {
    return this.availableCoupons.some(coupon => coupon.isSelected);
  }

  // 清空優惠券
  clearCouponSelection(): void {
    this.availableCoupons.forEach(c => c.isSelected = false);
  }

  // 範例頭像 URL，請替換為實際的會員服務獲取邏輯
  memberAvatarUrl: string = '';

  // 範例會員名稱
  memberName: string = '';

}
