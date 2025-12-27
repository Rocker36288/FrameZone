import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VideocreatorLayoutComponent } from './videocreator-layout.component';

describe('VideocreatorLayoutComponent', () => {
  let component: VideocreatorLayoutComponent;
  let fixture: ComponentFixture<VideocreatorLayoutComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VideocreatorLayoutComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VideocreatorLayoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
