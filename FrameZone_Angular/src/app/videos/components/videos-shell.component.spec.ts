import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VideosShellComponent } from './videos-shell.component';

describe('VideosShellComponent', () => {
  let component: VideosShellComponent;
  let fixture: ComponentFixture<VideosShellComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VideosShellComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VideosShellComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
