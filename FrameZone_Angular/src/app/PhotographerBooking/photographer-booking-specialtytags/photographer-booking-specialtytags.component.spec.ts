import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PhotographerBookingSpecialtytagsComponent } from './photographer-booking-specialtytags.component';

describe('PhotographerBookingSpecialtytagsComponent', () => {
  let component: PhotographerBookingSpecialtytagsComponent;
  let fixture: ComponentFixture<PhotographerBookingSpecialtytagsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PhotographerBookingSpecialtytagsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PhotographerBookingSpecialtytagsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
