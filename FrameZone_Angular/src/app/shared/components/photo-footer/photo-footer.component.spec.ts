import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PhotoFooterComponent } from './photo-footer.component';

describe('PhotoFooterComponent', () => {
  let component: PhotoFooterComponent;
  let fixture: ComponentFixture<PhotoFooterComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PhotoFooterComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PhotoFooterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
