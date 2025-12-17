import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PhotoPriceComponent } from './photo-price.component';

describe('PhotoPriceComponent', () => {
  let component: PhotoPriceComponent;
  let fixture: ComponentFixture<PhotoPriceComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PhotoPriceComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PhotoPriceComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
