import { Component, EventEmitter, Input, Output } from '@angular/core';
import { NgClass, NgIf } from "@angular/common";

@Component({
  selector: 'app-videos-shared-modal',
  imports: [NgClass, NgIf],
  templateUrl: './videos-shared-modal.component.html',
  styleUrl: './videos-shared-modal.component.css'
})
export class VideosSharedModalComponent {

  @Input() shareUrl: string = '';
  @Output() closed = new EventEmitter<void>();

  isCopied = false;

  // 處理輸入框焦點事件
  onInputFocus(event: Event) {
    const input = event.target as HTMLInputElement;
    input.select();
  }

  // 複製連結
  copyLink(input: HTMLInputElement) {
    input.select();
    navigator.clipboard.writeText(this.shareUrl).then(() => {
      this.isCopied = true;
      setTimeout(() => this.isCopied = false, 3000);
    });
  }

  // 關閉模態框
  close() {
    // 你的關閉邏輯
    this.closed.emit();
  }

  // 分享到 Facebook
  shareToFacebook() {
    window.open(`https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(this.shareUrl)}`, '_blank');
  }

  // 分享到 Twitter
  shareToTwitter() {
    window.open(`https://twitter.com/intent/tweet?url=${encodeURIComponent(this.shareUrl)}`, '_blank');
  }

  // 分享到 LINE
  shareToLine() {
    window.open(`https://social-plugins.line.me/lineit/share?url=${encodeURIComponent(this.shareUrl)}`, '_blank');
  }

  // 透過電子郵件分享
  shareViaEmail() {
    window.location.href = `mailto:?subject=分享連結&body=${encodeURIComponent(this.shareUrl)}`;
  }
}
