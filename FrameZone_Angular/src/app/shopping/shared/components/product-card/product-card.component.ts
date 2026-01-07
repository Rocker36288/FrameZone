import { CommonModule } from '@angular/common';
import { Component, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { FavoriteButtonComponent } from '../favorite-button/favorite-button.component';
import { ShopProduct } from '../../../interfaces/products';

@Component({
  selector: 'app-product-card',
  standalone: true,
  imports: [FormsModule, CommonModule, RouterLink, FavoriteButtonComponent],
  templateUrl: './product-card.component.html',
  styleUrl: './product-card.component.css'
})
export class ProductCardComponent {
  // 比 @Input() 更效能優異
  product = input.required<ShopProduct>();

  // 當收藏狀態改變時通知父元件（如果需要同步資料庫）
  favoriteChange = output<boolean>();

  onFavoriteToggle(isFavorite: boolean) {
    this.favoriteChange.emit(isFavorite);
  }
}
