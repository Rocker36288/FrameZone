import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

interface TeamMember {
  name: string;
  position: string;
  description: string;
  avatar: string;
  social?: {
    linkedin?: string;
    github?: string;
    email?: string;
  };
}

interface Feature {
  icon: string;
  title: string;
  description: string;
}

interface Stat {
  value: number;
  label: string;
  suffix: string;
  current: number;
}

@Component({
  selector: 'app-photo-about',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './photo-about.component.html',
  styleUrl: './photo-about.component.css'
})
export class PhotoAboutComponent implements OnInit {

  // ==================== 核心特色 ====================
  features = signal<Feature[]>([
    {
      icon: 'tag',
      title: '智能標籤管理',
      description: '基於 EXIF 資訊、拍攝地點、時間自動分類，支援階層式標籤系統'
    },
    {
      icon: 'shield-check',
      title: '安全可靠',
      description: 'Azure 雲端儲存，多重備份機制，確保您的珍貴回憶永不遺失'
    },
    {
      icon: 'devices',
      title: '跨平台同步',
      description: '無論使用電腦、手機或平板，隨時隨地存取您的照片庫'
    },
    {
      icon: 'share',
      title: '輕鬆分享',
      description: '一鍵產生分享連結，支援密碼保護和有效期限設定'
    },
    {
      icon: 'folder',
      title: '相簿整理',
      description: '彈性的相簿管理，支援公開/私密設定，輕鬆組織您的照片'
    },
    {
      icon: 'map-pin',
      title: '地理定位',
      description: '自動讀取 GPS 座標，在地圖上重溫您的旅程足跡'
    }
  ]);

  // ==================== 技術堆疊 ====================
  techStack = signal<Feature[]>([
    {
      icon: 'brand-angular',
      title: 'Angular 19',
      description: '最新版本前端框架，提供流暢的使用體驗'
    },
    {
      icon: 'brand-typescript',
      title: '.NET Core',
      description: '高效能後端 API，確保系統穩定運行'
    },
    {
      icon: 'cloud',
      title: 'Azure Cloud',
      description: '企業級雲端服務，全球 CDN 加速存取'
    },
    {
      icon: 'database',
      title: 'SQL Server',
      description: '可靠的資料庫系統，保障資料完整性'
    }
  ]);

  // ==================== 統計數據 ====================
  stats = signal<Stat[]>([
    { value: 10000, label: '活躍用戶', suffix: '+', current: 0 },
    { value: 5000000, label: '照片儲存', suffix: '+', current: 0 },
    { value: 50, label: '支援格式', suffix: '+', current: 0 },
    { value: 99.9, label: '系統穩定性', suffix: '%', current: 0 }
  ]);

  // ==================== 團隊成員 ====================
  teamMembers = signal<TeamMember[]>([
    {
      name: '陳建志',
      position: '創辦人 & CEO',
      description: '10年以上軟體開發經驗，專注於雲端服務與影像處理技術',
      avatar: 'https://ui-avatars.com/api/?name=Chen+Jian-Zhi&background=0D8ABC&color=fff&size=200',
      social: {
        linkedin: '#',
        github: '#',
        email: 'ceo@framezone.com'
      }
    },
    {
      name: '林雅婷',
      position: '技術長 CTO',
      description: '資深全端工程師，擅長系統架構設計與效能優化',
      avatar: 'https://ui-avatars.com/api/?name=Lin+Ya-Ting&background=6366F1&color=fff&size=200',
      social: {
        linkedin: '#',
        github: '#',
        email: 'cto@framezone.com'
      }
    },
    {
      name: '王大明',
      position: 'UX/UI 設計師',
      description: '注重使用者體驗，致力於打造直覺易用的介面',
      avatar: 'https://ui-avatars.com/api/?name=Wang+Da-Ming&background=EC4899&color=fff&size=200',
      social: {
        linkedin: '#',
        email: 'design@framezone.com'
      }
    },
    {
      name: '張小華',
      position: '產品經理',
      description: '深入了解用戶需求，推動產品持續創新與優化',
      avatar: 'https://ui-avatars.com/api/?name=Zhang+Xiao-Hua&background=F59E0B&color=fff&size=200',
      social: {
        linkedin: '#',
        email: 'pm@framezone.com'
      }
    }
  ]);

  // ==================== Lifecycle ====================

  ngOnInit(): void {
    this.startCountAnimation();
  }

  // ==================== 數字動畫 ====================

  private startCountAnimation(): void {
    const duration = 2000; // 2 秒
    const steps = 60; // 60 步
    const interval = duration / steps;

    const stats = this.stats();

    stats.forEach((stat, index) => {
      let currentStep = 0;

      const timer = setInterval(() => {
        currentStep++;
        const progress = currentStep / steps;
        const easeProgress = this.easeOutQuart(progress);

        stat.current = Math.floor(stat.value * easeProgress);

        if (currentStep >= steps) {
          stat.current = stat.value;
          clearInterval(timer);
        }

        // 更新 signal
        this.stats.set([...stats]);
      }, interval);
    });
  }

  // 緩動函數
  private easeOutQuart(x: number): number {
    return 1 - Math.pow(1 - x, 4);
  }

  // ==================== 格式化數字 ====================

  formatNumber(num: number): string {
    if (num >= 1000000) {
      return (num / 1000000).toFixed(1) + 'M';
    }
    if (num >= 1000) {
      return (num / 1000).toFixed(1) + 'K';
    }
    return num.toString();
  }

  // ==================== 輔助方法 ====================

  scrollToContact(): void {
    const contactSection = document.getElementById('contact-section');
    if (contactSection) {
      contactSection.scrollIntoView({ behavior: 'smooth' });
    }
  }
}
