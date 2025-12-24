import { CommonModule } from '@angular/common';
import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ShoppinghomeComponent } from "../shoppinghome/shoppinghome.component";
import { FooterComponent } from "../../shared/components/footer/footer.component";
import { ToastNotificationComponent } from "../shared/components/toast-notification/toast-notification.component";
import { FavoriteButtonComponent } from '../shared/components/favorite-button/favorite-button.component';
import { ToastService } from '../shared/services/toast.service';
import { CartService } from '../shared/services/cart.service';

interface ProductDetail {
  id: number;
  name: string;
  price: number;
  img: string;
  desc: string;
  isFavorite: boolean;
}

@Component({
  selector: 'app-shopping-product-detail',
  standalone: true,
  imports: [FormsModule, CommonModule, ShoppinghomeComponent, RouterLink, FooterComponent, ToastNotificationComponent, FavoriteButtonComponent],
  templateUrl: './shopping-product-detail.component.html',
  styleUrl: './shopping-product-detail.component.css'
})


export class ShoppingProductDetailComponent implements OnInit {

  // 新增屬性來儲存商品 ID
  productId: string | null = null;
  currentProductDetail: ProductDetail | null = null;

  @ViewChild('thumbnailScrollContainer', { static: true })
  thumbnailScrollContainer!: ElementRef;

  // 注入 ActivatedRoute 服務
  constructor(
    private route: ActivatedRoute,
    private toastService: ToastService,
    private cartService: CartService
  ) { }

  // 實作 ngOnInit 函式，用於在元件初始化時執行邏輯
  ngOnInit(): void {
    // 使用 paramMap 訂閱路由參數的變動 (最佳做法)
    this.route.paramMap.subscribe(params => {
      // 取得路由中名為 'productId' 的參數。
      // 注意：'productId' 必須與 app.routes.ts 中設定的 :productId 一致。
      this.productId = params.get('productId');

      if (this.productId) {
        console.log('取得的商品 ID:', this.productId);
        // *** 實際應用中：您會在這裡呼叫您的服務來根據 this.productId 載入商品詳細資訊 ***
        // this.loadProductDetails(this.productId);

        this.loadProductDetails(this.productId);

        window.scrollTo({ top: 0, behavior: 'smooth' });
      }
    });
  }

  // 從 similarProducts 列表中查找商品數據
  loadProductDetails(id: string): void {
    const targetId = parseInt(id);

    const foundProduct = this.similarProducts.find(item => item.id === targetId);

    if (foundProduct) {
      // 檢查 foundProduct 存在性後才賦值
      // 因為此時 foundProduct 的類型是 ProductDetail (非 undefined)，可以直接賦值。
      this.currentProductDetail = foundProduct;

      // 確保其他綁定的屬性使用 foundProduct 的屬性
      this.product.name = foundProduct.name;
      this.product.price = foundProduct.price;
      this.currentImage = foundProduct.img;
      this.product.isFavorite = foundProduct.isFavorite;

      console.log(`成功載入商品 ID: ${targetId}，名稱: ${foundProduct.name}`);
    } else {
      // 處理找不到商品的情況
      // 如果找不到，將 currentProductDetail 設為 null (符合其類型 ProductDetail | null)
      this.currentProductDetail = null;

      this.product.name = `商品載入失敗 (ID: ${targetId})`;
      this.product.price = 0;
      console.error(`找不到 ID 為 ${targetId} 的商品。`);
    }
  }

  // --- 基礎商品資訊 ---
  product = {
    name: '相機',
    price: 1580,
    isFavorite: false
  };
  quantity: number = 1;  // 確保 HTML 的數量輸入框綁定到這個變數
  selectedSpec: string = '精裝版';
  specs: string[] = ['精裝版', '電子書', '平裝版'];

  // 賣家資訊 (保持不變)
  sellerInfo = {
    account: 'ruka0711',
    avatar: 'seller-avatar.jpg', // 賣家頭像佔位符
    onlineStatus: '2小時前上線'
  };

  // 付款方式資料 (保持不變)
  paymentOptions = [
    { label: '信用卡 (VISA/Master/JCB)' },
    { label: '7-11取貨付款' },
    { label: '全家取貨付款' },
    { label: '銀行或郵局轉帳' },
    { label: '郵局無摺存款' },
  ];

