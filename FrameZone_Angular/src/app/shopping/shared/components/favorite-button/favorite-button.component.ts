import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { ToastService } from '../../services/toast.service';
import { FavoriteService } from '../../services/favorite.service';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-favorite-button',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './favorite-button.component.html',
  styleUrl: './favorite-button.component.css'
})
export class FavoriteButtonComponent {
  @Input() isFavorite: boolean = false;
  @Input() itemName: string = '';
  @Input() productId: number = 0;
  @Output() favoriteChange = new EventEmitter<boolean>();

  constructor(
    private toastService: ToastService,
    private favoriteService: FavoriteService,
    private authService: AuthService
  ) { }

  get isAuthenticated(): boolean {
    return this.authService.isAuthenticated();
  }

  toggleFavorite(event: Event) {
    event.preventDefault();
    event.stopPropagation();

    // 檢查登入狀態：如果未登入，主動阻斷並提示
    if (!this.authService.isAuthenticated()) {
      this.toastService.show('欲收藏商品需要先登入');
      return;
    }

    if (this.productId <= 0) {
      console.warn('FavoriteButton: productId 未設定');
      return;
    }

    this.favoriteService.toggleFavorite(this.productId).subscribe({
      next: () => {
        this.isFavorite = !this.isFavorite;
        this.favoriteChange.emit(this.isFavorite);

        const message = this.isFavorite
          ? `${this.itemName} 已成功加入收藏！`
          : `${this.itemName} 已從收藏移除`;

        this.toastService.show(message);
      },
      error: (err) => {
        console.error('收藏操作失敗：', err);
        // 這裡不需要再次提示登入，因為上面已經檢查過了，或者是 401 會由 Interceptor 處理
        this.toastService.show('操作失敗，請稍後再試');
      }
    });
  }

}
