import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CouponSettingsComponent } from './coupon-settings.component';

describe('CouponSettingsComponent', () => {
  let component: CouponSettingsComponent;
  let fixture: ComponentFixture<CouponSettingsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CouponSettingsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CouponSettingsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
