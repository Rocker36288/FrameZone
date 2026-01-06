import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Output } from '@angular/core';
interface Friend {
  id: number;
  name: string;
  avatar: string;
  online: boolean;
}

interface GroupChat {
  id: number;
  name: string;
  iconClass: string;
  bgColor: string;
  iconColor: string;
}
@Component({
  selector: 'app-social-friends',
  imports: [CommonModule],
  templateUrl: './social-friends.component.html',
  styleUrl: './social-friends.component.css'
})

export class SocialFriendsComponent {
  @Output() friendSelected = new EventEmitter<Friend>();

  friends: Friend[] = [
    { id: 1, name: 'Alex Thompson', avatar: 'https://i.pravatar.cc/150?u=a1', online: true },
    { id: 2, name: '李詩涵', avatar: 'https://i.pravatar.cc/150?img=37', online: true },
    { id: 3, name: 'Jessica Wu', avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=Jessica', online: true },
    { id: 4, name: '張佳玲', avatar: 'https://i.pravatar.cc/150?img=25', online: true },
    { id: 5, name: '林俊宇', avatar: 'https://api.dicebear.com/7.x/bottts/svg?seed=JunYu', online: true },
    { id: 6, name: '趙敏君', avatar: 'https://i.pravatar.cc/150?u=zh6', online: true },
    { id: 7, name: '夜羽', avatar: 'https://api.dicebear.com/7.x/micah/svg?seed=NightHaze', online: false },
    { id: 8, name: 'Emily Chen', avatar: 'https://api.dicebear.com/7.x/adventurer/svg?seed=Emily', online: true },
    { id: 9, name: '曾可欣', avatar: 'https://i.pravatar.cc/150?img=40', online: true },
    { id: 10, name: 'Amy Wang', avatar: 'https://api.dicebear.com/7.x/fun-emoji/svg?seed=Amy', online: false }
  ];

  groupChats: GroupChat[] = [
    {
      id: 1,
      name: '大學同學會',
      iconClass: 'bi bi-chat-left-quote-fill',
      bgColor: '#e7f3ff',
      iconColor: '#007bff'
    },
    {
      id: 2,
      name: '遊戲開團群',
      iconClass: 'bi bi-controller',
      bgColor: '#fce4ec',
      iconColor: '#d81b60'
    },
    {
      id: 3,
      name: '專題討論小組',
      iconClass: 'bi bi-briefcase-fill',
      bgColor: '#f0f2f5',
      iconColor: '#555'
    }
  ];

  selectFriend(friend: Friend) {
    this.friendSelected.emit(friend);
  }
}
