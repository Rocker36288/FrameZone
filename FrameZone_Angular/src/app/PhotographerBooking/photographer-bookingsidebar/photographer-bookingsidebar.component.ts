import { CommonModule } from '@angular/common';
import { Component, Input, OnChanges } from '@angular/core';
import { ServiceDto, AvailableSlotDto } from '../models/photographer-booking.models';
import { PhotographerBookingService } from '../services/photographer-booking.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-photographer-bookingsidebar',
  imports: [CommonModule, FormsModule],
  templateUrl: './photographer-bookingsidebar.component.html',
  styleUrl: './photographer-bookingsidebar.component.css',
})
export class PhotographerBookingsidebarComponent implements OnChanges {
  @Input() selectedService: ServiceDto | null = null;
  @Input() photographerId!: number;

  availableSlots: AvailableSlotDto[] = [];
  selectedSlotId: number | null = null;

  constructor(private bookingService: PhotographerBookingService) { }

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
    if (!this.selectedService) {
      alert('請先選擇拍攝方案');
      return;
    }

    if (!this.selectedSlotId) {
      alert('請選擇拍攝日期與時段');
      return;
    }

    const bookingDto = {
      photographerId: this.photographerId,
      availableSlotId: this.selectedSlotId,
      userId: 1, // TODO: Get from auth service
      location: this.selectedService.serviceName, // simplified
      paymentMethodId: 1 // simplified/mock
    };

    this.bookingService.createBooking(bookingDto).subscribe({
      next: (res) => {
        alert('預約成功! 編號: ' + res.bookingId);
        // Navigate to success page or booking history
      },
      error: (err) => alert('預約失敗: ' + err.message)
    });
  }

  onSelectDate(): void {
    // Replaced by slot dropdown for now
  }
}
