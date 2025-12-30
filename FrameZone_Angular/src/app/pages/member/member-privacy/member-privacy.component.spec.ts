import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MemberPrivacyComponent } from './member-privacy.component';

describe('MemberPrivacyComponent', () => {
  let component: MemberPrivacyComponent;
  let fixture: ComponentFixture<MemberPrivacyComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MemberPrivacyComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MemberPrivacyComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
