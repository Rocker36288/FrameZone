import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PhotographerbookingPageSearchComponent } from './photographerbooking-page-search.component';

describe('PhotographerbookingPageSearchComponent', () => {
  let component: PhotographerbookingPageSearchComponent;
  let fixture: ComponentFixture<PhotographerbookingPageSearchComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PhotographerbookingPageSearchComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PhotographerbookingPageSearchComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
