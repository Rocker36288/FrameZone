import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PhotographerbookingHeaderComponent } from '../photographerbooking-header/photographerbooking-header.component';
import { PhotographerBookingheroComponent } from "../photographer-bookinghero/photographer-bookinghero.component";
import { PhotographerBookingServicetypesComponent } from "../photographer-booking-servicetypes/photographer-booking-servicetypes.component";
import { PhotographerBookingSpecialtytagsComponent } from "../photographer-booking-specialtytags/photographer-booking-specialtytags.component";
import { FooterComponent } from "../../shared/components/footer/footer.component";
import { PhotographerBookingService } from '../services/photographer-booking.service';
import { PhotographerDto } from '../models/photographer-booking.models';
import { PhotographerbookingCardComponent } from '../photographerbooking-card/photographerbooking-card.component';

@Component({
  selector: 'app-photographer-bookinghome',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    PhotographerbookingHeaderComponent,
    PhotographerBookingheroComponent,
    PhotographerBookingServicetypesComponent,
    PhotographerBookingSpecialtytagsComponent,
    FooterComponent,
    PhotographerbookingCardComponent
  ],
  templateUrl: './photographer-bookinghome.component.html',
  styleUrl: './photographer-bookinghome.component.css',
})
export class PhotographerBookinghomeComponent implements OnInit {
  featuredPhotographers: PhotographerDto[] = [];

  constructor(private bookingService: PhotographerBookingService) { }

  ngOnInit(): void {
    // For now get all and take top 4 or use search parameters if API supports "featured"
    this.bookingService.getAllPhotographers().subscribe({
      next: (data) => {
        // Take first 4 as featured for now
        this.featuredPhotographers = data.slice(0, 4);
      },
      error: (err) => console.error('Error fetching photographers', err)
    });
  }
}
