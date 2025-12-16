import { CommonModule } from '@angular/common';
import { Component, ElementRef, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-shopping-product-detail',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './shopping-product-detail.component.html',
  styleUrl: './shopping-product-detail.component.css'
})


export class ShoppingProductDetailComponent {

  @ViewChild('thumbnailScrollContainer', { static: true })
  thumbnailScrollContainer!: ElementRef;

  // --- 基礎商品資訊 ---
  product = {
    name: '相機', price: 1580,
  };
  quantity: number = 1;
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
    { title: '物件導向設計原則', price: 899, img: 'images/products/1.jpg', desc: '深入設計模式', isFavorite: true },
    { title: '資料結構與演算法精解', price: 1250, img: 'images/products/1.jpg', desc: '提升程式效率', isFavorite: false },
    { title: '前端框架實戰', price: 950, img: 'images/products/4.jpg', desc: 'React/Vue/Angular', isFavorite: false },
    { title: 'UX/UI 設計入門', price: 780, img: 'images/products/5.jpg', desc: '用戶體驗與介面', isFavorite: true },
    { title: '雲端運算架構', price: 1500, img: 'images/products/1.jpg', desc: 'AWS/Azure/GCP', isFavorite: false },
    { title: 'Python 資料科學', price: 1100, img: 'images/products/6.jpg', desc: 'Pandas/Numpy', isFavorite: false },
    { title: '產品經理實務', price: 850, img: 'images/products/1.jpg', desc: '從概念到上市', isFavorite: false },
    { title: '區塊鏈技術概論', price: 1350, img: 'images/products/2.jpg', desc: 'Web3 基礎', isFavorite: true },
    { title: '現代 CSS 排版', price: 650, img: 'images/products/8.jpg', desc: 'Flexbox & Grid', isFavorite: false },
    { title: '全棧開發指南', price: 1400, img: 'images/products/8.jpg', desc: '前後端整合', isFavorite: false },
    { title: '軟體測試藝術', price: 990, img: 'images/products/7.jpg', desc: '單元與整合測試', isFavorite: true },
    { title: '敏捷開發方法', price: 720, img: 'images/products/6.jpg', desc: 'Scrum 實戰', isFavorite: false },
  ];

  showToastMessage(message: string) {
    this.toastMessage = message;
    this.showToast = true;
    setTimeout(() => {
      this.showToast = false;
    }, 2500);
  }

  toggleFavorite() {
    this.isFavorite = !this.isFavorite;
    const message = this.isFavorite ? '已成功加入收藏！' : '已從收藏移除。';
    this.showToastMessage(message);
  }

  toggleSimilarFavorite(item: any) {
    item.isFavorite = !item.isFavorite;
    const message = item.isFavorite ? `${item.title} 已收藏！` : `${item.title} 已移除收藏。`;
    this.showToastMessage(message);
  }

  toggleShippingModal(show: boolean) {
    this.showShippingModal = show;
  }

  toggleChatWindow() {
    this.showChatWindow = !this.showChatWindow;
  }

  goToStore() {
    this.showToastMessage('正在導向賣場頁面...');
  }

  addToCart() { this.showToastMessage('已將商品加入購物車'); }
  selectImage(image: string) { this.currentImage = image; }
  claimCoupon(coupon: any) {
    if (!coupon.isClaimed) {
      coupon.isClaimed = true;
      this.showToastMessage(`已領取 ${coupon.name}！`);
    } else {
      this.showToastMessage('此優惠券已領取。');
    }
  }

  // 新增方法：切換圖片放大彈窗的顯示狀態
  toggleImageModal(show: boolean): void {
    this.showImageModal = show;
  }
}
