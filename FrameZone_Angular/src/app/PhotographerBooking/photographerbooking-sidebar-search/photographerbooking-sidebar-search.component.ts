import { Component, OnInit } from '@angular/core';
import { PhotographerBookingService } from '../services/photographer-booking.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { forkJoin } from 'rxjs';
interface TagGroup {
  title: string;
  tags: string[];
  isLocation: boolean;
  expanded: boolean;
}
@Component({
  selector: 'app-photographerbooking-sidebar-search',
  imports: [FormsModule, CommonModule],
  templateUrl: './photographerbooking-sidebar-search.component.html',
  styleUrl: './photographerbooking-sidebar-search.component.css',
})
export class PhotographerbookingSidebarSearchComponent implements OnInit {
  tagGroups: TagGroup[] = [];
  maxPrice = 10000;
  minRating = 0;

  ratingOptions = [
    { label: '全部', value: 0 },
    { label: '4.5★+', value: 4.5 },
    { label: '4.8★+', value: 4.8 },
    { label: '5.0★', value: 5.0 },
  ];

  selectedLocations: Set<string> = new Set();
  selectedTags: Set<string> = new Set();

  constructor(private bookingService: PhotographerBookingService) { }

  ngOnInit(): void {
    this.initializeTagGroups();

    // Subscribe to filter changes to handle global reset
    this.bookingService.filters$.subscribe(filters => {
      this.selectedLocations = new Set(filters.locations);
      this.selectedTags = new Set(filters.tags);
      this.maxPrice = filters.maxPrice;
      this.minRating = filters.minRating;
    });
  }

  initializeTagGroups(): void {
    // 使用 forkJoin 同步處理兩個標籤來源，確保載入順序：1. 服務地區, 2. 專長分類
    forkJoin({
      cities: this.bookingService.getServiceCities(),
      categories: this.bookingService.getCategoriesWithTags()
    }).subscribe({
      next: (results) => {
        // 清空現有標籤群組，避免重複載入或順序混亂
        this.tagGroups = [];

        // 1. 處理服務地區
        this.tagGroups.push({
          title: '服務地區',
          tags: results.cities,
          isLocation: true,
          expanded: results.cities.length <= 6,
        });

        // 2. 處理專長分類
        results.categories.forEach((cat) => {
          this.tagGroups.push({
            title: cat.categoryName,
            tags: cat.tags,
            isLocation: false,
            expanded: cat.tags.length <= 6,
          });
        });
      },
      error: (err) => {
        console.error('Error loading sidebar data', err);
        // Fallback 邏輯：如果 API 失敗則顯示 Mock 資料（依樣維持固定順序）
        this.tagGroups = [];
        const locations = this.bookingService.getLocations();
        this.tagGroups.push({
          title: '服務地區',
          tags: locations,
          isLocation: true,
          expanded: locations.length <= 6,
        });

        const categories = this.bookingService.getCategories();
        categories.forEach((cat) => {
          const tags = this.bookingService.getTagsByCategory(cat.id);
          this.tagGroups.push({
            title: cat.name,
            tags: tags,
            isLocation: false,
            expanded: tags.length <= 6,
          });
        });
      }
    });
  }


  toggleTag(tag: string, isLocation: boolean): void {
    if (isLocation) {
      if (this.selectedLocations.has(tag)) {
        this.selectedLocations.delete(tag);
      } else {
        this.selectedLocations.add(tag);
      }
      this.bookingService.updateFilters({
        locations: Array.from(this.selectedLocations),
      });
    } else {
      if (this.selectedTags.has(tag)) {
        this.selectedTags.delete(tag);
      } else {
        this.selectedTags.add(tag);
      }
      this.bookingService.updateFilters({
        tags: Array.from(this.selectedTags),
      });
    }
  }

  isTagSelected(tag: string, isLocation: boolean): boolean {
    return isLocation
      ? this.selectedLocations.has(tag)
      : this.selectedTags.has(tag);
  }

  toggleExpand(group: TagGroup): void {
    group.expanded = !group.expanded;
  }

  getVisibleTags(group: TagGroup): string[] {
    return group.expanded ? group.tags : group.tags.slice(0, 6);
  }

  shouldShowToggle(group: TagGroup): boolean {
    return group.tags.length > 6;
  }

  onPriceChange(): void {
    this.bookingService.updateFilters({ maxPrice: this.maxPrice });
  }

  onRatingChange(value: number): void {
    this.minRating = value;
    this.bookingService.updateFilters({ minRating: value });
  }

  resetFilters(): void {
    this.selectedLocations.clear();
    this.selectedTags.clear();
    this.maxPrice = 10000;
    this.minRating = 0;
    this.bookingService.resetFilters();
  }

  get priceDisplay(): string {
    return `NT$${this.maxPrice.toLocaleString()}`;
  }
}
