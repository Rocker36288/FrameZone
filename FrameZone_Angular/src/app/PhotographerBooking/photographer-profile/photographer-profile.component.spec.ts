import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PhotographerProfileComponent } from './photographer-profile.component';

describe('PhotographerProfileComponent', () => {
  let component: PhotographerProfileComponent;
  let fixture: ComponentFixture<PhotographerProfileComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PhotographerProfileComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PhotographerProfileComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
