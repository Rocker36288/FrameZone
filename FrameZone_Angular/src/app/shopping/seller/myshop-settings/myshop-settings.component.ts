import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

interface ShopSettings {
  shopName: string;
  shopSlug: string;
  shopDescription: string;
  shopBanner: string;
  contactEmail: string;
  contactPhone: string;
  businessHours: string;
  address: string;
  returnPolicy: string;
  shippingPolicy: string;
  socialMedia: {
    facebook: string;
    instagram: string;
    line: string;
  };
  seoSettings: {
    metaTitle: string;
    metaDescription: string;
    metaKeywords: string;
  };
}

@Component({
  selector: 'app-myshop-settings',
  standalone: true,
  imports: [FormsModule, CommonModule, RouterLink],
  templateUrl: './myshop-settings.component.html',
  styleUrl: './myshop-settings.component.css'
})
export class MyshopSettingsComponent {
  settings: ShopSettings = {
    shopName: '我的精品小舖',
    shopSlug: 'my-boutique-shop',
    shopDescription: '提供優質商品,用心服務每一位顧客',
    shopBanner: '',
    contactEmail: 'shop@example.com',
    contactPhone: '0912-345-678',
    businessHours: '週一至週五 09:00-18:00',
    address: '台北市信義區信義路五段7號',
    returnPolicy: '提供7天鑑賞期,商品未使用可退換貨',
    shippingPolicy: '滿1000元免運費,一般商品3-5個工作天送達',
    socialMedia: {
      facebook: 'https://facebook.com/myshop',
      instagram: 'https://instagram.com/myshop',
      line: '@myshop'
    },
    seoSettings: {
      metaTitle: '我的精品小舖 - 優質商品專賣店',
      metaDescription: '提供各式優質商品,價格實惠,服務貼心',
      metaKeywords: '網路購物,優質商品,線上商店'
    }
  };

  activeTab: string = 'basic';
  logoPreview: string = 'https://via.placeholder.com/200x200?text=Logo';
  bannerPreview: string = 'https://via.placeholder.com/1200x300?text=Banner';

  ngOnInit(): void {
    // 初始化邏輯
  }

  switchTab(tab: string): void {
    this.activeTab = tab;
  }

  onLogoChange(event: any): void {
    const file = event.target.files[0];
    if (file) {
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.logoPreview = e.target.result;
      };
      reader.readAsDataURL(file);
    }
  }

  onBannerChange(event: any): void {
    const file = event.target.files[0];
    if (file) {
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.bannerPreview = e.target.result;
        this.settings.shopBanner = e.target.result;
      };
      reader.readAsDataURL(file);
    }
  }

  generateSlug(): void {
    const slug = this.settings.shopName
      .toLowerCase()
      .replace(/[^a-z0-9\u4e00-\u9fa5]+/g, '-')
      .replace(/^-+|-+$/g, '');
    this.settings.shopSlug = slug;
  }

  saveSettings(): void {
    console.log('Saving settings:', this.settings);
    alert('賣場設定已儲存!');
  }

  previewShop(): void {
    window.open(`/shop/${this.settings.shopSlug}`, '_blank');
  }
}
