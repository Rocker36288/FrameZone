import { Component, inject } from '@angular/core';
import { AsyncPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-social-menu',
  imports: [AsyncPipe, RouterLink],
  templateUrl: './social-menu.component.html',
  styleUrl: './social-menu.component.css'
})
export class SocialMenuComponent {
  private authService = inject(AuthService);
  currentUser$ = this.authService.currentUser$;
}
