import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ShoppingOrderSuccessComponent } from './shopping-order-success.component';

describe('ShoppingOrderSuccessComponent', () => {
  let component: ShoppingOrderSuccessComponent;
  let fixture: ComponentFixture<ShoppingOrderSuccessComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ShoppingOrderSuccessComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ShoppingOrderSuccessComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
