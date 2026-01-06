import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EcpayFormComponent } from './ecpay-form.component';

describe('EcpayFormComponent', () => {
  let component: EcpayFormComponent;
  let fixture: ComponentFixture<EcpayFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EcpayFormComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EcpayFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
