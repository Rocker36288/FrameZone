import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VideoHomeComponent } from './video-home.component';

describe('VideoHomeComponent', () => {
  let component: VideoHomeComponent;
  let fixture: ComponentFixture<VideoHomeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VideoHomeComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VideoHomeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
