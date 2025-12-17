import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { routes } from '../../app.routes';
import { RouterLink, RouterOutlet } from '@angular/router';
import { FooterComponent } from "../../shared/components/footer/footer.component";

@Component({
  selector: 'app-shopping-header',
  standalone: true,
  imports: [FormsModule, CommonModule, RouterOutlet, RouterLink, FooterComponent],
  templateUrl: './shopping-header.component.html',
  styleUrl: './shopping-header.component.css'
})
export class ShoppingHeaderComponent {
  // 範例頭像 URL，請替換為實際的會員服務獲取邏輯
  memberAvatarUrl: string = 'https://i.pravatar.cc/30?img=68';

  // 範例會員名稱
  memberName: string = 'Angular用戶001';

}
