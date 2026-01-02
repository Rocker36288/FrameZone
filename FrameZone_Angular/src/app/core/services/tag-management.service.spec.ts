import { TestBed } from '@angular/core/testing';

import { TagManagementService } from './tag-management.service';

describe('TagManagementService', () => {
  let service: TagManagementService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TagManagementService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
