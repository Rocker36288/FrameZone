import { animate, style, transition, trigger } from '@angular/animations';
import { Component, EventEmitter, Output } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-videos-notloginyet-modal',
  imports: [],
  templateUrl: './videos-notloginyet-modal.component.html',
  styleUrl: './videos-notloginyet-modal.component.css',
  animations: [
    // backdrop fade in
    trigger('backdropFade', [
      transition(':enter', [
        style({ opacity: 0 }),
        animate('200ms ease-out', style({ opacity: 1 }))
      ]),
      transition(':leave', [
        animate('200ms ease-in', style({ opacity: 0 }))
      ])
    ]),

    // modal slide up
    trigger('modalSlide', [
      transition(':enter', [
        style({ transform: 'translate(-50%, -45%)', opacity: 0 }),
        animate('300ms cubic-bezier(0.34, 1.56, 0.64, 1)',
          style({ transform: 'translate(-50%, -50%)', opacity: 1 }))
      ]),
      transition(':leave', [
        animate('200ms ease-in',
          style({ transform: 'translate(-50%, -45%)', opacity: 0 }))
      ])
    ])
  ]
})
export class VideosNotloginyetModalComponent {
  @Output() close = new EventEmitter<void>();

  constructor(private router: Router) { }

  onBackdropClick() {
    this.close.emit();
  }

  goLogin() {
    this.close.emit();
    this.router.navigate(['/login']);
  }

  goRegister() {
    this.close.emit();
    this.router.navigate(['/register']);
  }
}
