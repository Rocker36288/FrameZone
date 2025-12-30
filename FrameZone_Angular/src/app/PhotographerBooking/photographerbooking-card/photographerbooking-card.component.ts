import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { Photographer } from '../models/photographer-booking.models';

@Component({
  selector: 'app-photographerbooking-card',
  imports: [CommonModule],
  templateUrl: './photographerbooking-card.component.html',
  styleUrl: './photographerbooking-card.component.css',
})
export class PhotographerbookingCardComponent {
  @Input() photographer!: Photographer;

  onViewDetails(): void {
    console.log('View details for:', this.photographer.name);
    // 這裡可以導航到詳細頁面或開啟 modal
  }

  get formattedPrice(): string {
    return `NT$ ${this.photographer.price.toLocaleString()}`;
  }
}
