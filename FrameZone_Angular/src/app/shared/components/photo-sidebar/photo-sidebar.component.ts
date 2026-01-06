import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output, signal } from '@angular/core';
import { CategoryWithTags, TagNode } from '../../../core/models/photo.models';
import { PhotoService } from '../../../core/services/photo.service';
import { ToastrService } from 'ngx-toastr';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-photo-sidebar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './photo-sidebar.component.html',
  styleUrl: './photo-sidebar.component.css'
})
export class PhotoSidebarComponent implements OnInit {
  // ==================== Signals ====================

  /** 標籤階層資料 */
  tagHierarchy = signal<CategoryWithTags[]>([]);

  /** 已選取的標籤 ID 集合 */
  selectedTagIds = signal<Set<number>>(new Set());

  /** 載入中狀態 */
  isLoading = signal(false);

  /** Sidebar 是否開啟（手機版用） */
  isSidebarOpen = signal(true);

  @Input() set open(value: boolean) {
    this.isSidebarOpen.set(value);
  }

  // ==================== Outputs ====================

  @Output() openChange = new EventEmitter<boolean>();

  /** 標籤選取變更事件 */
  @Output() tagSelectionChange = new EventEmitter<number[]>();

  // ==================== Constructor ====================

  constructor(
    private photoService: PhotoService,
    private toastr: ToastrService
  ) { }

  // ==================== Lifecycle ====================

  ngOnInit(): void {
    this.loadTagHierarchy();
  }

  // ==================== 載入資料 ====================

  /**
   * 載入標籤階層
   */
  async loadTagHierarchy(): Promise<void> {
    try {
      this.isLoading.set(true);

      const response = await firstValueFrom(this.photoService.getTagHierarchy());

      if (response && response.success) {
        // 初始化展開狀態
        const selected = this.selectedTagIds();

        const categories = response.categories.map(category => ({
          ...category,
          isExpanded: category.isDefaultExpanded ?? true,
          tags: this.applySelectionRecursive(
            this.initializeTagExpansion(category.tags),
            selected
          )
        }));

        this.tagHierarchy.set(categories);

      } else {
        this.toastr.error('標籤階層載入失敗', '錯誤');
      }
    } catch (error) {
      this.toastr.error('載入標籤階層失敗', '錯誤');
    } finally {
      this.isLoading.set(false);
    }
  }

  /**
   * 初始化標籤的展開狀態（遞迴）
   */
  private initializeTagExpansion(tags: TagNode[]): TagNode[] {
    return tags.map(tag => ({
      ...tag,
      isExpanded: false, // 預設收合
      isSelected: false, // 預設未選取
      children: this.initializeTagExpansion(tag.children || [])
    }));
  }

  // ==================== 展開/收合 ====================

  /**
   * 切換分類的展開狀態
   */
  toggleCategory(categoryId: number): void {
    const categories = this.tagHierarchy();
    const category = categories.find(c => c.categoryId === categoryId);

    if (category) {
      category.isExpanded = !category.isExpanded;
      this.tagHierarchy.set([...categories]); // 觸發更新
    }
  }

  /**
   * 🆕 根據分類名稱展開分類（用於 AI 標籤自動展開）
   */
  expandCategoryByName(categoryName: string): void {
    const categories = this.tagHierarchy();
    const category = categories.find(c => c.categoryName === categoryName);

    if (category) {
      if (!category.isExpanded) {
        category.isExpanded = true;
        this.tagHierarchy.set([...categories]); // 觸發更新
        console.log(`✅ 已展開分類: ${categoryName}`);
      }
    } else {
      console.warn(`⚠️ 找不到分類: ${categoryName}`);
    }
  }

  /**
   * 切換標籤的展開狀態（遞迴查找）
   */
  toggleTag(tagId: number): void {
    const categories = this.tagHierarchy();

    for (const category of categories) {
      if (this.toggleTagRecursive(category.tags, tagId)) {
        this.tagHierarchy.set([...categories]); // 觸發更新
        break;
      }
    }
  }

  /**
   * 遞迴切換標籤展開狀態
   */
  private toggleTagRecursive(tags: TagNode[], targetTagId: number): boolean {
    for (const tag of tags) {
      if (tag.tagId === targetTagId) {
        tag.isExpanded = !tag.isExpanded;
        return true;
      }

      if (tag.children && tag.children.length > 0) {
        if (this.toggleTagRecursive(tag.children, targetTagId)) {
          return true;
        }
      }
    }
    return false;
  }

