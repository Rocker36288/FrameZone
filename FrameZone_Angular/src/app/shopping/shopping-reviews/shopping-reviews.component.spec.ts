import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ShoppingReviewsComponent } from './shopping-reviews.component';

describe('ShoppingReviewsComponent', () => {
  let component: ShoppingReviewsComponent;
  let fixture: ComponentFixture<ShoppingReviewsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ShoppingReviewsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ShoppingReviewsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
