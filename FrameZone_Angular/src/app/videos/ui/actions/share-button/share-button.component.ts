import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-share-button',
  imports: [],
  templateUrl: './share-button.component.html',
  styleUrl: './share-button.component.css'
})
export class ShareButtonComponent {
  @Input() videoid: number = 0
}
