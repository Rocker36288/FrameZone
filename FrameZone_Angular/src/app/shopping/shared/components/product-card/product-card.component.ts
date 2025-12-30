import { CommonModule } from '@angular/common';
import { Component, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { FavoriteButtonComponent } from '../favorite-button/favorite-button.component';

interface Product {
  id: number;
  name: string;
  image: string;
  description: string;
  price: number;
  seller: {
    name: string;
    avatar: string;
  };
  postedDate: string;
  sales: number;
  categoryId: number;
  isFavorite: boolean;
}

@Component({
  selector: 'app-product-card',
  standalone: true,
  imports: [FormsModule, CommonModule, RouterLink, FavoriteButtonComponent],
  templateUrl: './product-card.component.html',
  styleUrl: './product-card.component.css'
})
export class ProductCardComponent {
  // 比 @Input() 更效能優異
  product = input.required<Product>();

  // 當收藏狀態改變時通知父元件（如果需要同步資料庫）
  favoriteChange = output<boolean>();

  onFavoriteToggle(isFavorite: boolean) {
    this.favoriteChange.emit(isFavorite);
  }
}
