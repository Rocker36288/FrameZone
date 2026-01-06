import { PhotographerbookingCardComponent } from './../photographerbooking-card/photographerbooking-card.component';
import { PhotographerbookingSearchComponent } from './../photographerbooking-search/photographerbooking-search.component';
import { PhotographerbookingHeaderComponent } from './../photographerbooking-header/photographerbooking-header.component';
import { CommonModule } from '@angular/common';

import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { PhotographerBookingService } from '../services/photographer-booking.service';
import { PhotographerbookingSidebarSearchComponent } from '../photographerbooking-sidebar-search/photographerbooking-sidebar-search.component';
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
  ],
  templateUrl: './photographerbooking-page-search.component.html',
  styleUrl: './photographerbooking-page-search.component.css',
})
export class PhotographerbookingPageSearchComponent
  implements OnInit, OnDestroy {
  photographers: Photographer[] = [];
  sortOrder: 'default' | 'priceAsc' | 'priceDesc' | 'ratingDesc' = 'default';

  private destroy$ = new Subject<void>();

  constructor(private bookingService: PhotographerBookingService) { }

  ngOnInit(): void {
    // 訂閱篩選條件變更
    this.bookingService.filters$
      .pipe(takeUntil(this.destroy$))
      .subscribe((filters) => {
        this.performSearch(filters);
      });

    // 初始搜尋
    this.performSearch(this.bookingService.getCurrentFilters());
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  performSearch(filters: SearchFilters): void {
    const updatedFilters = { ...filters, sortOrder: this.sortOrder };
    this.bookingService.searchWithFilters(updatedFilters).subscribe({
      next: (data) => {
        this.photographers = data;
        // Client side sorting if backend doesn't handle it yet (DTO-based sorting)
        // this.sortPhotographers(); 
      },
      error: (err) => console.error('Error searching', err)
    });
  }

  onSortChange(): void {
    this.bookingService.updateFilters({ sortOrder: this.sortOrder });
  }

  get resultsCount(): number {
    return this.photographers.length;
  }
}
