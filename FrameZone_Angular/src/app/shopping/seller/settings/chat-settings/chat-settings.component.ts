import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';

interface ChatSettings {
  autoReply: boolean;
  autoReplyMessage: string;
  workingHours: {
    enabled: boolean;
    start: string;
    end: string;
    days: string[];
  };
  quickReplies: QuickReply[];
  notifications: {
    email: boolean;
    browser: boolean;
    sound: boolean;
  };
  blockedWords: string[];
  chatbotEnabled: boolean;
}

interface QuickReply {
  id: string;
  title: string;
  message: string;
  category: string;
}

@Component({
  selector: 'app-chat-settings',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './chat-settings.component.html',
  styleUrl: './chat-settings.component.css'
})
export class ChatSettingsComponent {
  settings: ChatSettings = {
    autoReply: true,
    autoReplyMessage: '您好!我們已收到您的訊息,客服人員將盡快回覆您。',
    workingHours: {
      enabled: true,
      start: '09:00',
      end: '18:00',
      days: ['週一', '週二', '週三', '週四', '週五']
    },
    quickReplies: [
      {
        id: '1',
        title: '歡迎訊息',
        message: '您好!歡迎光臨本店,有什麼可以為您服務的嗎?',
        category: '問候'
      },
      {
        id: '2',
        title: '訂單查詢',
        message: '請提供您的訂單編號,我將為您查詢訂單狀態。',
        category: '訂單'
      },
      {
        id: '3',
        title: '退換貨說明',
        message: '我們提供7天鑑賞期,如需退換貨請參考我們的退換貨政策。',
        category: '售後'
      },
      {
        id: '4',
        title: '運送時間',
        message: '一般商品會在3-5個工作天內送達,預購商品請以商品頁說明為準。',
        category: '物流'
      }
    ],
    notifications: {
      email: true,
      browser: true,
      sound: true
    },
    blockedWords: ['廣告', '詐騙', '騷擾'],
    chatbotEnabled: false
  };

  showQuickReplyModal = false;
  showBlockWordModal = false;
  currentQuickReply: QuickReply | null = null;
  newQuickReply: QuickReply = {
    id: '',
    title: '',
    message: '',
    category: '其他'
  };
  newBlockWord = '';

  weekDays = ['週一', '週二', '週三', '週四', '週五', '週六', '週日'];
  categories = ['問候', '訂單', '售後', '物流', '付款', '其他'];

  ngOnInit(): void {
    // 初始化邏輯
  }

  // 工作時間設定
  toggleWorkingHours(): void {
    this.settings.workingHours.enabled = !this.settings.workingHours.enabled;
  }

  toggleDay(day: string): void {
    const index = this.settings.workingHours.days.indexOf(day);
    if (index > -1) {
      this.settings.workingHours.days.splice(index, 1);
    } else {
      this.settings.workingHours.days.push(day);
    }
  }

  isDaySelected(day: string): boolean {
    return this.settings.workingHours.days.includes(day);
  }

  // 快速回覆管理
  openQuickReplyModal(reply?: QuickReply): void {
    if (reply) {
      this.currentQuickReply = { ...reply };
      this.newQuickReply = {
        id: Date.now().toString(),
        title: '',
        message: '',
        category: '其他'
      };
    } else {
      this.newQuickReply = {
        id: Date.now().toString(),
        title: '',
        message: '',
        category: '其他'
      };
      this.currentQuickReply = null;
    }
    this.showQuickReplyModal = true;
  }

  get modalQuickReply(): QuickReply {
    return this.currentQuickReply || this.newQuickReply;
  }

  closeQuickReplyModal(): void {
    this.showQuickReplyModal = false;
    this.currentQuickReply = null;
  }

  saveQuickReply(): void {
    if (this.currentQuickReply) {
      const index = this.settings.quickReplies.findIndex(r => r.id === this.currentQuickReply!.id);
      if (index !== -1) {
        this.settings.quickReplies[index] = { ...this.currentQuickReply };
      }
    } else {
      this.settings.quickReplies.push({ ...this.newQuickReply });
    }
    this.closeQuickReplyModal();
  }

  deleteQuickReply(id: string): void {
    if (confirm('確定要刪除此快速回覆嗎?')) {
      this.settings.quickReplies = this.settings.quickReplies.filter(r => r.id !== id);
    }
  }

  // 封鎖字詞管理
  openBlockWordModal(): void {
    this.newBlockWord = '';
    this.showBlockWordModal = true;
  }

  closeBlockWordModal(): void {
    this.showBlockWordModal = false;
  }

  addBlockWord(): void {
    if (this.newBlockWord.trim() && !this.settings.blockedWords.includes(this.newBlockWord.trim())) {
      this.settings.blockedWords.push(this.newBlockWord.trim());
      this.closeBlockWordModal();
    }
  }

  removeBlockWord(word: string): void {
    this.settings.blockedWords = this.settings.blockedWords.filter(w => w !== word);
  }

  // 儲存設定
  saveSettings(): void {
    alert('設定已儲存!');
    console.log('Saved settings:', this.settings);
  }
}
