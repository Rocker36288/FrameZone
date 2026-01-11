import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FooterComponent } from "../../shared/components/footer/footer.component";

interface Message {
  id: number;
  content: string;
  timestamp: Date;
  isOwn: boolean;
}

interface Chat {
  id: number;
  user: {
    name: string;
    avatar: string;
  };
  product: {
    name: string;
    image: string;
  };
  lastMessage: string;
  lastMessageTime: string;
  unreadCount: number;
  isOnline: boolean;
  messages: Message[];
}

@Component({
  selector: 'app-shopping-chats',
  standalone: true,
  imports: [FormsModule, CommonModule, FooterComponent],
  templateUrl: './shopping-chats.component.html',
  styleUrl: './shopping-chats.component.css'
})
export class ShoppingChatsComponent {
  chats: Chat[] = [];
  selectedChat: Chat | null = null;
  newMessage: string = '';
  searchText: string = '';

  ngOnInit(): void {
    this.generateChats();
  }

  generateChats(): void {
    const products = ['相機', '鏡頭', '腳架', '記憶卡', '閃光燈', '背包'];
    const lastMessages = [
      '請問還有現貨嗎？',
      '可以算便宜一點嗎？',
      '什麼時候可以出貨？',
      '商品狀況如何？',
      '可以面交嗎？',
      '運費怎麼算？',
      '好的，謝謝！',
      '我要購買'
    ];

    const messageTemplates = [
      '你好，請問這個商品還有貨嗎？',
      '價格可以再優惠嗎？',
      '商品的狀況怎麼樣？',
      '什麼時候可以寄出？',
      '好的，謝謝你的回覆',
      '了解，我再考慮看看',
      '可以提供更多照片嗎？',
      '運送方式有哪些？'
    ];

    for (let i = 1; i <= 20; i++) {
      const messages: Message[] = [];
      const messageCount = Math.floor(Math.random() * 5) + 3;

      for (let j = 1; j <= messageCount; j++) {
        messages.push({
          id: j,
          content: messageTemplates[Math.floor(Math.random() * messageTemplates.length)],
          timestamp: new Date(Date.now() - (messageCount - j) * 60000),
          isOwn: j % 2 === 0
        });
      }

      this.chats.push({
        id: i,
        user: {
          name: `使用者${i}`,
          avatar: `https://ui-avatars.com/api/?name=${encodeURIComponent(`使用者${i}`.charAt(0).toUpperCase())}&background=667eea&color=fff&size=128`
        },
        product: {
          name: products[Math.floor(Math.random() * products.length)],
          image: `https://images.unsplash.com/photo-${this.getRandomPhotoId()}?w=100`
        },
        lastMessage: lastMessages[Math.floor(Math.random() * lastMessages.length)],
        lastMessageTime: this.getTimeString(i),
        unreadCount: Math.floor(Math.random() * 5),
        isOnline: Math.random() > 0.5,
        messages: messages
      });
    }

    // 預設選擇第一個聊天
    if (this.chats.length > 0) {
      this.selectChat(this.chats[0]);
    }
  }

  getRandomPhotoId(): string {
    const photoIds = [
      '1606982801255-5ec8de5fee3f',
      '1526170375885-4d8ecf77b99f',
      '1495121605193-b116b5b9c5fe',
      '1502920917128-1aa500764cbd'
    ];
    return photoIds[Math.floor(Math.random() * photoIds.length)];
  }

  getTimeString(index: number): string {
    if (index === 1) return '剛剛';
    if (index <= 3) return `${index} 分鐘前`;
    if (index <= 8) return `${index * 5} 分鐘前`;
    if (index <= 12) return `${index - 8} 小時前`;
    return `${index - 12} 天前`;
  }

  selectChat(chat: Chat): void {
    this.selectedChat = chat;
    chat.unreadCount = 0;
  }

  sendMessage(): void {
    if (this.newMessage.trim() && this.selectedChat) {
      const message: Message = {
        id: this.selectedChat.messages.length + 1,
        content: this.newMessage,
        timestamp: new Date(),
        isOwn: true
      };

      this.selectedChat.messages.push(message);
      this.selectedChat.lastMessage = this.newMessage;
      this.selectedChat.lastMessageTime = '剛剛';
      this.newMessage = '';

      // 滾動到底部
      setTimeout(() => {
        const messagesContainer = document.querySelector('.messages-container');
        if (messagesContainer) {
          messagesContainer.scrollTop = messagesContainer.scrollHeight;
        }
      }, 100);
    }
  }

  formatTime(date: Date): string {
    return date.toLocaleTimeString('zh-TW', { hour: '2-digit', minute: '2-digit' });
  }

  get filteredChats(): Chat[] {
    if (!this.searchText.trim()) {
      return this.chats;
    }

    const search = this.searchText.toLowerCase();
    return this.chats.filter(chat =>
      chat.user.name.toLowerCase().includes(search) ||
      chat.product.name.toLowerCase().includes(search) ||
      chat.lastMessage.toLowerCase().includes(search)
    );
  }
}
