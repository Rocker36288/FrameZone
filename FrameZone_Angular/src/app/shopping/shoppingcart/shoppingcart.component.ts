import { CommonModule } from '@angular/common';
import { Component, ElementRef, ViewChild } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HeaderComponent } from "../../shared/components/header/header.component";
import { CartItem, Coupon } from '../interfaces/cart';

// interface CartItem {
//   id: number;
//   name: string;
//   price: number;
//   quantity: number;
//   selected: boolean; // <-- 關鍵: 用來追蹤是否被勾選
//   // 您可以加入其他屬性，例如: quantity: number;
// }

// interface Coupon {
//   id: number;
//   name: string;
//   discount: number; // 折扣金額
//   code: string;
//   expiryDate: Date;
//   isSelected: boolean; // 是否被使用者選中
// }

@Component({
  selector: 'app-shoppingcart',
  imports: [FormsModule, CommonModule, ReactiveFormsModule, HeaderComponent],
  templateUrl: './shoppingcart.component.html',
  styleUrl: './shoppingcart.component.css'
})


export class ShoppingcartComponent {

  @ViewChild('couponModalElement') couponModalElement!: ElementRef;


  // 2. 屬性 (Property): 模擬購物車資料
  cartItems: CartItem[] = [
    { id: 1, name: '相機 A', price: 2500, quantity: 1, selected: false },
    { id: 2, name: '相機 B', price: 3500, quantity: 1, selected: false },
    { id: 3, name: '相機 C', price: 4500, quantity: 1, selected: false },
    { id: 4, name: '相機 D', price: 5500, quantity: 1, selected: false },
    { id: 5, name: '相機 E', price: 6500, quantity: 1, selected: false },
    { id: 6, name: '相機 F', price: 7500, quantity: 1, selected: false },

    // 這裡的資料會從 API 服務中獲取，但目前先以硬編碼模擬
  ];

  constructor() { }

  ngOnInit(): void {
    // 可以在這裡執行元件初始化時的邏輯，例如從服務中載入資料
  }

  // 3. 方法 (Method): 實現「賣家點擊 -> 所有商品連動」邏輯
  /**
   * 賣家勾選框被點擊時，連動所有商品
   * @param isChecked 賣家勾選框的狀態 (true/false)
   */
  toggleAllItems(isChecked: boolean): void {
    this.cartItems.forEach(item => {
      item.selected = isChecked;
    });
  }

  // 4. 方法 (Method): 實現「商品點擊 -> 賣家勾勾連動」邏輯
  /**
   * 當任一商品勾選框被點擊時，更新賣家勾選框的狀態。
   * * 備註：這個方法主要觸發 Angular 偵測，讓 areAllItemsChecked() 重新計算。
   */
  updateMasterCheckbox(): void {
    // 檢查邏輯實際上在 areAllItemsChecked() 內執行，這裡無需重複計算。
    // 呼叫此方法是為了讓 Angular 知道資料可能已變動，進而更新 HTML 模板。
    console.log('商品狀態已變動，觸發檢查主勾選框狀態。');
  }

  // 5. 方法 (Method): 用於主勾選框的 [checked] 屬性綁定
  /**
   * 判斷是否所有商品都被勾選。
   * * @returns boolean
   */
  areAllItemsChecked(): boolean {
    // 使用 Array.prototype.every() 檢查陣列中是否所有元素的 selected 屬性都是 true
    return this.cartItems.every(item => item.selected);
  }

  getTotalQuantity(): number {
    return this.cartItems.reduce((total, item) => {
      // 只有在商品被選取時才加入計算
      if (item.selected) {
        return total + item.quantity;
      }
      return total;
    }, 0);
  }

  getTotalAmount(): number {
    return this.cartItems.reduce((total, item) => {
      // 只有在商品被選取時才加入計算 (價格 * 數量)
      if (item.selected) {
        return total + (item.price * item.quantity);
      }
      return total;
    }, 0);
  }

  couponCodeInput: string = ''; // 用於綁定輸入框
  selectedDiscount: number = 0; // 儲存最終選定的折扣金額
  private tempFinalAppliedDiscount: number = 0;// 新增一個屬性來儲存 Modal 開啟前的 finalAppliedDiscount
  finalAppliedDiscount: number = 0;  // 儲存最終套用的折扣金額，這個值才真正影響購物車小計

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
    // 呼叫 getAppliedDiscount() 取得使用者目前在 Modal 中選定的折扣金額
    const currentDiscount = this.getAppliedDiscount();

    // 將計算出的折扣金額存入 finalAppliedDiscount
    this.finalAppliedDiscount = currentDiscount;

    // 備註：您不需要在這裡呼叫 getFinalTotal()。
    // Angular 的變動偵測會自動偵測到 this.finalAppliedDiscount 變了，
    // 進而重新執行所有綁定到它的函式（包括 getFinalTotal()），並更新畫面。
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

    // 由於狀態改變了，也需要手動觸發一次最終總計的計算，
    // 確保小計區塊立刻更新回取消前的狀態 (即原本的 finalAppliedDiscount)。
    // 如果 finalAppliedDiscount 是唯一參考，則不需要，但為了安全起見，建議呼叫。
    // 更好的做法是確保 getAppliedDiscount() 保持使用最新的 this.availableCoupons 狀態
  }
}
