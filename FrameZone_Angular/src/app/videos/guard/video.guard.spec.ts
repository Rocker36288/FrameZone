import { TestBed } from '@angular/core/testing';
import { CanActivateFn } from '@angular/router';

import { videoGuard } from './video.guard';

describe('videoGuard', () => {
  const executeGuard: CanActivateFn = (...guardParameters) => 
      TestBed.runInInjectionContext(() => videoGuard(...guardParameters));

  beforeEach(() => {
    TestBed.configureTestingModule({});
  });

  it('should be created', () => {
    expect(executeGuard).toBeTruthy();
  });
});
