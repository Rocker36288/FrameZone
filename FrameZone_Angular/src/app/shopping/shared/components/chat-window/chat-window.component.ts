import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, ElementRef, Input, NgZone, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { ChatStateService } from '../../services/chat-state.service';

interface Message {
  text?: string;
  image?: string;
  product?: {      // 支援商品小卡
    id: number;
    name: string;
    price: number;
    image: string;
  };
  sender: 'me' | 'seller';
  time: string;
}

@Component({
  selector: 'app-chat-window',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './chat-window.component.html',
  styleUrl: './chat-window.component.css'
})
export class ChatWindowComponent {
  @ViewChild('scrollContainer') private scrollContainer!: ElementRef;
  @ViewChild('fileInput') fileInput!: ElementRef;

  /** UI 狀態（由 Service 控制） */
  isOpen = false;
  openedFromProduct = false;

  /** Chat 狀態 */
  sellerName = '客服人員';
  newMessage = '';
  previewImages: string[] = [];
  isStickerPanelOpen = false;
  lightboxImage: string | null = null;
  unreadMessages = false;



  messages: Message[] = [
    { text: '您好！有什麼可以為您服務的嗎？', sender: 'seller', time: '下午 05:29' }
  ];

  // 假資料：貼圖清單 (之後可換成您自己的 assets 路徑)
  stickers = [
    'https://cdn-icons-png.flaticon.com/512/4710/4710801.png',
    'https://cdn-icons-png.flaticon.com/512/4710/4710804.png',
    'https://cdn-icons-png.flaticon.com/512/4710/4710810.png',
    'https://cdn-icons-png.flaticon.com/512/4710/4710815.png',
    'https://cdn-icons-png.flaticon.com/512/4710/4710820.png',
    'https://cdn-icons-png.flaticon.com/512/4710/4710826.png',
    'https://cdn-icons-png.flaticon.com/512/4710/4710831.png',
    'https://cdn-icons-png.flaticon.com/512/4710/4710836.png',
  ];

  // --- 用來追蹤最後帶入的商品ID，避免重複發送 ---
  private lastProductId: number | null = null;
  private subs = new Subscription();

  constructor(
    private chatState: ChatStateService,
    private cdr: ChangeDetectorRef
  ) { }

  // ==============================
  // Lifecycle
  // ==============================
  ngOnInit(): void {
    this.subs.add(
      this.chatState.isOpen$.subscribe(v => {
        this.isOpen = v;
        this.cdr.detectChanges();
      })
    );

    this.subs.add(
      this.chatState.openedFromProduct$.subscribe(v => {
        this.openedFromProduct = v;
      })
    );

    this.subs.add(
      this.chatState.product$.subscribe(product => {
        if (product) {
          this.openWithProduct(product);
        }
      })
    );
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }

  // ==============================
  // UI 行為
  // ==============================
  openFloatingChat() {
    this.chatState.openFromFloating();
    this.unreadMessages = false;
  }

  closeChat() {
    this.chatState.close();
    this.isStickerPanelOpen = false;
  }

  // --- 外部呼叫方法：開啟並帶入商品 ---
  openWithProduct(product: any) {

    // 只有在商品ID不同時才發送新訊息
    if (this.lastProductId === product.id) return;

    const time = this.getCurrentTime();

    // 自動發送一條詢問文字
    this.messages.push({
      text: `您好！我想詢問這件商品：${product.name}`,
      sender: 'me',
      time
    });

    // 帶入商品小卡
    this.messages.push({
      product: {
        id: product.id,
        name: product.name,
        price: product.price,
        image: product.image
      },
      sender: 'me',
      time
    });

    this.lastProductId = product.id;

    setTimeout(() => this.scrollToBottom(), 50);
  }

  // ==============================
  // 訊息
  // ==============================
  sendMessage() {
    if (!this.newMessage.trim() && this.previewImages.length === 0) return;

    // 先送文字（如果有）
    if (this.newMessage.trim()) {
      this.messages.push({
        text: this.newMessage.trim(),
        sender: 'me',
        time: this.getCurrentTime(),
      });
    }

    // 再送圖片（一張一則）
    this.previewImages.forEach(img => {
      this.messages.push({
        image: img,
        sender: 'me',
        time: this.getCurrentTime(),
      });
    });

    this.newMessage = '';
    this.previewImages = [];
    this.isStickerPanelOpen = false;
    if (this.fileInput) this.fileInput.nativeElement.value = '';

    setTimeout(() => this.scrollToBottom(), 50);
  }

  sendSticker(url: string) {
    this.messages.push({
      image: url,
      sender: 'me',
      time: this.getCurrentTime(),
    });
    this.isStickerPanelOpen = false;
    setTimeout(() => this.scrollToBottom(), 50);
  }

  // ==============================
  // 工具
  // ==============================
  triggerFileUpload() {
    this.fileInput.nativeElement.click();
  }

  onFileSelected(event: any) {
    const files: File[] = Array.from(event.target.files || []);
    if (!files.length) return;

    files.forEach(file => {
      if (!file.type.startsWith('image/')) return;

      const reader = new FileReader();
      reader.onload = e => {
        this.previewImages.push(e.target?.result as string);
        this.cdr.detectChanges();
      };
      reader.readAsDataURL(file);
    });

    // 讓同一張圖可重選
    event.target.value = '';
  }

  toggleStickerPanel(event: Event) {
    event.stopPropagation();
    this.isStickerPanelOpen = !this.isStickerPanelOpen;
  }

  private scrollToBottom() {
    if (!this.scrollContainer) return;
    const el = this.scrollContainer.nativeElement;
    el.scrollTop = el.scrollHeight;
  }

  private getCurrentTime(): string {
    return new Date().toLocaleTimeString('zh-TW', {
      hour: '2-digit',
      minute: '2-digit',
      hour12: true,
    });
  }

  // 新訊息時呼叫
  markUnread() {
    this.unreadMessages = true;
  }
}
