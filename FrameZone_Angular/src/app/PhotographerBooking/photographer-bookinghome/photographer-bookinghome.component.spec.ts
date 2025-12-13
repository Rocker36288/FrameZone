import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PhotographerBookinghomeComponent } from './photographer-bookinghome.component';

describe('PhotographerBookinghomeComponent', () => {
  let component: PhotographerBookinghomeComponent;
  let fixture: ComponentFixture<PhotographerBookinghomeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PhotographerBookinghomeComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PhotographerBookinghomeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
