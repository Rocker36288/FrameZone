import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CartService } from '../shared/services/cart.service';
import { AuthService } from '../../core/services/auth.service';
import { ToastService } from '../shared/services/toast.service';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-shopping-order-success',
  standalone: true,
  imports: [FormsModule, CommonModule, RouterLink],
  templateUrl: './shopping-order-success.component.html',
  styleUrl: './shopping-order-success.component.css'
})
export class ShoppingOrderSuccessComponent {
  currentStep: number = 3;

  constructor(
    private cartService: CartService,
    private router: Router,
    private authService: AuthService,
    private toastService: ToastService
  ) { }

  private destroy$ = new Subject<void>();

  ngOnInit(): void {
    if (!this.authService.isAuthenticated()) {
      this.toastService.show('請先登入會員', 'top');
      setTimeout(() => {
        this.router.navigate(['/login'], { queryParams: { returnUrl: '/order-success' } });
      }, 1000);
      return;
    }

    // 從綠界頁面重導回來會清掉狀態
    // 先移除以下 code 以免無法呈現交易成功的頁面

    // if (!this.cartService.orderCompletedSignal()) {
    //   this.router.navigate(['/']);
    //   return;
    // }


    this.cartService.clearCart();
    this.cartService.resetOrderCompleted();

    // 訂閱會員資料
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        if (user) {
          this.memberName = user.account || user.displayName || '會員';
          if (user.avatar) {
            this.memberAvatarUrl = user.avatar;
          } else {
            const initial = (this.memberName || 'U').charAt(0).toUpperCase();
            this.memberAvatarUrl = `https://ui-avatars.com/api/?name=${encodeURIComponent(initial)}&background=667eea&color=fff&size=128`;
          }
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  goToOrders(): void {
    this.router.navigate(['/shopping/buyer-center']);
  }

  goToHome(): void {
    this.router.navigate(['/shopping']);
  }

  // 範例頭像 URL，請替換為實際的會員服務獲取邏輯
  memberAvatarUrl: string = '';

  // 範例會員名稱
  memberName: string = '';

}
