import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { FooterComponent } from "../../shared/components/footer/footer.component";
import { AuthService } from '../../core/services/auth.service';
import { FaqService } from '../shared/services/faq.service';
import { Faq } from '../interfaces/faq';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-shopping-help-center',
  standalone: true,
  imports: [FormsModule, CommonModule, RouterLink, FooterComponent],
  templateUrl: './shopping-help-center.component.html',
  styleUrl: './shopping-help-center.component.css'
})
export class ShoppingHelpCenterComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

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

  faqs: Faq[] = [];
  allFaqs: Faq[] = []; // 儲存所有 FAQ 用於搜尋
  searchKeyword: string = ''; // 搜尋關鍵字

  // 會員資料
  memberAvatarUrl: string = '';
  memberName: string = '';

  constructor(
    private authService: AuthService,
    private faqService: FaqService
  ) { }

  ngOnInit(): void {
    // 訂閱會員資料
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        if (user) {
          this.memberName = user.account || user.displayName || '會員';
          if (user.avatar) {
            this.memberAvatarUrl = user.avatar;
          } else {
            const initial = (this.memberName || 'U').charAt(0).toUpperCase();
            this.memberAvatarUrl = `https://ui-avatars.com/api/?name=${encodeURIComponent(initial)}&background=667eea&color=fff&size=128`;
          }
        }
      });

    // 載入 FAQ 資料
    this.faqService.getFaqs().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.allFaqs = response.data;
          this.faqs = response.data; // 初始顯示所有 FAQ
        }
      },
      error: (err) => {
        console.error('載入 FAQ 失敗：', err);
        // 如果載入失敗，使用預設資料
        this.allFaqs = [
          { faqId: 1, systemId: 4, category: '新手教學', question: '第一次在這裡買東西，該注意什麼？', answer: '建議先查看賣家的評價與成交紀錄，並詳閱商品描述，善用聊聊功能詢問細節。', createdAt: new Date(), updatedAt: new Date() },
          { faqId: 2, systemId: 4, category: '交易付款', question: '請問有哪些支付方式可以選擇？', answer: '目前支援信用卡刷卡、ATM 轉帳以及各大便利商店(7-11/全家）的取貨付款服務。', createdAt: new Date(), updatedAt: new Date() },
          { faqId: 3, systemId: 4, category: '訂單物流', question: '請問下單後多久會出貨？', answer: '超商取貨固定為 60 元，郵寄或宅配則視包裹大小而定（通常為 80-150 元）。若符合全站免運活動則不收費。', createdAt: new Date(), updatedAt: new Date() },
          { faqId: 4, systemId: 4, category: '其他問題', question: '如果不喜歡收到的商品可以退貨嗎？', answer: '個人賣家商品無適用七天鑑賞期（除非商品描述與實物不符），建議下單前詳細閱讀賣場說明。', createdAt: new Date(), updatedAt: new Date() }
        ];
        this.faqs = this.allFaqs;
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  /**
   * 搜尋 FAQ
   */
  onSearch(value: string) {
    this.searchKeyword = value.trim();

    if (!this.searchKeyword) {
      // 如果搜尋關鍵字為空，顯示所有 FAQ
      this.faqs = this.allFaqs;
      return;
    }

    // 過濾 FAQ：搜尋問題或答案中包含關鍵字的項目
    const keyword = this.searchKeyword.toLowerCase();
    this.faqs = this.allFaqs.filter(faq =>
      faq.question.toLowerCase().includes(keyword) ||
      faq.answer.toLowerCase().includes(keyword) ||
      faq.category.toLowerCase().includes(keyword)
    );
  }

  /**
   * 清除搜尋
   */
  clearSearch() {
    this.searchKeyword = '';
    this.faqs = this.allFaqs;
  }

  /**
   * 將 FAQ 依分類分組，並按照指定順序排列
   */
  getFaqsByCategory(): { category: string; faqs: Faq[] }[] {
    const grouped = new Map<string, Faq[]>();

    this.faqs.forEach(faq => {
      if (!grouped.has(faq.category)) {
        grouped.set(faq.category, []);
      }
      grouped.get(faq.category)!.push(faq);
    });

    // 定義分類的顯示順序
    const categoryOrder = ['新手教學', '交易付款', '訂單物流', '優惠券活動', '其他問題'];

    // 將分類轉換為陣列並排序
    const result = Array.from(grouped.entries()).map(([category, faqs]) => ({
      category,
      faqs
    }));

    // 按照指定順序排序
    result.sort((a, b) => {
      const indexA = categoryOrder.indexOf(a.category);
      const indexB = categoryOrder.indexOf(b.category);

      // 如果分類在順序列表中，使用該順序
      if (indexA !== -1 && indexB !== -1) {
        return indexA - indexB;
      }
      // 如果只有 A 在列表中，A 排前面
      if (indexA !== -1) return -1;
      // 如果只有 B 在列表中，B 排前面
      if (indexB !== -1) return 1;
      // 都不在列表中，按字母順序
      return a.category.localeCompare(b.category);
    });

    return result;
  }
}
