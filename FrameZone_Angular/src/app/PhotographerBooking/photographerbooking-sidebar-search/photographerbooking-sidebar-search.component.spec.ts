import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PhotographerbookingSidebarSearchComponent } from './photographerbooking-sidebar-search.component';

describe('PhotographerbookingSidebarSearchComponent', () => {
  let component: PhotographerbookingSidebarSearchComponent;
  let fixture: ComponentFixture<PhotographerbookingSidebarSearchComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PhotographerbookingSidebarSearchComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PhotographerbookingSidebarSearchComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
