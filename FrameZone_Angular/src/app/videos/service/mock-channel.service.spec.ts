import { TestBed } from '@angular/core/testing';

import { MockChannelService } from './mock-channel.service';

describe('MockChannelService', () => {
  let service: MockChannelService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MockChannelService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
