import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ShoppingHeaderComponent } from "../shopping-header/shopping-header.component";
import { FooterComponent } from "../../shared/components/footer/footer.component";
import { FavoriteButtonComponent } from '../shared/components/favorite-button/favorite-button.component';
import { ToastNotificationComponent } from '../shared/components/toast-notification/toast-notification.component';

@Component({
  selector: 'app-shoppinghome',
  standalone: true,
  imports: [FormsModule, CommonModule, RouterLink, ShoppingHeaderComponent, FooterComponent, FavoriteButtonComponent, ToastNotificationComponent],
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
      id: 1,
      name: '相機',
      condition: '二手使用過',
      seller: 'Maryjo Lebarree',
      avatar: '/static/avatars/023m.jpg',
      image: 'images/products/1.jpg',
      isFavorite: false
    },
    {
      id: 2,
      name: '相機',
      condition: '二手使用過',
      seller: 'Maryjo Lebarree',
      avatar: '/static/avatars/023m.jpg',
      image: 'images/products/6.jpg',
      isFavorite: false
    },
    {
      id: 3,
      name: '相機',
      condition: '二手使用過',
      seller: 'Maryjo Lebarree',
      avatar: '/static/avatars/023m.jpg',
      image: 'images/products/4.jpg',
      isFavorite: false
    },
    {
      id: 4,
      name: '相機',
      condition: '二手使用過',
      seller: 'Maryjo Lebarree',
      avatar: '/static/avatars/023m.jpg',
      image: 'images/products/7.jpg',
      isFavorite: false
    },
    {
      id: 5,
      name: '相機',
      condition: '二手使用過',
      seller: 'Maryjo Lebarree',
      avatar: '/static/avatars/023m.jpg',
      image: 'images/products/1.jpg',
      isFavorite: false
    },
    {
      id: 6,
      name: '相機',
      condition: '二手使用過',
      seller: 'Maryjo Lebarree',
      avatar: '/static/avatars/023m.jpg',
      image: 'images/products/4.jpg',
      isFavorite: false
    },
    {
      id: 7,
      name: '相機',
      condition: '二手使用過',
      seller: 'Maryjo Lebarree',
      avatar: '/static/avatars/023m.jpg',
      image: 'images/products/4.jpg',
      isFavorite: false
    },
    {
      id: 8,
      name: '相機',
      condition: '二手使用過',
      seller: 'Maryjo Lebarree',
      avatar: '/static/avatars/023m.jpg',
      image: 'images/products/5.jpg',
      isFavorite: false
    },
    {
      id: 9,
      name: '相機',
      condition: '二手使用過',
      seller: 'Maryjo Lebarree',
      avatar: '/static/avatars/023m.jpg',
      image: 'images/products/8.jpg',
      isFavorite: false
    },
    {
      id: 10,
      name: '相機',
      condition: '二手使用過',
      seller: 'Maryjo Lebarree',
      avatar: '/static/avatars/023m.jpg',
      image: 'images/products/6.jpg',
      isFavorite: false
    },
    {
      id: 11,
      name: '相機',
      condition: '二手使用過',
      seller: 'Maryjo Lebarree',
      avatar: '/static/avatars/023m.jpg',
      image: 'images/products/7.jpg',
      isFavorite: false
    },
    {
      id: 12,
      name: '相機',
      condition: '二手使用過',
      seller: 'Maryjo Lebarree',
      avatar: '/static/avatars/023m.jpg',
      image: 'images/products/8.jpg',
      isFavorite: false
    }
  ];

  // 近期瀏覽商品
  recentlyViewed = [
    {
      id: 13,
      name: '相機',
      condition: '二手使用過',
      seller: 'Maryjo Lebarree',
      avatar: '/static/avatars/023m.jpg',
      image: 'images/products/1.jpg',
      isFavorite: false
    },
    {
      id: 14,
      name: '相機',
      condition: '二手使用過',
      seller: 'Maryjo Lebarree',
      avatar: '/static/avatars/023m.jpg',
      image: 'images/products/4.jpg',
      isFavorite: false
    },
    {
      id: 15,
      name: '相機',
      condition: '二手使用過',
      seller: 'Maryjo Lebarree',
      avatar: '/static/avatars/023m.jpg',
      image: 'images/products/5.jpg',
      isFavorite: false
    },
    {
      id: 16,
      name: '相機',
      condition: '二手使用過',
      seller: 'Maryjo Lebarree',
      avatar: '/static/avatars/023m.jpg',
      image: 'images/products/6.jpg',
      isFavorite: false
    },
    {
      id: 17,
      name: '相機',
      condition: '二手使用過',
      seller: 'Maryjo Lebarree',
      avatar: '/static/avatars/023m.jpg',
      image: 'images/products/7.jpg',
      isFavorite: false
    },
    {
      id: 18,
      name: '相機',
      condition: '二手使用過',
      seller: 'Maryjo Lebarree',
      avatar: '/static/avatars/023m.jpg',
      image: 'images/products/8.jpg',
      isFavorite: false
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
