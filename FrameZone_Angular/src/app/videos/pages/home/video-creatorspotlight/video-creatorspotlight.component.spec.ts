import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VideoCreatorspotlightComponent } from './video-creatorspotlight.component';

describe('VideoCreatorspotlightComponent', () => {
  let component: VideoCreatorspotlightComponent;
  let fixture: ComponentFixture<VideoCreatorspotlightComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VideoCreatorspotlightComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VideoCreatorspotlightComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
