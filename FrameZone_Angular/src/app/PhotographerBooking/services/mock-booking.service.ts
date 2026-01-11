import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { map } from 'rxjs/operators';

@Injectable({
    providedIn: 'root'
})
export class MockBookingService {
    private reviews: any[] = [
        {
            reviewId: 1,
            photographerId: 1,
            reviewerName: '簡宜君',
            rating: 5,
            reviewContent: '攝影師非常專業, 拍攝過程氣氛很輕鬆！作品色調非常優雅。',
            createdAt: '2025年12月',
            avatarUrl: 'images/Photographer/re.png',
            photos: ['images/Photographer/Carousel01.png']
        },
        {
            reviewId: 2,
            photographerId: 1,
            reviewerName: 'Linda Wang',
            rating: 4,
            reviewContent: '整體的服務體驗很好，從諮詢到拍攝都很流暢。攝影師非常有美感。',
            createdAt: '2025年10月',
            avatarUrl: 'images/Photographer/re2.png',
            photos: []
        },
        {
            reviewId: 3,
            photographerId: 2,
            reviewerName: '王小明',
            rating: 5,
            reviewContent: '這是我第二次找這位攝影師了，專業水準依舊。',
            createdAt: '2025年09月',
            avatarUrl: 'images/Photographer/re.png',
            photos: []
        }
    ];

    private reviewsSubject = new BehaviorSubject<any[]>(this.reviews);

    // 符合資料表定義的模擬預約
    private bookings: any[] = [
        {
            bookingId: 102,
            bookingNumber: 'BK20260111001',
            photographerName: 'Alex Wang',
            serviceName: '戶外人像寫真',
            servicePrice: 3600,
            bookingStartDatetime: '2026-01-11 14:00',
            bookingStatus: '已確認',
            paymentStatus: '已付款',
            photographerId: 1,
            includedPhotos: 30,
            deliveryDays: 7,
            expectedDeliveryDate: new Date('2026-01-18')
        },
        {
            bookingId: 103,
            bookingNumber: 'BK20251225015',
            photographerName: 'Elena Lin',
            serviceName: '韓式個人證件照',
            servicePrice: 1200,
            bookingStartDatetime: '2025-12-25 10:30',
            bookingStatus: '已完成',
            paymentStatus: '已付款',
            isReviewed: true,
            photographerId: 1,
            includedPhotos: 2,
            deliveryDays: 3,
            expectedDeliveryDate: new Date('2025-12-28')
        },
        {
            bookingId: 104,
            bookingNumber: 'BK20251120005',
            photographerName: 'Jessica Lee',
            serviceName: '寵物戶外寫真',
            servicePrice: 2800,
            bookingStartDatetime: '2025-11-20 15:00',
            bookingStatus: '已完成',
            paymentStatus: '已付款',
            isReviewed: false,
            photographerId: 2,
            includedPhotos: 15,
            deliveryDays: 5,
            expectedDeliveryDate: new Date('2025-11-25')
        }
    ];

    private bookingsSubject = new BehaviorSubject<any[]>(this.bookings);

    getBookings(): Observable<any[]> {
        return this.bookingsSubject.asObservable();
    }

    // 依照規範生成 BookingNumber: BK + yyyyMMdd + 三位流水號
    generateBookingNumber(): string {
        const date = new Date();
        const yyyymmdd = date.getFullYear().toString() +
            (date.getMonth() + 1).toString().padStart(2, '0') +
            date.getDate().toString().padStart(2, '0');

        // 模擬取得今日第幾筆 (這裡暫以隨機三位數模擬流水感)
        const serial = Math.floor(Math.random() * 900 + 100).toString();
        return `BK${yyyymmdd}${serial}`;
    }

    addBooking(booking: any): void {
        const exists = this.bookings.find(b => b.bookingNumber === booking.bookingNumber);
        if (!exists) {
            // 確保欄位符合規範，並強制轉換 ID 為數字
            const normalized = {
                ...booking,
                photographerId: Number(booking.photographerId),
                bookingStatus: booking.bookingStatus || '已確認',
                paymentStatus: booking.paymentStatus || '未付款',
                servicePrice: booking.price || booking.servicePrice || 0
            };
            this.bookings = [normalized, ...this.bookings];
            this.bookingsSubject.next(this.bookings);
        }
    }

    getReviews(photographerId: any): Observable<any[]> {
        const id = Number(photographerId);
        return this.reviewsSubject.asObservable().pipe(
            map(allReviews => allReviews.filter(r => Number(r.photographerId) === id))
        );
    }

    addReview(review: any): void {
        const normalizedReview = {
            ...review,
            photographerId: Number(review.photographerId)
        };
        this.reviews = [normalizedReview, ...this.reviews];
        this.reviewsSubject.next(this.reviews);
    }

    updateBookingStatus(bookingNumber: string, updates: any): void {
        const index = this.bookings.findIndex(b => b.bookingNumber === bookingNumber);
        if (index !== -1) {
            if (typeof updates === 'string') {
                // 相容舊邏輯，若傳入字串則更新核心狀態
                if (updates === '已評價') {
                    this.bookings[index].isReviewed = true;
                } else if (updates === '已完成' || updates === '已取消' || updates === '已確認') {
                    this.bookings[index].bookingStatus = updates;
                } else if (updates === '已付款') {
                    this.bookings[index].paymentStatus = '已付款';
                    this.bookings[index].bookingStatus = '已確認';
                }
            } else {
                // 新邏輯：部分更新
                this.bookings[index] = { ...this.bookings[index], ...updates };
            }
            this.bookingsSubject.next(this.bookings);
        }
    }
}
