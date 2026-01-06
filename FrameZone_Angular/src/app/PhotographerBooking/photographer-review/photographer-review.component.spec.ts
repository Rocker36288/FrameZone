import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PhotographerReviewComponent } from './photographer-review.component';

describe('PhotographerReviewComponent', () => {
  let component: PhotographerReviewComponent;
  let fixture: ComponentFixture<PhotographerReviewComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PhotographerReviewComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PhotographerReviewComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
