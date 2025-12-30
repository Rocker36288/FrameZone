import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MemberSecurityComponent } from './member-security.component';

describe('MemberSecurityComponent', () => {
  let component: MemberSecurityComponent;
  let fixture: ComponentFixture<MemberSecurityComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MemberSecurityComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MemberSecurityComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
