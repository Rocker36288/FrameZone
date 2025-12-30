import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VideocreatorAnalysisComponent } from './videocreator-analysis.component';

describe('VideocreatorAnalysisComponent', () => {
  let component: VideocreatorAnalysisComponent;
  let fixture: ComponentFixture<VideocreatorAnalysisComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VideocreatorAnalysisComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VideocreatorAnalysisComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
