import { CommonModule } from '@angular/common';
import { Component, Input, OnChanges } from '@angular/core';
import { ServiceDto, AvailableSlotDto } from '../models/photographer-booking.models';
import { PhotographerBookingService } from '../services/photographer-booking.service';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { ToastrService } from 'ngx-toastr';


@Component({
  selector: 'app-photographer-bookingsidebar',
  imports: [CommonModule, FormsModule],
  templateUrl: './photographer-bookingsidebar.component.html',
  styleUrl: './photographer-bookingsidebar.component.css',
})
export class PhotographerBookingsidebarComponent implements OnChanges {
  @Input() selectedService: ServiceDto | null = null;
  @Input() photographerId!: number;
  @Input() photographerName: string = '';

  availableSlots: AvailableSlotDto[] = [];
  selectedSlotId: number | null = null;

  constructor(
    private bookingService: PhotographerBookingService,
    private authService: AuthService,
    private router: Router,
    private toastr: ToastrService
  ) { }


  selectedDate: Date | null = null;
  additionalFees: number = 0;
  discountAmount: number = 0;

  ngOnChanges(): void {
    if (this.photographerId) {
      this.loadAvailableSlots();
    }
  }

  loadAvailableSlots(): void {
    const start = new Date();
    const end = new Date();
    end.setDate(end.getDate() + 30); // Load next 30 days

    this.bookingService.getAvailableSlots(this.photographerId, start, end).subscribe({
      next: (slots) => {
        this.availableSlots = slots.filter(s => s.isAvailable);
      },
      error: (err) => console.error('Error loading slots', err)
    });
  }

  onSlotChange(event: any): void {
    const slotId = +event.target.value;
    this.selectedSlotId = slotId;
    const slot = this.availableSlots.find(s => s.availableSlotId === slotId);
    if (slot) {
      this.selectedDate = new Date(slot.startDateTime);
    }
  }

  get basePrice(): number {
    return this.selectedService?.basePrice || 0;
  }

  get totalPrice(): number {
    return this.basePrice + this.additionalFees - this.discountAmount;
  }

  onBookNow(): void {
    // 優先檢查登入狀態
    if (!this.authService.isAuthenticated()) {
      this.toastr.error('請先登入後再進行預約', '需要登入', {
        timeOut: 3000,
        progressBar: true,
        positionClass: 'toast-top-center'
      });
      this.router.navigate(['/login']);
      return;
    }

    if (!this.selectedService) {
      this.toastr.warning('請先選擇拍攝方案', '未選擇方案', {
        timeOut: 3000,
        progressBar: true,
        positionClass: 'toast-top-center'
      });
      return;
    }

    if (!this.selectedSlotId) {
      this.toastr.warning('請選擇拍攝日期與時段', '未選擇時段', {
        timeOut: 3000,
        progressBar: true,
        positionClass: 'toast-top-center'
      });
      return;
    }

    const currentUser = this.authService.getCurrentUser();
    const userId = currentUser?.userId || 0;

    // 前端模擬成功流程
    const bookingId = Math.floor(Math.random() * 100000);
    const bookingNumber = `BK${new Date().toISOString().slice(0, 10).replace(/-/g, '')}${Math.floor(Math.random() * 10000)}`;

    this.router.navigate(['/photographer-booking/success'], {
      state: {
        bookingId: bookingId,
        bookingNumber: bookingNumber,
        serviceName: this.selectedService.serviceName,
        price: this.totalPrice,
        includedPhotos: this.selectedService.includedPhotos || 30,
        deliveryDays: this.selectedService.deliveryDays || 7,
        location: this.selectedService.serviceName,
        date: this.selectedDate?.toISOString(),
        photographerName: this.photographerName,
        photographerId: this.photographerId
      }
    });

  }

  onSelectDate(): void {
    // Replaced by slot dropdown for now
  }
}
