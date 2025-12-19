import { TestBed } from '@angular/core/testing';

import { VideoWatchService } from './video-watch.service';

describe('VideoWatchService', () => {
  let service: VideoWatchService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(VideoWatchService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
