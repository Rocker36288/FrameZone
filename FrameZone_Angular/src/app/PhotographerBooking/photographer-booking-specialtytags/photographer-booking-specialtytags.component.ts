import { Component, OnInit } from '@angular/core';
import { Photographer, SearchFilters } from '../models/photographer-booking.models';
import { PhotographerBookingService } from '../services/photographer-booking.service';
import { PhotographerbookingCardComponent } from '../photographerbooking-card/photographerbooking-card.component';
import { CommonModule } from '@angular/common';

import { PhotographerSkeletonCardComponent } from '../photographer-skeleton-card/photographer-skeleton-card.component';

@Component({
  selector: 'app-photographer-booking-specialtytags',
  imports: [PhotographerbookingCardComponent, CommonModule, PhotographerSkeletonCardComponent],
  templateUrl: './photographer-booking-specialtytags.component.html',
  styleUrl: './photographer-booking-specialtytags.component.css',
})
export class PhotographerBookingSpecialtytagsComponent implements OnInit {
  photographers: Photographer[] = [];
  tags: string[] = [];
  selectedTag = '全部風格';
  filteredPhotographers: Photographer[] = [];
  isLoading: boolean = false;

  constructor(private bookingService: PhotographerBookingService) { }

  ngOnInit(): void {
    // 1. Load popular tags
    this.bookingService.getPopularTags(6).subscribe({
      next: (tags) => {
        this.tags = ['全部風格', ...tags];
        // After tags loaded, fetch initial data (All or first tag)
        this.filterByTag(this.selectedTag);
      },
      error: (err) => console.error('Error loading tags', err)
    });
  }

  filterByTag(tag: string) {
    this.selectedTag = tag;

    // Construct filters
    const filters: Partial<SearchFilters> = {};

    // If specific tag selected, filter by it. If "All", maybe fetch typical or featured? 
    // Or just fetch all. Let's assume fetch all recent or featured if "All".
    // Actually searchWithFilters implementation handles empty tags as "no tag filter".

    if (tag !== '全部風格') {
      filters.tags = [tag];
    }

    this.isLoading = true;
    this.bookingService.searchWithFilters(filters).subscribe({
      next: (data) => {
        this.filteredPhotographers = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error searching photographers', err);
        this.isLoading = false;
      }
    });
  }
}
