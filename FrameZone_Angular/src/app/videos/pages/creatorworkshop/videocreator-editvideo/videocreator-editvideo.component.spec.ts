import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VideocreatorEditvideoComponent } from './videocreator-editvideo.component';

describe('VideocreatorEditvideoComponent', () => {
  let component: VideocreatorEditvideoComponent;
  let fixture: ComponentFixture<VideocreatorEditvideoComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VideocreatorEditvideoComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VideocreatorEditvideoComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
