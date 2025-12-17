import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PhotoAboutComponent } from './photo-about.component';

describe('PhotoAboutComponent', () => {
  let component: PhotoAboutComponent;
  let fixture: ComponentFixture<PhotoAboutComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PhotoAboutComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PhotoAboutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
