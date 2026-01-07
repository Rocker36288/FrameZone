import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { FooterComponent } from "../../shared/components/footer/footer.component";
import { FavoriteButtonComponent } from '../shared/components/favorite-button/favorite-button.component';
import { ToastNotificationComponent } from '../shared/components/toast-notification/toast-notification.component';
import { ProductCardComponent } from "../shared/components/product-card/product-card.component";
import { ChatStateService } from '../shared/services/chat-state.service';
import { ActivatedRoute, Router } from '@angular/router';
import { ProductApiService } from '../shared/services/product-api.service';
import { ShopProduct } from '../interfaces/products';

interface Category {
  id: number;
  name: string;
}

@Component({
  selector: 'app-shopping-sellershop',
  standalone: true,
  imports: [FormsModule, CommonModule, RouterLink, FooterComponent, FavoriteButtonComponent, ToastNotificationComponent, ProductCardComponent],
  templateUrl: './shopping-sellershop.component.html',
  styleUrl: './shopping-sellershop.component.css'
})
export class ShoppingSellershopComponent {
  constructor(
    private chatState: ChatStateService,
    private route: ActivatedRoute,
    private router: Router,
    private productApiService: ProductApiService
  ) { }

  sellerAccount: string = '';
  isLoading: boolean = true;

  sellerInfo = {
    // name: 'Ruka 的生活選物',
    // avatar: 'images/avatar/11.jpg',
    // rating: 4.9,
    // reviewCount: 1253,
    // isOnline: true,
    // description: '哈囉！我是 Ruka 👋 一個熱愛生活、喜歡分享好物的賣家。這個賣場就像我的小天地，每件商品都是我精心挑選、親自使用過覺得不錯才放上來的。',
    // shopImage: 'images/sellshop/sellshop4.png',
    // productCount: 41

    name: '',
    avatar: '',
    rating: 0,
    reviewCount: 0,
    isOnline: false,
    description: '',
    shopImage: 'images/sellshop/sellshop4.png',
    productCount: 0

  };

  // 收藏相關
  favoriteProducts: Set<number> = new Set();
  showToast = false;
  toastMessage = '';

  sortBy = 'price';
  sortOrder: 'asc' | 'desc' = 'asc'; // asc: 低到高, desc: 高到低
  selectedCategoryId: number | null = null;

  // 搜尋相關
  searchKeyword = '';
  minPrice: number | null = null;
  maxPrice: number | null = null;

  categories: Category[] = [
    { id: 0, name: '全部' }
    // { id: 1, name: '相機' },
    // { id: 2, name: '拍立得' },
    // { id: 3, name: '腳架' },
    // { id: 4, name: '配件' },
    // { id: 5, name: '創作' }
  ];

