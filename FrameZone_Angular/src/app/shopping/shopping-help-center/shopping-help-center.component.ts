import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { FooterComponent } from "../../shared/components/footer/footer.component";

@Component({
  selector: 'app-shopping-help-center',
  standalone: true,
  imports: [FormsModule, CommonModule, RouterLink, FooterComponent],
  templateUrl: './shopping-help-center.component.html',
  styleUrl: './shopping-help-center.component.css'
})
export class ShoppingHelpCenterComponent {
  categories = [
    {
      title: '新手教學',
      icon: 'ti-book-2', // 更精緻的書本圖示
      description: '快速了解購物流程'
    },
    {
      title: '訂單與物流',
      icon: 'ti-truck-delivery',
      description: '追蹤您的包裹狀態'
    },
    {
      title: '付款或帳務問題',
      icon: 'ti-credit-card',
      description: '支付方式與帳單說明'
    },
    {
      title: '政策',
      icon: 'ti-shield-check', // 盾牌圖示，代表安全與政策
      description: '了解服務條款與隱私'
    }
  ];

  faqs = [
    { id: 1, question: '如何取消我的訂單？', answer: '您可以在訂單進入「處理中」狀態前，至買家中心點選取消訂單。' },
    { id: 2, question: '退款需要多久時間？', answer: '一般信用卡退款約需 7-14 個工作天，視銀行作業時間而定。' },
    { id: 3, question: '運費如何計算？', answer: '全館滿 $999 免運費，未達門檻則酌收 $60 運費。' }
  ];

  // 範例頭像 URL，請替換為實際的會員服務獲取邏輯
  memberAvatarUrl: string = 'https://i.pravatar.cc/30?img=68';

  // 範例會員名稱
  memberName: string = 'Angular用戶001';

  onSearch(value: string) {
    if (!value.trim()) return;
    console.log('搜尋關鍵字:', value);
    // 這裡可以寫過濾 FAQ 的邏輯
  }
}
