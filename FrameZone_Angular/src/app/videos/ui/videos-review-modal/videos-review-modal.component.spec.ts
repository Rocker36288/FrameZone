import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VideosReviewModalComponent } from './videos-review-modal.component';

describe('VideosReviewModalComponent', () => {
  let component: VideosReviewModalComponent;
  let fixture: ComponentFixture<VideosReviewModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VideosReviewModalComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VideosReviewModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
