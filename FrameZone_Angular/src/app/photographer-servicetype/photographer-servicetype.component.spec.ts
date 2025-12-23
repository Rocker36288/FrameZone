import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PhotographerServicetypeComponent } from './photographer-servicetype.component';

describe('PhotographerServicetypeComponent', () => {
  let component: PhotographerServicetypeComponent;
  let fixture: ComponentFixture<PhotographerServicetypeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PhotographerServicetypeComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PhotographerServicetypeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
