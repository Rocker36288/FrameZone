import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VideoCommentListComponent } from './video-comment-list.component';

describe('VideoCommentListComponent', () => {
  let component: VideoCommentListComponent;
  let fixture: ComponentFixture<VideoCommentListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VideoCommentListComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VideoCommentListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
