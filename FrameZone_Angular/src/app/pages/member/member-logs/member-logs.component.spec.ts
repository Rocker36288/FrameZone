import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MemberLogsComponent } from './member-logs.component';

describe('MemberLogsComponent', () => {
  let component: MemberLogsComponent;
  let fixture: ComponentFixture<MemberLogsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MemberLogsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MemberLogsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
