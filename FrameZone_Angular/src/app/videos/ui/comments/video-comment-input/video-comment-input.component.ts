import { AuthService } from './../../../../core/services/auth.service';
import { Component, EventEmitter, Output } from '@angular/core';
import { NgIf } from '@angular/common';
import { FormsModule } from "@angular/forms";
import { LoginResponseDto } from '../../../../core/models/auth.models';

@Component({
  selector: 'app-video-comment-input',
  imports: [NgIf, FormsModule],
  templateUrl: './video-comment-input.component.html',
  styleUrl: './video-comment-input.component.css'
})
export class CommentInputComponent {
  commentText: string = '';

  @Output() submitComment = new EventEmitter<string>();
  @Output() buttonClicked = new EventEmitter<void>();
  user: LoginResponseDto | null = null;

  constructor(private authService: AuthService) {

  }

  ngOnInit(): void {
    this.user = this.authService.getCurrentUser()
  }

  onSubmit() {
    if (this.commentText.trim().length === 0) return;

    this.submitComment.emit(this.commentText.trim());
    this.commentText = '';
  }

  onCancel() {
    this.buttonClicked.emit();
    this.commentText = '';
  }
}
