import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SocialPostsListComponent } from './social-posts-list.component';

describe('SocialPostlistComponent', () => {
  let component: SocialPostsListComponent;
  let fixture: ComponentFixture<SocialPostsListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SocialPostsListComponent]
    })
      .compileComponents();

    fixture = TestBed.createComponent(SocialPostsListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
