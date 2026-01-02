import { CommonModule } from '@angular/common';
import { Component, Input, OnChanges } from '@angular/core';
interface Service {
  serviceId: number;
  serviceName: string;
  basePrice: number;
  duration: number;
  deliveryDays: number;
  includedPhotos: number;
}
@Component({
  selector: 'app-photographer-bookingsidebar',
  imports: [CommonModule],
  templateUrl: './photographer-bookingsidebar.component.html',
  styleUrl: './photographer-bookingsidebar.component.css',
})
export class PhotographerBookingsidebarComponent implements OnChanges {
  @Input() selectedService: Service | null = null;

  selectedDate: Date | null = null;
  additionalFees: number = 0;
  discountAmount: number = 0;

  ngOnChanges(): void {
    // 當選擇的服務變更時,可以在這裡更新計算邏輯
  }

  get basePrice(): number {
    return this.selectedService?.basePrice || 0;
  }

  get totalPrice(): number {
    return this.basePrice + this.additionalFees - this.discountAmount;
  }

  onBookNow(): void {
    if (!this.selectedService) {
      alert('請先選擇拍攝方案');
      return;
    }

    if (!this.selectedDate) {
      alert('請選擇拍攝日期與時段');
      return;
    }

    // 實作預約邏輯
    console.log('預約資訊:', {
      service: this.selectedService,
      date: this.selectedDate,
      totalPrice: this.totalPrice,
    });

    alert('預約功能開發中...');
  }

  onSelectDate(): void {
    // 打開日期選擇器
    console.log('選擇日期');
    // 實際應用中會打開日曆組件
  }
}
