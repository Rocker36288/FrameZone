import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ShoppingHeaderComponent } from "../shopping-header/shopping-header.component";
import { FooterComponent } from "../../shared/components/footer/footer.component";
import { FavoriteButtonComponent } from '../shared/components/favorite-button/favorite-button.component';
import { ToastNotificationComponent } from '../shared/components/toast-notification/toast-notification.component';
import { ProductCardComponent } from "../shared/components/product-card/product-card.component";

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
  selector: 'app-shoppinghome',
  standalone: true,
  imports: [FormsModule, CommonModule, RouterLink, ShoppingHeaderComponent, FooterComponent, FavoriteButtonComponent, ToastNotificationComponent, ProductCardComponent],
  templateUrl: './shoppinghome.component.html',
  styleUrl: './shoppinghome.component.css'
})
export class ShoppinghomeComponent {

  isFavorite: boolean = false;
  showToast: boolean = false;
  toastMessage: string = '';

  displayProducts: any[] = [];

  // 輪播圖片
  carouselImages = [
    { src: 'images/carousel/1.png', alt: '全站免運券' },
    { src: 'images/carousel/2.png', alt: '器材+配件' },
    { src: 'images/carousel/3.png', alt: '杜絕詐騙' }
  ];

  // 分類資料
  categories = [
    { name: '相機', image: 'images/products/1.jpg' },
    { name: '拍立得', image: 'images/products/2.jpg' },
    { name: '攝影機', image: 'images/products/4.jpg' },
    { name: '閃光燈', image: 'images/products/5.jpg' },
    { name: '腳架', image: 'images/products/6.jpg' },
    { name: '底片', image: 'images/products/7.jpg' },
    { name: '相機包', image: 'images/products/8.jpg' },
    { name: '創作', image: 'images/products/1.jpg' }
  ];

  // 熱門推薦商品
  popularProducts = [
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
    }
  ];

  // 近期瀏覽商品
  recentlyViewed = [
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
    }
  ];

  constructor() { }

  // 點擊分類
  onCategoryClick(category: any): void {
    console.log('選擇分類:', category.name);
    // 導航到分類頁面或篩選商品
  }

  // 點擊商品
  onProductClick(product: any): void {
    console.log('查看商品:', product.name);
    // 導航到商品詳情頁
  }

  // 收藏商品
  // toggleFavorite() {
  //   this.isFavorite = !this.isFavorite;
  //   const message = this.isFavorite ? '已成功加入收藏！' : '已從收藏移除。';
  //   this.showToastMessage(message);
  // }

  // toggleSimilarFavorite(product: any) {
  //   product.isFavorite = !product.isFavorite;
  //   const message = product.isFavorite ? `${product.name} 已收藏！` : `${product.name} 已移除收藏。`;
  //   this.showToastMessage(message);
  // }

  // showToastMessage(message: string) {
  //   this.toastMessage = message;
  //   this.showToast = true;
  //   console.log('顯示提示:', message); // 用於除錯
  //   setTimeout(() => {
  //     this.showToast = false;
  //   }, 2500);
  // }
}
