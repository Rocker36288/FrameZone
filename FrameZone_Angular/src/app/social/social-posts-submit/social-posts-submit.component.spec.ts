import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SocialPostsSubmitComponent } from './social-posts-submit.component';

describe('SocialPostsComponent', () => {
  let component: SocialPostsSubmitComponent;
  let fixture: ComponentFixture<SocialPostsSubmitComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SocialPostsSubmitComponent]
    })
      .compileComponents();

    fixture = TestBed.createComponent(SocialPostsSubmitComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
