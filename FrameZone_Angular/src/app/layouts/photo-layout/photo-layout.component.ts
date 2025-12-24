import { Component, signal } from '@angular/core';
import { FooterComponent } from "../../shared/components/footer/footer.component";
import { NavigationEnd, Router, RouterModule, RouterOutlet } from "@angular/router";
import { UserMenuComponent } from "../../shared/components/user-menu/user-menu.component";
import { PhotoFooterComponent } from "../../shared/components/photo-footer/photo-footer.component";
import { filter } from 'rxjs';

@Component({
  selector: 'app-photo-layout',
  imports: [FooterComponent, RouterOutlet, RouterModule, UserMenuComponent, PhotoFooterComponent],
  templateUrl: './photo-layout.component.html',
  styleUrl: './photo-layout.component.css'
})
export class PhotoLayoutComponent {
  isHome = false;
  isNavOpen = signal(false);

  constructor(private router: Router) {
    const setFlag = () => {
      const url = this.router.url.split('?')[0];
      // 依你的實際首頁路由調整（截圖是 /photo-home）
      this.isHome = url === '/photo-home' || url === '/home';
    };

    setFlag();
    this.router.events
      .pipe(filter(e => e instanceof NavigationEnd))
      .subscribe(() => setFlag());
  }

  toggleNav(): void {
    this.isNavOpen.set(!this.isNavOpen());
  }

  closeNav(): void {
    this.isNavOpen.set(false);
  }
}
