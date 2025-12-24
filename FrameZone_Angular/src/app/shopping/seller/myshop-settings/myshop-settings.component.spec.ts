import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MyshopSettingsComponent } from './myshop-settings.component';

describe('MyshopSettingsComponent', () => {
  let component: MyshopSettingsComponent;
  let fixture: ComponentFixture<MyshopSettingsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MyshopSettingsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MyshopSettingsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
