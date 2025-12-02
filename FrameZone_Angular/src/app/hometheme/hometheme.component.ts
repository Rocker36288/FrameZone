import { Component, OnInit, Inject, PLATFORM_ID, Renderer2 } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

@Component({
  selector: 'app-hometheme',
  imports: [],
  templateUrl: './hometheme.component.html',
  styleUrl: './hometheme.component.css'
})
export class HomethemeComponent implements OnInit {
  isDarkMode: boolean = false;

  constructor(
    @Inject(PLATFORM_ID) private platformId: Object,
    private renderer: Renderer2
  ) {}

  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      // 從 localStorage 讀取保存的主題，如果沒有則使用系統偏好
      const savedTheme = localStorage.getItem('theme');
      const systemTheme = window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
      const currentTheme = savedTheme || systemTheme;

      // 設置初始主題
      this.isDarkMode = currentTheme === 'dark';
      this.setTheme(currentTheme);

      // 監聽系統主題變更
      window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
        if (!localStorage.getItem('theme')) {
          const newTheme = e.matches ? 'dark' : 'light';
          this.isDarkMode = newTheme === 'dark';
          this.setTheme(newTheme);
        }
      });
    }
  }

  toggleTheme(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.isDarkMode = !this.isDarkMode;
      const newTheme = this.isDarkMode ? 'dark' : 'light';
      this.setTheme(newTheme);
      localStorage.setItem('theme', newTheme);
    }
  }

  private setTheme(theme: string): void {
    if (isPlatformBrowser(this.platformId)) {
      const htmlElement = document.documentElement;
      this.renderer.setAttribute(htmlElement, 'data-bs-theme', theme);
    }
  }
}
