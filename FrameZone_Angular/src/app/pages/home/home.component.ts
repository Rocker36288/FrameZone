import { Component, signal } from '@angular/core';
import { FooterComponent } from "../../shared/components/footer/footer.component";
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { interval } from 'rxjs';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [FooterComponent, FormsModule, CommonModule, RouterLink],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {
  // 隨機生成 150 顆星星的數據
  // stars = Array.from({ length: 150 }).map(() => ({
  //   style: {
  //     left: Math.random() * 100 + 'vw',
  //     top: Math.random() * 100 + 'vh',
  //     opacity: Math.random(),
  //     animationDelay: Math.random() * 5 + 's',
  //     transform: `scale(${Math.random() * 0.7 + 0.3})`
  //   }
  // }));

  //生成150顆星星固定位置
  stars: any[] = [];

  constructor() {
    this.generateFixedStars();
  }

  generateFixedStars() {
    // 使用一個固定的種子，例如 123
    let seed = 123;
    const pseudoRandom = () => {
      // 線性同餘生成器公式 (Linear Congruential Generator)
      seed = (seed * 9301 + 49297) % 233280;
      return seed / 233280;
    };

    this.stars = Array.from({ length: 150 }).map(() => ({
      style: {
        'left': pseudoRandom() * 100 + '%',
        'top': pseudoRandom() * 100 + '%',
        'opacity': pseudoRandom() * 0.7 + 0.3,
        'animation-delay': pseudoRandom() * 5 + 's',
        'transform': `scale(${pseudoRandom() * 0.8 + 0.2})`
      }
    }));
  }

  shootingStars = Array.from({ length: 5 }).map((_, index) => {
    const totalDuration = 10; // 固定 10 秒一個循環
    return {
      style: {
        'top': Math.random() * 50 + '%',
        'left': Math.random() * 100 + '%',
        // 重點：平均分配延遲，例如 5 顆流星，分別在第 0, 2, 4, 6, 8 秒出發
        'animation-delay': (index * (totalDuration / 5)) + 's',
        'animation-duration': totalDuration + 's',
        '--rotation': `${Math.random() * 30 - 60}deg`
      }
    };
  });

  photos = signal([
    // 左側照片 (M:起點, L:轉折點, L:終點)
    { id: 1, src: 'images/home/home1.png', label: '照片太多不好找？讓分類幫你一鍵整理！', link: '/photo-home', side: 'left', pos: { top: '20%', left: '10%' }, rot: '-8deg', path: 'M 0 50 L 150 50 L 150 150', dir: 'down' },
    { id: 2, src: 'images/home/home2.png', label: '攝影不只是技術，更是分享的藝術！', link: '/social/index', side: 'left', pos: { top: '45%', left: '5%' }, rot: '5deg', path: 'M 0 50 L 180 50 L 180 150', dir: 'down' },
    { id: 3, src: 'images/home/home3.png', label: '找器材、買周邊，攝影人的購物天堂！', link: '/shopping/home', side: 'left', pos: { top: '70%', left: '12%' }, rot: '-5deg', path: 'M 0 50 L 150 50 L 150 -50', dir: 'up' },

    // 右側照片
    { id: 4, src: 'images/home/home4.png', label: '讓影像說話，讓聲音留下故事！', link: '/videos', side: 'right', pos: { top: '30%', right: '8%' }, rot: '10deg', path: 'M 100 50 L -80 50 L -80 150', dir: 'down' },
    { id: 5, src: 'images/home/home5.png', label: '想拍就拍，預約你的專屬攝影時光！', link: '/photographer-bookinghome', side: 'right', pos: { top: '60%', right: '10%' }, rot: '-10deg', path: 'M 100 50 L -80 50 L -80 -50', dir: 'up' },
  ]);
}
