import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { Router } from '@angular/router';
import { Photographer } from '../models/photographer-booking.models';

@Component({
  selector: 'app-photographerbooking-card',
  imports: [CommonModule],
  templateUrl: './photographerbooking-card.component.html',
  styleUrl: './photographerbooking-card.component.css',
})
export class PhotographerbookingCardComponent {
  constructor(private router: Router) { }
  @Input() photographer!: Photographer;

  get formattedPrice(): string {
    // If we have services, show min price
    if (this.photographer.services && this.photographer.services.length > 0) {
      const min = Math.min(...this.photographer.services.map(s => s.basePrice));
      return `NT$ ${min.toLocaleString()} 起`;
    }
    return '價格詳談';
  }

  onViewDetails(): void {
    this.router.navigate(['/photographer-detail'], { queryParams: { id: this.photographer.photographerId } });
  }
}
