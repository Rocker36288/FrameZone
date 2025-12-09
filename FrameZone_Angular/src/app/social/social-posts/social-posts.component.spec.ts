import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SocialPostsComponent } from './social-posts.component';

describe('SocialPostsComponent', () => {
  let component: SocialPostsComponent;
  let fixture: ComponentFixture<SocialPostsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SocialPostsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SocialPostsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
