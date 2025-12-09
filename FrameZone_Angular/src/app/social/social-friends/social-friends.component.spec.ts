import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SocialFriendsComponent } from './social-friends.component';

describe('SocialFriendsComponent', () => {
  let component: SocialFriendsComponent;
  let fixture: ComponentFixture<SocialFriendsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SocialFriendsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SocialFriendsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
