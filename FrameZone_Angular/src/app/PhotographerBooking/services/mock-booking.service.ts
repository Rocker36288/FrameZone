import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class MockBookingService {
    private bookings: any[] = [
        {
            bookingId: 101,
            bookingNumber: 'BK202601110001',
            photographerName: 'Alex Wang',
            serviceName: '戶外人像寫真',
            price: 3600,
            bookingDate: new Date('2026-01-11 14:00'),
            status: '已確認',
            includedPhotos: 30,
            deliveryDays: 7,
            expectedDeliveryDate: new Date('2026-01-18')
        },
        {
            bookingId: 102,
            bookingNumber: 'BK202512250015',
            photographerName: 'Elena Lin',
            serviceName: '韓式個人證件照',
            price: 1200,
            bookingDate: new Date('2025-12-25 10:30'),
            status: '已完成',
            includedPhotos: 2,
            deliveryDays: 3,
            expectedDeliveryDate: new Date('2025-12-28')
        }
    ];

    private bookingsSubject = new BehaviorSubject<any[]>(this.bookings);

    getBookings(): Observable<any[]> {
        return this.bookingsSubject.asObservable();
    }

    addBooking(booking: any): void {
        // 避免重複加入同一筆預約 (雖然是模擬，但防止開發時 hot reload 重複觸發)
        const exists = this.bookings.find(b => b.bookingNumber === booking.bookingNumber);
        if (!exists) {
            this.bookings = [booking, ...this.bookings];
            this.bookingsSubject.next(this.bookings);
        }
    }
}
