import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';

interface Coupon {
  id: string;
  code: string;
  discount: number;
  type: 'percentage' | 'fixed';
  minAmount: number;
  usageLimit: number;
  usedCount: number;
  startDate: Date;
  endDate: Date;
  active: boolean;
}

@Component({
  selector: 'app-coupon-settings',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './coupon-settings.component.html',
  styleUrl: './coupon-settings.component.css'
})
export class CouponSettingsComponent {
  coupons: Coupon[] = [
    {
      id: '1',
      code: 'WELCOME100',
      discount: 100,
      type: 'fixed',
      minAmount: 500,
      usageLimit: 100,
      usedCount: 45,
      startDate: new Date('2024-12-01'),
      endDate: new Date('2024-12-31'),
      active: true
    },
    {
      id: '2',
      code: 'SALE20',
      discount: 20,
      type: 'percentage',
      minAmount: 1000,
      usageLimit: 50,
      usedCount: 32,
      startDate: new Date('2024-12-15'),
      endDate: new Date('2024-12-25'),
      active: true
    },
    {
      id: '3',
      code: 'NEWUSER',
      discount: 200,
      type: 'fixed',
      minAmount: 1000,
      usageLimit: 200,
      usedCount: 158,
      startDate: new Date('2024-11-01'),
      endDate: new Date('2024-11-30'),
      active: false
    }
  ];

  addCoupon(): void {
    alert('新增優惠券功能');
  }

  editCoupon(coupon: Coupon): void {
    alert(`編輯優惠券: ${coupon.code}`);
  }

  toggleCoupon(coupon: Coupon): void {
    coupon.active = !coupon.active;
    alert(`優惠券 ${coupon.code} 已${coupon.active ? '啟用' : '停用'}`);
  }

  deleteCoupon(coupon: Coupon): void {
    if (confirm(`確定要刪除優惠券「${coupon.code}」嗎？`)) {
      this.coupons = this.coupons.filter(c => c.id !== coupon.id);
    }
  }
}
