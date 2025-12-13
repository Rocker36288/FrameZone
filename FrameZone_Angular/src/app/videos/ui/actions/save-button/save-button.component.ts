import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-save-button',
  imports: [],
  templateUrl: './save-button.component.html',
  styleUrl: './save-button.component.css'
})
export class SaveButtonComponent {
  @Input() videoid: number = 0
}
