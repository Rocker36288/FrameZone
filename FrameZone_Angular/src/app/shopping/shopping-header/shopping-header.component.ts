import { CommonModule } from '@angular/common';
import { Component, effect, HostListener, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { routes } from '../../app.routes';
import { NavigationEnd, Router, RouterLink, RouterOutlet } from '@angular/router';
import { FooterComponent } from "../../shared/components/footer/footer.component";
import { CartService } from '../shared/services/cart.service';
import { SearchService } from '../shared/services/search.service';
import { filter, Subject, takeUntil } from 'rxjs';
import { ChatWindowComponent } from "../shared/components/chat-window/chat-window.component";
import { AuthService } from '../../core/services/auth.service';
import { LoginResponseDto } from '../../core/models/auth.models';
import { ToastService } from '../shared/services/toast.service';
import { ToastNotificationComponent } from '../shared/components/toast-notification/toast-notification.component';

@Component({
  selector: 'app-shopping-header',
  standalone: true,
  imports: [FormsModule, CommonModule, RouterOutlet, RouterLink, FooterComponent, ChatWindowComponent, ToastNotificationComponent],
  templateUrl: './shopping-header.component.html',
  styleUrl: './shopping-header.component.css'
})
export class ShoppingHeaderComponent {
  // 搜尋關鍵字變數
  searchText: string = '';

  currentUser: LoginResponseDto | null = null;
  // 控制「畫面顯示邏輯」的變數，Interface 裡不會有，所以必須保留
  isLoggedIn: boolean = false;
  isDropdownOpen: boolean = false;

  displayUserName: string = '';
  userEmail: string = '';
  userSession = { email: '' };
  displayAccount: string = '';

  private destroy$ = new Subject<void>();

  // 注入 Service 以取得購物車數量
  constructor(
    public cartService: CartService,
    private searchService: SearchService,
    private router: Router,
    private authService: AuthService,
    private toastService: ToastService
  ) {
    // 關鍵：監聽 Service 的 Signal 變化
    effect(() => {
      // 當 Service 裡的 keyword 改變時，同步更新導覽列的輸入框文字
      this.searchText = this.searchService.keyword();
    });

    // 路由監聽：偵測導向位置
    this.router.events.pipe(
      // 只處理「導航結束」的事件
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: any) => {
      // 如果目前的網址「不包含」產品列表頁的路徑
      if (!event.urlAfterRedirects.includes('/shopping/products')) {
        this.searchService.clear();
      }
    });
  }

  ngOnInit() {
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        this.updateUserState(user);
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // --- 邏輯方法 ---
  toggleDropdown(): void {
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    // 改用 closest 檢查非querySelector，購物導覽列才可以跳出下拉選單
    const isClickInside = target.closest('.member-dropdown-container');

    if (!isClickInside) {
      this.isDropdownOpen = false;
    }
  }

  private updateUserState(user: LoginResponseDto | null) {
    console.log('目前登入的使用者資料：', user);
    this.currentUser = user;
    this.isLoggedIn = !!user;
    if (user) {
      this.displayAccount = user.account || '帳號'; // 導覽列用
      this.displayUserName = user.displayName || '使用者'; // 選單內部用
      this.userSession.email = user.email || '';
    }
  }

  getUserAvatar(): string {
    if (this.currentUser?.avatar) return this.currentUser.avatar;
    const initial = (this.displayUserName || 'U').charAt(0).toUpperCase();
    return `https://ui-avatars.com/api/?name=${encodeURIComponent(initial)}&background=667eea&color=fff&size=128`;
  }

  onLogout() {
    this.isDropdownOpen = false;
    this.authService.logout();
    this.router.navigate(['/']);
  }

  goToMyReviews(): void {
    this.checkAuthAndNavigate('/shopping/reviews');
  }

  checkAuthAndNavigate(targetUrl: string, queryParams: any = {}): void {
    if (this.isLoggedIn && this.currentUser) {
      // 特殊處理：評價頁面需要帶入 userId
      if (targetUrl === '/shopping/reviews') {
        queryParams = { ...queryParams, userId: this.currentUser.userId };
      }
      this.router.navigate([targetUrl], { queryParams });
    } else {
      this.toastService.show('請先登入會員', 'top');
      // 延遲 1000ms 再跳轉，確保使用者看得到提示
      setTimeout(() => {
        this.router.navigate(['/login'], { queryParams: { returnUrl: targetUrl } });
      }, 1000);
    }
  }

  navigateToLogin(): void {
    this.router.navigate(['/login']);
  }


  // 範例頭像 URL，請替換為實際的會員服務獲取邏輯
  // memberAvatarUrl: string = 'images/avatar/11.jpg';

  // 範例會員名稱
  // memberName: string = 'ruka711';

  search(): void {
    const keyword = this.searchText.trim();
    // 即使 keyword 為空也建議 setKeyword，這樣才能「清空」搜尋結果
    this.searchService.setKeyword(keyword);

    // 如果不在產品頁，則導向產品頁
    if (!this.router.url.includes('/shopping/products')) {
      this.router.navigate(['/shopping/products']);
    }
  }

}
