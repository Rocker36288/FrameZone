export interface RecentChat {
  roomId: number;
  targetUserId: number;
  targetUserName: string;
  targetUserAvatar?: string | null;
  lastMessage?: string | null;
  lastMessageType?: string | null;
  lastMessageCreatedAt: string;
}
