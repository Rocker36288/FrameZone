import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CartService } from '../shared/services/cart.service';

@Component({
  selector: 'app-shopping-order-success',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './shopping-order-success.component.html',
  styleUrl: './shopping-order-success.component.css'
})
export class ShoppingOrderSuccessComponent {
  currentStep: number = 3;

  constructor(
    private cartService: CartService,
    private router: Router

  ) { }

  ngOnInit(): void {
    if (!this.cartService.orderCompletedSignal()) {
      this.router.navigate(['/']);
      return;
    }

    this.cartService.clearCart();
    this.cartService.resetOrderCompleted();
  }

  goToOrders(): void {
    this.router.navigate(['/shopping/buyer-center']);
  }

  goToHome(): void {
    this.router.navigate(['/shopping']);
  }

  // 範例頭像 URL，請替換為實際的會員服務獲取邏輯
  memberAvatarUrl: string = 'https://i.pravatar.cc/30?img=68';

  // 範例會員名稱
  memberName: string = 'Angular用戶001';

}
