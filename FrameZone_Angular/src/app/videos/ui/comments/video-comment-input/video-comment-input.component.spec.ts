import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CommentInputComponent } from './video-comment-input.component';

describe('CommentInputComponent', () => {
  let component: CommentInputComponent;
  let fixture: ComponentFixture<CommentInputComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CommentInputComponent]
    })
      .compileComponents();

    fixture = TestBed.createComponent(CommentInputComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
