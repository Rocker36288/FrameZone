import { CommonModule } from '@angular/common';
import { Component, computed, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PhotoListItem, PhotoQueryRequest } from '../../../core/models/photo.models';
import { PhotoService } from '../../../core/services/photo.service';
import { ToastrService } from 'ngx-toastr';
import { firstValueFrom } from 'rxjs';
import { PhotoSidebarComponent } from "../../../shared/components/photo-sidebar/photo-sidebar.component";
import { PhotoConstants } from '../../../shared/constants/photo.constants';

@Component({
  selector: 'app-photo-myphoto',
  imports: [CommonModule, RouterLink, PhotoSidebarComponent],
  templateUrl: './photo-myphoto.component.html',
  styleUrl: './photo-myphoto.component.css'
})
export class PhotoMyphotoComponent implements OnInit {

  // ==================== Signals ====================

  photos = signal<PhotoListItem[]>([]);
  isLoading = signal(false);
  currentPage = signal(1);
  pageSize = signal(PhotoConstants.DEFAULT_PAGE_SIZE);
  totalCount = signal(0);
  viewMode = signal<'grid' | 'list'>('grid');

  // 只有編輯模式才會用到選取
  selectedPhotos = signal<Set<number>>(new Set());
  isEditMode = signal(false);

  // 詳細資訊（點照片開）
  activePhoto = signal<PhotoListItem | null>(null);

  /** 目前篩選的標籤 ID */
  filterTagIds = signal<number[]>([]);

  /** Sidebar 是否開啟（手機版用 */
  isSidebarOpen = signal(typeof window !== 'undefined' ? window.innerWidth >= 992 : true);

  // ==================== Computed ====================

  totalPages = computed(() => Math.ceil(this.totalCount() / this.pageSize()));
  hasPhotos = computed(() => {
    const photoList = this.photos();
    return photoList !== null && photoList !== undefined && photoList.length > 0;
  });
  selectedCount = computed(() => this.selectedPhotos().size);
  allSelected = computed(() =>
    this.photos().length > 0 && this.photos().every(p => this.selectedPhotos().has(p.photoId))
  );

  /** 是否有篩選條件 */
  hasFilter = computed(() => this.filterTagIds().length > 0);

  // ==================== Constructor ====================

  constructor(
    private photoService: PhotoService,
    private toastr: ToastrService
  ) { }

  // ==================== Lifecycle ====================

  ngOnInit() {
    this.loadPhotos();
  }

  // ==================== 載入照片s ====================

  /**
   * 載入照片列表
   */
  async loadPhotos() {
    try {
      this.isLoading.set(true);

      // 判斷是否有標籤篩選
      if (this.filterTagIds().length > 0) {
        // 使用標籤篩選
        await this.loadPhotosWithFilter();
      } else {
        // 一般查詢
        const response = await firstValueFrom(
          this.photoService.getPhotosList(this.currentPage(), this.pageSize())
        );

        if (response.success) {
          this.photos.set(response.data);
          this.totalCount.set(response.totalCount);
        }
      }

    } catch (error) {
      console.error('載入照片失敗:', error);
      this.toastr.error('載入照片失敗', '錯誤');
    } finally {
      this.isLoading.set(false);
    }
  }

  /**
   * 使用篩選條件載入照片
   */
  async loadPhotosWithFilter() {
    try {
      const request: PhotoQueryRequest = {
        tagIds: this.filterTagIds(),
        pageNumber: this.currentPage(),
        pageSize: this.pageSize(),
        sortBy: 'DateTaken',
        sortOrder: 'desc'
      };

      const response = await firstValueFrom(
        this.photoService.queryPhotos(request)
      );

      if (response.success) {
        this.photos.set(response.photos);
        this.totalCount.set(response.totalCount);

        console.log('✅ 標籤篩選完成', {
          tagIds: this.filterTagIds(),
          totalCount: response.totalCount,
          executionTime: response.executionTimeMs
        });
      } else {
        this.toastr.error(response.message || '查詢失敗', '錯誤');
      }
    } catch (error) {
      console.error('標籤篩選失敗:', error);
      this.toastr.error('標籤篩選失敗', '錯誤');
    }
  }

  // ==================== 標籤篩選 ====================

  /**
   * 標籤選取變更（接收來自 Sidebar 的事件）
   */
  onTagSelectionChange(tagIds: number[]): void {
    console.log('🏷️ 標籤選取變更', tagIds);

    this.filterTagIds.set(tagIds);
    this.currentPage.set(1); // 重置到第一頁
    this.selectedPhotos.set(new Set()); // 清除照片選取

    // 重新載入照片
    this.loadPhotos();
  }

  /**
   * 清除篩選
   */
  clearFilter(): void {
    this.filterTagIds.set([]);
    this.currentPage.set(1);
    this.selectedPhotos.set(new Set());
    this.loadPhotos();
  }

  /**
   * 切換 Sidebar（手機版）
   */
  toggleSidebar(): void {
    this.isSidebarOpen.set(!this.isSidebarOpen());
  }

  /**
   * 切換檢視模式
   */
  toggleViewMode() {
    this.viewMode.set(this.viewMode() === 'grid' ? 'list' : 'grid');
  }

