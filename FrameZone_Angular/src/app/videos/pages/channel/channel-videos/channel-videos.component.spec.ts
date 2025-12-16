import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ChannelVideosComponent } from './channel-videos.component';

describe('ChannelVideosComponent', () => {
  let component: ChannelVideosComponent;
  let fixture: ComponentFixture<ChannelVideosComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ChannelVideosComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ChannelVideosComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
