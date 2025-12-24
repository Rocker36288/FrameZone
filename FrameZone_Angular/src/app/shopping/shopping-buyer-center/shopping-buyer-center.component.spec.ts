import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ShoppingBuyerCenterComponent } from './shopping-buyer-center.component';

describe('ShoppingBuyerCenterComponent', () => {
  let component: ShoppingBuyerCenterComponent;
  let fixture: ComponentFixture<ShoppingBuyerCenterComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ShoppingBuyerCenterComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ShoppingBuyerCenterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
