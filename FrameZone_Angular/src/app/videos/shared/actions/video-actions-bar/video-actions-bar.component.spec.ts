import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VideoActionsBarComponent } from './video-actions-bar.component';

describe('VideoActionsBarComponent', () => {
  let component: VideoActionsBarComponent;
  let fixture: ComponentFixture<VideoActionsBarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VideoActionsBarComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VideoActionsBarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
