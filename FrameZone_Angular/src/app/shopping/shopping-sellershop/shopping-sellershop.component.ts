import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { FooterComponent } from "../../shared/components/footer/footer.component";
import { FavoriteButtonComponent } from '../shared/components/favorite-button/favorite-button.component';
import { ToastNotificationComponent } from '../shared/components/toast-notification/toast-notification.component';

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

interface Category {
  id: number;
  name: string;
}

@Component({
  selector: 'app-shopping-sellershop',
  standalone: true,
  imports: [FormsModule, CommonModule, RouterLink, FooterComponent, FavoriteButtonComponent, ToastNotificationComponent],
  templateUrl: './shopping-sellershop.component.html',
  styleUrl: './shopping-sellershop.component.css'
})
export class ShoppingSellershopComponent {
  sellerInfo = {
    name: 'Ruby 的生活選物',
    avatar: 'images/products/1.jpg',
    rating: 4.9,
    reviewCount: 1253,
    isOnline: true,
    description: '哈囉！我是 Ruby 👋 一個熱愛生活、喜歡分享好物的賣家。這個賣場就像我的小天地，每件商品都是我精心挑選、親自使用過覺得不錯才放上來的。',
    shopImage: 'images/products/1.jpg',
    productCount: 41
  };

  // 聊天室相關
  showChatRoom = false;
  chatMessages: Array<{ text: string, sender: 'user' | 'seller', time: string }> = [];
  newMessage = '';

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
    { id: 0, name: '全部' },
    { id: 1, name: '手工藝品' },
    { id: 2, name: '時尚配件' },
    { id: 3, name: '居家裝飾' },
    { id: 4, name: '生活用品' },
    { id: 5, name: '3C配件' }
  ];

  allProducts: Product[] = [
    {
      id: 1, name: '精美手工藝品', image: 'images/products/1.jpg',
      description: '手工製作的精美藝術品，獨一無二的設計風格', price: 1299,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '3 天前', sales: 45, categoryId: 1, isFavorite: false
    },
    {
      id: 2, name: '時尚配件組合', image: 'images/products/1.jpg',
      description: '最新流行的時尚配件，多種顏色可選', price: 899,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '5 天前', sales: 78, categoryId: 2, isFavorite: false
    },
    {
      id: 3, name: '居家裝飾品', image: 'images/products/1.jpg',
      description: '簡約北歐風格居家裝飾', price: 2599,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '1 週前', sales: 32, categoryId: 3, isFavorite: false
    },
    {
      id: 4, name: '創意生活用品', image: 'images/products/1.jpg',
      description: '實用又有趣的生活小物', price: 499,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '2 週前', sales: 156, categoryId: 4, isFavorite: false
    },
    {
      id: 5, name: '手機支架', image: 'images/products/1.jpg',
      description: '多角度調整手機支架', price: 299,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '3 天前', sales: 89, categoryId: 5, isFavorite: false
    },
    {
      id: 6, name: '藍牙耳機', image: 'images/products/1.jpg',
      description: '高音質無線藍牙耳機', price: 1899,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '4 天前', sales: 67, categoryId: 5, isFavorite: false
    },
    {
      id: 7, name: '手工皮革錢包', image: 'images/products/1.jpg',
      description: '真皮手工製作錢包', price: 1599,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '5 天前', sales: 43, categoryId: 1, isFavorite: false
    },
    {
      id: 8, name: '時尚手錶', image: 'images/products/1.jpg',
      description: '簡約風格石英錶', price: 2199,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '6 天前', sales: 54, categoryId: 2, isFavorite: false
    },
    {
      id: 9, name: '香氛蠟燭', image: 'images/products/1.jpg',
      description: '天然植物精油香氛', price: 599,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '1 週前', sales: 92, categoryId: 3, isFavorite: false
    },
    {
      id: 10, name: '保溫杯', image: 'images/products/1.jpg',
      description: '316不鏽鋼保溫杯', price: 799,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '1 週前', sales: 128, categoryId: 4, isFavorite: false
    },
    {
      id: 11, name: '無線充電板', image: 'images/products/1.jpg',
      description: '快速無線充電', price: 699,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '1 週前', sales: 76, categoryId: 5, isFavorite: false
    },
    {
      id: 12, name: '手工陶瓷杯', image: 'images/products/1.jpg',
      description: '日式風格陶瓷杯', price: 399,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '2 週前', sales: 103, categoryId: 1, isFavorite: false
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
      id: 15, name: '便攜餐具組', image: 'images/products/1.jpg',
      description: '環保不鏽鋼餐具', price: 299,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '3 週前', sales: 145, categoryId: 4, isFavorite: false
    },
    {
      id: 16, name: '數據線', image: 'images/products/1.jpg',
      description: '快充編織數據線', price: 199,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '3 週前', sales: 198, categoryId: 5, isFavorite: false
    },
    {
      id: 17, name: '木質筆筒', image: 'images/products/1.jpg',
      description: '原木手工筆筒', price: 459,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '3 週前', sales: 47, categoryId: 1, isFavorite: false
    },
    {
      id: 18, name: '太陽眼鏡', image: 'images/products/1.jpg',
      description: '偏光太陽眼鏡', price: 999,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '3 週前', sales: 72, categoryId: 2, isFavorite: false
    },
    {
      id: 19, name: '桌面收納盒', image: 'images/products/1.jpg',
      description: '多層收納整理盒', price: 699,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '4 週前', sales: 84, categoryId: 3, isFavorite: false
    },
    {
      id: 20, name: '運動水壺', image: 'images/products/1.jpg',
      description: 'Tritan材質運動水壺', price: 399,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '4 週前', sales: 167, categoryId: 4, isFavorite: false
    },
    {
      id: 21, name: '植物盆栽', image: 'images/products/1.jpg',
      description: '多肉植物組合盆栽', price: 349,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '4 週前', sales: 95, categoryId: 3, isFavorite: false
    },
    {
      id: 22, name: '筆記本套裝', image: 'images/products/1.jpg',
      description: '精裝硬皮筆記本', price: 559,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '1 個月前', sales: 112, categoryId: 4, isFavorite: false
    },
    {
      id: 23, name: '無線滑鼠', image: 'images/products/1.jpg',
      description: '人體工學無線滑鼠', price: 599,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '1 個月前', sales: 134, categoryId: 5, isFavorite: false
    },
    {
      id: 24, name: '手工香皂', image: 'images/products/1.jpg',
      description: '天然精油手工皂', price: 259,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '1 個月前', sales: 187, categoryId: 4, isFavorite: false
    },
    {
      id: 25, name: '編織購物袋', image: 'images/products/1.jpg',
      description: '環保手工編織袋', price: 399,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '1 個月前', sales: 76, categoryId: 1, isFavorite: false
    },
    {
      id: 26, name: '項鍊吊墜', image: 'images/products/1.jpg',
      description: '925純銀項鍊', price: 1299,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '1 個月前', sales: 53, categoryId: 2, isFavorite: false
    },
    {
      id: 27, name: '桌燈', image: 'images/products/1.jpg',
      description: 'LED護眼檯燈', price: 899,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '1 個月前', sales: 98, categoryId: 3, isFavorite: false
    },
    {
      id: 28, name: '折疊雨傘', image: 'images/products/1.jpg',
      description: '自動開收折疊傘', price: 459,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '1 個月前', sales: 145, categoryId: 4, isFavorite: false
    },
    {
      id: 29, name: '手機殼', image: 'images/products/1.jpg',
      description: '透明防摔手機殼', price: 199,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '1 個月前', sales: 223, categoryId: 5, isFavorite: false
    },
    {
      id: 30, name: '木質相框', image: 'images/products/1.jpg',
      description: '復古風格木製相框', price: 329,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '1 個月前', sales: 67, categoryId: 3, isFavorite: false
    },
    {
      id: 31, name: '咖啡杯組', image: 'images/products/1.jpg',
      description: '雙層隔熱咖啡杯', price: 699,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '5 週前', sales: 89, categoryId: 4, isFavorite: false
    },
    {
      id: 32, name: '鑰匙圈', image: 'images/products/1.jpg',
      description: '真皮鑰匙扣', price: 259,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '5 週前', sales: 156, categoryId: 1, isFavorite: false
    },
    {
      id: 33, name: '髮飾組合', image: 'images/products/1.jpg',
      description: '日系髮夾髮圈組', price: 349,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '5 週前', sales: 102, categoryId: 2, isFavorite: false
    },
    {
      id: 34, name: '抱枕', image: 'images/products/1.jpg',
      description: '北歐風格抱枕套', price: 459,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '5 週前', sales: 78, categoryId: 3, isFavorite: false
    },
    {
      id: 35, name: '旅行收納袋', image: 'images/products/1.jpg',
      description: '防水旅行收納包', price: 399,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '5 週前', sales: 134, categoryId: 4, isFavorite: false
    },
    {
      id: 36, name: '耳機收納盒', image: 'images/products/1.jpg',
      description: '便攜耳機保護盒', price: 159,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '5 週前', sales: 189, categoryId: 5, isFavorite: false
    },
    {
      id: 37, name: '陶藝花瓶', image: 'images/products/1.jpg',
      description: '手工陶瓷花瓶', price: 899,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '6 週前', sales: 45, categoryId: 1, isFavorite: false
    },
    {
      id: 38, name: '圍巾', image: 'images/products/1.jpg',
      description: '純羊毛保暖圍巾', price: 1299,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '6 週前', sales: 67, categoryId: 2, isFavorite: false
    },
    {
      id: 39, name: '掛鐘', image: 'images/products/1.jpg',
      description: '靜音掛鐘', price: 659,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '6 週前', sales: 56, categoryId: 3, isFavorite: false
    },
    {
      id: 40, name: '便當盒', image: 'images/products/1.jpg',
      description: '304不鏽鋼便當盒', price: 559,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '6 週前', sales: 123, categoryId: 4, isFavorite: false
    },
    {
      id: 41, name: '滑鼠墊', image: 'images/products/1.jpg',
      description: '加大遊戲滑鼠墊', price: 299,
      seller: { name: '賣場名稱', avatar: 'images/products/1.jpg' },
      postedDate: '6 週前', sales: 178, categoryId: 5, isFavorite: false
    }
  ];

  filteredProducts: Product[] = [];
  displayProducts: Product[] = [];

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
    this.filterProducts();
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
        p => p.categoryId === this.selectedCategoryId
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
          this.sortOrder === 'asc' ? a.id - b.id : b.id - a.id
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

  toggleChatRoom() {
    this.showChatRoom = !this.showChatRoom;
    if (this.showChatRoom && this.chatMessages.length === 0) {
      // 初始化歡迎訊息
      this.chatMessages.push({
        text: '您好！有什麼可以為您服務的嗎？',
        sender: 'seller',
        time: new Date().toLocaleTimeString('zh-TW', { hour: '2-digit', minute: '2-digit' })
      });
    }
  }

  sendChatMessage() {
    if (this.newMessage.trim()) {
      // 添加使用者訊息
      this.chatMessages.push({
        text: this.newMessage,
        sender: 'user',
        time: new Date().toLocaleTimeString('zh-TW', { hour: '2-digit', minute: '2-digit' })
      });

      this.newMessage = '';

      // 模擬賣家回覆
      setTimeout(() => {
        this.chatMessages.push({
          text: '收到您的訊息，我會盡快回覆您！',
          sender: 'seller',
          time: new Date().toLocaleTimeString('zh-TW', { hour: '2-digit', minute: '2-digit' })
        });
      }, 1000);
    }
  }

  closeChatRoom() {
    this.showChatRoom = false;
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
