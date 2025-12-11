import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VideoCommentReplyComponent } from './video-comment-reply.component';

describe('VideoCommentReplyComponent', () => {
  let component: VideoCommentReplyComponent;
  let fixture: ComponentFixture<VideoCommentReplyComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VideoCommentReplyComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VideoCommentReplyComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
