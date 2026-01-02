import { Component, OnInit, OnDestroy, AfterViewInit, Input, Output, EventEmitter, ViewChild, ElementRef, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, firstValueFrom } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';

import { TagManagementService } from '../../../core/services/tag-management.service';
import { TagItem, NewTagItem } from '../../../core/models/tag-management.models';
import { PhotoService } from '../../../core/services/photo.service';
import { CategoryWithTags, TagNode } from '../../../core/models/photo.models';

type ParentTagOption = {
  tagId: number;
  label: string;
  categoryName: string;
};

type ParentTagGroup = {
  categoryName: string;
  options: ParentTagOption[];
};

@Component({
  selector: 'app-batch-add-tags-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './batch-add-tags-dialog.component.html',
  styleUrls: ['./batch-add-tags-dialog.component.css']
})
export class BatchAddTagsDialogComponent implements OnInit, AfterViewInit, OnDestroy {

  // ===========================
  // 🔧 Input/Output
  // ===========================

  /** 選取的照片數量 */
  @Input() selectedPhotoCount: number = 0;

  /** 確認事件 - 返回選中的標籤資料 */
  @Output() confirm = new EventEmitter<{
    existingTagIds: number[];
    newTags: NewTagItem[];
  }>();

  /** 取消事件 */
  @Output() cancel = new EventEmitter<void>();

  // ===========================
  // 🎯 ViewChild - 自動聚焦
  // ===========================

  @ViewChild('searchInput') searchInput?: ElementRef<HTMLInputElement>;

  // ===========================
  // 📊 Signals - 狀態管理
  // ===========================

  // 搜尋相關
  searchKeyword = signal('');               // 搜尋關鍵字
  searchResults = signal<TagItem[]>([]);    // 搜尋結果
  isSearching = signal(false);              // 是否正在搜尋

  // 選擇相關
  selectedTagIds = signal<Set<number>>(new Set());  // 已選標籤 ID
  selectedTags = signal<TagItem[]>([]);             // 已選標籤完整資料

  // 新建標籤相關
  newTagsToCreate = signal<NewTagItem[]>([]);       // 待建立的新標籤

  // 父標籤（階層式標籤）
  tagHierarchy = signal<CategoryWithTags[]>([]);
  isHierarchyLoading = signal(false);

  /** 建立新標籤時選擇的父標籤（可選） */
  selectedParentTagId: number | null = null;

  // UI 狀態
  isSubmitting = signal(false);             // 是否正在提交
  showCreateNewTag = signal(false);         // 是否顯示「建立新標籤」按鈕

  /** 父標籤下拉選單（依分類分組） */
  parentTagGroups = computed<ParentTagGroup[]>(() => {
    const options = this.flattenHierarchyToParentOptions(this.tagHierarchy());

    // 優先只顯示「自訂」相關分類，找不到就顯示全部
    const hasCustom = options.some(o => o.categoryName.includes('自訂'));
    const finalOptions = hasCustom ? options.filter(o => o.categoryName.includes('自訂')) : options;

    const map = new Map<string, ParentTagOption[]>();
    for (const opt of finalOptions) {
      const key = opt.categoryName || '未分類';
      if (!map.has(key)) map.set(key, []);
      map.get(key)!.push(opt);
    }

    return Array.from(map.entries()).map(([categoryName, opts]) => ({
      categoryName,
      options: opts
    }));
  });

  // ===========================
  // 🧮 Computed - 計算屬性
  // ===========================

  /** 已選標籤總數（包含現有 + 新建） */
  totalSelectedCount = computed(() =>
    this.selectedTagIds().size + this.newTagsToCreate().length
  );

  /** 是否可以提交（至少選擇一個標籤且未提交中） */
  canSubmit = computed(() =>
    this.totalSelectedCount() > 0 && !this.isSubmitting()
  );

  /** 是否有搜尋結果 */
  hasSearchResults = computed(() =>
    this.searchResults().length > 0
  );

