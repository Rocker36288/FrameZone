import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VideosSidebarComponent } from './videos-sidebar.component';

describe('VideosSidebarComponent', () => {
  let component: VideosSidebarComponent;
  let fixture: ComponentFixture<VideosSidebarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VideosSidebarComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VideosSidebarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
