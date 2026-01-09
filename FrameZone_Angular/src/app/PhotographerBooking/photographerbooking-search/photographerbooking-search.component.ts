import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PhotographerBookingService } from '../services/photographer-booking.service';
import { ServiceType } from '../models/photographer-booking.models';

@Component({
  selector: 'app-photographerbooking-search',
  imports: [CommonModule, FormsModule],
  templateUrl: './photographerbooking-search.component.html',
  styleUrl: './photographerbooking-search.component.css',
})
export class PhotographerbookingSearchComponent implements OnInit {
  serviceTypes: ServiceType[] = [];
  selectedServiceType = '';
  searchKeyword = '';
  startDate = '';
  endDate = '';

  constructor(private bookingService: PhotographerBookingService) { }

  ngOnInit(): void {
    this.bookingService.getServiceTypes().subscribe((types) => {
      this.serviceTypes = types;
    });
  }

  onServiceTypeChange(): void {
    this.updateSearch();
  }

  onKeywordChange(): void {
    this.updateSearch();
  }

  onDateChange(): void {
    // Only update search if both dates are selected or both are cleared
    if ((this.startDate && this.endDate) || (!this.startDate && !this.endDate)) {
      this.updateSearch();
    }
  }

  onSearch(): void {
    this.updateSearch();
  }

  private updateSearch(): void {
    this.bookingService.updateFilters({
      serviceType: this.selectedServiceType,
      keyword: this.searchKeyword,
      startDate: this.startDate,
      endDate: this.endDate
    });
  }
}
