import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VideosNotloginyetModalComponent } from './videos-notloginyet-modal.component';

describe('VideosNotloginyetModalComponent', () => {
  let component: VideosNotloginyetModalComponent;
  let fixture: ComponentFixture<VideosNotloginyetModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VideosNotloginyetModalComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VideosNotloginyetModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
