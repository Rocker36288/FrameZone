import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-videos-notloginyet-modal',
  imports: [],
  templateUrl: './videos-notloginyet-modal.component.html',
  styleUrl: './videos-notloginyet-modal.component.css'
})
export class VideosNotloginyetModalComponent {
  @Output() close = new EventEmitter<void>();

  constructor(private router: Router) { }

  goLogin() {
    this.close.emit();
    this.router.navigate(['/login']);
  }

  goRegister() {
    this.close.emit();
    this.router.navigate(['/register']);
  }
}
