import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VideoCommentItemComponent } from './video-comment-item.component';

describe('VideoCommentItemComponent', () => {
  let component: VideoCommentItemComponent;
  let fixture: ComponentFixture<VideoCommentItemComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VideoCommentItemComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VideoCommentItemComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
