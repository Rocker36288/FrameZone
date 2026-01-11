import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { MockBookingService } from '../services/mock-booking.service';

@Component({
    selector: 'app-booking-success',
    standalone: true,
    imports: [CommonModule, RouterModule],
    templateUrl: './booking-success.component.html',
    styleUrl: './booking-success.component.css'
})
export class BookingSuccessComponent implements OnInit {
    bookingData: any = {};

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private mockService: MockBookingService
    ) { }

    ngOnInit(): void {
        this.route.queryParams.subscribe(params => {
            if (Object.keys(params).length === 0) return;

            this.bookingData = { ...params };

            // 計算預計交付日期
            if (this.bookingData.date && this.bookingData.deliveryDays) {
                const date = new Date(this.bookingData.date);
                date.setDate(date.getDate() + parseInt(this.bookingData.deliveryDays));
                this.bookingData.expectedDeliveryDate = date;
            }

            // 同步到模擬列表服務
            this.mockService.addBooking({
                ...this.bookingData,
                bookingNumber: this.mockService.generateBookingNumber(), // 生成符合規範的編號
                bookingStartDatetime: this.bookingData.date,
                servicePrice: this.bookingData.price,
                bookingStatus: '已確認',
                paymentStatus: '未付款'
            });
        });
    }

    goToMyBookings(): void {
        this.router.navigate(['/booking-center']);
    }

    goHome(): void {
        this.router.navigate(['/home']);
    }
}
