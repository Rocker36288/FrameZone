import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-photographerbooking-card',
  imports: [CommonModule],
  templateUrl: './photographerbooking-card.component.html',
  styleUrl: './photographerbooking-card.component.css',
})
export class PhotographerbookingCardComponent {
  @Input() photographer!: any;
}
