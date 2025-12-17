import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VideocreatorHomeComponent } from './videocreator-home.component';

describe('VideocreatorHomeComponent', () => {
  let component: VideocreatorHomeComponent;
  let fixture: ComponentFixture<VideocreatorHomeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VideocreatorHomeComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VideocreatorHomeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
