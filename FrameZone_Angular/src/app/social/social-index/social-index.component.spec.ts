import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SocialIndexComponent } from './social-index.component';

describe('SocialIndexComponent', () => {
  let component: SocialIndexComponent;
  let fixture: ComponentFixture<SocialIndexComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SocialIndexComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SocialIndexComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
