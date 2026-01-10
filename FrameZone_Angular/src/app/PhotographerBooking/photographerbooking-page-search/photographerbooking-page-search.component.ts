import { PhotographerbookingCardComponent } from './../photographerbooking-card/photographerbooking-card.component';
import { PhotographerbookingSearchComponent } from './../photographerbooking-search/photographerbooking-search.component';
import { PhotographerbookingHeaderComponent } from './../photographerbooking-header/photographerbooking-header.component';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';

import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { PhotographerBookingService } from '../services/photographer-booking.service';
import { PhotographerbookingSidebarSearchComponent } from '../photographerbooking-sidebar-search/photographerbooking-sidebar-search.component';
import { PhotographerSkeletonCardComponent } from '../photographer-skeleton-card/photographer-skeleton-card.component';
import {
  Photographer,
  SearchFilters,
} from '../models/photographer-booking.models';

@Component({
  selector: 'app-photographerbooking-page-search',
  imports: [
    CommonModule,
    FormsModule,
    PhotographerbookingHeaderComponent,
    PhotographerbookingSearchComponent,
    PhotographerbookingCardComponent,
    PhotographerbookingSidebarSearchComponent,
    PhotographerSkeletonCardComponent,
  ],
  templateUrl: './photographerbooking-page-search.component.html',
  styleUrl: './photographerbooking-page-search.component.css',
})
export class PhotographerbookingPageSearchComponent
  implements OnInit, OnDestroy {
  photographers: Photographer[] = [];
  sortOrder: 'default' | 'priceAsc' | 'priceDesc' | 'ratingDesc' = 'default';
  isLoading: boolean = false;

  private destroy$ = new Subject<void>();

  constructor(
    private bookingService: PhotographerBookingService,
    private route: ActivatedRoute
  ) { }

  ngOnInit(): void {
    // 1. Subscribe to URL Query Params (From Hero or external link)
    this.route.queryParams
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {
        // Only update if params exist to avoid clearing default state on pure nav if handled elsewhere
        // But normally we want URL to drive state.
        if (Object.keys(params).length > 0) {
          const newFilters: Partial<SearchFilters> = {};

          if (params['keyword']) newFilters.keyword = params['keyword'];
          if (params['location']) newFilters.locations = [params['location']]; // Support single location from query

          if (params['serviceTypeId']) {
            // Ensure it handles string from URL
            newFilters.serviceType = params['serviceTypeId'];
          }

          if (params['startDate']) newFilters.startDate = params['startDate'];
          if (params['endDate']) newFilters.endDate = params['endDate'];
          if (params['tag']) newFilters.tags = [params['tag']];

          // Update service state which will trigger the filter subscription below
          this.bookingService.updateFilters(newFilters);
        }
      });

    // 2. 訂閱篩選條件變更 (Global State -> UI Search)
    this.bookingService.filters$
      .pipe(takeUntil(this.destroy$))
      .subscribe((filters) => {
        this.sortOrder = filters.sortOrder;
        this.performSearch(filters);
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  performSearch(filters: SearchFilters): void {
    this.isLoading = true;
    const updatedFilters = { ...filters, sortOrder: this.sortOrder };
    this.bookingService.searchWithFilters(updatedFilters).subscribe({
      next: (data) => {
        this.photographers = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error searching', err);
        this.isLoading = false;
      }
    });
  }

  onSortChange(): void {
    this.bookingService.updateFilters({ sortOrder: this.sortOrder });
  }

  get resultsCount(): number {
    return this.photographers.length;
  }
}
