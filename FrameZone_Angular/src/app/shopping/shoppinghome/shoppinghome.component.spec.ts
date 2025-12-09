import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ShoppinghomeComponent } from './shoppinghome.component';

describe('ShoppinghomeComponent', () => {
  let component: ShoppinghomeComponent;
  let fixture: ComponentFixture<ShoppinghomeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ShoppinghomeComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ShoppinghomeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
