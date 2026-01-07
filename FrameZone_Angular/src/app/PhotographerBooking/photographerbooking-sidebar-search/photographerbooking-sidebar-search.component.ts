import { Component, OnInit } from '@angular/core';
import { PhotographerBookingService } from '../services/photographer-booking.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
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

  constructor(private bookingService: PhotographerBookingService) {}

  ngOnInit(): void {
    this.initializeTagGroups();
  }

  initializeTagGroups(): void {
    // 服務地區
    const locations = this.bookingService.getLocations();
    this.tagGroups.push({
      title: '服務地區',
      tags: locations,
      isLocation: true,
      expanded: locations.length <= 6,
    });

    // 動態標籤（拍攝風格、技術專長）
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