  /** 是否顯示空狀態（有關鍵字但無結果） */
  showEmptyState = computed(() =>
    this.searchKeyword().trim().length > 0 &&
    this.searchResults().length === 0 &&
    !this.isSearching()
  );

  // ===========================
  // 🔄 RxJS - 搜尋 Subject
  // ===========================

  private searchSubject = new Subject<string>();

  // ===========================
  // 🏗️ Constructor
  // ===========================

  constructor(
    private tagService: TagManagementService,
    private photoService: PhotoService,
    private toastr: ToastrService
  ) {}

  // ===========================
  // 🔄 生命週期
  // ===========================

  ngOnInit(): void {
    console.log('🏷️ [Dialog] Initialized with selectedPhotoCount:', this.selectedPhotoCount);
    this.setupSearchDebounce();
    this.loadTagHierarchyForParentSelection();
  }

  ngAfterViewInit(): void {
    // 自動聚焦搜尋框
    setTimeout(() => {
      this.searchInput?.nativeElement.focus();
    }, 100);
  }

  ngOnDestroy(): void {
    // 清理 Subject
    this.searchSubject.complete();
  }

  // ===========================
  // 🔍 搜尋功能
  // ===========================

  /**
   * 設置搜尋防抖
   */
  private setupSearchDebounce(): void {
    this.searchSubject.pipe(
      debounceTime(600),              // 🔧 延遲 600ms，讓用戶有更多時間打完字
      distinctUntilChanged(),         // 過濾重複值
      switchMap(keyword => {
        // 返回 Promise 包裝為 Observable
        return new Promise<void>(resolve => {
          this.searchTags(keyword).then(() => resolve());
        });
      })
    ).subscribe({
      error: (err) => {
        console.error('🏷️ [Dialog] Search error:', err);
      }
    });
  }

  /**
   * 處理搜尋輸入
   */
  onSearchInput(keyword: string): void {
    this.searchKeyword.set(keyword);
    this.searchSubject.next(keyword);
  }

  /**
   * 執行標籤搜尋
   */
  private async searchTags(keyword: string): Promise<void> {
    // 如果關鍵字為空，清空結果
    if (!keyword.trim()) {
      this.searchResults.set([]);
      this.showCreateNewTag.set(false);
      return;
    }

    // 🔧 新增：過濾注音符號（ㄅㄆㄇ...）
    const isBopomofo = /^[ㄅ-ㄩ]+$/.test(keyword.trim());
    if (isBopomofo) {
      console.log('🏷️ [Dialog] Skipping bopomofo input:', keyword);
      return;
    }

    // 🔧 新增：最小字符長度限制（至少 1 個完整字）
    if (keyword.trim().length < 1) {
      console.log('🏷️ [Dialog] Keyword too short, skipping search');
      return;
    }

    console.log('🏷️ [Dialog] Searching tags with keyword:', keyword);
    this.isSearching.set(true);

    try {
      const response = await firstValueFrom(
        this.tagService.searchTags({
          keyword: keyword.trim(),
          limit: 20
        })
      );

      console.log('🏷️ [Dialog] Raw response:', response);

      // 🔧 修正：容錯處理，支持多種響應格式
      let tags: any[] = [];

      if (response?.success && response?.tags) {
        // 標準格式：{ success: true, tags: [...] }
        tags = response.tags;
        console.log('🏷️ [Dialog] Search results (standard):', tags.length);
      } else if (response?.tags && Array.isArray(response.tags)) {
        // 只有 tags 數組：{ tags: [...] }
        tags = response.tags;
        console.log('🏷️ [Dialog] Search results (tags only):', tags.length);
      } else if (Array.isArray(response)) {
        // 直接返回數組：[...]
        tags = response;
        console.log('🏷️ [Dialog] Search results (array):', tags.length);
      } else {
        console.warn('🏷️ [Dialog] Unexpected response format:', response);
      }

      this.searchResults.set(tags);

      // 判斷是否可建立新標籤（無搜尋結果時顯示）
      this.showCreateNewTag.set(tags.length === 0);

    } catch (error) {
      console.error('🏷️ [Dialog] Search error:', error);
      // 🔧 修正：不顯示 Toast，避免過多錯誤提示
      // this.toastr.error('搜尋標籤時發生錯誤', '錯誤');
      this.searchResults.set([]);
      this.showCreateNewTag.set(false);
    } finally {
      this.isSearching.set(false);
    }
  }

