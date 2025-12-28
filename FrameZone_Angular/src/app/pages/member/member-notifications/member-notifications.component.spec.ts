import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MemberNotificationsComponent } from './member-notifications.component';

describe('MemberNotificationsComponent', () => {
  let component: MemberNotificationsComponent;
  let fixture: ComponentFixture<MemberNotificationsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MemberNotificationsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MemberNotificationsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
