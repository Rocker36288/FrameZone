import { TestBed } from '@angular/core/testing';

import { NotificationHandlerService } from './notification-handler.service';

describe('NotificationHandlerService', () => {
  let service: NotificationHandlerService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(NotificationHandlerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
