import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ShoppingChatsComponent } from './shopping-chats.component';

describe('ShoppingChatsComponent', () => {
  let component: ShoppingChatsComponent;
  let fixture: ComponentFixture<ShoppingChatsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ShoppingChatsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ShoppingChatsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
