import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

interface MenuItem {
  icon: string;
  label: string;
  route?: string;
  badge?: number;
  children?: MenuItem[];
  expanded?: boolean;
}

@Component({
  selector: 'app-seller-sidebar',
  standalone: true,
  imports: [FormsModule, CommonModule, RouterLink],
  templateUrl: './seller-sidebar.component.html',
  styleUrl: './seller-sidebar.component.css'
})
export class SellerSidebarComponent {
  // 使用 @Input 裝飾器接收來自父元件的狀態
  @Input() isCollapsed: boolean = false;

  menuItems: MenuItem[] = [
    {
      icon: 'building-store',
      label: '賣場管理',
      expanded: false,
      children: [
        { icon: 'settings', label: '我的賣場設定', route: '/seller/myshop-settings' },
        { icon: 'category', label: '分類設定', route: '/seller/category-settings' }
      ]
    },
    {
      icon: 'package',
      label: '商品管理',
      expanded: false,
      children: [
        { icon: 'list', label: '全部', route: '/seller/products/all' },
        { icon: 'eye', label: '上架中', route: '/seller/products/active' },
        { icon: 'eye-off', label: '未上架', route: '/seller/products/inactive' },
        { icon: 'alert-triangle', label: '違規/刪除', route: '/seller/products/violation' },
        { icon: 'clock', label: '審核中', route: '/seller/products/pending' }
      ]
    },
    {
      icon: 'shopping-cart',
      label: '訂單管理',
      badge: 8,
      expanded: false,
      children: [
        { icon: 'list', label: '全部', route: '/seller/orders/all' },
        { icon: 'credit-card-off', label: '未付款', route: '/seller/orders/unpaid' },
        { icon: 'package', label: '待出貨', route: '/seller/orders/pending' },
        { icon: 'truck', label: '運送中', route: '/seller/orders/shipping' },
        { icon: 'circle-check', label: '已完成', route: '/seller/orders/completed' },
        { icon: 'x', label: '不成立', route: '/seller/orders/cancelled' }
      ]
    },
    {
      icon: 'star',
      label: '評價管理',
      route: '/seller/reviews',
      badge: 3
    },
    {
      icon: 'wallet',
      label: '財務管理',
      expanded: false,
      children: [
        { icon: 'wallet', label: '我的錢包', route: '/seller/wallet' },
        { icon: 'building-bank', label: '銀行帳號', route: '/seller/bank-account' }
      ]
    },
    {
      icon: 'chart-line',
      label: '管理表現',
      route: '/seller/dashboard'
    },
    {
      icon: 'settings',
      label: '細項設定',
      expanded: false,
      children: [
        { icon: 'ticket', label: '優惠券設定', route: '/seller/settings/coupons' },
        { icon: 'truck', label: '物流設定', route: '/seller/settings/shipping' },
        { icon: 'credit-card', label: '金流設定', route: '/seller/settings/payment' },
        { icon: 'message-circle', label: '聊聊設定', route: '/seller/settings/chat' }
      ]
    }
  ];

  // toggleSubmenu(item: MenuItem): void {
  //   if (item.children) {
  //     item.expanded = !item.expanded;
  //   }
  // }

  toggleSubmenu(item: any) {
    // 如果目前是收合狀態，點擊時可以考慮先展開側邊欄
    if (this.isCollapsed) return;
    item.expanded = !item.expanded;
  }
}
