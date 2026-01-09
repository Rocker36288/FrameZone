import { CommonModule } from '@angular/common';
import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';
import { PhotographerBookingService } from '../services/photographer-booking.service';
import { ServiceType } from '../models/photographer-booking.models';

@Component({
  selector: 'app-photographerbooking-search',
  imports: [CommonModule, FormsModule],
  templateUrl: './photographerbooking-search.component.html',
  styleUrl: './photographerbooking-search.component.css',
})
export class PhotographerbookingSearchComponent implements OnInit, OnDestroy {
  serviceTypes: ServiceType[] = [];
  selectedServiceType = '';
  searchKeyword = '';
  startDate = '';
  endDate = '';

  private searchSubject = new Subject<string>();
  private destroy$ = new Subject<void>();

  constructor(private bookingService: PhotographerBookingService) { }

  ngOnInit(): void {
    this.bookingService.getServiceTypes().subscribe((types) => {
      this.serviceTypes = types;
    });

    // Re-subscribe to filter changes to sync UI (Global Reset requirement)
    this.bookingService.filters$
      .pipe(takeUntil(this.destroy$))
      .subscribe(filters => {
        // Only update input if value is different to avoid interrupting typing/cursor
        if (filters.keyword !== this.searchKeyword) {
          this.searchKeyword = filters.keyword || '';
        }
        this.selectedServiceType = filters.serviceType || '';
        this.startDate = filters.startDate || '';
        this.endDate = filters.endDate || '';
      });

    // Implement debounce for keyword search
    this.searchSubject.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe(keyword => {
      this.bookingService.updateFilters({ keyword: keyword });
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onServiceTypeChange(): void {
    this.updateSearch();
  }

  onKeywordInput(value: string): void {
    this.searchSubject.next(value);
  }

  // Not used directly anymore for typing, but maybe for direct trigger if needed, though input handles it.
  // We keep it for consistency or remove. User wants "Instant Search", so input event drives it.
  onKeywordChange(): void {
    // Legacy direct call, redirected to debounce subject
    this.onKeywordInput(this.searchKeyword);
  }

  onDateChange(): void {
    // Only update search if both dates are selected or both are cleared
    if ((this.startDate && this.endDate) || (!this.startDate && !this.endDate)) {
      this.updateSearch();
    }
  }

  // Changed from "Search" to "Reset"
  onReset(): void {
    this.bookingService.resetFilters();
  }

  private updateSearch(): void {
    this.bookingService.updateFilters({
      serviceType: this.selectedServiceType,
      // keyword is handled by debounce subject
      startDate: this.startDate,
      endDate: this.endDate
    });
  }
}
