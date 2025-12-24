import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';

interface PaymentMethod {
  id: string;
  name: string;
  type: 'credit_card' | 'atm' | 'ewallet' | 'cod';
  provider: string;
  description: string;
  transactionFee: number;
  feeType: 'percentage' | 'fixed';
  isEnabled: boolean;
  icon: string;
  processingTime: string;
}

@Component({
  selector: 'app-payment-settings',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './payment-settings.component.html',
  styleUrl: './payment-settings.component.css'
})
export class PaymentSettingsComponent {
  paymentMethods: PaymentMethod[] = [
    {
      id: '1',
      name: '信用卡付款',
      type: 'credit_card',
      provider: '綠界科技',
      description: 'Visa、Mastercard、JCB',
      transactionFee: 2.8,
      feeType: 'percentage',
      isEnabled: true,
      icon: 'credit-card',
      processingTime: '即時'
    },
    {
      id: '2',
      name: 'ATM 轉帳',
      type: 'atm',
      provider: '綠界科技',
      description: '虛擬帳號轉帳',
      transactionFee: 10,
      feeType: 'fixed',
      isEnabled: true,
      icon: 'building-bank',
      processingTime: '1-3 工作天'
    },
    {
      id: '3',
      name: 'LINE Pay',
      type: 'ewallet',
      provider: 'LINE Pay',
      description: '使用 LINE Pay 快速付款',
      transactionFee: 2.5,
      feeType: 'percentage',
      isEnabled: false,
      icon: 'wallet',
      processingTime: '即時'
    },
    {
      id: '4',
      name: '貨到付款',
      type: 'cod',
      provider: '自行處理',
      description: '收到商品時再付款',
      transactionFee: 30,
      feeType: 'fixed',
      isEnabled: true,
      icon: 'truck-delivery',
      processingTime: '送達時'
    }
  ];

  ngOnInit(): void {
    // 初始化邏輯
    this.loadPaymentSettings();
  }

  toggleMethod(method: PaymentMethod): void {
    method.isEnabled = !method.isEnabled;
    this.savePaymentSettings();

    // 可以在這裡加入通知訊息
    console.log(`${method.name} 已${method.isEnabled ? '啟用' : '停用'}`);
  }

  getMethodsByType(type: string): PaymentMethod[] {
    return this.paymentMethods.filter(m => m.type === type);
  }

  private loadPaymentSettings(): void {
    // 從後端或 localStorage 載入設定
    const savedSettings = localStorage.getItem('paymentSettings');
    if (savedSettings) {
      try {
        const settings = JSON.parse(savedSettings);
        this.paymentMethods.forEach(method => {
          const saved = settings.find((s: PaymentMethod) => s.id === method.id);
          if (saved) {
            method.isEnabled = saved.isEnabled;
          }
        });
      } catch (error) {
        console.error('載入金流設定失敗:', error);
      }
    }
  }

  private savePaymentSettings(): void {
    // 儲存到後端或 localStorage
    try {
      const settings = this.paymentMethods.map(m => ({
        id: m.id,
        isEnabled: m.isEnabled
      }));
      localStorage.setItem('paymentSettings', JSON.stringify(settings));
    } catch (error) {
      console.error('儲存金流設定失敗:', error);
    }
  }
}
