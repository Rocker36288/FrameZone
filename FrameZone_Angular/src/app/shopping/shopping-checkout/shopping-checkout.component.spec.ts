import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ShoppingCheckoutComponent } from './shopping-checkout.component';

describe('ShoppingCheckoutComponent', () => {
  let component: ShoppingCheckoutComponent;
  let fixture: ComponentFixture<ShoppingCheckoutComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ShoppingCheckoutComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ShoppingCheckoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
