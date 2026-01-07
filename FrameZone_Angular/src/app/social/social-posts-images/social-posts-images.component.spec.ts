import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SocialPostsImagesComponent } from './social-posts-images.component';

describe('SocialPostsImagesComponent', () => {
  let component: SocialPostsImagesComponent;
  let fixture: ComponentFixture<SocialPostsImagesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SocialPostsImagesComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SocialPostsImagesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
