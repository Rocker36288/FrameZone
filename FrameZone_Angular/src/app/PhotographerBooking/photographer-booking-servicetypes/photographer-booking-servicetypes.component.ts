import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

@Component({
  selector: 'app-photographer-booking-servicetypes',
  imports: [CommonModule],
  templateUrl: './photographer-booking-servicetypes.component.html',
  styleUrl: './photographer-booking-servicetypes.component.css',
})
export class PhotographerBookingServicetypesComponent {
  // 當前展開的卡片索引（預設為第一張）
  activeCardIndex: number = 0;

  services = [
    {
      title: '婚禮攝影',
      desc: '紀錄婚禮當天的重要時刻與感動回憶',
      img: '/images/Photographer/Type-Wedding.png', // 圖片路徑
      link: '#',
    },
    {
      title: '產品攝影',
      desc: '拍攝高質感商品照片，提升銷售力',
      img: 'images/Photographer/Type-Product.png',
      link: '#',
    },
    {
      title: '商業活動攝影',
      desc: '完整記錄企業活動與品牌形象',
      img: 'images/Photographer/Type-Event.jpg',
      link: '#',
    },
    {
      title: '空間攝影',
      desc: '拍攝室內空間與建築展示照片',
      img: 'images/Photographer/Type-Space.png',
      link: '#',
    },
  ];

  // 設置當前活動卡片
  setActive(index: number): void {
    this.activeCardIndex = index;
  }
}
