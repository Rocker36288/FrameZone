import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
interface CarouselImage {
  url: string;
  author: string;
  style: string;
}
@Component({
  selector: 'app-photographer-bookinghero',
  imports: [CommonModule, FormsModule],
  templateUrl: './photographer-bookinghero.component.html',
  styleUrl: './photographer-bookinghero.component.css',
})
export class PhotographerBookingheroComponent implements OnInit {
  dbImages: CarouselImage[] = [
    {
      url: '/images/Photographer/Carousel01.png',
      author: '林曉美',
      style: '空間攝影',
    },
    {
      url: '/images/Photographer/Carousel03.png',
      author: '張大衛',
      style: '活動紀錄',
    },
    {
      url: '/images/Photographer/Carousel02.png',
      author: '陳小雅',
      style: '商業產品',
    },
  ];

  currentIndex = 0;

  // 搜尋欄
  city = '';
  date = '';
  type = '';

  // 快速標籤
  tags = ['韓系婚紗', '寵物攝影', '日系寫真'];

  ngOnInit() {
    setInterval(() => this.nextSlide(), 5000);
  }

  nextSlide() {
    this.currentIndex = (this.currentIndex + 1) % this.dbImages.length;
  }

  onSearch() {
    if (!this.city || !this.date || !this.type) {
      alert('請完整選擇地區、日期與類型以查詢 2025 檔期');
      return;
    }
    alert(`正在搜尋 ${this.date} 有空的攝影師...`);
    console.log(
      `/search?city=${this.city}&date=${this.date}&type=${this.type}`
    );
  }

  quickSearch(tag: string) {
    alert('正在為您過濾：' + tag);
  }
}
