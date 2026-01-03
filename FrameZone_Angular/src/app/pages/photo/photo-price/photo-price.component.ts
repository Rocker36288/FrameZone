import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

interface PricingPlan {
  name: string;
  code: string;
  monthlyPrice: number;
  yearlyPrice: number;
  description: string;
  isFeatured: boolean;
  features: string[];
}

interface FeatureComparison {
  category: string;
  features: {
    name: string;
    free: boolean | string;
    basic: boolean | string;
    pro: boolean | string;
    enterprise: boolean | string;
  }[];
}

@Component({
  selector: 'app-photo-price',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './photo-price.component.html',
  styleUrl: './photo-price.component.css'
})
export class PhotoPriceComponent {
  // 計費週期切換
  isYearly = signal(false);

  // 方案資料
  plans: PricingPlan[] = [
    {
      name: '免費版',
      code: 'free',
      monthlyPrice: 0,
      yearlyPrice: 0,
      description: '個人輕量使用',
      isFeatured: false,
      features: [
        '500 張照片儲存',
        '基本分類功能',
        '相簿管理',
        '基礎搜尋',
        '社群支援'
      ]
    },
    {
      name: '基礎版',
      code: 'basic',
      monthlyPrice: 299,
      yearlyPrice: 2990,
      description: '個人深度使用',
      isFeatured: true,
      features: [
        '50,000 張照片儲存',
        '完整分類功能',
        '地理定位',
        '進階搜尋',
        '分享連結',
        '密碼保護',
        '優先客服'
      ]
    },
    {
      name: '進階版',
      code: 'pro',
      monthlyPrice: 599,
      yearlyPrice: 5990,
      description: '小型團隊使用',
      isFeatured: false,
      features: [
        '無限照片儲存',
        '所有分類功能',
        '團隊協作 (5人)',
        'API 整合',
        '自訂標籤系統',
        '進階分享設定',
        '專屬客服'
      ]
    },
    {
      name: '專業版',
      code: 'enterprise',
      monthlyPrice: 999,
      yearlyPrice: 9990,
      description: '大型企業使用',
      isFeatured: false,
      features: [
        '無限照片儲存',
        '所有分類功能',
        '無限團隊協作',
        '完整 API 存取',
        '自訂品牌化',
        'SSO 單一登入',
        '專屬客服經理',
        'SLA 服務保證'
      ]
    }
  ];

  // 詳細功能比較
  featureComparisons: FeatureComparison[] = [
    {
      category: '儲存空間',
      features: [
        { name: '照片儲存數量', free: '500 張', basic: '50,000 張', pro: '無限', enterprise: '無限' },
        { name: '單檔大小限制', free: '10 MB', basic: '50 MB', pro: '100 MB', enterprise: '200 MB' },
        { name: '影片上傳', free: false, basic: true, pro: true, enterprise: true },
        { name: 'RAW 格式支援', free: false, basic: true, pro: true, enterprise: true }
      ]
    },
    {
      category: '分類與管理',
      features: [
        { name: '基於 EXIF 分類', free: true, basic: true, pro: true, enterprise: true },
        { name: '地理定位', free: false, basic: true, pro: true, enterprise: true },
        { name: '階層式標籤', free: false, basic: true, pro: true, enterprise: true },
        { name: '自訂分類規則', free: false, basic: false, pro: true, enterprise: true },
        { name: '批次處理', free: false, basic: true, pro: true, enterprise: true }
      ]
    },
    {
      category: '搜尋與篩選',
      features: [
        { name: '基礎關鍵字搜尋', free: true, basic: true, pro: true, enterprise: true },
        { name: '進階篩選', free: false, basic: true, pro: true, enterprise: true },
        { name: '日期範圍搜尋', free: true, basic: true, pro: true, enterprise: true },
        { name: '地點搜尋', free: false, basic: true, pro: true, enterprise: true },
        { name: '多條件組合搜尋', free: false, basic: false, pro: true, enterprise: true }
      ]
    },
    {
      category: '分享與協作',
      features: [
        { name: '分享連結', free: false, basic: true, pro: true, enterprise: true },
        { name: '密碼保護', free: false, basic: true, pro: true, enterprise: true },
        { name: '有效期限設定', free: false, basic: true, pro: true, enterprise: true },
        { name: '團隊協作', free: false, basic: false, pro: '5 人', enterprise: '無限' },
        { name: '權限管理', free: false, basic: false, pro: true, enterprise: true },
        { name: '評論功能', free: false, basic: false, pro: true, enterprise: true }
      ]
    },
    {
      category: '安全性',
      features: [
        { name: 'SSL 加密傳輸', free: true, basic: true, pro: true, enterprise: true },
        { name: 'Azure 雲端備份', free: true, basic: true, pro: true, enterprise: true },
        { name: '雙因素驗證', free: false, basic: true, pro: true, enterprise: true },
        { name: 'IP 白名單', free: false, basic: false, pro: false, enterprise: true },
        { name: 'SSO 單一登入', free: false, basic: false, pro: false, enterprise: true },
        { name: '稽核日誌', free: false, basic: false, pro: true, enterprise: true }
      ]
    },
    {
      category: '整合與 API',
      features: [
        { name: 'API 存取', free: false, basic: false, pro: '限制', enterprise: '完整' },
        { name: 'Webhook', free: false, basic: false, pro: true, enterprise: true },
        { name: '第三方整合', free: false, basic: false, pro: '5 個', enterprise: '無限' },
        { name: '自訂開發', free: false, basic: false, pro: false, enterprise: true }
      ]
    },
    {
      category: '支援服務',
      features: [
        { name: '社群支援', free: true, basic: true, pro: true, enterprise: true },
        { name: 'Email 支援', free: false, basic: true, pro: true, enterprise: true },
        { name: '優先回應', free: false, basic: true, pro: true, enterprise: true },
        { name: '專屬客服', free: false, basic: false, pro: true, enterprise: true },
        { name: '電話支援', free: false, basic: false, pro: false, enterprise: true },
        { name: 'SLA 保證', free: false, basic: false, pro: false, enterprise: '99.9%' }
      ]
    }
  ];

  toggleBillingCycle() {
    this.isYearly.update(value => !value);
  }

  getPrice(plan: PricingPlan): number {
    return this.isYearly() ? plan.yearlyPrice : plan.monthlyPrice;
  }

  getMonthlyPrice(plan: PricingPlan): number {
    return this.isYearly() ? Math.round(plan.yearlyPrice / 12) : plan.monthlyPrice;
  }

  getSavings(plan: PricingPlan): number {
    if (plan.monthlyPrice === 0) return 0;
    const yearlyTotal = plan.monthlyPrice * 12;
    const savings = yearlyTotal - plan.yearlyPrice;
    return Math.round(savings);
  }
}