  // ===========================
  // ✅ 標籤選擇功能
  // ===========================

  /**
   * 切換標籤選取狀態
   */
  toggleTagSelection(tag: TagItem): void {
    const selected = new Set(this.selectedTagIds());

    if (selected.has(tag.tagId)) {
      // 取消選取
      console.log('🏷️ [Dialog] Unselecting tag:', tag.tagName);
      selected.delete(tag.tagId);
      this.selectedTags.update(tags =>
        tags.filter(t => t.tagId !== tag.tagId)
      );
    } else {
      // 選取
      console.log('🏷️ [Dialog] Selecting tag:', tag.tagName);
      selected.add(tag.tagId);
      this.selectedTags.update(tags => [...tags, tag]);
    }

    this.selectedTagIds.set(selected);
  }

  /**
   * 檢查標籤是否已選取
   */
  isTagSelected(tagId: number): boolean {
    return this.selectedTagIds().has(tagId);
  }

  /**
   * 移除已選標籤
   */
  removeSelectedTag(tagId: number): void {
    console.log('🏷️ [Dialog] Removing selected tag:', tagId);
    const selected = new Set(this.selectedTagIds());
    selected.delete(tagId);

    this.selectedTagIds.set(selected);
    this.selectedTags.update(tags =>
      tags.filter(t => t.tagId !== tagId)
    );
  }

  /**
   * 移除待建立的新標籤
   */
  removeNewTag(index: number): void {
    console.log('🏷️ [Dialog] Removing new tag at index:', index);
    this.newTagsToCreate.update(tags =>
      tags.filter((_, i) => i !== index)
    );
  }

  // ===========================
  // ➕ 建立新標籤功能
  // ===========================

  /**
   * 載入標籤階層（用於建立新標籤時選擇父標籤）
   */
  private async loadTagHierarchyForParentSelection(): Promise<void> {
    try {
      this.isHierarchyLoading.set(true);
      const response = await firstValueFrom(this.photoService.getTagHierarchy());
      if (response?.success && Array.isArray(response.categories)) {
        this.tagHierarchy.set(response.categories);
      } else {
        this.tagHierarchy.set([]);
      }
    } catch (e) {
      this.tagHierarchy.set([]);
    } finally {
      this.isHierarchyLoading.set(false);
    }
  }

  /**
   * 將標籤樹攤平成下拉選項（包含縮排）
   */
  private flattenHierarchyToParentOptions(categories: CategoryWithTags[]): ParentTagOption[] {
    const result: ParentTagOption[] = [];

    const walk = (nodes: TagNode[], categoryName: string, depth: number) => {
      for (const n of nodes || []) {
        const indent = depth > 0 ? `${'—'.repeat(depth)} ` : '';
        result.push({
          tagId: n.tagId,
          label: `${indent}${n.tagName}`,
          categoryName
        });
        if (n.children && n.children.length > 0) {
          walk(n.children, categoryName, depth + 1);
        }
      }
    };

    for (const c of categories || []) {
      const categoryName = (c as any).categoryName ?? '未分類';
      walk(c.tags || [], categoryName, 0);
    }

    return result;
  }

