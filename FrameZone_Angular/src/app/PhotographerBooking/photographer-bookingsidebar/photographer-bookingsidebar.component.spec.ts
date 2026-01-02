import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PhotographerBookingsidebarComponent } from './photographer-bookingsidebar.component';

describe('PhotographerBookingsidebarComponent', () => {
  let component: PhotographerBookingsidebarComponent;
  let fixture: ComponentFixture<PhotographerBookingsidebarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PhotographerBookingsidebarComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PhotographerBookingsidebarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
