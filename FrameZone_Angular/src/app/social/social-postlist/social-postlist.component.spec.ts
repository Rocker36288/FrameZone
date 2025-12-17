import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SocialPostlistComponent } from './social-postlist.component';

describe('SocialPostlistComponent', () => {
  let component: SocialPostlistComponent;
  let fixture: ComponentFixture<SocialPostlistComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SocialPostlistComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SocialPostlistComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
