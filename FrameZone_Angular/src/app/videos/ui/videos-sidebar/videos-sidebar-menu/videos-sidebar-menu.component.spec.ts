import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VideosSidebarMenuComponent } from './videos-sidebar-menu.component';

describe('VideosSidebarMenuComponent', () => {
  let component: VideosSidebarMenuComponent;
  let fixture: ComponentFixture<VideosSidebarMenuComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VideosSidebarMenuComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VideosSidebarMenuComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
