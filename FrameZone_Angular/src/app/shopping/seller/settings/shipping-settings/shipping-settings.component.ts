import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';

interface ShippingMethod {
  id: string;
  name: string;
  provider: string;
  description: string;
  basePrice: number;
  estimatedDays: string;
  isEnabled: boolean;
  freeShippingThreshold?: number;
  coverageAreas: string[];
}

@Component({
  selector: 'app-shipping-settings',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './shipping-settings.component.html',
  styleUrl: './shipping-settings.component.css'
})
export class ShippingSettingsComponent {
  shippingMethods: ShippingMethod[] = [
    {
      id: '1',
      name: '7-11 取貨',
      provider: '7-ELEVEN',
      description: '便利商店取貨付款',
      basePrice: 60,
      estimatedDays: '2-3',
      isEnabled: true,
      freeShippingThreshold: 1000,
      coverageAreas: ['台灣本島', '澎湖', '金門', '馬祖']
    },
    {
      id: '2',
      name: '全家取貨',
      provider: 'FamilyMart',
      description: '便利商店取貨付款',
      basePrice: 60,
      estimatedDays: '2-3',
      isEnabled: true,
      freeShippingThreshold: 1000,
      coverageAreas: ['台灣本島', '澎湖', '金門', '馬祖']
    },
    {
      id: '3',
      name: '宅配',
      provider: '黑貓宅急便',
      description: '宅配到府服務',
      basePrice: 100,
      estimatedDays: '1-2',
      isEnabled: true,
      freeShippingThreshold: 1500,
      coverageAreas: ['台灣本島']
    },
    {
      id: '4',
      name: '郵局配送',
      provider: '中華郵政',
      description: '郵局包裹配送',
      basePrice: 80,
      estimatedDays: '3-5',
      isEnabled: false,
      coverageAreas: ['台灣本島', '離島']
    }
  ];

  showAddModal = false;
  showEditModal = false;
  currentMethod: ShippingMethod | null = null;

  newMethod: ShippingMethod = {
    id: '',
    name: '',
    provider: '',
    description: '',
    basePrice: 0,
    estimatedDays: '',
    isEnabled: true,
    coverageAreas: []
  };

  ngOnInit(): void {
    // 初始化邏輯
  }

  openAddModal(): void {
    this.newMethod = {
      id: Date.now().toString(),
      name: '',
      provider: '',
      description: '',
      basePrice: 0,
      estimatedDays: '',
      isEnabled: true,
      coverageAreas: []
    };
    this.showAddModal = true;
  }

  openEditModal(method: ShippingMethod): void {
    this.currentMethod = { ...method };
    this.showEditModal = true;
  }

  closeAddModal(): void {
    this.showAddModal = false;
  }

  closeEditModal(): void {
    this.showEditModal = false;
    this.currentMethod = null;
  }

  saveNewMethod(): void {
    this.shippingMethods.push({ ...this.newMethod });
    this.closeAddModal();
  }

  saveEditMethod(): void {
    if (this.currentMethod) {
      const index = this.shippingMethods.findIndex(m => m.id === this.currentMethod!.id);
      if (index !== -1) {
        this.shippingMethods[index] = { ...this.currentMethod };
      }
    }
    this.closeEditModal();
  }

  toggleMethod(method: ShippingMethod): void {
    method.isEnabled = !method.isEnabled;
  }

  deleteMethod(id: string): void {
    if (confirm('確定要刪除此物流方式嗎?')) {
      this.shippingMethods = this.shippingMethods.filter(m => m.id !== id);
    }
  }
}