  // allProducts: Product[] = [
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
  //     id: 15, name: '便攜餐具組', image: 'images/products/1.jpg',
  //     description: '環保不鏽鋼餐具', price: 299,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '3 週前', sales: 145, categoryId: 4, isFavorite: false
  //   },
  //   {
  //     id: 16, name: '數據線', image: 'images/products/1.jpg',
  //     description: '快充編織數據線', price: 199,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '3 週前', sales: 198, categoryId: 5, isFavorite: false
  //   },
  //   {
  //     id: 17, name: '木質筆筒', image: 'images/products/1.jpg',
  //     description: '原木手工筆筒', price: 459,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '3 週前', sales: 47, categoryId: 1, isFavorite: false
  //   },
  //   {
  //     id: 18, name: '太陽眼鏡', image: 'images/products/1.jpg',
  //     description: '偏光太陽眼鏡', price: 999,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '3 週前', sales: 72, categoryId: 2, isFavorite: false
  //   },
  //   {
  //     id: 19, name: '桌面收納盒', image: 'images/products/1.jpg',
  //     description: '多層收納整理盒', price: 699,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '4 週前', sales: 84, categoryId: 3, isFavorite: false
  //   },
  //   {
  //     id: 20, name: '運動水壺', image: 'images/products/1.jpg',
  //     description: 'Tritan材質運動水壺', price: 399,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '4 週前', sales: 167, categoryId: 4, isFavorite: false
  //   },
  //   {
  //     id: 21, name: '植物盆栽', image: 'images/products/1.jpg',
  //     description: '多肉植物組合盆栽', price: 349,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '4 週前', sales: 95, categoryId: 3, isFavorite: false
  //   },
  //   {
  //     id: 22, name: '筆記本套裝', image: 'images/products/1.jpg',
  //     description: '精裝硬皮筆記本', price: 559,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '1 個月前', sales: 112, categoryId: 4, isFavorite: false
  //   },
  //   {
  //     id: 23, name: '無線滑鼠', image: 'images/products/1.jpg',
  //     description: '人體工學無線滑鼠', price: 599,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '1 個月前', sales: 134, categoryId: 5, isFavorite: false
  //   },
  //   {
  //     id: 24, name: '手工香皂', image: 'images/products/1.jpg',
  //     description: '天然精油手工皂', price: 259,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '1 個月前', sales: 187, categoryId: 4, isFavorite: false
  //   },
  //   {
  //     id: 25, name: '編織購物袋', image: 'images/products/1.jpg',
  //     description: '環保手工編織袋', price: 399,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '1 個月前', sales: 76, categoryId: 1, isFavorite: false
  //   },
  //   {
  //     id: 26, name: '項鍊吊墜', image: 'images/products/1.jpg',
  //     description: '925純銀項鍊', price: 1299,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '1 個月前', sales: 53, categoryId: 2, isFavorite: false
  //   },
  //   {
  //     id: 27, name: '桌燈', image: 'images/products/1.jpg',
  //     description: 'LED護眼檯燈', price: 899,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '1 個月前', sales: 98, categoryId: 3, isFavorite: false
  //   },
  //   {
  //     id: 28, name: '折疊雨傘', image: 'images/products/1.jpg',
  //     description: '自動開收折疊傘', price: 459,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '1 個月前', sales: 145, categoryId: 4, isFavorite: false
  //   },
  //   {
  //     id: 29, name: '手機殼', image: 'images/products/1.jpg',
  //     description: '透明防摔手機殼', price: 199,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '1 個月前', sales: 223, categoryId: 5, isFavorite: false
  //   },
  //   {
  //     id: 30, name: '木質相框', image: 'images/products/1.jpg',
  //     description: '復古風格木製相框', price: 329,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '1 個月前', sales: 67, categoryId: 3, isFavorite: false
  //   },
  //   {
  //     id: 31, name: '咖啡杯組', image: 'images/products/1.jpg',
  //     description: '雙層隔熱咖啡杯', price: 699,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '5 週前', sales: 89, categoryId: 4, isFavorite: false
  //   },
  //   {
  //     id: 32, name: '鑰匙圈', image: 'images/products/1.jpg',
  //     description: '真皮鑰匙扣', price: 259,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '5 週前', sales: 156, categoryId: 1, isFavorite: false
  //   },
  //   {
  //     id: 33, name: '髮飾組合', image: 'images/products/1.jpg',
  //     description: '日系髮夾髮圈組', price: 349,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '5 週前', sales: 102, categoryId: 2, isFavorite: false
  //   },
  //   {
  //     id: 34, name: '抱枕', image: 'images/products/1.jpg',
  //     description: '北歐風格抱枕套', price: 459,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '5 週前', sales: 78, categoryId: 3, isFavorite: false
  //   },
  //   {
  //     id: 35, name: '旅行收納袋', image: 'images/products/1.jpg',
  //     description: '防水旅行收納包', price: 399,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '5 週前', sales: 134, categoryId: 4, isFavorite: false
  //   },
  //   {
  //     id: 36, name: '耳機收納盒', image: 'images/products/1.jpg',
  //     description: '便攜耳機保護盒', price: 159,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '5 週前', sales: 189, categoryId: 5, isFavorite: false
  //   },
  //   {
  //     id: 37, name: '陶藝花瓶', image: 'images/products/1.jpg',
  //     description: '手工陶瓷花瓶', price: 899,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '6 週前', sales: 45, categoryId: 1, isFavorite: false
  //   },
  //   {
  //     id: 38, name: '圍巾', image: 'images/products/1.jpg',
  //     description: '純羊毛保暖圍巾', price: 1299,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '6 週前', sales: 67, categoryId: 2, isFavorite: false
  //   },
  //   {
  //     id: 39, name: '掛鐘', image: 'images/products/1.jpg',
  //     description: '靜音掛鐘', price: 659,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '6 週前', sales: 56, categoryId: 3, isFavorite: false
  //   },
  //   {
  //     id: 40, name: '便當盒', image: 'images/products/1.jpg',
  //     description: '304不鏽鋼便當盒', price: 559,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '6 週前', sales: 123, categoryId: 4, isFavorite: false
  //   },
  //   {
  //     id: 41, name: '滑鼠墊', image: 'images/products/1.jpg',
  //     description: '加大遊戲滑鼠墊', price: 299,
  //     seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
  //     postedDate: '6 週前', sales: 178, categoryId: 5, isFavorite: false
  //   }
  // ];

