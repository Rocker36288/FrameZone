import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VideosRecommendedListComponent } from './videos-recommended-list.component';

describe('VideosRecommendedListComponent', () => {
  let component: VideosRecommendedListComponent;
  let fixture: ComponentFixture<VideosRecommendedListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VideosRecommendedListComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VideosRecommendedListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
