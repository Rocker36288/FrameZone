import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VideocreatorSidebarComponent } from './videocreator-sidebar.component';

describe('VideocreatorSidebarComponent', () => {
  let component: VideocreatorSidebarComponent;
  let fixture: ComponentFixture<VideocreatorSidebarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VideocreatorSidebarComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VideocreatorSidebarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
