import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-share-button',
  imports: [],
  templateUrl: './share-button.component.html',
  styleUrl: './share-button.component.css'
})
export class ShareButtonComponent {
  @Output() share = new EventEmitter<void>();
  onClick() {
    console.log('✅ ShareButton clicked');
    this.share.emit();
  }
}
