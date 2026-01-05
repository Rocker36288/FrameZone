import { CommonModule } from '@angular/common';
import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FooterComponent } from "../../shared/components/footer/footer.component";
import { ToastNotificationComponent } from "../shared/components/toast-notification/toast-notification.component";
import { FavoriteButtonComponent } from '../shared/components/favorite-button/favorite-button.component';
import { ToastService } from '../shared/services/toast.service';
import { CartService } from '../shared/services/cart.service';
import { ProductCardComponent } from "../shared/components/product-card/product-card.component";
import { ChatStateService } from '../shared/services/chat-state.service';
import { AuthService } from '../../core/services/auth.service';

import { ProductApiService } from './../shared/services/product-api.service';
import { ShopProduct } from '../interfaces/products';


@Component({
  selector: 'app-shopping-product-detail',
  standalone: true,
  imports: [FormsModule, CommonModule, RouterLink, FooterComponent, ToastNotificationComponent, FavoriteButtonComponent, ProductCardComponent],
  templateUrl: './shopping-product-detail.component.html',
  styleUrl: './shopping-product-detail.component.css'
})


export class ShoppingProductDetailComponent implements OnInit {

  // 新增屬性來儲存商品 ID
  productId!: number;
  currentProduct: any = {};
  isLoading: boolean = true;
  errorMessage: string = '';
  currentImage: string = '';
  visibleImages: any[] = [];  // 用於顯示縮圖

  // 目前選擇的規格
  selectedSpecification: any = null;
  // selectedSpec: string = '精裝版';
  // specs: string[] = ['精裝版', '電子書', '平裝版'];
  quantity: number = 1;
  isFavorite: boolean = false;


  @ViewChild('thumbnailScrollContainer', { static: true })
  thumbnailScrollContainer!: ElementRef;

  // 賣家資訊 (從 API 載入後會被替換)

  sellerInfo = {
    userId: 0,
    account: '未知賣家',
    avatar: '',
    onlineStatus: '',
    rating: 0,
    reviewCount: 0
  };

  // 付款方式資料
  paymentOptions = [
    { label: '信用卡 (VISA/Master/JCB)' },
    { label: '7-11取貨付款' },
    { label: '全家取貨付款' },
    { label: '銀行或郵局轉帳' },
    { label: '郵局無摺存款' },
  ];

  // 運費選項
  shippingOptions = [
    { label: '7-11取貨', fee: 60, isCombined: false, note: '' },
    { label: '全家取貨', fee: 60, isCombined: false, note: '' },
    { label: '7-11取貨付款', fee: 70, isCombined: false, note: '' },
    { label: '全家取貨付款', fee: 70, isCombined: false, note: '' },
    { label: '郵寄', fee: 65, isCombined: false, note: '' },
    { label: '合併運費', fee: 60, isCombined: true, note: '（適用兩件以上）' },
  ];

  // 優惠券
  availableCoupons = [
    { name: '滿千折100', discount: 100, isClaimed: true },
    { name: '新客折50', discount: 50, isClaimed: false },
  ];

  // 互動狀態
  showShippingModal: boolean = false;
  showImageModal: boolean = false;

  // 相似商品（從 API 載入，暫時保留原本的陣列作為備用）
  similarProducts: ShopProduct[] = [];
  recommendedProducts: ShopProduct[] = [];


