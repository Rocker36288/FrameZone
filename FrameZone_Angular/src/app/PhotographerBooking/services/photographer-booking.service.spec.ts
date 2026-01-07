import { TestBed } from '@angular/core/testing';

import { PhotographerBookingService } from './photographer-booking.service';

describe('PhotographerBookingService', () => {
  let service: PhotographerBookingService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PhotographerBookingService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
