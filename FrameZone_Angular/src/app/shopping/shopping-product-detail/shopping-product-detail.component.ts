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
import { ProductCardComponent } from "../shared/components/product-card/product-card.component";
import { ChatStateService } from '../shared/services/chat-state.service';

interface Product {
  id: number;
  name: string;
  image: string;
  description: string;
  price: number;
  seller: {
    name: string;
    avatar: string;
  };
  postedDate: string;
  sales: number;
  categoryId: number;
  isFavorite: boolean;
}

@Component({
  selector: 'app-shopping-product-detail',
  standalone: true,
  imports: [FormsModule, CommonModule, ShoppinghomeComponent, RouterLink, FooterComponent, ToastNotificationComponent, FavoriteButtonComponent, ProductCardComponent],
  templateUrl: './shopping-product-detail.component.html',
  styleUrl: './shopping-product-detail.component.css'
})


export class ShoppingProductDetailComponent implements OnInit {

  // 新增屬性來儲存商品 ID
  productId: string | null = null;
  currentProduct: Product | null = null;

  @ViewChild('thumbnailScrollContainer', { static: true })
  thumbnailScrollContainer!: ElementRef;

  // 注入 ActivatedRoute 服務
  constructor(
    private route: ActivatedRoute,
    private toastService: ToastService,
    private cartService: CartService,
    private chatState: ChatStateService
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
        // this.loadProduct(this.productId);

        this.loadProduct(this.productId);

        window.scrollTo({ top: 0, behavior: 'smooth' });
      }
    });
  }

  // 從 similarProducts 列表中查找商品數據
  loadProduct(id: string): void {
    const targetId = parseInt(id);

    const foundProduct = this.similarProducts.find(item => item.id === targetId);

    if (foundProduct) {
      // 檢查 foundProduct 存在性後才賦值
      // 因為此時 foundProduct 的類型是 Product (非 undefined)，可以直接賦值。
      this.currentProduct = foundProduct;

      // 確保其他綁定的屬性使用 foundProduct 的屬性
      // this.currentProduct.name = foundProduct.name;
      // this.currentProduct.price = foundProduct.price;
      this.currentImage = foundProduct.image;
      // this.currentProduct.isFavorite = foundProduct.isFavorite;

      console.log(`成功載入商品 ID: ${targetId}，名稱: ${foundProduct.name}`);
    } else {
      // 處理找不到商品的情況
      // 如果找不到，將 currentProduct 設為 null (符合其類型 Product | null)
      this.currentProduct = null;

      // this.currentProduct.name = `商品載入失敗 (ID: ${targetId})`;
      // this.currentProduct.price = 0;
      console.error(`找不到 ID 為 ${targetId} 的商品。`);
    }
  }

  // --- 基礎商品資訊 ---
  // product = {
  //   name: '相機',
  //   price: 1580,
  //   isFavorite: false
  // };
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
  // showChatWindow: boolean = false;
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
    {
      id: 1, name: 'Manfrotto Befree Advanced 碳纖維旅行三腳架', image: 'images/products/2.jpg',
      description: '輕巧穩定，全新未拆封', price: 7200,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '3 天前', sales: 1, categoryId: 3, isFavorite: false
    },
    {
      id: 2, name: '【庫存出清】Kodak Ektar 100 底片 135', image: 'images/products/3.jpg',
      description: '已過期一年，全程防潮箱保存', price: 350,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '5 天前', sales: 2, categoryId: 4, isFavorite: false
    },
    {
      id: 3, name: 'Leica M6 Classic 0.72 相機 (二手)', image: 'images/products/4.jpg',
      description: '保存良好，功能正常', price: 85000,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '1 週前', sales: 0, categoryId: 1, isFavorite: false
    },
    {
      id: 4, name: '「Wanderlust」精選旅遊攝影集', image: 'images/products/9.jpg',
      description: '世界各地人文風景', price: 600,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '1 週前', sales: 40, categoryId: 5, isFavorite: false
    },
    {
      id: 5, name: '【二手】拍立得相機 Fujifilm Instax mini 9', image: 'images/products/6.jpg',
      description: '九成新，功能正常，適合入門拍立得玩家', price: 1999,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '3 天前', sales: 1, categoryId: 1, isFavorite: false
    },
    {
      id: 6, name: 'Godox V860II-C 佳能專用閃光燈 (二手)', image: 'images/products/7.jpg',
      description: '功能正常，僅在室內棚拍使用過幾次', price: 3500,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '4 天前', sales: 1, categoryId: 4, isFavorite: false
    },
    {
      id: 7, name: '攝影多功能單肩相機包 (全新)', image: 'images/products/8.jpg',
      description: '可容納一機兩鏡，側邊快取設計', price: 1490,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '5 天前', sales: 2, categoryId: 4, isFavorite: false
    },
    {
      id: 8, name: '自製旅遊明信片A', image: 'images/products/10.jpg',
      description: '世界各地旅遊景點', price: 50,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '6 天前', sales: 30, categoryId: 5, isFavorite: false
    },
    {
      id: 9, name: '自製旅遊明信片B', image: 'images/products/11.jpg',
      description: '世界各地旅遊景點', price: 50,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '1 週前', sales: 28, categoryId: 5, isFavorite: false
    },
    {
      id: 10, name: '復古皮革相機背帶（棕色）', image: 'images/products/5.jpg',
      description: '全新，多買一條故出售，尺寸 125 x 1.5 cm（長x 寬） · 重量68g · 最大承重力10kg', price: 800,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '2 週前', sales: 1, categoryId: 4, isFavorite: false
    },
    {
      id: 11, name: '【九成新】Fujifilm X-T3 相機', image: 'images/products/1.jpg',
      description: '僅使用半年，快門數約 5000，功能正常', price: 38500,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '1 週前', sales: 0, categoryId: 1, isFavorite: false
    },
    {
      id: 12, name: '自製旅遊明信片C', image: 'images/products/12.jpg',
      description: '世界各地旅遊景點', price: 50,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '2 週前', sales: 69, categoryId: 1, isFavorite: false
    },
    {
      id: 13, name: '真皮手環', image: 'images/products/1.jpg',
      description: '復古風格皮革手環', price: 499,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '2 週前', sales: 61, categoryId: 2, isFavorite: false
    },
    {
      id: 14, name: '壁掛裝飾畫', image: 'images/products/1.jpg',
      description: '現代簡約裝飾畫', price: 1299,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '2 週前', sales: 38, categoryId: 3, isFavorite: false
    },
    {
      id: 115, name: '便攜餐具組', image: 'images/products/1.jpg',
      description: '環保不鏽鋼餐具', price: 299,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '3 週前', sales: 145, categoryId: 4, isFavorite: false
    },
    {
      id: 116, name: '數據線', image: 'images/products/1.jpg',
      description: '快充編織數據線', price: 199,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '3 週前', sales: 198, categoryId: 5, isFavorite: false
    },
    {
      id: 117, name: '木質筆筒', image: 'images/products/1.jpg',
      description: '原木手工筆筒', price: 459,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '3 週前', sales: 47, categoryId: 1, isFavorite: false
    },
    {
      id: 118, name: '太陽眼鏡', image: 'images/products/1.jpg',
      description: '偏光太陽眼鏡', price: 999,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '3 週前', sales: 72, categoryId: 2, isFavorite: false
    },
    {
      id: 119, name: '桌面收納盒', image: 'images/products/1.jpg',
      description: '多層收納整理盒', price: 699,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '4 週前', sales: 84, categoryId: 3, isFavorite: false
    },
    {
      id: 120, name: '運動水壺', image: 'images/products/1.jpg',
      description: 'Tritan材質運動水壺', price: 399,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '4 週前', sales: 167, categoryId: 4, isFavorite: false
    },
    {
      id: 121, name: '運動水壺', image: 'images/products/1.jpg',
      description: 'Tritan材質運動水壺', price: 399,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '4 週前', sales: 167, categoryId: 4, isFavorite: false
    },
    {
      id: 122, name: '運動水壺', image: 'images/products/1.jpg',
      description: 'Tritan材質運動水壺', price: 399,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '4 週前', sales: 167, categoryId: 4, isFavorite: false
    },
    {
      id: 123, name: '運動水壺', image: 'images/products/1.jpg',
      description: 'Tritan材質運動水壺', price: 399,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '4 週前', sales: 167, categoryId: 4, isFavorite: false
    },
    {
      id: 124, name: '運動水壺', image: 'images/products/1.jpg',
      description: 'Tritan材質運動水壺', price: 399,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '4 週前', sales: 167, categoryId: 4, isFavorite: false
    }
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

  goToStore() {
    this.toastService.show('正在導向賣場頁面...');
  }

  /**
   * addToCart 方法
   * 根據currentProduct 結構進行資料封裝
   */
  addToCart(): void {
    // 安全檢查：確保商品資料已正確載入
    if (!this.currentProduct) {
      this.toastService.show('商品資訊載入中，請稍後再試');
      return;
    }

    // 封裝成 CartService 需要的 CartItem 格式
    const itemToAdd = {
      id: this.currentProduct.id,
      name: this.currentProduct.name,
      price: this.currentProduct.price,
      quantity: this.quantity,  // 取得畫面上選取的數量
      imageUrl: this.currentProduct.image, // 將您的 img 欄位對應到購物車需要的 imageUrl
      selected: true   // 加入時預設為勾選狀態
    };

    // 呼叫 Service 執行加入動作
    this.cartService.addToCart(itemToAdd);

    // 顯示提示訊息
    this.toastService.show(`已將 ${this.quantity} 件 ${this.currentProduct.name} 加入購物車`);
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

  openChatFromProduct() {
    if (!this.currentProduct) {
      this.toastService.show('商品資料尚未載入');
      return;
    }

    this.chatState.openFromProduct({
      id: this.currentProduct.id,
      name: this.currentProduct.name,
      price: this.currentProduct.price,
      image: this.currentProduct.image
    });
  }
}
