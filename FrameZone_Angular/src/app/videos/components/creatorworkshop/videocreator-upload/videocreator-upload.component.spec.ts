import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VideocreatorUploadComponent } from './videocreator-upload.component';

describe('VideocreatorUploadComponent', () => {
  let component: VideocreatorUploadComponent;
  let fixture: ComponentFixture<VideocreatorUploadComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VideocreatorUploadComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VideocreatorUploadComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
