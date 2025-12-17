import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PhotoClassifyComponent } from './photo-classify.component';

describe('PhotoClassifyComponent', () => {
  let component: PhotoClassifyComponent;
  let fixture: ComponentFixture<PhotoClassifyComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PhotoClassifyComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PhotoClassifyComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
