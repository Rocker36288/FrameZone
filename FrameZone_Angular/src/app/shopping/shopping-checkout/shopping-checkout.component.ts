import { HeaderComponent } from './../../shared/components/header/header.component';
import { Component, computed, ElementRef, inject, ViewChild } from '@angular/core';
import { CartItem, Coupon } from '../interfaces/cart';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { CartService } from '../shared/services/cart.service';
import { OrderService } from '../shared/services/order.service';
import { EcpayFormComponent } from '../shared/components/ecpay-form/ecpay-form.component';
import { frontendPublicUrl, backendPublicUrl } from '../shared/configuration/url';
import { AuthService } from '../../core/services/auth.service';
import { ToastService } from '../shared/services/toast.service';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-shopping-checkout',
  standalone: true,
  imports: [FormsModule, CommonModule, ReactiveFormsModule, HeaderComponent, RouterLink, EcpayFormComponent],
  templateUrl: './shopping-checkout.component.html',
  styleUrl: './shopping-checkout.component.css'
})
export class ShoppingCheckoutComponent {
  // 在 constructor 注入
  constructor(
    private fb: FormBuilder,
    private router: Router,
    public cartService: CartService, // 改為 public 方便 HTML 直接讀取 Service 的 Signal
    private orderService: OrderService,
    private authService: AuthService,
    private toastService: ToastService
  ) { }

  private destroy$ = new Subject<void>();


  currentStep: number = 2;

  @ViewChild('couponModalElement') couponModalElement!: ElementRef;

  // 定義表單模型
  checkoutForm!: FormGroup;

  // 定義運費對照表
  shippingRates: { [key: string]: number } = {
    'post': 65,
    '711': 60,
    '711_pay': 60,
    'fami': 60,
    'fami_pay': 60
  };

  // 定義收件人資料陣列
  savedAddresses = [
    {
      id: 1,
      name: '王小明',
      phone: '0912-345-678',
      address: '110 台北市信義區信義路五段7號 (台北 101)'
    },
    {
      id: 2,
      name: '家裡 (李大華)',
      phone: '0988-123-456',
      address: '220 新北市板橋區中山路一段1號'
    }
  ];

  // 使用 Signal 的 Computed 屬性：從 Service 篩選出「已勾選」商品並進行分組
  groupedSelectedItems = computed(() => {
    const items = this.cartService.items().filter(item => item.selected);
    const groups: { sellerId: string | number; sellerName: string; sellerAvatar?: string; items: CartItem[] }[] = [];

    items.forEach(item => {
      let group = groups.find(g => g.sellerId === item.sellerId);
      if (!group) {
        group = {
          sellerId: item.sellerId,
          sellerName: item.sellerName || '官方賣場',
          sellerAvatar: item.sellerAvatar || `https://i.pravatar.cc/150?u=${item.sellerId}`,
          items: []
        };
        groups.push(group);
      }
      group.items.push(item);
    });

    return groups;
  });

  // 2. 屬性 (Property): 模擬購物車資料
  // cartItems: CartItem[] = [
  //   { id: 1, name: '相機 A', price: 2500, quantity: 1, selected: false },
  //   { id: 2, name: '相機 B', price: 3500, quantity: 1, selected: false },
  //   { id: 3, name: '相機 C', price: 4500, quantity: 1, selected: false },
  //   { id: 4, name: '相機 D', price: 5500, quantity: 1, selected: false },
  //   { id: 5, name: '相機 E', price: 6500, quantity: 1, selected: false },
  //   { id: 6, name: '相機 F', price: 7500, quantity: 1, selected: false },

  //   // 這裡的資料會從 API 服務中獲取，但目前先以硬編碼模擬
  // ];

  // --- 優惠券相關屬性 ---
  couponCodeInput: string = '';
  selectedDiscount: number = 0;
  private tempFinalAppliedDiscount: number = 0;
  finalAppliedDiscount: number = 0;
  private tempCouponState: Coupon[] = [];

  availableCoupons: Coupon[] = [
    { id: 1, name: '運費抵用券', discount: 60, code: 'FREESHIP', expiryDate: new Date('2025/12/31'), isSelected: false },
    { id: 2, name: '滿千折百券', discount: 100, code: 'SAVE100', expiryDate: new Date('2025/11/30'), isSelected: false },
  ];

  // 會員資料
  memberAvatarUrl: string = '';
  memberName: string = '';


  // 使用綠界 API 需要的相關參數
  ecpayParams: any = null;

