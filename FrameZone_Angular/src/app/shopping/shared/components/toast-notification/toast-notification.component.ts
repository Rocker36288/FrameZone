import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-toast-notification',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './toast-notification.component.html',
  styleUrl: './toast-notification.component.css'
})
export class ToastNotificationComponent {
  message = '';
  visible = false;
  currentPosition: 'top' | 'bottom' = 'bottom';
  private timerId: any;

  constructor(private toastService: ToastService) { }

  ngOnInit() {
    this.toastService.toastState$.subscribe(data => {
      if (this.timerId) {
        clearTimeout(this.timerId);
        this.visible = false; // 先快速重設，增加視覺觸發感
        setTimeout(() => {
          this.showToast(data);
        }, 10);
      } else {
        this.showToast(data);
      }
    });
  }

  private showToast(data: { message: string, position: 'top' | 'bottom' }) {
    this.message = data.message;
    this.currentPosition = data.position;
    this.visible = true;
    this.timerId = setTimeout(() => {
      this.visible = false;
      this.timerId = null;
    }, 2000);
  }

}
