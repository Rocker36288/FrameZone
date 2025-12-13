import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-subscribe-button',
  imports: [],
  templateUrl: './subscribe-button.component.html',
  styleUrl: './subscribe-button.component.css'
})
export class SubscribeButtonComponent {
  @Input() videoid: number = 0;
  FollowerBtn() {

  }
}
