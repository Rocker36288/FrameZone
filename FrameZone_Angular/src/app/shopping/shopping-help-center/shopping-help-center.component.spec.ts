import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ShoppingHelpCenterComponent } from './shopping-help-center.component';

describe('ShoppingHelpCenterComponent', () => {
  let component: ShoppingHelpCenterComponent;
  let fixture: ComponentFixture<ShoppingHelpCenterComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ShoppingHelpCenterComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ShoppingHelpCenterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