  // 注入 ActivatedRoute 服務
  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private toastService: ToastService,
    private cartService: CartService,
    private chatState: ChatStateService,
    private productApiService: ProductApiService,
    private authService: AuthService
  ) { }

  // 實作 ngOnInit 函式，用於在元件初始化時執行邏輯
  ngOnInit(): void {
    // 使用 paramMap 訂閱路由參數的變動 (最佳做法)
    this.route.paramMap.subscribe(params => {
      // 取得路由中名為 'productId' 的參數。
      // 注意：'productId' 必須與 app.routes.ts 中設定的 :productId 一致。
      const id = params.get('productId');

      if (id) {
        this.productId = Number(id);
        console.log('取得的商品 ID:', this.productId);
        // 從 API 載入商品詳情
        this.loadProductFromApi(this.productId);

        window.scrollTo({ top: 0, behavior: 'smooth' });
      } else {
        this.errorMessage = '找不到商品';
        this.isLoading = false;
      }
    });
  }

  //從API載入商品詳情
  loadProductFromApi(id: number): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.productApiService.getProductDetail(id).subscribe({
      next: (data: any) => {
        console.log('API 回傳商品詳情:', data);

        // 記錄到近期瀏覽 (LocalStorage)
        this.saveToRecentlyViewed(id);

        // 將 API 資料轉換成元件需要的格式
        this.currentProduct = {
          productId: data.productId,
          name: data.productName,
          price: data.specifications && data.specifications.length > 0
            ? data.specifications[0].price
            : 0,
          image: data.images && data.images.length > 0
            ? data.images[0].imageUrl
            : 'assets/images/default.jpg',
          description: data.description,
          seller: {
            userId: data.seller.userId,
            displayName: data.seller.displayName,
            avatar: data.seller.avatar,
            rating: data.seller.rating,
            reviewCount: data.seller.reviewCount
          },
          postedDate: this.formatDate(data.createdAt),
          sales: 0,
          categoryId: data.categoryId,
          isFavorite: data.isFavorite || false,
          averageRating: data.averageRating || 0,
          reviewCount: data.reviewCount || 0,
          reviews: data.reviews || [],

          // 新增完整的圖片和規格資訊
          images: data.images,
          specifications: data.specifications,
          status: data.status,
          auditStatus: data.auditStatus
        };

        // 設定賣家資訊
        this.sellerInfo = {
          userId: data.seller.userId,
          account: data.seller.displayName,
          avatar: data.seller.avatar,
          onlineStatus: '線上',
          rating: data.seller.rating,
          reviewCount: data.seller.reviewCount
        };

        // 執行類似商品的請求
        if (data.categoryId) {
          this.loadSimilarProducts(id);
        } else {
          // 如果沒有分類ID導致沒有跑類似商品，直接跑猜你喜歡
          this.loadRecommendedProducts();
        }

        // 設定預設圖片（主圖）
        if (data.images && data.images.length > 0) {
          this.currentImage = data.images[0].imageUrl;
          this.visibleImages = data.images.map((img: any) => img.imageUrl);
        } else {
          // 若無圖片，重置為預設圖並清空縮圖列表
          this.currentImage = 'assets/images/default.jpg';
          this.visibleImages = [];
        }

        // 設定預設規格
        if (data.specifications && data.specifications.length > 0) {
          this.selectedSpecification = data.specifications[0];
        }

        this.isLoading = false;
      },
      error: (err: any) => {
        console.error('載入商品詳情失敗:', err);
        this.errorMessage = '載入商品資料失敗，請稍後再試';
        this.isLoading = false;

        // 顯示錯誤訊息
        this.toastService.show('載入商品失敗');
      }
    });
  }

  /**
   * 打開聊天視窗
   */
  openSharedChat(): void {
    if (this.currentProduct) {
      this.chatState.openFromProduct(this.currentProduct);
    } else {
      this.chatState.openFromFloating();
    }
  }

  /**
   * 格式化日期
   */
  formatDate(dateString: string): string {
    const date = new Date(dateString);
    const now = new Date();
    const diffTime = Math.abs(now.getTime() - date.getTime());
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

    if (diffDays === 0) return '今天';
    if (diffDays === 1) return '昨天';
    if (diffDays < 7) return `${diffDays} 天前`;
    if (diffDays < 30) return `${Math.floor(diffDays / 7)} 週前`;
    return `${Math.floor(diffDays / 30)} 個月前`;
  }

  /**
   * 選擇圖片
   */
  selectImage(imageUrl: string): void {
    this.currentImage = imageUrl;
  }

  /**
   * 選擇規格
   */
  selectSpecification(spec: any): void {
    this.selectedSpecification = spec;

    // 更新當前商品的價格
    if (this.currentProduct) {
      this.currentProduct.price = spec.price;
    }
  }

  /**
   * 捲動縮圖
   */
  scrollThumbnails(direction: 'left' | 'right'): void {
    const container = this.thumbnailScrollContainer?.nativeElement as HTMLDivElement;
    if (!container) return;

    const scrollAmount = 80;

    if (direction === 'left') {
      container.scrollLeft -= scrollAmount;
    } else {
      container.scrollLeft += scrollAmount;
    }
  }

  /**
   * 加入購物車
   */
  addToCart(): void {
    if (!this.authService.isAuthenticated()) {
      this.toastService.show('請先登入會員', 'top');
      setTimeout(() => {
        this.router.navigate(['/login'], { queryParams: { returnUrl: this.router.url } });
      }, 1000);
      return;
    }

    if (!this.currentProduct) {
      this.toastService.show('商品資訊載入中，請稍後再試');
      return;
    }

    if (!this.selectedSpecification) {
      this.toastService.show('請選擇商品規格');
      return;
    }

    if (this.selectedSpecification.stock < this.quantity) {
      this.toastService.show('庫存不足');
      return;
    }

    const itemToAdd = {
      id: this.currentProduct.productId, // 修正 ID 引用
      name: `${this.currentProduct.name} - ${this.selectedSpecification.specName}`,
      price: this.selectedSpecification.price,
      quantity: this.quantity,
      imageUrl: this.currentProduct.image,
      selected: true,
      specificationId: this.selectedSpecification.specificationId,
      sellerId: this.currentProduct.seller.userId,
      sellerName: this.currentProduct.seller.displayName,
      sellerAvatar: this.currentProduct.seller.avatar
    };

    this.cartService.addToCart(itemToAdd);
    this.toastService.show(`已將 ${this.quantity} 件 ${this.currentProduct.name} 加入購物車`);
  }

  /**
   * 領取優惠券
   */
  claimCoupon(coupon: any): void {
    if (!coupon.isClaimed) {
      coupon.isClaimed = true;
      this.toastService.show(`已領取 ${coupon.name}！`);
    } else {
      this.toastService.show('此優惠券已領取。');
    }
  }

  /**
   * 顯示/隱藏運費說明彈窗
   */
  toggleShippingModal(show: boolean): void {
    this.showShippingModal = show;
  }

  /**
   * 顯示/隱藏圖片放大彈窗
   */
  toggleImageModal(show: boolean): void {
    this.showImageModal = show;
  }

  /**
   * 跳轉至賣家評價頁面
   */
  goToSellerReviews(userId: any): void {
    if (userId) {
      this.router.navigate(['/shopping/reviews'], { queryParams: { userId: userId } });
    }
  }

  /**
   * 跳轉至賣家商店
   */
  goToSellerShop(userId: any): void { // Changed type to any as per instruction's implied change
    if (userId) {
      this.router.navigate(['/shopping/sellershop', userId]);
    }
  }

  /**
   * 前往賣場
   */
  goToStore(): void {
    this.toastService.show('正在導向賣場頁面...');
    // TODO: 實作前往賣場的邏輯
  }

  /**
   * 從商品頁開啟聊天
   */
  openChatFromProduct(): void {
    if (!this.currentProduct) {
      this.toastService.show('商品資料尚未載入');
      return;
    }

    this.chatState.openFromProduct({
      productId: this.currentProduct.productId,
      name: this.currentProduct.name,
      price: this.currentProduct.price,
      image: this.currentProduct.image
    });
  }

  loadSimilarProducts(productId: number): void {
    this.productApiService.getSimilarProducts(productId).subscribe({
      next: (res: any[]) => { // 明確指定型別為陣列
        this.similarProducts = res.map(item => ({
          productId: item.productId, // 對應 productId 到 productId
          name: item.productName, // 對應 productName 到 name
          image: item.mainImageUrl || 'assets/images/default.jpg', // 使用 mainImageUrl，若無則用預設圖
          description: item.description,
          price: Number(item.price) || 0, // 直接使用 price 欄位，並轉為數字
          seller: {
            name: item.seller ? item.seller.displayName : '未知賣家',
            avatar: item.seller ? item.seller.avatar : ''
          },
          postedDate: this.formatDate(item.createdAt), // 格式化日期
          sales: item.salesCount || 0,
          categoryId: item.categoryId,
          sellerCategoryIds: item.sellerCategoryIds || [],
          isFavorite: item.isFavorite || false,
          averageRating: item.averageRating || 0,
          reviewCount: item.reviewCount || 0
        }));

        // 載入猜你喜歡 (確保在類似商品載入後執行，以便過濾重複)
        this.loadRecommendedProducts();
      },
      error: (err) => {
        console.error('無法載入類似商品', err);
        // 即使類似商品失敗，也要載入猜你喜歡
        this.loadRecommendedProducts();
      }
    });
  }

  loadRecommendedProducts(): void {
    this.productApiService.getRecommendedProducts().subscribe({
      next: (res: any[]) => {
        // 1. 取得類似商品的 ID 列表 (用於過濾)
        const similarIds = new Set(this.similarProducts.map(p => p.productId));
        // 加上目前商品的 ID
        if (this.currentProduct && this.currentProduct.productId) {
          similarIds.add(this.currentProduct.productId);
        }

        // 2. 映射並過濾
        this.recommendedProducts = res
          .map(item => ({
            productId: item.productId,
            name: item.productName,
            image: item.mainImageUrl || 'assets/images/default.jpg',
            description: item.description,
            price: Number(item.price) || 0,
            seller: {
              name: item.seller ? item.seller.displayName : '未知賣家',
              avatar: item.seller ? item.seller.avatar : ''
            },
            postedDate: this.formatDate(item.createdAt),
            sales: item.salesCount || 0,
            categoryId: item.categoryId,
            sellerCategoryIds: item.sellerCategoryIds || [],
            isFavorite: item.isFavorite || false,
            averageRating: item.averageRating || 0,
            reviewCount: item.reviewCount || 0
          }))
          // 過濾掉已存在於類似商品或當前商品的項目
          .filter(item => !similarIds.has(item.productId))
          // 取前 12 筆
          .slice(0, 12);
      },
      error: (err) => console.error('無法載入推薦商品', err)
    });
  }

  private saveToRecentlyViewed(productId: number): void {
    const key = 'recentlyViewed';
    let viewed = [];
    try {
      viewed = JSON.parse(localStorage.getItem(key) || '[]');
    } catch (e) {
      viewed = [];
    }

    // 確保是數字陣列
    if (!Array.isArray(viewed)) viewed = [];

    // 移除已存在的 (避免重複並移到最前)
    viewed = viewed.filter((id: any) => id !== productId);

    // 加入到最前面
    viewed.unshift(productId);

    // 限制數量
    if (viewed.length > 12) {
      viewed = viewed.slice(0, 12);
    }

    localStorage.setItem(key, JSON.stringify(viewed));
  }

  /**
   * 返回商品列表
   */
  goBack(): void {
    this.router.navigate(['/shopping/products']);
  }

  /**
   * 切換收藏
   */
  // toggleFavorite(): void {
  //   this.isFavorite = !this.isFavorite;
  //   const message = this.isFavorite ? '已成功加入收藏！' : '已從收藏移除。';
  //   this.toastService.show(message);
  // }


  // // 從 similarProducts 列表中查找商品數據
  // loadProduct(id: string): void {
  //   const targetId = parseInt(id);

  //   const foundProduct = this.similarProducts.find(item => item.id === targetId);

  //   if (foundProduct) {
  //     // 檢查 foundProduct 存在性後才賦值
  //     // 因為此時 foundProduct 的類型是 Product (非 undefined)，可以直接賦值。
  //     this.currentProduct = foundProduct;

  //     // 確保其他綁定的屬性使用 foundProduct 的屬性
  //     // this.currentProduct.name = foundProduct.name;
  //     // this.currentProduct.price = foundProduct.price;
  //     this.currentImage = foundProduct.image;
  //     // this.currentProduct.isFavorite = foundProduct.isFavorite;

  //     console.log(`成功載入商品 ID: ${targetId}，名稱: ${foundProduct.name}`);
  //   } else {
  //     // 處理找不到商品的情況
  //     // 如果找不到，將 currentProduct 設為 null (符合其類型 Product | null)
  //     this.currentProduct = null;

  //     // this.currentProduct.name = `商品載入失敗 (ID: ${targetId})`;
  //     // this.currentProduct.price = 0;
  //     console.error(`找不到 ID 為 ${targetId} 的商品。`);
  //   }
  // }

  // // --- 基礎商品資訊 ---
  // // product = {
  // //   name: '相機',
  // //   price: 1580,
  // //   isFavorite: false
  // // };
  // quantity: number = 1;  // 確保 HTML 的數量輸入框綁定到這個變數
  // selectedSpec: string = '精裝版';
  // specs: string[] = ['精裝版', '電子書', '平裝版'];

  // // 賣家資訊 (保持不變)
  // sellerInfo = {
  //   account: 'ruka0711',
  //   avatar: 'seller-avatar.jpg', // 賣家頭像佔位符
  //   onlineStatus: '2小時前上線'
  // };

  // allImages: string[] = ['images/products/1.jpg', 'images/products/2.jpg', 'images/products/4.jpg', 'images/products/5.jpg', 'images/products/6.jpg', 'images/products/7.jpg', 'images/products/8.jpg', 'images/products/1.jpg'];

  // // 2. 綁定到 *ngFor 的可見圖片（最多七張）
  // // 這裡我們直接使用 allImages，但 CSS 將限制顯示數量
  // visibleImages: string[] = this.allImages;

  // currentImage: string = this.allImages[0];

  // // --- 互動狀態與方法 (保持不變) ---
  // isFavorite: boolean = false;
  // showShippingModal: boolean = false;
  // // showChatWindow: boolean = false;
  // showToast: boolean = false;
  // toastMessage: string = '';
  // showImageModal: boolean = false; // 控制圖片放大彈窗是否顯示

  // // 4. 新增捲動邏輯
  // scrollThumbnails(direction: 'left' | 'right'): void {
  //   const container = this.thumbnailScrollContainer.nativeElement as HTMLDivElement;
  //   const scrollAmount = 80; // 捲動距離 (可根據 thumb 寬度調整)

  //   if (direction === 'left') {
  //     container.scrollLeft -= scrollAmount;
  //   } else {
  //     container.scrollLeft += scrollAmount;
  //   }
  // }

  // similarProducts = [
  //   {
  //     id: 1, name: 'Manfrotto Befree Advanced 碳纖維旅行三腳架', image: 'images/products/2.jpg',
  //     description: '輕巧穩定，全新未拆封', price: 7200,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '3 天前', sales: 1, categoryId: 3, isFavorite: false
  //   },
  //   {
  //     id: 2, name: '【庫存出清】Kodak Ektar 100 底片 135', image: 'images/products/3.jpg',
  //     description: '已過期一年，全程防潮箱保存', price: 350,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '5 天前', sales: 2, categoryId: 4, isFavorite: false
  //   },
  //   {
  //     id: 3, name: 'Leica M6 Classic 0.72 相機 (二手)', image: 'images/products/4.jpg',
  //     description: '保存良好，功能正常', price: 85000,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '1 週前', sales: 0, categoryId: 1, isFavorite: false
  //   },
  //   {
  //     id: 4, name: '「Wanderlust」精選旅遊攝影集', image: 'images/products/9.jpg',
  //     description: '世界各地人文風景', price: 600,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '1 週前', sales: 40, categoryId: 5, isFavorite: false
  //   },
  //   {
  //     id: 5, name: '【二手】拍立得相機 Fujifilm Instax mini 9', image: 'images/products/6.jpg',
  //     description: '九成新，功能正常，適合入門拍立得玩家', price: 1999,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '3 天前', sales: 1, categoryId: 1, isFavorite: false
  //   },
  //   {
  //     id: 6, name: 'Godox V860II-C 佳能專用閃光燈 (二手)', image: 'images/products/7.jpg',
  //     description: '功能正常，僅在室內棚拍使用過幾次', price: 3500,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '4 天前', sales: 1, categoryId: 4, isFavorite: false
  //   },
  //   {
  //     id: 7, name: '攝影多功能單肩相機包 (全新)', image: 'images/products/8.jpg',
  //     description: '可容納一機兩鏡，側邊快取設計', price: 1490,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '5 天前', sales: 2, categoryId: 4, isFavorite: false
  //   },
  //   {
  //     id: 8, name: '自製旅遊明信片A', image: 'images/products/10.jpg',
  //     description: '世界各地旅遊景點', price: 50,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '6 天前', sales: 30, categoryId: 5, isFavorite: false
  //   },
  //   {
  //     id: 9, name: '自製旅遊明信片B', image: 'images/products/11.jpg',
  //     description: '世界各地旅遊景點', price: 50,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '1 週前', sales: 28, categoryId: 5, isFavorite: false
  //   },
  //   {
  //     id: 10, name: '復古皮革相機背帶（棕色）', image: 'images/products/5.jpg',
  //     description: '全新，多買一條故出售，尺寸 125 x 1.5 cm（長x 寬） · 重量68g · 最大承重力10kg', price: 800,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '2 週前', sales: 1, categoryId: 4, isFavorite: false
  //   },
  //   {
  //     id: 11, name: '【九成新】Fujifilm X-T3 相機', image: 'images/products/1.jpg',
  //     description: '僅使用半年，快門數約 5000，功能正常', price: 38500,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '1 週前', sales: 0, categoryId: 1, isFavorite: false
  //   },
  //   {
  //     id: 12, name: '自製旅遊明信片C', image: 'images/products/12.jpg',
  //     description: '世界各地旅遊景點', price: 50,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '2 週前', sales: 69, categoryId: 1, isFavorite: false
  //   },
  //   {
  //     id: 13, name: '真皮手環', image: 'images/products/1.jpg',
  //     description: '復古風格皮革手環', price: 499,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '2 週前', sales: 61, categoryId: 2, isFavorite: false
  //   },
  //   {
  //     id: 14, name: '壁掛裝飾畫', image: 'images/products/1.jpg',
  //     description: '現代簡約裝飾畫', price: 1299,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '2 週前', sales: 38, categoryId: 3, isFavorite: false
  //   },
  //   {
  //     id: 115, name: '便攜餐具組', image: 'images/products/1.jpg',
  //     description: '環保不鏽鋼餐具', price: 299,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '3 週前', sales: 145, categoryId: 4, isFavorite: false
  //   },
  //   {
  //     id: 116, name: '數據線', image: 'images/products/1.jpg',
  //     description: '快充編織數據線', price: 199,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '3 週前', sales: 198, categoryId: 5, isFavorite: false
  //   },
  //   {
  //     id: 117, name: '木質筆筒', image: 'images/products/1.jpg',
  //     description: '原木手工筆筒', price: 459,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '3 週前', sales: 47, categoryId: 1, isFavorite: false
  //   },
  //   {
  //     id: 118, name: '太陽眼鏡', image: 'images/products/1.jpg',
  //     description: '偏光太陽眼鏡', price: 999,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '3 週前', sales: 72, categoryId: 2, isFavorite: false
  //   },
  //   {
  //     id: 119, name: '桌面收納盒', image: 'images/products/1.jpg',
  //     description: '多層收納整理盒', price: 699,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '4 週前', sales: 84, categoryId: 3, isFavorite: false
  //   },
  //   {
  //     id: 120, name: '運動水壺', image: 'images/products/1.jpg',
  //     description: 'Tritan材質運動水壺', price: 399,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '4 週前', sales: 167, categoryId: 4, isFavorite: false
  //   },
  //   {
  //     id: 121, name: '運動水壺', image: 'images/products/1.jpg',
  //     description: 'Tritan材質運動水壺', price: 399,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '4 週前', sales: 167, categoryId: 4, isFavorite: false
  //   },
  //   {
  //     id: 122, name: '運動水壺', image: 'images/products/1.jpg',
  //     description: 'Tritan材質運動水壺', price: 399,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '4 週前', sales: 167, categoryId: 4, isFavorite: false
  //   },
  //   {
  //     id: 123, name: '運動水壺', image: 'images/products/1.jpg',
  //     description: 'Tritan材質運動水壺', price: 399,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '4 週前', sales: 167, categoryId: 4, isFavorite: false
  //   },
  //   {
  //     id: 124, name: '運動水壺', image: 'images/products/1.jpg',
  //     description: 'Tritan材質運動水壺', price: 399,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '4 週前', sales: 167, categoryId: 4, isFavorite: false
  //   }
  // ];

  // /**
  //  * addToCart 方法
  //  * 根據currentProduct 結構進行資料封裝
  //  */
  // addToCart(): void {
  //   // 安全檢查：確保商品資料已正確載入
  //   if (!this.currentProduct) {
  //     this.toastService.show('商品資訊載入中，請稍後再試');
  //     return;
  //   }

  //   // 封裝成 CartService 需要的 CartItem 格式
  //   const itemToAdd = {
  //     id: this.currentProduct.id,
  //     name: this.currentProduct.name,
  //     price: this.currentProduct.price,
  //     quantity: this.quantity,  // 取得畫面上選取的數量
  //     imageUrl: this.currentProduct.image, // 將您的 img 欄位對應到購物車需要的 imageUrl
  //     selected: true   // 加入時預設為勾選狀態
  //   };

  //   // 呼叫 Service 執行加入動作
  //   this.cartService.addToCart(itemToAdd);

  //   // 顯示提示訊息
  //   this.toastService.show(`已將 ${this.quantity} 件 ${this.currentProduct.name} 加入購物車`);
  // }

  // selectImage(image: string) {
  //   this.currentImage = image;
  // }

}
