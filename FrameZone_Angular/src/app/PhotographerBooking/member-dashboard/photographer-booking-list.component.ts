import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MockBookingService } from '../services/mock-booking.service';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'app-photographer-booking-list',
    standalone: true,
    imports: [CommonModule, RouterModule, FormsModule],
    templateUrl: './photographer-booking-list.component.html',
    styleUrl: './photographer-booking-list.component.css',
})
export class PhotographerBookingListComponent implements OnInit {
    bookings: any[] = [];
    // 評價彈窗相關狀態
    showReviewModal: boolean = false;
    currentReviewBooking: any = null;
    tempRating: number = 5;
    tempComment: string = '';

    constructor(
        private mockService: MockBookingService,
        private router: Router,
        private toastr: ToastrService
    ) { }

    ngOnInit(): void {
        this.mockService.getBookings().subscribe((data) => {
            // 初始化時為每個預約增加展開狀態
            this.bookings = data.map((b) => ({ ...b, isExpanded: false }));
        });
    }

    toggleDetails(booking: any): void {
        booking.isExpanded = !booking.isExpanded;
    }

    // 根據正規化的 BookingStatus 與 PaymentStatus 獲取顯示標籤
    getDisplayStatus(booking: any): string {
        if (booking.bookingStatus === '已完成' && booking.isReviewed) {
            return '已評價';
        }
        if (booking.paymentStatus === '未付款' && booking.bookingStatus !== '已取消') {
            return '待付款';
        }
        return booking.bookingStatus;
    }

    getStatusClass(booking: any): string {
        const status = this.getDisplayStatus(booking);
        switch (status) {
            case '已確認':
                return 'status-confirmed';
            case '待付款':
                return 'status-unpaid';
            case '已完成':
                return 'status-completed';
            case '已評價':
                return 'status-reviewed';
            case '已取消':
                return 'status-cancelled';
            default:
                return '';
        }
    }

    openReviewModal(booking: any): void {
        this.currentReviewBooking = booking;
        this.tempRating = 5;
        this.tempComment = '';
        this.showReviewModal = true;
    }

    closeReviewModal(): void {
        this.showReviewModal = false;
        this.currentReviewBooking = null;
    }

    setRating(rating: number): void {
        this.tempRating = rating;
    }

    submitReview(): void {
        if (!this.currentReviewBooking) return;

        const newReview = {
            reviewId: Date.now(),
            photographerId: this.currentReviewBooking.photographerId || 1,
            reviewerName: 'LIU SHU',
            rating: this.tempRating,
            reviewContent: this.tempComment || '這是一個預設評價內容',
            createdAt: new Date().toLocaleDateString('zh-TW', {
                year: 'numeric',
                month: 'long',
            }),
            avatarUrl: 'images/Photographer/me.png',
            photos: [],
        };

        this.mockService.addReview(newReview);
        // 更新為「已評價」標記，但保持訂單狀態為「已完成」
        this.mockService.updateBookingStatus(
            this.currentReviewBooking.bookingNumber,
            { isReviewed: true }
        );
        this.toastr.success(
            `您對 ${this.currentReviewBooking.photographerName} 的評價已送出。`,
            '評價成功！',
            {
                timeOut: 3000,
                progressBar: true,
                closeButton: true,
                positionClass: 'toast-top-center'
            }
        );
        this.closeReviewModal();
    }

    onPay(booking: any): void {
        const price = booking.servicePrice || booking.price || 0;
        if (confirm(`確認支付 TWD ${price.toLocaleString()} 嗎？`)) {
            this.mockService.updateBookingStatus(booking.bookingNumber, {
                paymentStatus: '已付款',
                bookingStatus: '已確認'
            });
            alert('支付成功！預約已確認。');
        }
    }

    viewVoucher(booking: any): void {
        // 使用 state 傳遞資料，避免參數出現在網址
        this.router.navigate(['/photographer-booking/success'], {
            state: {
                ...booking,
                price: booking.servicePrice || booking.price,
                date: booking.bookingStartDatetime || booking.bookingDate
            }
        });
    }
}
