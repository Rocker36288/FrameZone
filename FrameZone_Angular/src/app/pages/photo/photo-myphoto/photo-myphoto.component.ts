import { CommonModule } from '@angular/common';
import { Component, ViewChild, computed, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PhotoListItem, PhotoQueryRequest } from '../../../core/models/photo.models';
import { PhotoService } from '../../../core/services/photo.service';
import { TagManagementService } from '../../../core/services/tag-management.service';
import { BatchAddTagsRequest, NewTagItem } from '../../../core/models/tag-management.models';
import { ToastrService } from 'ngx-toastr';
import { firstValueFrom } from 'rxjs';
import { PhotoSidebarComponent } from "../../../shared/components/photo-sidebar/photo-sidebar.component";
import { PhotoConstants } from '../../../shared/constants/photo.constants';
import { BatchAddTagsDialogComponent } from "../batch-add-tags-dialog/batch-add-tags-dialog.component";

// 🆕 AI 功能相關 imports
import {
  PhotoAIAnalysisRequest,
  PhotoAIAnalysisResponse,
  AITagSuggestion,
  ApplyAITagsRequest,
  BatchPhotoAIAnalysisRequest,

  PhotoAIAnalysisStatus,
  AIAnalysisDefaults
} from '../../../core/models/photo-ai.models';

