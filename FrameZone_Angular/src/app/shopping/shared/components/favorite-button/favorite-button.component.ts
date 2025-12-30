import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { ToastService } from '../../services/toast.service';

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
  @Output() favoriteChange = new EventEmitter<boolean>();

  constructor(private toastService: ToastService) { }

  toggleFavorite(event: Event) {
    event.preventDefault();
    event.stopPropagation();

    this.isFavorite = !this.isFavorite;
    this.favoriteChange.emit(this.isFavorite);

    const message = this.isFavorite
      ? `${this.itemName} 已成功加入收藏！`
      : `${this.itemName} 已從收藏移除`;

    this.toastService.show(message);
  }

}
