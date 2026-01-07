import { AfterViewInit, ElementRef, ViewChild, OnDestroy, OnInit, Component, signal } from '@angular/core';
import { FooterComponent } from "../../shared/components/footer/footer.component";
import { NavigationEnd, Router, RouterModule, RouterOutlet } from "@angular/router";
import { UserMenuComponent } from "../../shared/components/user-menu/user-menu.component";
import { PhotoFooterComponent } from "../../shared/components/photo-footer/photo-footer.component";
import { NotificationBellComponent } from "../../shared/components/notification-bell/notification-bell.component";
import { SignalRService } from "../../core/services/signalr.service";
import { NotificationHandlerService } from "../../core/services/notification-handler.service";
import { filter } from 'rxjs';

@Component({
  selector: 'app-photo-layout',
  imports: [
    FooterComponent,
    RouterOutlet,
    RouterModule,
    UserMenuComponent,
    PhotoFooterComponent,
    NotificationBellComponent
  ],
  templateUrl: './photo-layout.component.html',
  styleUrl: './photo-layout.component.css'
})
export class PhotoLayoutComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('appNavbar', { static: true }) appNavbar!: ElementRef<HTMLElement>;
  private ro?: ResizeObserver;

  isHome = false;
  isNavOpen = signal(false);

  constructor(
    private router: Router,
    private signalRService: SignalRService,
    private notificationHandler: NotificationHandlerService
  ) {
    const setFlag = () => {
      const url = this.router.url.split('?')[0];
      this.isHome = url === '/photo-home' || url === '/home';
    };

    setFlag();
    this.router.events
      .pipe(filter(e => e instanceof NavigationEnd))
      .subscribe(() => setFlag());
  }

  ngOnInit(): void {
    // ⭐ 啟動 SignalR 連線和通知處理器
    this.initializeSignalR();
  }

  ngAfterViewInit() {
    const el = this.appNavbar.nativeElement;

    const sync = () => {
      const h = el.getBoundingClientRect().height;
      document.documentElement.style.setProperty('--navbar-height', `${Math.ceil(h)}px`);
    };

    sync();
    this.ro = new ResizeObserver(sync);
    this.ro.observe(el);
  }

  ngOnDestroy() {
    this.ro?.disconnect();

    // ⭐ 清理 SignalR 連線和通知處理器
    this.cleanupSignalR();
  }

  toggleNav(): void {
    this.isNavOpen.set(!this.isNavOpen());
  }

  closeNav(): void {
    this.isNavOpen.set(false);
  }

  /**
   * 初始化 SignalR 連線和通知處理器
   */
  private async initializeSignalR(): Promise<void> {
    try {
      console.log('🚀 初始化 SignalR 連線...');

      // 1. 啟動 SignalR 連線
      await this.signalRService.startConnection();

      // 2. 初始化通知處理器（訂閱 SignalR 事件）
      this.notificationHandler.initialize();

      console.log('✅ SignalR 和通知處理器初始化完成');
    } catch (error) {
      console.error('❌ SignalR 初始化失敗:', error);
      // 可以選擇顯示錯誤提示給用戶
      // this.toastr.error('即時通知連線失敗，將使用輪詢模式', '連線錯誤');
    }
  }

  /**
   * 清理 SignalR 連線和通知處理器
   */
  private async cleanupSignalR(): Promise<void> {
    try {
      console.log('🧹 清理 SignalR 連線...');

      // 1. 清理通知處理器訂閱
      this.notificationHandler.destroy();

      // 2. 停止 SignalR 連線
      await this.signalRService.stopConnection();

      console.log('✅ SignalR 清理完成');
    } catch (error) {
      console.error('❌ SignalR 清理失敗:', error);
    }
  }
}
