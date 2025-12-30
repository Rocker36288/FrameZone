import { CommonModule } from '@angular/common';
import { Component,  OnInit} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PhotographerBookingService } from '../services/photographer-booking.service';

@Component({
  selector: 'app-photographerbooking-search',
  imports: [CommonModule, FormsModule],
  templateUrl: './photographerbooking-search.component.html',
  styleUrl: './photographerbooking-search.component.css',
})
export class PhotographerbookingSearchComponent implements OnInit {
  serviceTypes: string[] = [];
  selectedServiceType = '';
  searchKeyword = '';
  dateRange: string = '';

  constructor(private bookingService: PhotographerBookingService) {}

  ngOnInit(): void {
    this.serviceTypes = this.bookingService.getServiceTypes();
  }

  onServiceTypeChange(): void {
    this.bookingService.updateFilters({
      serviceType: this.selectedServiceType,
    });
  }

  onKeywordChange(): void {
    this.bookingService.updateFilters({ keyword: this.searchKeyword });
  }

  onSearch(): void {
    this.bookingService.updateFilters({
      serviceType: this.selectedServiceType,
      keyword: this.searchKeyword,
    });
  }

  onDateChange(event: any): void {
    // 處理日期變更，可以在後續整合 flatpickr 或其他日期選擇器
    console.log('Date changed:', event);
  }
}
