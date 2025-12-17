import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PhotoLayoutComponent } from './photo-layout.component';

describe('PhotoLayoutComponent', () => {
  let component: PhotoLayoutComponent;
  let fixture: ComponentFixture<PhotoLayoutComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PhotoLayoutComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PhotoLayoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
