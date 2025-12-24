import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SocialCommentsComponent } from './social-comments.component';

describe('SocialCommentsComponent', () => {
  let component: SocialCommentsComponent;
  let fixture: ComponentFixture<SocialCommentsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SocialCommentsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SocialCommentsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
