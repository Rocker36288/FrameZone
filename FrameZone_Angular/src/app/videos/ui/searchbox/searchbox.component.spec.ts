import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SearchboxComponent } from './searchbox.component';

describe('SearchboxComponent', () => {
  let component: SearchboxComponent;
  let fixture: ComponentFixture<SearchboxComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SearchboxComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SearchboxComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