  // **修正：運費選項 (根據您的最新要求)**
  shippingOptions = [
    { label: '7-11取貨', fee: 60, isCombined: false, note: '' },
    { label: '全家取貨', fee: 60, isCombined: false, note: '' },
    { label: '7-11取貨付款', fee: 70, isCombined: false, note: '' },
    { label: '全家取貨付款', fee: 70, isCombined: false, note: '' },
    { label: '郵寄', fee: 65, isCombined: false, note: '' },
    // 模擬合併運費選項
    { label: '合併運費', fee: 60, isCombined: true, note: '（適用兩件以上）' },
  ];

  availableCoupons = [
    { name: '滿千折百', discount: 100, isClaimed: true },
    { name: '新客折五十', discount: 50, isClaimed: false },
  ];
  allImages: string[] = ['images/products/1.jpg', 'images/products/2.jpg', 'images/products/4.jpg', 'images/products/5.jpg', 'images/products/6.jpg', 'images/products/7.jpg', 'images/products/8.jpg', 'images/products/1.jpg'];

  // 2. 綁定到 *ngFor 的可見圖片（最多七張）
  // 這裡我們直接使用 allImages，但 CSS 將限制顯示數量
  visibleImages: string[] = this.allImages;

  currentImage: string = this.allImages[0];

  // --- 互動狀態與方法 (保持不變) ---
  isFavorite: boolean = false;
  showShippingModal: boolean = false;
  showChatWindow: boolean = false;
  showToast: boolean = false;
  toastMessage: string = '';
  showImageModal: boolean = false; // 控制圖片放大彈窗是否顯示

  // 4. 新增捲動邏輯
  scrollThumbnails(direction: 'left' | 'right'): void {
    const container = this.thumbnailScrollContainer.nativeElement as HTMLDivElement;
    const scrollAmount = 80; // 捲動距離 (可根據 thumb 寬度調整)

    if (direction === 'left') {
      container.scrollLeft -= scrollAmount;
    } else {
      container.scrollLeft += scrollAmount;
    }
  }

  similarProducts = [
    { id: 1, name: '相機', price: 1899, img: 'images/products/1.jpg', desc: '二手使用過', isFavorite: false },
    { id: 2, name: '相機', price: 899, img: 'images/products/6.jpg', desc: '二手使用過', isFavorite: false },
    { id: 3, name: '相機', price: 2899, img: 'images/products/4.jpg', desc: '二手使用過', isFavorite: false },
    { id: 4, name: '相機', price: 889, img: 'images/products/7.jpg', desc: '二手使用過', isFavorite: false },
    { id: 5, name: '相機', price: 7009, img: 'images/products/1.jpg', desc: '二手使用過', isFavorite: false },
    { id: 6, name: '相機', price: 4490, img: 'images/products/4.jpg', desc: '二手使用過', isFavorite: false },
    { id: 7, name: '相機', price: 899, img: 'images/products/4.jpg', desc: '二手使用過', isFavorite: false },
    { id: 8, name: '相機', price: 899, img: 'images/products/5.jpg', desc: '二手使用過', isFavorite: false },
    { id: 9, name: '相機', price: 9999, img: 'images/products/8.jpg', desc: '二手使用過', isFavorite: false },
    { id: 10, name: '相機', price: 869, img: 'images/products/6.jpg', desc: '二手使用過', isFavorite: false },
    { id: 11, name: '相機', price: 699, img: 'images/products/7.jpg', desc: '二手使用過', isFavorite: false },
    { id: 12, name: '相機', price: 8979, img: 'images/products/8.jpg', desc: '二手使用過', isFavorite: false },
    { id: 13, name: '相機', price: 1299, img: 'images/products/1.jpg', desc: '二手使用過', isFavorite: false },
    { id: 14, name: '相機', price: 2399, img: 'images/products/4.jpg', desc: '二手使用過', isFavorite: false },
    { id: 15, name: '相機', price: 3499, img: 'images/products/5.jpg', desc: '二手使用過', isFavorite: false },
    { id: 16, name: '相機', price: 5699, img: 'images/products/6.jpg', desc: '二手使用過', isFavorite: false },
    { id: 17, name: '相機', price: 7899, img: 'images/products/7.jpg', desc: '二手使用過', isFavorite: false },
    { id: 18, name: '相機', price: 4899, img: 'images/products/8.jpg', desc: '二手使用過', isFavorite: false },
    { id: 101, name: '物件導向設計原則', price: 899, img: 'images/products/1.jpg', desc: '深入設計模式', isFavorite: false },
    { id: 102, name: '資料結構與演算法精解', price: 1250, img: 'images/products/1.jpg', desc: '提升程式效率', isFavorite: false },
    { id: 103, name: '前端框架實戰', price: 950, img: 'images/products/4.jpg', desc: 'React/Vue/Angular', isFavorite: false },
    { id: 104, name: 'UX/UI 設計入門', price: 780, img: 'images/products/5.jpg', desc: '用戶體驗與介面', isFavorite: false },
    { id: 105, name: '雲端運算架構', price: 1500, img: 'images/products/1.jpg', desc: 'AWS/Azure/GCP', isFavorite: false },
    { id: 106, name: 'Python 資料科學', price: 1100, img: 'images/products/6.jpg', desc: 'Pandas/Numpy', isFavorite: false },
    { id: 107, name: '產品經理實務', price: 850, img: 'images/products/1.jpg', desc: '從概念到上市', isFavorite: false },
    { id: 108, name: '區塊鏈技術概論', price: 1350, img: 'images/products/2.jpg', desc: 'Web3 基礎', isFavorite: false },
    { id: 109, name: '現代 CSS 排版', price: 650, img: 'images/products/8.jpg', desc: 'Flexbox & Grid', isFavorite: false },
    { id: 110, name: '全棧開發指南', price: 1400, img: 'images/products/8.jpg', desc: '前後端整合', isFavorite: false },
    { id: 111, name: '軟體測試藝術', price: 990, img: 'images/products/7.jpg', desc: '單元與整合測試', isFavorite: false },
    { id: 112, name: '敏捷開發方法', price: 720, img: 'images/products/6.jpg', desc: 'Scrum 實戰', isFavorite: false },
  ];

