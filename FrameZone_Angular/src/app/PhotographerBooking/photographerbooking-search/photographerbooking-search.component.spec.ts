import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PhotographerbookingSearchComponent } from './photographerbooking-search.component';

describe('PhotographerbookingSearchComponent', () => {
  let component: PhotographerbookingSearchComponent;
  let fixture: ComponentFixture<PhotographerbookingSearchComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PhotographerbookingSearchComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PhotographerbookingSearchComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
