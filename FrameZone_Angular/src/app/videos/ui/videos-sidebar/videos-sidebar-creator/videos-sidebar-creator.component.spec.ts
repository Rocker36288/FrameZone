import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VideosSidebarCreatorComponent } from './videos-sidebar-creator.component';

describe('VideosSidebarCreatorComponent', () => {
  let component: VideosSidebarCreatorComponent;
  let fixture: ComponentFixture<VideosSidebarCreatorComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VideosSidebarCreatorComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VideosSidebarCreatorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