  /**
   * 建立新標籤
   */
  createNewTag(): void {
    const keyword = this.searchKeyword().trim();

    if (!keyword) {
      this.toastr.warning('請輸入標籤名稱', '提示');
      return;
    }

    // 檢查是否已存在於搜尋結果中
    const existsInSearch = this.searchResults().some(tag =>
      tag.tagName.toLowerCase() === keyword.toLowerCase()
    );

    if (existsInSearch) {
      this.toastr.warning('此標籤已存在，請直接選擇', '提示');
      return;
    }

    // 檢查是否已在待建立列表中
    const existsInNew = this.newTagsToCreate().some(tag =>
      tag.tagName.toLowerCase() === keyword.toLowerCase()
    );

    if (existsInNew) {
      this.toastr.warning('此標籤已在待建立列表中', '提示');
      return;
    }

    // 添加到待建立列表
    console.log('🏷️ [Dialog] Creating new tag:', keyword);
    // NewTagItem 可能尚未在前端 model 裡加入 parentTagId / categoryId 欄位。
    // 這裡用 any 避免 TS 類型限制，同時確保送到後端的 payload 有 parentTagId。
    const newTag: any = { tagName: keyword };
    if (this.selectedParentTagId) {
      newTag.parentTagId = this.selectedParentTagId;
    }

    this.newTagsToCreate.update(tags => [...tags, newTag as NewTagItem]);

    // 清空搜尋框
    this.searchKeyword.set('');
    this.searchResults.set([]);
    this.showCreateNewTag.set(false);

    this.toastr.success(`已添加新標籤 "${keyword}"`, '成功');

    // 重新聚焦搜尋框
    setTimeout(() => {
      this.searchInput?.nativeElement.focus();
    }, 100);
  }

  // ===========================
  // 📤 提交與取消
  // ===========================

  /**
   * 確認提交
   */
  onConfirm(): void {
    if (!this.canSubmit()) {
      console.warn('🏷️ [Dialog] Cannot submit - no tags selected');
      return;
    }

    this.isSubmitting.set(true);

    // 準備資料
    const data = {
      existingTagIds: Array.from(this.selectedTagIds()),
      newTags: this.newTagsToCreate()
    };

    console.log('🏷️ [Dialog] Confirming with data:', data);
    console.log('  - Existing tag IDs:', data.existingTagIds);
    console.log('  - New tags:', data.newTags);

    // 發送 confirm 事件
    this.confirm.emit(data);

    // 注意：提交後由父組件處理 loading 狀態和關閉對話框
    // 這裡不需要 reset，因為對話框會被銷毀
  }

  /**
   * 取消並關閉
   */
  onCancel(): void {
    // 如果有未保存的選擇，詢問確認
    if (this.totalSelectedCount() > 0) {
      const confirmClose = confirm('已選擇的標籤將不會保存，確定要關閉嗎？');
      if (!confirmClose) {
        return;
      }
    }

    console.log('🏷️ [Dialog] Cancelled');
    this.cancel.emit();
  }

  /**
   * 點擊背景遮罩關閉
   */
  onBackdropClick(): void {
    this.onCancel();
  }

  /**
   * 阻止點擊事件冒泡（避免點擊對話框內容關閉）
   */
  onDialogClick(event: MouseEvent): void {
    event.stopPropagation();
  }

  // ===========================
  // 🎨 輔助方法
  // ===========================

  /**
   * 獲取標籤類型圖標
   */
  getTagIcon(tag: TagItem): string {
    if (tag.categoryName?.includes('時間')) return '📅';
    if (tag.categoryName?.includes('地點')) return '🌍';
    if (tag.categoryName?.includes('人物')) return '👤';
    if (tag.categoryName?.includes('事件')) return '🎉';
    if (tag.categoryName === 'AI 辨識標籤') return '🤖';
    return '🏷️';
  }

  /**
   * 獲取標籤類型文字
   */
  getTagTypeText(tag: TagItem): string {
    if (tag.categoryName) {
      return tag.categoryName;
    }
    return '用戶標籤';
  }
}
