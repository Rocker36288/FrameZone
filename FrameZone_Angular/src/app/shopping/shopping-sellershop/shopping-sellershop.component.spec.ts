import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ShoppingSellershopComponent } from './shopping-sellershop.component';

describe('ShoppingSellershopComponent', () => {
  let component: ShoppingSellershopComponent;
  let fixture: ComponentFixture<ShoppingSellershopComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ShoppingSellershopComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ShoppingSellershopComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