  ngOnInit(): void {
    if (!this.authService.isAuthenticated()) {
      this.toastService.show('請先登入會員', 'top');
      setTimeout(() => {
        this.router.navigate(['/login'], { queryParams: { returnUrl: '/checkout' } });
      }, 1000);
      return;
    }

    // 1. 初始化表單
    this.checkoutForm = this.fb.group({
      paymentMethod: ['Credit', Validators.required],
      shippingMethod: ['standard', Validators.required],
      selectedAddressId: ['', Validators.required] // 必選收件人
    });

    // 2. 從 Service 同步優惠券狀態
    // 這樣進入頁面時，finalAppliedDiscount 就不會是 0
    const savedCoupon = this.cartService.selectedCoupon();
    if (savedCoupon) {
      this.finalAppliedDiscount = savedCoupon.discount;
    }
    // 監聽運送方式變動：切換運送方式時，清空已選收件人，強迫重新選擇
    this.checkoutForm.get('shippingMethod')?.valueChanges.subscribe(() => {
      this.checkoutForm.get('selectedAddressId')?.reset('');
    });

    // 安全檢查：若無勾選商品，退回購物車
    if (this.groupedSelectedItems().length === 0) {
      this.router.navigate(['/shoppingcart']);
    }

    // 訂閱會員資料
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        if (user) {
          this.memberName = user.account || user.displayName || '會員';
          if (user.avatar) {
            this.memberAvatarUrl = user.avatar;
          } else {
            const initial = (this.memberName || 'U').charAt(0).toUpperCase();
            this.memberAvatarUrl = `https://ui-avatars.com/api/?name=${encodeURIComponent(initial)}&background=667eea&color=fff&size=128`;
          }
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // 取得目前選取的運費金額
  getShippingFee(): number {
    const method = this.checkoutForm?.get('shippingMethod')?.value;
    return this.shippingRates[method] || 0;
  }

  /** 取得商品小計 (從 Service 直接獲取已選商品總額) */
  getTotalAmount(): number {
    return this.cartService.totalAmount();
  }

  /** 最終應付金額計算 (商品總額 - 折扣 + 運費) */
  getFinalTotal(): number {
    // 直接從 Service 讀取折扣金額，最保險
    const discount = this.cartService.appliedDiscount();
    const totalAmount = this.getTotalAmount();
    const shipping = this.getShippingFee();

    return Math.max(0, totalAmount - discount + shipping);
  }

  selectCoupon(clickedCoupon: Coupon): void {
    this.availableCoupons.forEach(coupon => {
      if (coupon.id === clickedCoupon.id) {
        coupon.isSelected = !coupon.isSelected;
      } else {
        coupon.isSelected = false;
      }
    });
  }

  applyCouponCode(): void {
    const enteredCode = this.couponCodeInput.trim().toUpperCase();
    const foundCoupon = this.availableCoupons.find(c => c.code === enteredCode);

    if (foundCoupon) {
      this.selectCoupon(foundCoupon);
      alert(`折扣碼 ${enteredCode} 已選取!`);
    } else {
      alert(`折扣碼 ${enteredCode} 無效或不可用。`);
    }
  }

  getAppliedDiscount(): number {
    const selectedCoupon = this.availableCoupons.find(c => c.isSelected);
    return selectedCoupon ? selectedCoupon.discount : 0;
  }

  getDiscountDisplay(): string {
    const coupon = this.cartService.selectedCoupon();
    // 如果有優惠券，顯示折扣金額；沒有則顯示預設文字
    return coupon ? `-$${coupon.discount}` : '未套用優惠券';
  }

  // 當 Modal 點擊「確定」時呼叫的方法
  calculateFinalTotal(): void {
    const selectedCoupon = this.availableCoupons.find(c => c.isSelected) || null;
    this.cartService.applyCoupon(selectedCoupon);
    // 這裡也要同步更新本地變數，讓 getDiscountDisplay() 能正確顯示
    this.finalAppliedDiscount = selectedCoupon ? selectedCoupon.discount : 0;
  }

  saveCouponState(): void {
    this.tempCouponState = this.availableCoupons.map(coupon => ({ ...coupon }));
    this.tempFinalAppliedDiscount = this.finalAppliedDiscount;
  }

  restoreCouponState(): void {
    this.availableCoupons = this.tempCouponState.map(coupon => ({ ...coupon }));
    this.finalAppliedDiscount = this.tempFinalAppliedDiscount;
  }


  //取得優惠券顯示
  discountInfo = computed(() => {
    const coupon = this.cartService.selectedCoupon();
    return coupon ? `${coupon.code} (-$${coupon.discount})` : '選擇折扣碼';
  });

  /** 結帳完成（模擬訂單成功） */
  onConfirmCheckout(): void {
    if (this.checkoutForm.invalid) {
      return;
    }

    // 呼叫 API 建立訂單
    var orderItems = this.cartService.items().filter((item) => item.selected).map((item) => {
      return {
        id: item.id,
        name: item.name,
        price: item.price,
        quantity: item.quantity
      }
    });

    var clientBackURL = "";
    if (this.checkoutForm.value.paymentMethod == "Credit") {
      clientBackURL = `${frontendPublicUrl}/shopping/home`;
    }
    else {
      clientBackURL = `${frontendPublicUrl}/order-success`;
    }

    this.orderService.createOrder({
      orderItems: orderItems,
      totalAmount: this.getFinalTotal(),
      paymentMethod: this.checkoutForm.value.paymentMethod,
      returnURL: `${backendPublicUrl}/api/order/pay-result`,
      optionParams: {
        ClientBackURL: clientBackURL,
        OrderResultURL: `${backendPublicUrl}/api/order/success-redirect`
      }
    }).subscribe({
      next: (res) => {
        console.log("Success", res);

        // 訂單完成：清空購物車 + 優惠券
        this.cartService.markOrderCompleted();

        // 用參數填到表單並用綠界 API 發送訂單
        this.ecpayParams = res;
      },
      error: (err) => {
        console.log("Error", err);
      }
    });

    // 前往成功頁
    //this.router.navigate(['/order-success']);
  }

}
