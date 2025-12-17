import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PhotographerbookingHeaderComponent } from './photographerbooking-header.component';

describe('PhotographerbookingHeaderComponent', () => {
  let component: PhotographerbookingHeaderComponent;
  let fixture: ComponentFixture<PhotographerbookingHeaderComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PhotographerbookingHeaderComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PhotographerbookingHeaderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