@Component({
  selector: 'app-photo-myphoto',
  imports: [CommonModule, RouterLink, PhotoSidebarComponent, BatchAddTagsDialogComponent],
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

  /** 🆕 批次添加標籤對話框是否開啟 */
  isBatchAddTagsDialogOpen = signal(false);

  /** Sidebar 實例（用於刷新標籤階層） */
  @ViewChild(PhotoSidebarComponent) sidebar?: PhotoSidebarComponent;

  // ==================== 🆕 AI 功能 Signals ====================

  /** AI 分析狀態（每張照片的狀態） */
  aiAnalysisStatus = signal<Map<number, PhotoAIAnalysisStatus>>(new Map());

  /** 是否正在進行 AI 分析 */
  isAnalyzing = signal(false);

  /** AI 建議列表 */
  aiSuggestions = signal<AITagSuggestion[]>([]);

  /** 是否顯示 AI 建議面板 */
  showAISuggestionsPanel = signal(false);

  /** 當前要處理 AI 建議的照片 ID */
  activePhotoForAI = signal<number | null>(null);

  /** 最低信心分數過濾（預設 0.6） */
  minConfidenceFilter = signal(0.6);

  /** 選中的 AI 建議 ID */
  selectedAISuggestions = signal<Set<number>>(new Set());

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

  /** 🆕 過濾後的 AI 建議（按信心分數） */
  filteredAISuggestions = computed(() => {
    const suggestions = this.aiSuggestions();
    const minConfidence = this.minConfidenceFilter();
    return suggestions.filter(s => s.confidence >= minConfidence && !s.isAdopted);
  });

  /** 🆕 選中的 AI 建議數量 */
  selectedAISuggestionsCount = computed(() => this.selectedAISuggestions().size);

  // ==================== Constructor ====================

  constructor(
    public photoService: PhotoService, // 🆕 改為 public，讓 HTML 可以使用輔助方法
    private tagService: TagManagementService,
    private toastr: ToastrService
  ) { }

  // ==================== Lifecycle ====================

  async ngOnInit() {
    await this.loadPhotos();

    // 🆕 載入照片的 AI 狀態
    const photoIds = this.photos().map(p => p.photoId);
    if (photoIds.length > 0) {
      await this.loadAIStatusForPhotos(photoIds);
    }
  }

  // ==================== 載入照片 ====================

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

  // ==================== 🆕 AI 功能方法 ====================

  /**
   * 🆕🔥 分析單張照片（自動套用高信心標籤）
   */
  async analyzePhoto(photoId: number, event?: Event) {
    event?.stopPropagation();

    try {
      this.isAnalyzing.set(true);
      this.toastr.info('正在分析照片...', 'AI 分析', { progressBar: true });

      const request: PhotoAIAnalysisRequest = {
        photoId: photoId,
        useThumbnail: AIAnalysisDefaults.USE_THUMBNAIL,
        minConfidenceScore: AIAnalysisDefaults.MIN_CONFIDENCE,
        enableTouristSpotDetection: AIAnalysisDefaults.ENABLE_TOURIST_SPOT_DETECTION,
        enableObjectDetection: AIAnalysisDefaults.ENABLE_OBJECT_DETECTION,
        placeSearchRadius: AIAnalysisDefaults.PLACE_SEARCH_RADIUS
      };

      const result = await firstValueFrom(
        this.photoService.analyzePhoto(request)
      );

      if (result.status === 'Success') {
        const totalSuggestions = result.tagSuggestions.length;
        const highConfidenceSuggestions = result.tagSuggestions.filter(s => s.confidence >= 0.85).length;

        this.toastr.success(
          `找到 ${totalSuggestions} 個標籤建議`,
          '✅ 分析完成'
        );

        // 🔥 自動套用高信心標籤（>= 0.85）
        if (highConfidenceSuggestions > 0) {
          await this.autoApplyHighConfidenceTags(photoId);
        }

        // 更新 AI 狀態
        await this.loadAIStatusForPhotos([photoId]);

        // 顯示 AI 建議面板（顯示剩餘的低信心建議）
        this.aiSuggestions.set(result.tagSuggestions);
        this.activePhotoForAI.set(photoId);
        this.showAISuggestionsPanel.set(true);

        console.log('✅ AI 分析完成', result);
      } else {
        this.toastr.error(result.errorMessage || '分析失敗', 'AI 分析');
      }
    } catch (error) {
      console.error('AI 分析失敗:', error);
      this.toastr.error('AI 分析失敗，請稍後再試', '錯誤');
    } finally {
      this.isAnalyzing.set(false);
    }
  }

  /**
   * 🆕🔥 自動套用高信心標籤（信心分數 >= 0.85）
   */
  private async autoApplyHighConfidenceTags(photoId: number): Promise<void> {
    try {
      const request: ApplyAITagsRequest = {
        photoId: photoId,
        suggestionIds: [],  // 空陣列表示套用所有
        minConfidence: 0.85  // 只套用高信心分數
      };

      const result = await firstValueFrom(
        this.photoService.applyAITags(photoId, request)
      );

      if (result.appliedCount > 0) {
        this.toastr.success(
          `自動套用 ${result.appliedCount} 個高信心 AI 標籤`,
          '🎯 智能套用',
          { timeOut: 3000 }
        );

        // 🔥 重新載入照片和 Sidebar
        await this.loadPhotos();

        if (this.sidebar) {
          await this.sidebar.loadTagHierarchy();
          // 🔥 自動展開「AI 標籤」分類
          setTimeout(() => {
            this.sidebar?.expandCategoryByName('AI 標籤');
          }, 500);
        }
      }

      console.log('✅ 自動套用高信心標籤完成', result);
    } catch (error) {
      console.error('❌ 自動套用高信心標籤失敗:', error);
      // 不顯示錯誤訊息，因為這是自動動作
    }
  }

  /**
   * 🆕 批次分析選中的照片
   */
  async batchAnalyzeSelectedPhotos() {
    const selectedIds = Array.from(this.selectedPhotos());

    if (selectedIds.length === 0) {
      this.toastr.warning('請先選擇要分析的照片', '提示');
      return;
    }

    if (selectedIds.length > AIAnalysisDefaults.BATCH_MAX_SIZE) {
      this.toastr.warning(
        `最多只能同時分析 ${AIAnalysisDefaults.BATCH_MAX_SIZE} 張照片`,
        '提示'
      );
      return;
    }

    try {
      this.isAnalyzing.set(true);

      const useAsync = selectedIds.length > AIAnalysisDefaults.BATCH_ASYNC_THRESHOLD;

      this.toastr.info(
        useAsync
          ? `正在背景分析 ${selectedIds.length} 張照片...`
          : `正在分析 ${selectedIds.length} 張照片...`,
        'AI 批次分析'
      );

      const request: BatchPhotoAIAnalysisRequest = {
        photoIds: selectedIds,
        processAsync: useAsync,
        options: {
          useThumbnail: AIAnalysisDefaults.USE_THUMBNAIL,
          minConfidenceScore: AIAnalysisDefaults.MIN_CONFIDENCE,
          enableTouristSpotDetection: AIAnalysisDefaults.ENABLE_TOURIST_SPOT_DETECTION,
          enableObjectDetection: AIAnalysisDefaults.ENABLE_OBJECT_DETECTION,
          placeSearchRadius: AIAnalysisDefaults.PLACE_SEARCH_RADIUS
        }
      };

      const result = await firstValueFrom(
        this.photoService.batchAnalyzePhotos(request)
      );

      if (result.isAsync) {
        // 非同步處理
        this.toastr.success(
          `批次分析任務已建立，預計 ${Math.ceil(selectedIds.length * 5 / 60)} 分鐘完成`,
          '批次分析'
        );
      } else {
        // 同步處理
        this.toastr.success(
          `成功分析 ${result.successCount} 張，失敗 ${result.failedCount} 張`,
          '✅ 批次分析完成'
        );

        // 重新載入 AI 狀態
        await this.loadAIStatusForPhotos(selectedIds);
      }

      // 清除選擇
      this.selectedPhotos.set(new Set());

    } catch (error) {
      console.error('批次分析失敗:', error);
      this.toastr.error('批次分析失敗，請稍後再試', '錯誤');
    } finally {
      this.isAnalyzing.set(false);
    }
  }

  /**
   * 🆕 載入照片的 AI 狀態（批次查詢）
   */
  async loadAIStatusForPhotos(photoIds: number[]) {
    try {
      // 批次查詢每張照片的 AI 狀態
      const statusPromises = photoIds.map(id =>
        firstValueFrom(this.photoService.getPhotoAIStatus(id))
      );

      const statuses = await Promise.all(statusPromises);

      // 更新 signal
      const statusMap = new Map(this.aiAnalysisStatus());
      statuses.forEach((status, index) => {
        statusMap.set(photoIds[index], status);
      });

      this.aiAnalysisStatus.set(statusMap);

      console.log('✅ AI 狀態載入完成');
    } catch (error) {
      console.error('載入 AI 狀態失敗:', error);
    }
  }

  /**
   * 🆕 查看 AI 建議（打開面板）
   */
  async viewAISuggestions(photoId: number, event?: Event) {
    event?.stopPropagation();

    try {
      const suggestions = await firstValueFrom(
        this.photoService.getAISuggestions(photoId, this.minConfidenceFilter())
      );

      this.aiSuggestions.set(suggestions);
      this.activePhotoForAI.set(photoId);
      this.showAISuggestionsPanel.set(true);
      this.selectedAISuggestions.set(new Set());

      console.log('✅ AI 建議載入完成', suggestions);
    } catch (error) {
      console.error('載入 AI 建議失敗:', error);
      this.toastr.error('載入 AI 建議失敗', '錯誤');
    }
  }

  /**
   * 🆕 套用選中的 AI 建議
   */
  async applySelectedAISuggestions() {
    const photoId = this.activePhotoForAI();
    if (!photoId) return;

    const selectedIds = Array.from(this.selectedAISuggestions());

    if (selectedIds.length === 0) {
      this.toastr.warning('請先選擇要套用的建議', '提示');
      return;
    }

    try {
      const request: ApplyAITagsRequest = {
        photoId: photoId,
        suggestionIds: selectedIds
      };

      const result = await firstValueFrom(
        this.photoService.applyAITags(photoId, request)
      );

      this.toastr.success(
        `成功套用 ${result.appliedCount} 個標籤，跳過 ${result.skippedCount} 個`,
        '✅ 套用標籤'
      );

      // 關閉面板
      this.closeAISuggestionsPanel();

      // 重新載入照片（更新標籤）
      await this.loadPhotos();

      // 刷新 Sidebar（更新標籤計數）
      if (this.sidebar) {
        await this.sidebar.loadTagHierarchy();
      }

    } catch (error) {
      console.error('套用 AI 標籤失敗:', error);
      this.toastr.error('套用 AI 標籤失敗', '錯誤');
    }
  }

  /**
   * 🆕 套用所有顯示的建議
   */
  async applyAllVisibleSuggestions() {
    const photoId = this.activePhotoForAI();
    if (!photoId) return;

    const visibleIds = this.filteredAISuggestions().map(s => s.suggestionId);

    if (visibleIds.length === 0) {
      this.toastr.warning('沒有可套用的建議', '提示');
      return;
    }

    try {
      const request: ApplyAITagsRequest = {
        photoId: photoId,
        suggestionIds: visibleIds
      };

      const result = await firstValueFrom(
        this.photoService.applyAITags(photoId, request)
      );

      this.toastr.success(
        `成功套用 ${result.appliedCount} 個標籤`,
        '✅ 套用標籤'
      );

      this.closeAISuggestionsPanel();
      await this.loadPhotos();

      if (this.sidebar) {
        await this.sidebar.loadTagHierarchy();
      }

    } catch (error) {
      console.error('套用標籤失敗:', error);
      this.toastr.error('套用標籤失敗', '錯誤');
    }
  }

  /**
   * 🆕 套用高信心分數的建議（>= 0.85）
   */
  async applyHighConfidenceSuggestions() {
    const photoId = this.activePhotoForAI();
    if (!photoId) return;

    try {
      const request: ApplyAITagsRequest = {
        photoId: photoId,
        suggestionIds: [],  // 空陣列表示套用所有
        minConfidence: 0.85  // 只套用高信心分數
      };

      const result = await firstValueFrom(
        this.photoService.applyAITags(photoId, request)
      );

      this.toastr.success(
        `自動套用 ${result.appliedCount} 個高信心標籤`,
        '✅ 智能套用'
      );

      this.closeAISuggestionsPanel();
      await this.loadPhotos();

      if (this.sidebar) {
        await this.sidebar.loadTagHierarchy();
      }

    } catch (error) {
      console.error('自動套用失敗:', error);
      this.toastr.error('自動套用失敗', '錯誤');
    }
  }

  /**
   * 🆕 切換 AI 建議的選取
   */
  toggleAISuggestionSelection(suggestionId: number) {
    const selected = new Set(this.selectedAISuggestions());
    if (selected.has(suggestionId)) {
      selected.delete(suggestionId);
    } else {
      selected.add(suggestionId);
    }
    this.selectedAISuggestions.set(selected);
  }

  /**
   * 🆕 關閉 AI 建議面板
   */
  closeAISuggestionsPanel() {
    this.showAISuggestionsPanel.set(false);
    this.aiSuggestions.set([]);
    this.activePhotoForAI.set(null);
    this.selectedAISuggestions.set(new Set());
  }

  /**
   * 🆕 調整信心分數過濾
   */
  adjustConfidenceFilter(value: number) {
    this.minConfidenceFilter.set(value);
  }

  // ==================== 標籤篩選 ====================

  /**
   * 標籤選取變更（接收來自 Sidebar 的事件）
   */
  onTagSelectionChange(tagIds: number[]): void {
    console.log('🏷️ 標籤選取變更', tagIds);

    this.filterTagIds.set(tagIds);
    this.currentPage.set(1);
    this.selectedPhotos.set(new Set());

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

  // ==================== 🆕 批次添加標籤功能 ====================

  /**
   * 🆕 打開批次添加標籤對話框
   */
  openBatchAddTagsDialog(): void {
    if (this.selectedCount() === 0) {
      this.toastr.warning('請先選擇照片', '提示');
      return;
    }

    console.log('🏷️ [MyPhoto] Opening batch add tags dialog for', this.selectedCount(), 'photos');
    this.isBatchAddTagsDialogOpen.set(true);
  }

  /**
   * 🆕 關閉批次添加標籤對話框
   */
  closeBatchAddTagsDialog(): void {
    console.log('🏷️ [MyPhoto] Closing batch add tags dialog');
    this.isBatchAddTagsDialogOpen.set(false);
  }

  /**
   * 🆕 處理批次添加標籤
   */
  async handleBatchAddTags(data: { existingTagIds: number[]; newTags: NewTagItem[] }): Promise<void> {
    try {
      console.log('🏷️ [MyPhoto] Handling batch add tags', data);

      if (this.selectedCount() === 0) {
        this.toastr.warning('請先選擇照片', '提示');
        return;
      }

      if (data.existingTagIds.length === 0 && data.newTags.length === 0) {
        this.toastr.warning('請至少選擇一個標籤或建立新標籤', '提示');
        return;
      }

      this.isLoading.set(true);

      const request: BatchAddTagsRequest = {
        photoIds: Array.from(this.selectedPhotos()),
        existingTagIds: data.existingTagIds.length > 0 ? data.existingTagIds : undefined,
        newTags: data.newTags.length > 0 ? data.newTags : undefined
      };

      console.log('📤 [MyPhoto] Sending batch add tags request:', request);

      const response = await firstValueFrom(
        this.tagService.batchAddTags(request)
      );

      if (response.success) {
        let message = `成功為 ${response.successCount} 張照片添加標籤`;

        if (response.createdTags.length > 0) {
          const tagNames = response.createdTags.map(t => t.tagName).join('、');
          message += `\n新建標籤：${tagNames}`;
        }

        if (response.failedCount > 0) {
          message += `\n失敗：${response.failedCount} 張`;
        }

        this.toastr.success(message, '✅ 標籤添加成功', {
          timeOut: 5000,
          progressBar: true
        });

        console.log('✅ [MyPhoto] Batch add tags succeeded:', response);

        this.closeBatchAddTagsDialog();
        this.selectedPhotos.set(new Set());
        await this.loadPhotos();
        await this.sidebar?.loadTagHierarchy();

      } else {
        this.toastr.error(response.message || '添加標籤失敗', '錯誤');
        console.error('❌ [MyPhoto] Batch add tags failed:', response);
      }

    } catch (error: any) {
      console.error('❌ [MyPhoto] Batch add tags error:', error);
      const errorMessage = error?.message || '添加標籤時發生錯誤';
      this.toastr.error(errorMessage, '錯誤');
    } finally {
      this.isLoading.set(false);
    }
  }

  // ==================== 刪除照片 ====================

  async deleteSelectedPhotos() {
    const selectedCount = this.selectedCount();

    if (selectedCount === 0) {
      this.toastr.warning('請先選擇要刪除的照片', '提示');
      return;
    }

    if (!confirm(`確定要刪除 ${selectedCount} 張照片嗎？此操作無法復原。`)) {
      return;
    }

    try {
      this.isLoading.set(true);

      const deletePromises = Array.from(this.selectedPhotos()).map(photoId =>
        firstValueFrom(this.photoService.deletePhoto(photoId))
      );

      await Promise.all(deletePromises);

      this.toastr.success(`成功刪除 ${selectedCount} 張照片`, '✔ 刪除成功');
      this.selectedPhotos.set(new Set());

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

    if (!confirm('確定要刪除這張照片嗎？此操作無發復原。')) {
      return;
    }

    try {
      this.isLoading.set(true);

      await firstValueFrom(this.photoService.deletePhoto(photoId));

      this.toastr.success('照片已刪除', '✔ 成功');

      if (this.activePhoto()?.photoId === photoId) {
        this.closePhotoDetail();
      }

      await this.loadPhotos();
    } catch (error) {
      console.error('刪除照片失敗:', error);
      this.toastr.error('刪除照片失敗', '錯誤');
    } finally {
      this.isLoading.set(false);
    }
  }

  // ==================== 分頁 ====================

  /**
   * 換頁
   */
  async goToPage(page: number) {
    if (page < 1 || page > this.totalPages()) return;

    this.currentPage.set(page);
    this.selectedPhotos.set(new Set());
    await this.loadPhotos();

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

  // ==================== 輔助方法 ====================

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
      const token = localStorage.getItem('authToken');

      let fullUrl = photo.thumbnailUrl;
      if (fullUrl.startsWith('/api/')) {
        fullUrl = `https://localhost:7213${fullUrl}`;
      }

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

    const maxPages = 7;
    let start = Math.max(1, current - Math.floor(maxPages / 2));
    let end = Math.min(total, start + maxPages - 1);

    if (end - start < maxPages - 1) {
      start = Math.max(1, end - maxPages + 1);
    }

    for (let i = start; i <= end; i++) {
      range.push(i);
    }

    return range;
  }
}
