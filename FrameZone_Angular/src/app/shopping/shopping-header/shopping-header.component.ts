import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { routes } from '../../app.routes';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { FooterComponent } from "../../shared/components/footer/footer.component";
import { CartService } from '../shared/services/cart.service';

@Component({
  selector: 'app-shopping-header',
  standalone: true,
  imports: [FormsModule, CommonModule, RouterOutlet, RouterLink, FooterComponent],
  templateUrl: './shopping-header.component.html',
  styleUrl: './shopping-header.component.css'
})
export class ShoppingHeaderComponent {
  // 注入 Service 以取得購物車數量
  public cartService = inject(CartService);
  private router = inject(Router);

  // 搜尋關鍵字變數
  searchText: string = '';

  // 範例頭像 URL，請替換為實際的會員服務獲取邏輯
  memberAvatarUrl: string = 'https://i.pravatar.cc/30?img=68';

  // 範例會員名稱
  memberName: string = 'Angular用戶001';

  // 搜尋執行函式
  onSearch() {
    const term = this.searchText.trim();
    if (term) {
      // 導向購物首頁並帶入搜尋參數
      this.router.navigate(['/shopping/home'], {
        queryParams: { search: term }
      });
    }
  }

}
