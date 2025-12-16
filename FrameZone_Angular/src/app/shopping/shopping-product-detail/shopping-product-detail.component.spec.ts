import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ShoppingProductDetailComponent } from './shopping-product-detail.component';

describe('ShoppingProductDetailComponent', () => {
  let component: ShoppingProductDetailComponent;
  let fixture: ComponentFixture<ShoppingProductDetailComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ShoppingProductDetailComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ShoppingProductDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
