import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PhotographerbookingHeaderComponent } from '../photographerbooking-header/photographerbooking-header.component';
import { PhotographerBookingheroComponent } from "../photographer-bookinghero/photographer-bookinghero.component";
import { PhotographerBookingServicetypesComponent } from "../photographer-booking-servicetypes/photographer-booking-servicetypes.component";
import { PhotographerBookingSpecialtytagsComponent } from "../photographer-booking-specialtytags/photographer-booking-specialtytags.component";
import { FooterComponent } from "../../shared/components/footer/footer.component";

@Component({
  selector: 'app-photographer-bookinghome',
  imports: [CommonModule, FormsModule, PhotographerbookingHeaderComponent, PhotographerBookingheroComponent, PhotographerBookingServicetypesComponent, PhotographerBookingSpecialtytagsComponent, FooterComponent],
  templateUrl: './photographer-bookinghome.component.html',
  styleUrl: './photographer-bookinghome.component.css',
})
export class PhotographerBookinghomeComponent {}
