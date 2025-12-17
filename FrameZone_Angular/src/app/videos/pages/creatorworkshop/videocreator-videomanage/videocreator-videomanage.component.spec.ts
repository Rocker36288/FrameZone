import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VideocreatorVideomanageComponent } from './videocreator-videomanage.component';

describe('VideocreatorVideomanageComponent', () => {
  let component: VideocreatorVideomanageComponent;
  let fixture: ComponentFixture<VideocreatorVideomanageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VideocreatorVideomanageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VideocreatorVideomanageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
