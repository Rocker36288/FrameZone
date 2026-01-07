import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PhotographerServiceinfoComponent } from './photographer-serviceinfo.component';

describe('PhotographerServiceinfoComponent', () => {
  let component: PhotographerServiceinfoComponent;
  let fixture: ComponentFixture<PhotographerServiceinfoComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PhotographerServiceinfoComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PhotographerServiceinfoComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
