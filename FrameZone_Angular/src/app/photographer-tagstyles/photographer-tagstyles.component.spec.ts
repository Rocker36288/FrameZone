import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PhotographerTagstylesComponent } from './photographer-tagstyles.component';

describe('PhotographerTagstylesComponent', () => {
  let component: PhotographerTagstylesComponent;
  let fixture: ComponentFixture<PhotographerTagstylesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PhotographerTagstylesComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PhotographerTagstylesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
