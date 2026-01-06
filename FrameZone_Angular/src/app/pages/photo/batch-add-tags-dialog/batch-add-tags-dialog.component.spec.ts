import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BatchAddTagsDialogComponent } from './batch-add-tags-dialog.component';

describe('BatchAddTagsDialogComponent', () => {
  let component: BatchAddTagsDialogComponent;
  let fixture: ComponentFixture<BatchAddTagsDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BatchAddTagsDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BatchAddTagsDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
