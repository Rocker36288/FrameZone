import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterModule, Router } from '@angular/router';
import { CommonModule } from '@angular/common';

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
        private router: Router
    ) { }

    ngOnInit(): void {
        this.route.queryParams.subscribe(params => {
            this.bookingData = { ...params };

            // 計算預計交付日期
            if (this.bookingData.date && this.bookingData.deliveryDays) {
                const date = new Date(this.bookingData.date);
                date.setDate(date.getDate() + parseInt(this.bookingData.deliveryDays));
                this.bookingData.expectedDeliveryDate = date;
            }
        });
    }

    goHome(): void {
        this.router.navigate(['/home']);
    }
}