  // ==================== 標籤選取 ====================

  /**
   * 切換標籤的選取狀態
   */
  selectTag(tagId: number, event?: Event): void {
    // 阻止事件冒泡（避免觸發展開/收合）
    if (event) {
      event.stopPropagation();
    }

    const selected = new Set(this.selectedTagIds());

    if (selected.has(tagId)) {
      selected.delete(tagId);
    } else {
      selected.add(tagId);
    }

    this.selectedTagIds.set(selected);

    // 更新標籤的選取狀態（視覺回饋）
    this.updateTagSelectionState(tagId);

    // 發送選取變更事件
    this.emitSelectionChange();
  }

  /**
   * 更新標籤的選取狀態（遞迴）
   */
  private updateTagSelectionState(tagId: number): void {
    const categories = this.tagHierarchy();

    for (const category of categories) {
      this.updateTagSelectionRecursive(category.tags, tagId);
    }

    this.tagHierarchy.set([...categories]);
  }

  /**
   * 遞迴更新標籤選取狀態
   */
  private updateTagSelectionRecursive(tags: TagNode[], targetTagId: number): boolean {
    for (const tag of tags) {
      if (tag.tagId === targetTagId) {
        tag.isSelected = this.selectedTagIds().has(targetTagId);
        return true;
      }

      if (tag.children && tag.children.length > 0) {
        if (this.updateTagSelectionRecursive(tag.children, targetTagId)) {
          return true;
        }
      }
    }
    return false;
  }

  /**
   * 清除所有選取
   */
  clearSelection(): void {
    this.selectedTagIds.set(new Set());

    // 更新所有標籤的選取狀態
    const categories = this.tagHierarchy().map(category => ({
      ...category,
      tags: this.clearTagSelectionRecursive(category.tags)
    }));

    this.tagHierarchy.set(categories);
    this.emitSelectionChange();
  }

  /**
   * 遞迴清除標籤選取狀態
   */
  private clearTagSelectionRecursive(tags: TagNode[]): TagNode[] {
    return tags.map(tag => ({
      ...tag,
      isSelected: false,
      children: this.clearTagSelectionRecursive(tag.children || [])
    }));
  }

  /**
   * 發送選取變更事件
   */
  private emitSelectionChange(): void {
    const selectedIds = Array.from(this.selectedTagIds());
    console.log('🏷️ 標籤選取變更', selectedIds);
    this.tagSelectionChange.emit(selectedIds);
  }

  private applySelectionRecursive(tags: TagNode[], selected: Set<number>): TagNode[] {
    return tags.map(tag => {
      const children = this.applySelectionRecursive(tag.children || [], selected);
      const isSelected = selected.has(tag.tagId);

      // 如果子層有被選到，順便展開（可選）
      const hasSelectedChild = children.some(c => c.isSelected);

      return {
        ...tag,
        isSelected,
        isExpanded: tag.isExpanded || hasSelectedChild,
        children,
      };
    });
  }


  // ==================== Sidebar 控制 ====================

  /**
   * 切換 Sidebar 開關（手機版）
   */
  toggleSidebar(): void {
    this.isSidebarOpen.set(!this.isSidebarOpen());
  }

  // ==================== 輔助方法 ====================

  /**
   * 取得分類圖示的 Tabler Icon class
   * 修正：確保 icon 名稱格式正確
   */
  getCategoryIconClass(icon: string | undefined): string {
    if (!icon) {
      return 'ti ti-folder'; // 預設 icon
    }

    // 如果 icon 已經包含 'ti ti-'，直接返回
    if (icon.startsWith('ti ti-')) {
      return icon;
    }

    // 如果 icon 只包含 'ti-'，補上 'ti '
    if (icon.startsWith('ti-')) {
      return `ti ${icon}`;
    }

    // 否則補上完整的 'ti ti-'
    return `ti ti-${icon}`;
  }

  /**
   * 檢查標籤是否有子標籤
   */
  hasChildren(tag: TagNode): boolean {
    return tag.children && tag.children.length > 0;
  }

  /**
   * 檢查分類是否有標籤
   */
  hasTags(category: CategoryWithTags): boolean {
    return category.tags && category.tags.length > 0;
  }

  /**
   * 取得已選取數量
   */
  getSelectedCount(): number {
    return this.selectedTagIds().size;
  }

  /**
   * 檢查是否有任何選取
   */
  hasSelection(): boolean {
    return this.selectedTagIds().size > 0;
  }

}
