import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HomethemeComponent } from './hometheme.component';

describe('HomethemeComponent', () => {
  let component: HomethemeComponent;
  let fixture: ComponentFixture<HomethemeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HomethemeComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(HomethemeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
