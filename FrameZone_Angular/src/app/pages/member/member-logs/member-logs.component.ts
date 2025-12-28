import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MemberService } from '../../../core/services/member.service';
import {
  UserLogDto,
  UserLogQueryDto,
  UserLogStatsDto,
  PagedData
} from '../../../core/models/member.models';

@Component({
  selector: 'app-member-logs',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './member-logs.component.html',
  styleUrl: './member-logs.component.css'
})
export class MemberLogsComponent implements OnInit {
  // 載入狀態（不顯示全畫面載入，改為背景載入）
  isLoading = false;
  isLoadingStats = false;
  isExporting = false;

  // 錯誤訊息
  errorMessage = '';

  // 日誌資料
  logs: UserLogDto[] = [];
  pagedData: PagedData<UserLogDto> | null = null;

  // 統計資料
  stats: UserLogStatsDto | null = null;

  // 查詢參數
  queryParams: UserLogQueryDto = {
    pageNumber: 1,
    pageSize: 10,
    actionType: '',
    actionCategory: '',
    startDate: '',
    endDate: '',
    status: '',
    severity: ''
  };

  // 篩選選項
  actionTypes = [
    { value: '', label: '全部' },
    { value: 'Login', label: '登入' },
    { value: 'Logout', label: '登出' },
    { value: 'ProfileUpdate', label: '更新個人資料' },
    { value: 'PasswordChange', label: '變更密碼' },
    { value: 'AvatarUpload', label: '上傳頭像' },
    { value: 'CoverImageUpload', label: '上傳封面' }
  ];

  actionCategories = [
    { value: '', label: '全部' },
    { value: 'Security', label: '安全' },
    { value: 'Profile', label: '個人資料' },
    { value: 'Settings', label: '設定' },
    { value: 'System', label: '系統' }
  ];

  statuses = [
    { value: '', label: '全部' },
    { value: 'Success', label: '成功' },
    { value: 'Failure', label: '失敗' }
  ];

  severities = [
    { value: '', label: '全部' },
    { value: 'Info', label: '資訊' },
    { value: 'Warning', label: '警告' },
    { value: 'Error', label: '錯誤' }
  ];

  pageSizes = [
    { value: 10, label: '10 筆/頁' },
    { value: 20, label: '20 筆/頁' },
    { value: 50, label: '50 筆/頁' }
  ];

  // 顯示篩選面板
  showFilters = false;

  constructor(private memberService: MemberService) {}

  ngOnInit(): void {
    this.loadLogs();
    this.loadStats();
  }

  /**
   * 載入日誌列表（背景載入，不阻擋 UI）
   */
  loadLogs(): void {
    this.errorMessage = '';

    this.memberService.getUserLogs(this.queryParams).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.logs = response.data.items;
          this.pagedData = response.data;
        } else {
          this.errorMessage = response.message || '載入失敗';
        }
      },
      error: (error) => {
        console.error('載入日誌失敗:', error);
        this.errorMessage = '載入失敗，請稍後再試';
      }
    });
  }

  /**
   * 載入統計資料（背景載入）
   */
  loadStats(): void {
    this.memberService.getUserLogStats().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.stats = response.data;
        }
      },
      error: (error) => {
        console.error('載入統計失敗:', error);
      }
    });
  }

  /**
   * 切換篩選面板
   */
  toggleFilters(): void {
    this.showFilters = !this.showFilters;
  }

  /**
   * 應用篩選
   */
  applyFilters(): void {
    this.queryParams.pageNumber = 1; // 重置到第一頁
    this.loadLogs();
  }

  /**
   * 重置篩選
   */
  resetFilters(): void {
    this.queryParams = {
      pageNumber: 1,
      pageSize: 10,
      actionType: '',
      actionCategory: '',
      startDate: '',
      endDate: '',
      status: '',
      severity: ''
    };
    this.loadLogs();
  }

  /**
   * 換頁
   */
  changePage(page: number): void {
    if (page < 1 || (this.pagedData && page > this.pagedData.totalPages)) {
      return;
    }
    this.queryParams.pageNumber = page;
    this.loadLogs();
    // 滾動到頂部
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  /**
   * 變更每頁筆數
   */
  changePageSize(): void {
    this.queryParams.pageNumber = 1; // 重置到第一頁
    this.loadLogs();
  }

  /**
   * 匯出 CSV
   */
  exportLogs(): void {
    this.isExporting = true;

    this.memberService.exportUserLogs(this.queryParams).subscribe({
      next: (blob) => {
        // 建立下載連結
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `activity_logs_${this.formatDateForFilename(new Date())}.csv`;
        link.click();
        window.URL.revokeObjectURL(url);
        this.isExporting = false;
      },
      error: (error) => {
        console.error('匯出失敗:', error);
        alert('匯出失敗，請稍後再試');
        this.isExporting = false;
      }
    });
  }

  /**
   * 取得狀態標籤樣式
   */
  getStatusClass(status: string): string {
    return status === 'Success' ? 'status-success' : 'status-failure';
  }

  /**
   * 取得嚴重性標籤樣式
   */
  getSeverityClass(severity: string): string {
    switch (severity) {
      case 'Info':
        return 'severity-info';
      case 'Warning':
        return 'severity-warning';
      case 'Error':
        return 'severity-error';
      default:
        return '';
    }
  }

  /**
   * 取得裝置類型圖標
   */
  getDeviceIcon(deviceType: string | null): string {
    if (!deviceType) return '💻';

    const type = deviceType.toLowerCase();
    if (type.includes('mobile') || type.includes('android') || type.includes('ios')) {
      return '📱';
    }
    if (type.includes('tablet')) {
      return '📲';
    }
    return '💻';
  }

  /**
   * 格式化日期時間
   */
  formatDateTime(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleString('zh-TW', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit'
    });
  }

  /**
   * 格式化日期（用於檔名）
   */
  private formatDateForFilename(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    return `${year}${month}${day}_${hours}${minutes}`;
  }

  /**
   * 取得分頁範圍
   */
  getPageRange(): number[] {
    if (!this.pagedData) return [];

    const totalPages = this.pagedData.totalPages;
    const currentPage = this.pagedData.pageNumber;
    const range: number[] = [];

    // 顯示當前頁前後各2頁
    const start = Math.max(1, currentPage - 2);
    const end = Math.min(totalPages, currentPage + 2);

    for (let i = start; i <= end; i++) {
      range.push(i);
    }

    return range;
  }

  /**
   * 取得統計數字的百分比
   */
  getSuccessRate(): number {
    if (!this.stats || this.stats.totalLogs === 0) return 0;
    return Math.round((this.stats.successCount / this.stats.totalLogs) * 100);
  }

  /**
   * 格式化相對時間
   */
  formatRelativeTime(dateString: string | null): string {
    if (!dateString) return '無記錄';

    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) return '剛剛';
    if (diffMins < 60) return `${diffMins} 分鐘前`;
    if (diffHours < 24) return `${diffHours} 小時前`;
    if (diffDays < 7) return `${diffDays} 天前`;

    return this.formatDateTime(dateString);
  }
}
