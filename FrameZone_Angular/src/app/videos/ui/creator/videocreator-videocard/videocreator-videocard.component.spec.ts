import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VideocreatorVideocardComponent } from './videocreator-videocard.component';

describe('VideocreatorVideocardComponent', () => {
  let component: VideocreatorVideocardComponent;
  let fixture: ComponentFixture<VideocreatorVideocardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VideocreatorVideocardComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VideocreatorVideocardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
