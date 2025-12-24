import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-toast-notification',
  imports: [CommonModule],
  templateUrl: './toast-notification.component.html',
  styleUrl: './toast-notification.component.css'
})
export class ToastNotificationComponent {
  message = '';
  visible = false;

  constructor(private toastService: ToastService) { }

  ngOnInit() {
    this.toastService.toastState$.subscribe(msg => {
      this.message = msg;
      this.visible = true;
      setTimeout(() => this.visible = false, 2000); // 配合您原本的 2000 毫秒
    });
  }

}
