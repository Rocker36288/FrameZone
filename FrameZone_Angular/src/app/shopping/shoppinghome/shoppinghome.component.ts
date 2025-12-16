import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ShoppingHeaderComponent } from "../shopping-header/shopping-header.component";

@Component({
  selector: 'app-shoppinghome',
  standalone: true,
  imports: [FormsModule, CommonModule, RouterLink, ShoppingHeaderComponent],
  templateUrl: './shoppinghome.component.html',
  styleUrl: './shoppinghome.component.css'
})
export class ShoppinghomeComponent {
  // 輪播圖片
  carouselImages = [
    { src: 'images/carousel/1.png', alt: '全站免運券' },
    { src: 'images/carousel/2.png', alt: '器材+配件' },
    { src: 'images/carousel/3.png', alt: '杜絕詐騙' }
  ];

  // 分類資料
  categories = [
    { name: '相機', image: 'images/1.jpg' },
    { name: '拍立得', image: 'images/1.jpg' },
    { name: '攝影機', image: 'images/1.jpg' },
    { name: '閃光燈', image: 'images/1.jpg' },
    { name: '腳架', image: 'images/1.jpg' },
    { name: '底片', image: 'images/1.jpg' },
    { name: '相機包', image: 'images/1.jpg' },
    { name: '創作', image: 'images/1.jpg' }
  ];

  // 熱門推薦商品
  popularProducts = [
    {
      id: 1,
      name: '相機',
      condition: '二手使用過',
      seller: 'Maryjo Lebarree',
      avatar: '/static/avatars/023m.jpg',
      image: 'images/1.jpg'
    },
    {
      id: 2,
      name: '相機',
      condition: '二手使用過',
      seller: 'Maryjo Lebarree',
      avatar: '/static/avatars/023m.jpg',
      image: 'images/1.jpg'
    },
    {
      id: 3,
      name: '相機',
      condition: '二手使用過',
      seller: 'Maryjo Lebarree',
      avatar: '/static/avatars/023m.jpg',
      image: 'images/1.jpg'
    },
    {
      id: 4,
      name: '相機',
      condition: '二手使用過',
      seller: 'Maryjo Lebarree',
      avatar: '/static/avatars/023m.jpg',
      image: 'images/1.jpg'
    },
    {
      id: 5,
      name: '相機',
      condition: '二手使用過',
      seller: 'Maryjo Lebarree',
      avatar: '/static/avatars/023m.jpg',
      image: 'images/1.jpg'
    },
    {
      id: 6,
      name: '相機',
      condition: '二手使用過',
      seller: 'Maryjo Lebarree',
      avatar: '/static/avatars/023m.jpg',
      image: 'images/1.jpg'
    },
    {
      id: 7,
      name: '相機',
      condition: '二手使用過',
      seller: 'Maryjo Lebarree',
      avatar: '/static/avatars/023m.jpg',
      image: 'images/1.jpg'
    },
    {
      id: 8,
      name: '相機',
      condition: '二手使用過',
      seller: 'Maryjo Lebarree',
      avatar: '/static/avatars/023m.jpg',
      image: 'images/1.jpg'
    },
    {
      id: 9,
      name: '相機',
      condition: '二手使用過',
      seller: 'Maryjo Lebarree',
      avatar: '/static/avatars/023m.jpg',
      image: 'images/1.jpg'
    },
    {
      id: 10,
      name: '相機',
      condition: '二手使用過',
      seller: 'Maryjo Lebarree',
      avatar: '/static/avatars/023m.jpg',
      image: 'images/1.jpg'
    },
    {
      id: 11,
      name: '相機',
      condition: '二手使用過',
      seller: 'Maryjo Lebarree',
      avatar: '/static/avatars/023m.jpg',
      image: 'images/1.jpg'
    },
    {
      id: 12,
      name: '相機',
      condition: '二手使用過',
      seller: 'Maryjo Lebarree',
      avatar: '/static/avatars/023m.jpg',
      image: 'images/1.jpg'
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
      image: 'images/1.jpg'
    },
    {
      id: 14,
      name: '相機',
      condition: '二手使用過',
      seller: 'Maryjo Lebarree',
      avatar: '/static/avatars/023m.jpg',
      image: 'images/1.jpg'
    },
    {
      id: 15,
      name: '相機',
      condition: '二手使用過',
      seller: 'Maryjo Lebarree',
      avatar: '/static/avatars/023m.jpg',
      image: 'images/1.jpg'
    },
    {
      id: 16,
      name: '相機',
      condition: '二手使用過',
      seller: 'Maryjo Lebarree',
      avatar: '/static/avatars/023m.jpg',
      image: 'images/1.jpg'
    },
    {
      id: 17,
      name: '相機',
      condition: '二手使用過',
      seller: 'Maryjo Lebarree',
      avatar: '/static/avatars/023m.jpg',
      image: 'images/1.jpg'
    },
    {
      id: 18,
      name: '相機',
      condition: '二手使用過',
      seller: 'Maryjo Lebarree',
      avatar: '/static/avatars/023m.jpg',
      image: 'images/1.jpg'
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
  onFavoriteClick(event: Event, product: any): void {
    event.preventDefault();
    event.stopPropagation();
    console.log('收藏商品:', product.name);
    // 處理收藏邏輯
  }

}
