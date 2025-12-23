import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Output } from '@angular/core';

@Component({
  selector: 'app-photographer-tagstyles',
  imports: [CommonModule],
  templateUrl: './photographer-tagstyles.component.html',
  styleUrl: './photographer-tagstyles.component.css',
})
export class PhotographerTagstylesComponent {
  @Output() styleChange = new EventEmitter<string>();

  styles = [
    '韓系清透',
    '日系底片',
    '美式紀實',
    '復古油畫',
    '歐美時尚',
    '法式浪漫',
  ];

  active = 'All';
  select(style: string) {
    this.active = style;
    this.styleChange.emit(style);
  }
}
