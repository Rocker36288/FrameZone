import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PhotographerbookingCardComponent } from './photographerbooking-card.component';

describe('PhotographerbookingCardComponent', () => {
  let component: PhotographerbookingCardComponent;
  let fixture: ComponentFixture<PhotographerbookingCardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PhotographerbookingCardComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PhotographerbookingCardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
