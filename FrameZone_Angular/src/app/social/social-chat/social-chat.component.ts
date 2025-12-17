import { Component } from '@angular/core';

@Component({
  selector: 'app-social-chat',
  imports: [],
  templateUrl: './social-chat.component.html',
  styleUrl: './social-chat.component.css'
})
export class SocialChatComponent {
  isOpen = false;

  toggleChat() {
    this.isOpen = !this.isOpen;
  }
}
