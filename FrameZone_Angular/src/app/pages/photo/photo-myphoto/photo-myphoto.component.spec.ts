import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PhotoMyphotoComponent } from './photo-myphoto.component';

describe('PhotoMyphotoComponent', () => {
  let component: PhotoMyphotoComponent;
  let fixture: ComponentFixture<PhotoMyphotoComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PhotoMyphotoComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PhotoMyphotoComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