  allProducts: ShopProduct[] = [];
  filteredProducts: ShopProduct[] = [];
  displayProducts: ShopProduct[] = [];

  // 分頁相關
  currentPage = 1;
  itemsPerPage = 20; // 5x4 = 20
  totalPages = 1;
  maxPagesToShow = 5; // 最多顯示5個頁碼

  get visiblePages(): number[] {
    const pages: number[] = [];
    let startPage = Math.max(1, this.currentPage - 2);
    let endPage = Math.min(this.totalPages, startPage + this.maxPagesToShow - 1);

    if (endPage - startPage < this.maxPagesToShow - 1) {
      startPage = Math.max(1, endPage - this.maxPagesToShow + 1);
    }

    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }

    return pages;
  }

  showFirstPageDots(): boolean {
    return this.currentPage > 3;
  }

  showLastPageDots(): boolean {
    return this.currentPage < this.totalPages - 2;
  }

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      this.sellerAccount = params.get('sellerAccount') || '';
      if (this.sellerAccount) {
        this.loadSellerData();
      }
    });
  }

  loadSellerData() {
    this.isLoading = true;
    // 獲取賣家基本資料
    this.productApiService.getSellerProfile(this.sellerAccount).subscribe({
      next: (data: any) => {
        this.sellerInfo = {
          name: data.storeName || data.displayName,
          avatar: data.avatar,
          rating: data.rating || 0,
          reviewCount: data.reviewCount || 0,
          isOnline: true,
          description: data.storeDescription || data.bio || '歡迎來到我的賣場！',
          shopImage: data.coverImage || 'images/sellshop/sellshop4.png',
          productCount: data.productCount || 0
        };
      },
      error: (err) => {
        console.error('無法載入賣家資料', err);
      }
    });

    // 獲取賣家自定義分類
    this.productApiService.getSellerCategories(this.sellerAccount).subscribe({
      next: (res: any[]) => {
        if (res && res.length > 0) {
          this.categories = [
            { id: 0, name: '全部' },
            ...res.map(c => ({ id: c.id, name: c.name }))
          ];
        }
      },
      error: (err) => {
        console.error('無法載入賣家分類', err);
      }
    });

    // 獲取賣家所有商品
    this.productApiService.getProductsBySeller(this.sellerAccount).subscribe({
      next: (res: any[]) => {
        this.allProducts = res.map(item => ({
          productId: item.productId,
          name: item.productName,
          image: item.mainImageUrl || 'assets/images/default.jpg',
          description: item.description,
          price: Number(item.price) || 0,
          seller: {
            name: item.seller?.displayName || this.sellerInfo.name,
            avatar: item.seller?.avatar || this.sellerInfo.avatar
          },
          postedDate: this.formatDate(item.createdAt),
          sales: item.salesCount || 0,
          categoryId: item.categoryId,
          sellerCategoryIds: item.sellerCategoryIds || [],
          isFavorite: item.isFavorite || false,
          averageRating: item.averageRating || 0,
          reviewCount: item.reviewCount || 0
        }));
        this.filterProducts();
        this.isLoading = false;
      },
      error: (err) => {
        console.error('無法載入賣家商品', err);
        this.isLoading = false;
      }
    });
  }

  /**
   * 跳轉至評價頁面
   */
  goToReviews(): void {
    if (this.sellerAccount) {
      // 使用賣家帳號跳轉至評價頁面，查詢該賣家的所有評價
      this.router.navigate(['/shopping/reviews'], { queryParams: { userId: this.sellerAccount } });
    }
  }

  /**
   * 平滑捲動至商品區
   */
  scrollToProducts(): void {
    const element = document.getElementById('all-products');
    if (element) {
      element.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
  }

  formatDate(dateString: string): string {
    if (!dateString) return '';
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

  selectCategory(categoryId: number | null) {
    this.selectedCategoryId = categoryId;
    this.currentPage = 1;
    this.filterProducts();
  }

  filterProducts() {
    // 根據分類篩選
    if (this.selectedCategoryId === null || this.selectedCategoryId === 0) {
      this.filteredProducts = [...this.allProducts];
    } else {
      this.filteredProducts = this.allProducts.filter(
        p => p.sellerCategoryIds && p.sellerCategoryIds.includes(this.selectedCategoryId!)
      );
    }

    // 根據價格區間篩選
    if (this.minPrice !== null) {
      this.filteredProducts = this.filteredProducts.filter(
        p => p.price >= this.minPrice!
      );
    }
    if (this.maxPrice !== null) {
      this.filteredProducts = this.filteredProducts.filter(
        p => p.price <= this.maxPrice!
      );
    }

    // 根據關鍵字篩選
    if (this.searchKeyword.trim()) {
      const keyword = this.searchKeyword.toLowerCase();
      this.filteredProducts = this.filteredProducts.filter(
        p => p.name.toLowerCase().includes(keyword) ||
          p.description.toLowerCase().includes(keyword)
      );
    }

    // 排序
    this.applySorting();

    // 計算總頁數
    this.totalPages = Math.ceil(this.filteredProducts.length / this.itemsPerPage);

    // 更新顯示的商品
    this.updateDisplayProducts();
  }

  onSearch() {
    this.currentPage = 1;
    this.filterProducts();
  }

  onPriceFilter() {
    this.currentPage = 1;
    this.filterProducts();
  }

  applySorting() {
    switch (this.sortBy) {
      case 'price':
        this.filteredProducts.sort((a, b) =>
          this.sortOrder === 'asc' ? a.price - b.price : b.price - a.price
        );
        break;
      case 'latest':
        // 假設 id 越大越新
        this.filteredProducts.sort((a, b) =>
          this.sortOrder === 'asc' ? a.productId - b.productId : b.productId - a.productId
        );
        break;
      case 'sales':
        this.filteredProducts.sort((a, b) =>
          this.sortOrder === 'asc' ? a.sales - b.sales : b.sales - a.sales
        );
        break;
    }
  }

  onSort(sortType: string) {
    if (this.sortBy === sortType) {
      // 如果點擊相同的排序，切換升降序
      this.sortOrder = this.sortOrder === 'asc' ? 'desc' : 'asc';
    } else {
      // 如果點擊不同的排序，設置為降序（最新/最高）
      this.sortBy = sortType;
      this.sortOrder = 'desc';
    }
    this.applySorting();
    this.updateDisplayProducts();
  }

  updateDisplayProducts() {
    const startIndex = (this.currentPage - 1) * this.itemsPerPage;
    const endIndex = startIndex + this.itemsPerPage;
    this.displayProducts = this.filteredProducts.slice(startIndex, endIndex);
  }

  goToPage(page: number) {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.updateDisplayProducts();
    }
  }

  get pages(): number[] {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }

  openSharedChat() {
    this.chatState.openFromSeller({
      sellerName: this.sellerInfo.name,
      sellerAvatar: this.sellerInfo.avatar
    });
  }

  // toggleFavorite(product: Product, event: Event) {
  //   event.preventDefault();
  //   event.stopPropagation();

  //   if (this.favoriteProducts.has(product.id)) {
  //     this.favoriteProducts.delete(product.id);
  //     this.showToastMessage(`${product.name} 已從收藏移除`);
  //   } else {
  //     this.favoriteProducts.add(product.id);
  //     this.showToastMessage(`${product.name} 已成功加入收藏！`);
  //   }
  // }

  // isFavorite(productId: number): boolean {
  //   return this.favoriteProducts.has(productId);
  // }

  // showToastMessage(message: string) {
  //   this.toastMessage = message;
  //   this.showToast = true;
  //   setTimeout(() => {
  //     this.showToast = false;
  //   }, 2000);
  // }
}
