import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SellerSidebarComponent } from './seller-sidebar.component';

describe('SellerSidebarComponent', () => {
  let component: SellerSidebarComponent;
  let fixture: ComponentFixture<SellerSidebarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SellerSidebarComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SellerSidebarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
