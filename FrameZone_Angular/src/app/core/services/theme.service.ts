import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private isDarkModeSubject = new BehaviorSubject<boolean>(false);
  public isDarkMode$: Observable<boolean> = this.isDarkModeSubject.asObservable();

  private isBrowser: boolean;

  constructor(@Inject(PLATFORM_ID) private platformId: Object) {
    this.isBrowser = isPlatformBrowser(this.platformId);
    this.initTheme();
  }

  private initTheme(): void {
    if (!this.isBrowser) return;

    const savedTheme = localStorage.getItem('theme');
    const isDark = savedTheme === 'dark';

    this.isDarkModeSubject.next(isDark);
    this.applyTheme(isDark ? 'dark' : 'light');
  }

  toggleTheme(): void {
    if (!this.isBrowser) return;

    const newIsDark = !this.isDarkModeSubject.value;
    const newTheme = newIsDark ? 'dark' : 'light';

    this.isDarkModeSubject.next(newIsDark);
    this.applyTheme(newTheme);
    localStorage.setItem('theme', newTheme);
  }

  private applyTheme(theme: string): void {
    if (!this.isBrowser) return;
    document.documentElement.setAttribute('data-bs-theme', theme);
  }

  get isDarkMode(): boolean {
    return this.isDarkModeSubject.value;
  }
}
