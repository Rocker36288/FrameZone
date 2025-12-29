import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PhotographerBookingServicetypesComponent } from './photographer-booking-servicetypes.component';

describe('PhotographerBookingServicetypesComponent', () => {
  let component: PhotographerBookingServicetypesComponent;
  let fixture: ComponentFixture<PhotographerBookingServicetypesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PhotographerBookingServicetypesComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PhotographerBookingServicetypesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
