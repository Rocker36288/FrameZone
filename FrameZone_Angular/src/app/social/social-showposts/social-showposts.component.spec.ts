import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SocialShowpostsComponent } from './social-showposts.component';

describe('SocialShowpostsComponent', () => {
  let component: SocialShowpostsComponent;
  let fixture: ComponentFixture<SocialShowpostsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SocialShowpostsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SocialShowpostsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
