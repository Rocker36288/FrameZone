import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-like-button',
  imports: [],
  templateUrl: './like-button.component.html',
  styleUrl: './like-button.component.css'
})
export class LikeButtonComponent {
  @Input() videoid: number = 0
}
