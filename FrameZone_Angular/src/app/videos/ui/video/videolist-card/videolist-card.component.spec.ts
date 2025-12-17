import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VideolistCardComponent } from './videolist-card.component';

describe('VideolistCardComponent', () => {
  let component: VideolistCardComponent;
  let fixture: ComponentFixture<VideolistCardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VideolistCardComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VideolistCardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
