export interface VideoCardData {
  id: number;
  title: string;
  thumbnail: string;
  duration: number; // 使用秒數，方便我們在元件中用 Pipe 轉換
  views: number;
  uploadDate: Date;
  channelName: string;
  userAvatarUrl: string;
  description: string;
}
