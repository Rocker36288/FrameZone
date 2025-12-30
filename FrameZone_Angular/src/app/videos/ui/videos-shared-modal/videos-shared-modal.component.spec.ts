import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VideosSharedModalComponent } from './videos-shared-modal.component';

describe('VideosSharedModalComponent', () => {
  let component: VideosSharedModalComponent;
  let fixture: ComponentFixture<VideosSharedModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VideosSharedModalComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VideosSharedModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
