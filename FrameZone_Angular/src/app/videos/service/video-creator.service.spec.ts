import { TestBed } from '@angular/core/testing';

import { VideoCreatorService } from './video-creator.service';

describe('VideoCreatorService', () => {
  let service: VideoCreatorService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(VideoCreatorService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