  // showToastMessage(message: string) {
  //   this.toastMessage = message;
  //   this.showToast = true;
  //   setTimeout(() => {
  //     this.showToast = false;
  //   }, 2500);
  // }

  // toggleFavorite() {
  //   this.isFavorite = !this.isFavorite;
  //   const message = this.isFavorite ? '已成功加入收藏！' : '已從收藏移除。';
  //   this.showToastMessage(message);
  // }

  // toggleSimilarFavorite(item: any) {
  //   item.isFavorite = !item.isFavorite;
  //   const message = item.isFavorite ? `${item.title} 已收藏！` : `${item.title} 已移除收藏。`;
  //   this.showToastMessage(message);
  // }

  toggleShippingModal(show: boolean) {
    this.showShippingModal = show;
  }

  toggleChatWindow() {
    this.showChatWindow = !this.showChatWindow;
  }

  goToStore() {
    this.toastService.show('正在導向賣場頁面...');
  }

  /**
   * addToCart 方法
   * 根據currentProductDetail 結構進行資料封裝
   */
  addToCart(): void {
    // 安全檢查：確保商品資料已正確載入
    if (!this.currentProductDetail) {
      this.toastService.show('商品資訊載入中，請稍後再試');
      return;
    }

    // 封裝成 CartService 需要的 CartItem 格式
    const itemToAdd = {
      id: this.currentProductDetail.id,
      name: this.currentProductDetail.name,
      price: this.currentProductDetail.price,
      quantity: this.quantity,  // 取得畫面上選取的數量
      imageUrl: this.currentProductDetail.img, // 將您的 img 欄位對應到購物車需要的 imageUrl
      selected: true   // 加入時預設為勾選狀態
    };

    // 呼叫 Service 執行加入動作
    this.cartService.addToCart(itemToAdd);

    // 顯示提示訊息
    this.toastService.show(`已將 ${this.quantity} 件 ${this.currentProductDetail.name} 加入購物車`);
  }

  selectImage(image: string) {
    this.currentImage = image;
  }

  claimCoupon(coupon: any) {
    if (!coupon.isClaimed) {
      coupon.isClaimed = true;
      this.toastService.show(`已領取 ${coupon.name}！`);
    } else {
      this.toastService.show('此優惠券已領取。');
    }
  }

  // 新增方法：切換圖片放大彈窗的顯示狀態
  toggleImageModal(show: boolean): void {
    this.showImageModal = show;
  }
}
