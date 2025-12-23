import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PhotographerBookingheroComponent } from './photographer-bookinghero.component';

describe('PhotographerBookingheroComponent', () => {
  let component: PhotographerBookingheroComponent;
  let fixture: ComponentFixture<PhotographerBookingheroComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PhotographerBookingheroComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PhotographerBookingheroComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