  /**
   * 編輯模式切換
   */
  toggleEditMode(): void {
    const next = !this.isEditMode();
    this.isEditMode.set(next);

    if (!next) {
      this.selectedPhotos.set(new Set());
    }
  }

  onPhotoClick(photo: PhotoListItem): void {
    if (this.isEditMode()) {
      this.togglePhotoSelection(photo.photoId);
      return;
    }
    this.activePhoto.set(photo);
  }

  closePhotoDetail(): void {
    this.activePhoto.set(null);
  }

  /**
   * 切換照片選取
   */
  togglePhotoSelection(photoId: number) {
    const selected = new Set(this.selectedPhotos());
    if (selected.has(photoId)) {
      selected.delete(photoId);
    } else {
      selected.add(photoId);
    }
    this.selectedPhotos.set(selected);
  }

  /**
   * 全選/取消全選
   */
  toggleSelectAll() {
    if (this.allSelected()) {
      this.selectedPhotos.set(new Set());
    } else {
      const allIds = new Set(this.photos().map(p => p.photoId));
      this.selectedPhotos.set(allIds);
    }
  }

  async deleteSelectedPhotos() {
    const selectedCount = this.selectedCount();

    if (selectedCount === 0) {
      this.toastr.warning('請先選擇要刪除的照片', '提示');
      return;
    }

    if (!confirm(`確定要刪除 ${selectedCount} 張照片嗎?此操作無法復原。`)) {
      return;
    }

    try {
      this.isLoading.set(true);

      const deletePromises = Array.from(this.selectedPhotos()).map(photoId =>
        firstValueFrom(this.photoService.deletePhoto(photoId))
      );

      await Promise.all(deletePromises);

      this.toastr.success(`成功刪除 ${selectedCount} 張照片`, '✓ 刪除成功');
      this.selectedPhotos.set(new Set());

      // 重新載入照片列表
      await this.loadPhotos();
    } catch (error) {
      console.error('刪除照片失敗:', error);
      this.toastr.error('刪除照片失敗', '錯誤');
    } finally {
      this.isLoading.set(false);
    }
  }

  /**
   * 刪除單張照片
   */
  async deletePhoto(photoId: number, event?: Event) {
    event?.stopPropagation();

    if (!confirm('確定要刪除這張照片嗎?此操作無發復原。')) {
      return;
    }

    try {
      this.isLoading.set(true);

      await firstValueFrom(this.photoService.deletePhoto(photoId));

      this.toastr.success('照片已刪除', '✓ 成功');

      // 刪除成功後，如果正在看這張，就關閉詳細資訊 modal
      if (this.activePhoto()?.photoId === photoId) {
        this.closePhotoDetail(); // activePhoto.set(null)
      }

      // 重新載入照片列表
      await this.loadPhotos();
    } catch (error) {
      console.error('刪除照片失敗:', error);
      this.toastr.error('刪除照片失敗', '錯誤');
    } finally {
      this.isLoading.set(false);
    }
  }

  /**
   * 換頁
   */
  async goToPage(page: number) {
    if (page < 1 || page > this.totalPages()) return;

    this.currentPage.set(page);
    this.selectedPhotos.set(new Set()); // 清除選取
    await this.loadPhotos();

    // 滾動到頁面頂部
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  /**
   * 上一頁
   */
  async previousPage() {
    await this.goToPage(this.currentPage() - 1);
  }

  /**
   * 下一頁
   */
  async nextPage() {
    await this.goToPage(this.currentPage() + 1);
  }

  /**
   * 格式化日期
   */
  formatDate(dateString: string): string {
    if (!dateString) return '-';

    const date = new Date(dateString);
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');

    return `${year}-${month}-${day} ${hours}:${minutes}`;
  }

  /**
   * 取得縮圖 URL（加入 Token）
   */
  getThumbnailUrl(photo: PhotoListItem): string {
    if (photo.thumbnailUrl) {
      // 從 localStorage 取得 Token
      const token = localStorage.getItem('authToken');

      // 組合完整 URL
      let fullUrl = photo.thumbnailUrl;
      if (fullUrl.startsWith('/api/')) {
        fullUrl = `https://localhost:7213${fullUrl}`;
      }

      // 加入 Token 作為 URL 參數
      if (token) {
        const separator = fullUrl.includes('?') ? '&' : '?';
        return `${fullUrl}${separator}token=${token}`;
      }

      return fullUrl;
    }

    return `https://placehold.co/300x200?text=${photo.fileName}`;
  }

  /**
   * 取得分頁範圍
   */
  getPageRange(): number[] {
    const total = this.totalPages();
    const current = this.currentPage();
    const range: number[] = [];

    // 最多顯示 7 個頁碼
    const maxPages = 7;
    let start = Math.max(1, current - Math.floor(maxPages / 2));
    let end = Math.min(total, start + maxPages - 1);

    // 調整起始位置
    if (end - start < maxPages - 1) {
      start = Math.max(1, end - maxPages + 1);
    }

    for (let i = start; i <= end; i++) {
      range.push(i);
    }

    return range;
  }
}
