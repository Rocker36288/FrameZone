import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MockBookingService } from '../services/mock-booking.service';

@Component({
    selector: 'app-photographer-booking-list',
    standalone: true,
    imports: [CommonModule, RouterModule],
    templateUrl: './photographer-booking-list.component.html',
    styleUrl: './photographer-booking-list.component.css'
})
export class PhotographerBookingListComponent implements OnInit {
    bookings: any[] = [];

    constructor(private mockService: MockBookingService) { }

    ngOnInit(): void {
        this.mockService.getBookings().subscribe(data => {
            this.bookings = data;
        });
    }

    getStatusClass(status: string): string {
        switch (status) {
            case '已確認': return 'status-confirmed';
            case '待拍攝': return 'status-pending';
            case '已完成': return 'status-completed';
            default: return '';
        }
    }
}
