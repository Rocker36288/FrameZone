import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SocialMenuComponent } from './social-menu.component';

describe('SocialMenuComponent', () => {
  let component: SocialMenuComponent;
  let fixture: ComponentFixture<SocialMenuComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SocialMenuComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SocialMenuComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
