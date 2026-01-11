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
        // 從 Router State 獲取資料，這樣參數就不會顯示在網址上
        const state = history.state;

        if (state && state.bookingNumber) {
            this.bookingData = { ...state };

            // 計算預計交付日期
            if (this.bookingData.date && this.bookingData.deliveryDays) {
                const date = new Date(this.bookingData.date);
                date.setDate(date.getDate() + parseInt(this.bookingData.deliveryDays));
                this.bookingData.expectedDeliveryDate = date;
            }

            // 同步到模擬列表服務
            this.mockService.addBooking({
                ...this.bookingData,
                bookingStartDatetime: this.bookingData.date,
                servicePrice: this.bookingData.price,
                bookingStatus: '已確認',
                paymentStatus: '未付款'
            });
        } else {
            // 如果沒有 state (例如直接重整頁面)，導回預約中心
            // 或者您可以選擇保留 queryParams 作為降級方案，但這裡為了隱私優先選擇導回
            this.router.navigate(['/booking-center']);
        }
    }

    goToMyBookings(): void {
        this.router.navigate(['/booking-center']);
    }

    goHome(): void {
        this.router.navigate(['/home']);
    }
}
