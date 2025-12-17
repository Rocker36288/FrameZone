import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-photographerbooking-search',
  imports: [CommonModule, FormsModule],
  templateUrl: './photographerbooking-search.component.html',
  styleUrl: './photographerbooking-search.component.css',
})
export class PhotographerbookingSearchComponent {
  @Input() keyword = '';
  @Input() filterLocation = '';
  @Input() filterService = '';
  @Input() filterPrice = '';

  @Output() keywordChange = new EventEmitter<string>();
  @Output() filterLocationChange = new EventEmitter<string>();
  @Output() filterServiceChange = new EventEmitter<string>();
  @Output() filterPriceChange = new EventEmitter<string>();

  clear() {
    this.keywordChange.emit('');
    this.filterLocationChange.emit('');
    this.filterServiceChange.emit('');
    this.filterPriceChange.emit('');
  }
}
